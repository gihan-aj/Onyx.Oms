using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Onyx.Oms.Infrastructure.Services
{
    public class ProductSheetGenerator : IProductSheetGenerator
    {
        private readonly IApplicationDbContext _context;

        public ProductSheetGenerator(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<byte[]>> GenerateAsync(Guid productId, string imageStoragePath, CancellationToken cancellationToken = default)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
            if (product == null)
                return Result.Failure<byte[]>(Error.NotFound("Product.NotFound", "Product not found."));

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(36, Unit.Point);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Lato));

                    // Header
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Text("LOGO HERE").FontSize(20).Black();
                        row.RelativeItem().AlignRight().Text("PRODUCT SHEET").FontSize(12).FontColor(Colors.Grey.Medium);
                    });

                    // Content
                    page.Content().PaddingVertical(18, Unit.Point).Column(col =>
                    {
                        // Hero Section: Image Left, Info Right
                        col.Item().Row(row =>
                        {
                            // Left: Main Image
                            var mainImage = product.Images.FirstOrDefault(i => i.IsMain);
                            string? mainImagePath = mainImage != null ? Path.Combine(imageStoragePath, mainImage.Url) : null;
                            if (File.Exists(mainImagePath))
                            {
                                row.ConstantItem(250).Image(mainImagePath);
                            }
                            else
                            {
                                row.ConstantItem(250).Background(Colors.Grey.Lighten3).Height(250); // Placeholder
                            }

                            // Right: Title, Sku, Price, Desc
                            row.RelativeItem().PaddingLeft(20).Column(rightCol =>
                            {
                                rightCol.Item().Text(product.Name).FontSize(24).SemiBold();
                                rightCol.Item().Text($"SKU: {product.BaseSku}").FontSize(12).FontColor(Colors.Grey.Medium);

                                rightCol.Item().PaddingTop(10).Text($"{product.BasePrice.Currency} {product.BasePrice.Amount:N2}")
                                        .FontSize(18).FontColor(Colors.Blue.Darken2).SemiBold();

                                rightCol.Item().PaddingTop(15).Text(product.Description).FontSize(11);
                            });
                        });

                        // Specifications Table
                        if (product.Specifications.Any())
                        {
                            col.Item().PaddingTop(30).Text("SPECIFICATIONS").FontSize(14).SemiBold().Underline();
                            col.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(150);
                                    columns.RelativeColumn();
                                });

                                foreach (var spec in product.Specifications)
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(5).Text(spec.Key).SemiBold();
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(5).Text(spec.Value);
                                }
                            });
                        }

                        // Available Options (If Variants exist)
                        if (product.HasVariants && product.Options.Any())
                        {
                            col.Item().PaddingTop(20).Text("AVAILABLE OPTIONS").FontSize(14).SemiBold().Underline();
                            col.Item().PaddingTop(10).Column(optCol =>
                            {
                                foreach (var option in product.Options.OrderBy(o => o.DisplayOrder))
                                {
                                    string values = string.Join(", ", option.Values);
                                    optCol.Item().Text($"{option.Name}: {values}").FontSize(11);
                                }
                            });
                        }
                    });

                    // FOOTER
                    page.Footer().AlignCenter().Text("www.yourcompany.com | contact@yourcompany.com | +94 77 123 4567")
                        .FontSize(10).FontColor(Colors.Grey.Medium);
                });
            });

            return document.GeneratePdf();
        }
    }
}
