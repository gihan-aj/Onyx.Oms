using Onyx.Oms.Core.Common.Interfaces;
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

                    page.Content().Column(col =>
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

                        // Push the COD box to the absolute bottom of the label
                        //col.Item().Element(filler => filler.ExtendVertical());

                        // --- SECTION 4: COD AMOUNT (Massive Box at the bottom) ---
                        //col.Item().PaddingTop(10).Border(3).BorderColor(Colors.Black).Background(Colors.Grey.Lighten4).Padding(10).Row(row =>
                        //{
                        //    if (order.IsCashOnDelivery && order.BalanceAmount.Amount > 0)
                        //    {
                        //        row.RelativeItem().AlignMiddle().Text("COD TO COLLECT:").FontSize(14).Black();
                        //        row.RelativeItem().AlignRight().AlignMiddle().Text($"Rs. {order.BalanceAmount.Amount:N0}").FontSize(24).Black();
                        //    }
                        //    else
                        //    {
                        //        row.RelativeItem().AlignCenter().AlignMiddle().Text("PRE-PAID / NO COD").FontSize(18).Black();
                        //    }
                        //});
                    });

                    page.Footer().Border(3).BorderColor(Colors.Black).Background(Colors.Grey.Lighten4).Padding(10).Row(row =>
                    {
                        if (order.IsCashOnDelivery && order.BalanceAmount.Amount > 0)
                        {
                            row.RelativeItem().AlignMiddle().Text("COD TO COLLECT:").FontSize(14).Black();
                            row.RelativeItem().AlignRight().AlignMiddle().Text($"Rs. {order.BalanceAmount.Amount:N0}").FontSize(24).Black();
                        }
                        else
                        {
                            row.RelativeItem().AlignCenter().AlignMiddle().Text("PRE-PAID / NO COD").FontSize(18).Black();
                        }
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
