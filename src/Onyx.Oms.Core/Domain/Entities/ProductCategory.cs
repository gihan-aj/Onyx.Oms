using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Core.Domain.Entities;

public class ProductCategory : AuditableEntity<Guid>, IMustHaveTenant
{
    public const int MaxDepth = 3; // 0=Root, 1=Sub, 2=SubSub
    public const char PathSeparator = '/';
    public const string NameSeparator = " / ";

    // Private constructor for EF Core
    private ProductCategory(): base(Guid.NewGuid()) { }

    // Internal constructor for Factory
    internal ProductCategory(
        Guid tenantId,
        string name,
        int level,
        string path,
        string namePath,
        string? description,
        Guid? parentCategoryId,
        int displayOrder,
        string? iconUrl,
        string? color,
        List<SpecDefinition>? specifications = null) : base(Guid.NewGuid())
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        ParentCategoryId = parentCategoryId;
        DisplayOrder = displayOrder;
        IconUrl = iconUrl;
        Color = color;
        Level = level;
        Path = path;
        NamePath = namePath;
        IsActive = true;

        if(specifications != null)
        {
            _specifications.AddRange(specifications);
        }
    }

    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public bool IsActive { get; private set; }
    public int DisplayOrder { get; private set; }

    // Metadata properties
    public string? IconUrl { get; private set; }
    public string? Color { get; private set; }

    // Hierarchy management
    public int Level { get; private set; } // 0 for root

    // Materialized Path: /RootId/ChildId/GrandChildId
    public string Path { get; private set; } = string.Empty;

    // Breadcrumb: "Root / Child / GrandChild"
    public string NamePath { get; private set; } = string.Empty;

    // Dynamic Specifications
    // Stored as json in the database
    private readonly List<SpecDefinition> _specifications = new();
    public virtual IReadOnlyCollection<SpecDefinition> Specifications => _specifications.AsReadOnly();

    // Navigation properties
    public virtual ProductCategory? ParentCategory { get; private set; }

    private readonly List<ProductCategory> _subCategories = new();
    public virtual IReadOnlyCollection<ProductCategory> SubCategories => _subCategories.AsReadOnly();

    public static Result<ProductCategory> Create(
        Guid tenantId,
        string name,
        string? description = null,
        ProductCategory? parent = null,
        int displayOrder = 0,
        string? iconUrl = null,
        string? color = null,
        List<SpecDefinition>? specifications = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<ProductCategory>(Error.Validation("ProductCategory.NameRequired", "Category name is required."));

        int level = 0;
        Guid? parentId = null;
        string parentPath = string.Empty;
        string parentNamePath = string.Empty;

        if (parent is not null)
        {
            if (parent.Level >= MaxDepth)
            {
                return Result.Failure<ProductCategory>(Error.Validation("ProductCategory.MaxDepth",($"Maximum category depth of {MaxDepth + 1} levels reached.")));
            }

            level = parent.Level + 1;
            parentId = parent.Id;
            parentPath = parent.Path;
            parentNamePath = parent.NamePath;
        }

        var id = Guid.NewGuid();

        // Generate path: /ParentPath/Id/ or /Id/
        var path = string.IsNullOrEmpty(parentPath)
            ? $"{PathSeparator}{id}{PathSeparator}"
            : $"{parentPath}{id}{PathSeparator}";

        var namePath = string.IsNullOrEmpty(parentNamePath)
            ? name
            : $"{parentNamePath}{NameSeparator}{name}";

        var category = new ProductCategory(id, name, level, path, namePath, description, parentId, displayOrder, iconUrl, color, specifications);

        return Result.Success(category);
    }

    public void UpdateDetails(string name, string? description, int displayOrder, string? iconUrl, string? color)
    {
        if (Name != name)
        {
            // If we have a parent, rebuild name path from it, otherwise it is root.
            if(ParentCategory != null)
            {
                NamePath = $"{ParentCategory.NamePath}{NameSeparator}{name}";
            }
            else
            {
                NamePath = Name;
            }
        }

        Name = name;
        Description = description;
        DisplayOrder = displayOrder;
        IconUrl = iconUrl;
        Color = color;
    }

    public Result ChangeParent(ProductCategory? newParent)
    {
        // 1. Circular Ref (Self)
        if (newParent?.Id == Id)
        {
            return Result.Failure(Error.Conflict("ProductCategory.Circular", "Cannot map category to itself."));
        }

        // 2. Deep Circular Ref (Moving into own child)
        if (newParent is not null && newParent.Path.Contains($"{PathSeparator}{Id}{PathSeparator}"))
        {
            return Result.Failure(Error.Conflict("ProductCategory.CircularDeep", "Cannot move a category into its own child."));
        }

        // 3. Max Depth Check
        // Calculate the depth of the *subtree* we are moving.
        // Current behavior: Level = NewParentLevel + 1. 
        // We need to ensure that the deepest child of THIS category will not exceed MaxDepth.
        // This is complex without loading all descendants.
        // For simple V1, checking immediate level is often done, but technically risky.
        // However, with MaxDepth=2 (Root->Sub->SubSub), the tree is very shallow.
        
        int newLevel = newParent?.Level + 1 ?? 0;
        
        // If we have children, we need to check if they would exceed depth.
        // Since we don't know the depth of the subtree efficiently in memory without loading value,
        // we might defer this check or enforce loading.
        // For now, let's enforce simple check.
        if (newLevel > MaxDepth)
        {
             return Result.Failure(Error.Validation("ProductCategory.MaxDepth", "Moving here exceeds maximum depth."));
        }

        ParentCategoryId = newParent?.Id;
        Level = newLevel;

        var newParentPath = newParent?.Path ?? string.Empty;
        Path = string.IsNullOrEmpty(newParentPath)
            ? $"{PathSeparator}{Id}{PathSeparator}"
            : $"{newParentPath}{Id}{PathSeparator}";

        var newParentNamePath = newParent?.NamePath ?? string.Empty;
        NamePath = string.IsNullOrEmpty(newParentNamePath)
            ? $"{Name}"
            : $"{newParentNamePath}{NameSeparator}{Name}";

        // Recursively update children
        foreach (var child in _subCategories)
        {
             var result = child.UpdatePathFromParent(Path, NamePath, Level);
             if (result.IsFailure) return result;
        }

        return Result.Success();
    }

    internal Result UpdatePathFromParent(string parentPath, string parentNamePath, int parentLevel)
    {
        var newLevel = parentLevel + 1;
        if (newLevel > MaxDepth)
        {
             return Result.Failure(Error.Validation("ProductCategory.MaxDepth", $"A sub-category exceeds the maximum depth of {MaxDepth + 1} levels."));
        }

        Path = $"{parentPath}{Id}{PathSeparator}";
        NamePath = $"{parentNamePath}{NameSeparator}{Name}";
        Level = newLevel;

        foreach (var child in _subCategories)
        {
             var result = child.UpdatePathFromParent(Path, NamePath, Level);
             if (result.IsFailure) return result;
        }

        return Result.Success();
    }

    public Result UpdateSubCategoriesPaths()
    {
        foreach (var child in _subCategories)
        {
             var result = child.UpdatePathFromParent(Path, NamePath, Level);
             if (result.IsFailure) return result;
        }
        return Result.Success();
    }

    public Result UpdateSpecifications(List<SpecDefinition> newSpecs)
    {
        var duplicateKeys = newSpecs
            .GroupBy(x => x.Key)
            .Where(g => g.Count() > 1)
            .Select(y => y.Key).ToList();

        if (duplicateKeys.Any())
            return Result.Failure(Error.Validation("ProductCategory.DuplicateSpecKeys", $"Duplicate specification keys found: {string.Join(", ", duplicateKeys)}"));

        _specifications.Clear();
        _specifications.AddRange(newSpecs);

        return Result.Success();
    }

    public void Activate() => IsActive = true;

    public void Deactivate()
    {
        IsActive = false;
        foreach (var child in _subCategories)
        {
            child.Deactivate();
        }
    }
}
