using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Onyx.Oms.Infrastructure.Services
{
    public class ProductSheetGenerator : IProductSheetGenerator
    {
        public byte[] Generate(Product product, List<SpecDefinition>? allSpecDefs, Tenant tenant, string imageStoragePath, string logoStoragePath)
        {
            bool hasPriceVariance = false;
            decimal displayPrice = product.BasePrice.Amount;

            if (product.HasVariants && product.Variants.Any(v => v.IsActive))
            {
                var activeVariants = product.Variants.Where(v => v.IsActive).ToList();
                var minPrice = activeVariants.Min(v => v.Price.Amount);
                var maxPrice = activeVariants.Max(v => v.Price.Amount);

                if (minPrice < maxPrice)
                {
                    hasPriceVariance = true;
                    displayPrice = minPrice;
                }
            }

            if(allSpecDefs == null)
            {
                allSpecDefs = new List<SpecDefinition>();
            }

            var specs = allSpecDefs
                .Where(s => product.Specifications.TryGetValue(s.Key, out var value) && !string.IsNullOrWhiteSpace(value))
                .Select(s => new KeyValuePair<string, string>(s.Label, product.Specifications[s.Key]))
                .ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Lato));

                    // --- HEADER: Branding & Contact Info ---
                    page.Header().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(10).Row(row =>
                    {
                        // Left: Logo or Company Name
                        row.ConstantItem(120).Column(col =>
                        {
                            string logoFile = !string.IsNullOrEmpty(tenant.LogoUrl)
                                ? Path.Combine(logoStoragePath, tenant.LogoUrl) : string.Empty;

                            if (File.Exists(logoFile))
                            {
                                col.Item().Height(50).Image(logoFile).FitArea();
                            }
                            else
                            {
                                col.Item().Text(tenant.CompanyName).FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                            }
                        });

                        // Right: Company Details
                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text("PRODUCT SPECIFICATION").FontSize(16).Bold().FontColor(Colors.Grey.Darken3);
                            col.Item().Text(tenant.CompanyName).FontSize(10).SemiBold();

                            if (tenant.StoreAddress != null)
                            {
                                string addressLine = $"{tenant.StoreAddress.Street}, {tenant.StoreAddress.City}, {tenant.StoreAddress.Country}";
                                col.Item().Text(addressLine).FontSize(9).FontColor(Colors.Grey.Medium);
                            }

                            string contactInfo = $"{tenant.ContactEmail} | {tenant.ContactPhone}";
                            col.Item().Text(contactInfo.Trim(' ', '|')).FontSize(9).FontColor(Colors.Grey.Medium);

                            if (!string.IsNullOrEmpty(tenant.TaxRegistrationNumber))
                                col.Item().Text($"Tax ID: {tenant.TaxRegistrationNumber}").FontSize(9).FontColor(Colors.Grey.Medium);
                        });
                    });

                    // --- CONTENT: Product Data ---
                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        // 1. Hero Section (Image + Title/Price)
                        col.Item().Row(row =>
                        {
                            // Main Product Image
                            row.ConstantItem(200).Column(imgCol =>
                            {
                                var mainImage = product.Images.FirstOrDefault(i => i.IsMain) ?? product.Images.FirstOrDefault();
                                string imgFile = mainImage != null ? Path.Combine(imageStoragePath, mainImage.Url) : string.Empty;

                                if (File.Exists(imgFile))
                                {
                                    imgCol.Item().Image(imgFile).FitWidth();
                                }
                                else
                                {
                                    imgCol.Item().Background(Colors.Grey.Lighten4).Height(200).AlignCenter().AlignMiddle()
                                          .Text("NO IMAGE").FontColor(Colors.Grey.Medium);
                                }
                            });

                            // Product Title, SKU, Price, Description
                            row.RelativeItem().PaddingLeft(25).Column(infoCol =>
                            {
                                infoCol.Item().Text(product.Category?.Name?.ToUpperInvariant() ?? "PRODUCT").FontSize(9).FontColor(Colors.Grey.Medium).SemiBold();
                                infoCol.Item().Text(product.Name).FontSize(24).Bold().FontColor(Colors.Black);
                                infoCol.Item().Text($"SKU: {product.BaseSku}").FontSize(11).FontColor(Colors.Grey.Darken1);

                                infoCol.Item().PaddingTop(10).Text(text =>
                                {
                                    if (hasPriceVariance)
                                    {
                                        text.Span("From ").FontSize(14).FontColor(Colors.Grey.Darken2);
                                    }
                                    text.Span($"{product.BasePrice.Currency} {displayPrice:N2}")
                                        .FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                                });

                                if (!string.IsNullOrWhiteSpace(product.Description))
                                {
                                    infoCol.Item().PaddingTop(15).Text(product.Description).FontSize(10).LineHeight(1.4f);
                                }
                            });
                        });

                        // 2. Specifications Table
                        if (product.Specifications.Any())
                        {
                            col.Item().PaddingTop(30).Text("TECHNICAL SPECIFICATIONS").FontSize(12).Bold().FontColor(Colors.Grey.Darken3);
                            col.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(150);
                                    columns.RelativeColumn();
                                });

                                foreach (var spec in specs)
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(6)
                                         .Text(spec.Key).FontSize(10).SemiBold().FontColor(Colors.Grey.Darken2);

                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(6)
                                         .Text(spec.Value).FontSize(10);
                                }

                                // Add Base Weight if it exists
                                if (product.BaseWeight != null)
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(6)
                                         .Text("Base Weight").FontSize(10).SemiBold().FontColor(Colors.Grey.Darken2);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(6)
                                         .Text($"{product.BaseWeight.Value} {product.BaseWeight.Unit}").FontSize(10);
                                }
                            });
                        }

                        // 3. Options (Sizes, Colors, etc.)
                        if (product.HasVariants && product.Options.Any())
                        {
                            col.Item().PaddingTop(25).Text("AVAILABLE CONFIGURATIONS").FontSize(12).Bold().FontColor(Colors.Grey.Darken3);
                            col.Item().PaddingTop(10).Row(row =>
                            {
                                foreach (var option in product.Options.OrderBy(o => o.DisplayOrder))
                                {
                                    row.AutoItem().PaddingRight(30).Column(optCol =>
                                    {
                                        optCol.Item().Text(option.Name.ToUpperInvariant()).FontSize(9).SemiBold().FontColor(Colors.Grey.Medium);
                                        optCol.Item().PaddingTop(2).Text(string.Join(", ", option.Values)).FontSize(10).Bold();
                                    });
                                }
                            });
                        }

                        // 4. Additional Image Gallery ---
                        // Grab up to 4 other images that are NOT the main image
                        var additionalImages = product.Images
                            .Where(i => !i.IsMain && i.Url != (product.Images.FirstOrDefault(m => m.IsMain)?.Url ?? ""))
                            .OrderBy(i => i.DisplayOrder)
                            .Take(4)
                            .ToList();

                        if (additionalImages.Any())
                        {
                            col.Item().PaddingTop(30).Text("ADDITIONAL VIEWS").FontSize(12).Bold().FontColor(Colors.Grey.Darken3);
                            col.Item().PaddingTop(10).Row(galleryRow =>
                            {
                                foreach (var img in additionalImages)
                                {
                                    string thumbFile = Path.Combine(imageStoragePath, img.Url);
                                    if (File.Exists(thumbFile))
                                    {
                                        // Give them a fixed height so they look like uniform thumbnails
                                        galleryRow.AutoItem().PaddingRight(15).Height(80).Image(thumbFile).FitArea();
                                    }
                                }
                            });
                        }
                    });

                    // --- FOOTER ---
                    page.Footer().Column(col =>
                    {
                        if (!string.IsNullOrWhiteSpace(tenant.InvoiceFooterText))
                        {
                            col.Item().AlignCenter().Text(tenant.InvoiceFooterText).FontSize(9).FontColor(Colors.Grey.Medium).Italic();
                        }

                        col.Item().PaddingTop(5).AlignCenter().Text(x =>
                        {
                            x.Span("Generated on ");
                            x.Span(DateTime.Now.ToString("MMM dd, yyyy")).SemiBold();
                            x.Span("  |  Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
