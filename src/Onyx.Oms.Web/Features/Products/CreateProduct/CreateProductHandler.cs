using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.CreateProduct
{
    public class CreateProductHandler : ICommandHandler<CreateProductCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAppSequenceService _appSequenceService;

        public CreateProductHandler(IApplicationDbContext context, IAppSequenceService appSequenceService)
        {
            _context = context;
            _appSequenceService = appSequenceService;
        }

        public async Task<Result<Guid>> Handle(CreateProductCommand command, CancellationToken cancellationToken)
        {
            var category = await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == command.CategoryId, cancellationToken);
            if(category is null)
                return Result.Failure<Guid>(Error.NotFound("ProductCategory.NotFound","Product category not found."));

            string? baseSku = command.BaseSku;
            if (string.IsNullOrWhiteSpace(baseSku))
                baseSku = await _appSequenceService.GetNextNumberAsync("PRD", "PRD", cancellationToken);

            var baseCost = new Money(command.BaseCost.Amount, command.BaseCost.Currency);
            var basePrice = new Money(command.BasePrice.Amount, command.BasePrice.Currency);

            Weight? baseWeight = null;
            if (command.BaseWeight != null)
                baseWeight = new Weight(command.BaseWeight.Value, command.BaseWeight.Unit);

            var options = command.Options.Select(o => new ProductOption 
            {
                Name = o.Name,
                Values = o.Values,
            }).ToList();

            bool hasVariants = options.Any();

            var productResult = Product.Create(
                command.Name,
                baseSku,
                command.Description,
                category,
                command.Specifications,
                baseCost,
                basePrice,
                baseWeight,
                options,
                command.Tags
            );
            if (productResult.IsFailure)
                return Result.Failure<Guid>(productResult.Error);

            var product = productResult.Value;

            if (hasVariants)
            {
                foreach(var variantDto in command.Variants)
                {
                    var attributes = variantDto.Attributes
                        .Select(a => new VariantAttribute
                        {
                            Name = a.Name,
                            Value = a.Value,
                        }).ToList();

                    string? variantSku = variantDto.Sku;
                    if (string.IsNullOrWhiteSpace(variantSku))
                    {
                        // Simple generator: BaseSku + first 3 chars of each attribute value
                        var suffix = string.Join("-", attributes.Select(a => a.Value.Length > 3 ? a.Value[..3].ToUpper() : a.Value.ToUpper()));
                        variantSku = $"{baseSku}-{suffix}";
                    }

                    var cost = variantDto.Cost != null ? new Money(variantDto.Cost.Amount, variantDto.Cost.Currency) : baseCost;
                    var price = variantDto.Price != null ? new Money(variantDto.Price.Amount, variantDto.Price.Currency) : basePrice;
                    var weight = variantDto.Weight != null ? new Weight(variantDto.Weight.Value, variantDto.Weight.Unit) : baseWeight;

                    var varinatResult = ProductVariant.Create(
                        product,
                        variantSku,
                        attributes,
                        cost,
                        price,
                        weight,
                        variantDto.StockOnHand
                    );
                    if (varinatResult.IsFailure)
                        return Result.Failure<Guid>(varinatResult.Error);

                    var variant = varinatResult.Value;

                    var variantAddResult = product.AddVariant(variant);
                    if (variantAddResult.IsFailure)
                        return Result.Failure<Guid>(variantAddResult.Error);
                }
            }
            else
            {
                var varinatResult = product.CreateDefaultVariant(command.BaseStockOnHand ?? 0);
    
                if (varinatResult.IsFailure)
                    return Result.Failure<Guid>(varinatResult.Error);

                var variant = varinatResult.Value;

                var variantAddResult = product.AddVariant(variant);
                if(variantAddResult.IsFailure)
                    return Result.Failure<Guid>(variantAddResult.Error);
            }

            if (command.Images.Any())
            {
                foreach(var imgDto in command.Images)
                {
                    var image = new ProductImage(product.Id, imgDto.Url, imgDto.DisplayOrder, imgDto.IsMain);
                    if (!string.IsNullOrWhiteSpace(imgDto.OptionName) && !string.IsNullOrWhiteSpace(imgDto.OptionValue))
                    {
                        var imageResult = image.LinkToOption(imgDto.OptionName, imgDto.OptionValue, product.Options);
                        if (imageResult.IsFailure)
                            return Result.Failure<Guid>(imageResult.Error);
                    }

                    product.AddImage(image);
                }
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync(cancellationToken);

            return product.Id;
        }
    }
}
