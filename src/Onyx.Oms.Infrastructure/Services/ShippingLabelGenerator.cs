using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Utils;
using Onyx.Oms.Core.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Onyx.Oms.Infrastructure.Services
{
    public class ShippingLabelGenerator : IShippingLabelGenerator
    {
        public byte[] Generate(Order order, Customer customer, Tenant tenant)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Standard Shipping Label Size: 4x6 inches
                    page.Size(new PageSize(4 * 72, 6 * 72));
                    page.Margin(0.2f, Unit.Inch);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Lato).FontColor(Colors.Black));

                    page.Content().Border(3).BorderColor(Colors.Black).Padding(0.2f, Unit.Inch).Column(col =>
                    {
                        // --- SECTION 1: SENDER INFO (Top Left) ---
                        col.Item().BorderBottom(2).BorderColor(Colors.Black).PaddingBottom(5).Row(row =>
                        {
                            row.RelativeItem().Column(fromCol =>
                            {
                                fromCol.Item().Text("FROM:").FontSize(8).Bold();
                                fromCol.Item().Text(tenant.CompanyName).FontSize(10).Bold();
                                if (tenant.StoreAddress != null)
                                {
                                    fromCol.Item().Text(tenant.StoreAddress.Street).FontSize(8);
                                    fromCol.Item().Text($"{tenant.StoreAddress.City}, {tenant.StoreAddress.Country}").FontSize(8);
                                }
                                fromCol.Item().Text(tenant.ContactPhone).FontSize(8);
                            });

                            // Order Number top right for easy warehouse reference
                            row.ConstantItem(100).AlignRight().Column(ordCol =>
                            {
                                ordCol.Item().Text("ORDER #").FontSize(8).Bold();
                                ordCol.Item().Text(order.OrderNumber).FontSize(12).Bold();

                                if (!string.IsNullOrWhiteSpace(order.TrackingNumber))
                                {
                                    ordCol.Item().PaddingTop(8).Text("TRACKING #").FontSize(8).Bold();
                                    ordCol.Item().Text(order.TrackingNumber).FontSize(11).Bold().FontColor(Colors.Blue.Darken2);
                                }
                            });
                        });

                        // --- SECTION 2: RECIPIENT INFO (Center - Massive text) ---
                        col.Item().PaddingTop(15).Column(toCol =>
                        {
                            toCol.Item().Text("SHIP TO:").FontSize(12).Bold();
                            toCol.Item().PaddingTop(5).Text(customer.Name).FontSize(16).Bold();

                            // Phones are the most important thing for delivery drivers
                            toCol.Item().Text($"📞 {customer.PrimaryPhone}").FontSize(14).Bold();
                            if (!string.IsNullOrWhiteSpace(customer.SecondaryPhone))
                            {
                                toCol.Item().Text($"📞 {customer.SecondaryPhone}").FontSize(12).Bold();
                            }

                            // Address blocks
                            toCol.Item().PaddingTop(10).Text(order.ShippingAddress.Street).FontSize(14);
                            toCol.Item().Text(order.ShippingAddress.City).FontSize(16).Bold(); // City is bolded for courier sorting
                            toCol.Item().Text($"{order.ShippingAddress.District}, {order.ShippingAddress.State}").FontSize(14);
                            toCol.Item().Text($"{order.ShippingAddress.PostalCode} {order.ShippingAddress.Country}").FontSize(14);
                        });

                        // --- SECTION 3: DELIVERY NOTES ---
                        if (!string.IsNullOrWhiteSpace(order.DeliveryInstructions))
                        {
                            col.Item().PaddingTop(15).Border(1).Padding(5).Column(noteCol =>
                            {
                                noteCol.Item().Text("DELIVERY INSTRUCTIONS:").FontSize(8).Bold();
                                noteCol.Item().Text(order.DeliveryInstructions).FontSize(10).Bold();
                            });
                        }
                    });

                    page.Footer().Border(3).BorderColor(Colors.Black).Background(Colors.Grey.Lighten4).Padding(10).Row(row =>
                    {
                        if (order.IsCashOnDelivery && order.BalanceAmount.Amount > 0)
                        {
                            string currencySymbol = CurrencyHelper.GetSymbol(order.BalanceAmount.Currency);

                            row.RelativeItem(1).AlignMiddle().Text("COD TO COLLECT:").FontSize(14).Black().Bold();
                            row.RelativeItem(2).AlignRight().AlignMiddle().Text(text =>
                            {
                                text.Span($"{currencySymbol} ").FontSize(14).Black();
                                text.Span($"{order.BalanceAmount.Amount:N0}").FontSize(24).Black().Bold();
                            });
                        }
                        else
                        {
                            row.RelativeItem().AlignCenter().AlignMiddle().Text("PRE-PAID / NO COD").FontSize(18).Black().Bold();
                        }
                    });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateBulk(List<Order> orders, Dictionary<Guid, Customer> customers, Tenant tenant)
        {
            var document = Document.Create(container =>
            {
                foreach (var order in orders)
                {
                    // Safely grab the matching customer
                    if (!customers.TryGetValue(order.CustomerId, out var customer))
                        continue;

                    container.Page(page =>
                    {
                        // Standard Shipping Label Size: 4x6 inches
                        page.Size(new PageSize(4 * 72, 6 * 72));
                        page.Margin(0.2f, Unit.Inch);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontFamily(Fonts.Lato).FontColor(Colors.Black));

                        page.Content().Border(3).BorderColor(Colors.Black).Padding(0.2f, Unit.Inch).Column(col =>
                        {
                            // --- SECTION 1: SENDER INFO (Top Left) ---
                            col.Item().BorderBottom(2).BorderColor(Colors.Black).PaddingBottom(5).Row(row =>
                            {
                                row.RelativeItem().Column(fromCol =>
                                {
                                    fromCol.Item().Text("FROM:").FontSize(8).Bold();
                                    fromCol.Item().Text(tenant.CompanyName).FontSize(10).Bold();
                                    if (tenant.StoreAddress != null)
                                    {
                                        fromCol.Item().Text(tenant.StoreAddress.Street).FontSize(8);
                                        fromCol.Item().Text($"{tenant.StoreAddress.City}, {tenant.StoreAddress.Country}").FontSize(8);
                                    }
                                    fromCol.Item().Text(tenant.ContactPhone).FontSize(8);
                                });

                                // Order Number top right for easy warehouse reference
                                row.ConstantItem(100).AlignRight().Column(ordCol =>
                                {
                                    ordCol.Item().Text("ORDER #").FontSize(8).Bold();
                                    ordCol.Item().Text(order.OrderNumber).FontSize(12).Bold();

                                    if (!string.IsNullOrWhiteSpace(order.TrackingNumber))
                                    {
                                        ordCol.Item().PaddingTop(8).Text("TRACKING #").FontSize(8).Bold();
                                        ordCol.Item().Text(order.TrackingNumber).FontSize(11).Bold().FontColor(Colors.Blue.Darken2);
                                    }
                                });
                            });

                            // --- SECTION 2: RECIPIENT INFO (Center - Massive text) ---
                            col.Item().PaddingTop(15).Column(toCol =>
                            {
                                toCol.Item().Text("SHIP TO:").FontSize(12).Bold();
                                toCol.Item().PaddingTop(5).Text(customer.Name).FontSize(16).Bold();

                                // Phones are the most important thing for delivery drivers
                                toCol.Item().Text($"📞 {customer.PrimaryPhone}").FontSize(14).Bold();
                                if (!string.IsNullOrWhiteSpace(customer.SecondaryPhone))
                                {
                                    toCol.Item().Text($"📞 {customer.SecondaryPhone}").FontSize(12).Bold();
                                }

                                // Address blocks
                                toCol.Item().PaddingTop(10).Text(order.ShippingAddress.Street).FontSize(14);
                                toCol.Item().Text(order.ShippingAddress.City).FontSize(16).Bold(); // City is bolded for courier sorting
                                toCol.Item().Text($"{order.ShippingAddress.District}, {order.ShippingAddress.State}").FontSize(14);
                                toCol.Item().Text($"{order.ShippingAddress.PostalCode} {order.ShippingAddress.Country}").FontSize(14);
                            });

                            // --- SECTION 3: DELIVERY NOTES ---
                            if (!string.IsNullOrWhiteSpace(order.DeliveryInstructions))
                            {
                                col.Item().PaddingTop(15).Border(1).Padding(5).Column(noteCol =>
                                {
                                    noteCol.Item().Text("DELIVERY INSTRUCTIONS:").FontSize(8).Bold();
                                    noteCol.Item().Text(order.DeliveryInstructions).FontSize(10).Bold();
                                });
                            }
                        });

                        page.Footer().Border(3).BorderColor(Colors.Black).Background(Colors.Grey.Lighten4).Padding(10).Row(row =>
                        {
                            if (order.IsCashOnDelivery && order.BalanceAmount.Amount > 0)
                            {
                                string currencySymbol = CurrencyHelper.GetSymbol(order.BalanceAmount.Currency);

                                row.RelativeItem().AlignMiddle().Text("COD TO COLLECT:").FontSize(14).Black().Bold();
                                row.RelativeItem().AlignRight().AlignMiddle().Text(text =>
                                {
                                    text.Span($"{currencySymbol} ").FontSize(14).Black();
                                    text.Span($"{order.BalanceAmount.Amount:N0}").FontSize(24).Black().Bold();
                                });
                            }
                            else
                            {
                                row.RelativeItem().AlignCenter().AlignMiddle().Text("PRE-PAID / NO COD").FontSize(18).Black().Bold();
                            }
                        });
                    });
                }
            });

            return document.GeneratePdf();
        }
    }
}
