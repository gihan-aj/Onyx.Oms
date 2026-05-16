using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Utils;
using Onyx.Oms.Core.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Onyx.Oms.Infrastructure.Services
{
    public class OrderInvoiceGenerator : IOrderInvoiceGenerator
    {
        public byte[] Generate(Order order, Customer customer, Tenant tenant, string logoStoragePath)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Lato));

                    // --- HEADER ---
                    page.Header().PaddingBottom(20).Row(row =>
                    {
                        // Left: Logo / Tenant Info
                        row.RelativeItem().Column(col =>
                        {
                            string logoFile = !string.IsNullOrEmpty(tenant.LogoUrl)
                                ? Path.Combine(logoStoragePath, tenant.LogoUrl) : string.Empty;

                            if (File.Exists(logoFile))
                            {
                                col.Item().Height(40).Image(logoFile).FitArea();
                            }
                            else
                            {
                                col.Item().Text(tenant.CompanyName).FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                            }

                            col.Item().PaddingTop(5).Text(tenant.ContactEmail).FontColor(Colors.Grey.Darken1);
                            if (!string.IsNullOrWhiteSpace(tenant.ContactPhone))
                                col.Item().Text(tenant.ContactPhone).FontColor(Colors.Grey.Darken1);
                        });

                        // Right: Invoice Details
                        row.ConstantItem(200).AlignRight().Column(col =>
                        {
                            col.Item().Text(order.PaymentStatus == Onyx.Oms.Core.Domain.Enums.PaymentStatus.FullyPaid ? "RECEIPT" : "INVOICE")
                                 .FontSize(24).Bold().FontColor(Colors.Grey.Darken3);

                            col.Item().Text($"Order #: {order.OrderNumber}").FontSize(12).SemiBold();
                            col.Item().Text($"Date: {order.OrderDate?.ToString("MMM dd, yyyy") ?? DateTime.Now.ToString("MMM dd, yyyy")}").FontColor(Colors.Grey.Darken1);

                            // Status Badge
                            col.Item().PaddingTop(5).Text($"Status: {order.Status}").FontSize(11).SemiBold().FontColor(Colors.Blue.Darken1);
                        });
                    });

                    // --- CONTENT ---
                    page.Content().Column(col =>
                    {
                        // 1. BILL TO / SHIP TO
                        col.Item().PaddingBottom(20).Row(row =>
                        {
                            row.RelativeItem().Column(billCol =>
                            {
                                billCol.Item().PaddingBottom(5).Text("CUSTOMER DETAILS").SemiBold().FontColor(Colors.Grey.Medium);
                                billCol.Item().Text(customer.Name).Bold().FontSize(12);
                                billCol.Item().Text(customer.PrimaryPhone);
                                if (!string.IsNullOrWhiteSpace(customer.Email))
                                    billCol.Item().Text(customer.Email);
                            });

                            row.RelativeItem().AlignRight().Column(shipCol =>
                            {
                                shipCol.Item().PaddingBottom(5).Text("SHIPPING ADDRESS").SemiBold().FontColor(Colors.Grey.Medium);
                                if (order.ShippingAddress != null && order.ShippingAddress.IsValid)
                                {
                                    shipCol.Item().Text(order.ShippingAddress.Street);
                                    shipCol.Item().Text($"{order.ShippingAddress.City}, {order.ShippingAddress.District}");
                                    shipCol.Item().Text($"{order.ShippingAddress.PostalCode}, {order.ShippingAddress.Country}");
                                }
                                else
                                {
                                    shipCol.Item().Text("No shipping address provided.").Italic().FontColor(Colors.Grey.Medium);
                                }
                            });
                        });

                        // 2. ORDER ITEMS TABLE
                        col.Item().Table(table =>
                        {
                            // Define columns
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Item description
                                columns.ConstantColumn(60); // Qty
                                columns.RelativeColumn(1); // Unit Price
                                columns.RelativeColumn(1); // Total
                            });

                            // Table Header
                            table.Header(header =>
                            {
                                header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).Text("ITEM").SemiBold();
                                header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).AlignRight().Text("QTY").SemiBold();
                                header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).AlignRight().Text("PRICE").SemiBold();
                                header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).AlignRight().Text("TOTAL").SemiBold();
                            });

                            // Table Rows
                            foreach (var item in order.Items)
                            {
                                table.Cell().PaddingVertical(8).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Column(itemCol =>
                                {
                                    itemCol.Item().Text(item.ProductName).SemiBold();
                                    itemCol.Item().Text($"SKU: {item.Sku}").FontSize(9).FontColor(Colors.Grey.Medium);
                                    if (item.DiscountAmount.Amount > 0)
                                    {
                                        itemCol.Item().Text($"Discount: {item.DiscountReason}").FontSize(8).FontColor(Colors.Green.Darken1);
                                    }
                                });

                                table.Cell().PaddingVertical(8).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).AlignRight().AlignMiddle()
                                     .Text(item.Quantity.ToString());

                                table.Cell().PaddingVertical(8).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).AlignRight().AlignMiddle()
                                     .Text($"{item.UnitPrice.Amount:N2}");

                                table.Cell().PaddingVertical(8).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).AlignRight().AlignMiddle()
                                     .Text($"{item.LineTotal.Amount:N2}");
                            }
                        });

                        // 3. FINANCIAL SUMMARY (Right Aligned)
                        col.Item().PaddingTop(15).Row(row =>
                        {
                            row.RelativeItem(); // Empty space to push summary to the right

                            row.ConstantItem(250).Column(sumCol =>
                            {
                                // Subtotal
                                sumCol.Item().Row(r => { 
                                    r.RelativeItem().Text("Subtotal:"); 
                                    r.RelativeItem().AlignRight().Text(text => 
                                    {
                                        text.Span($"{CurrencyHelper.GetSymbol(order.SubTotal.Currency)} ").FontSize(8);
                                        text.Span($"{order.SubTotal.Amount:N2}");
                                    }); 
                                });

                                // Shipping (Only show if > 0)
                                if (order.ShippingCost.Amount > 0)
                                    sumCol.Item().PaddingTop(4).Row(r => { r.RelativeItem().Text("Shipping Fee:"); r.RelativeItem().AlignRight().Text($"{order.ShippingCost.Amount:N2}"); });

                                // Discount (Only show if > 0)
                                if (order.DiscountAmount.Amount > 0)
                                    sumCol.Item().PaddingTop(4).Row(r => { r.RelativeItem().Text("Order Discount:"); r.RelativeItem().AlignRight().Text($"- {order.DiscountAmount.Amount:N2}").FontColor(Colors.Green.Darken2); });

                                // Grand Total
                                sumCol.Item().PaddingTop(10).BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(5).Row(r =>
                                {
                                    r.RelativeItem().AlignMiddle().Text("GRAND TOTAL:").SemiBold().FontSize(12);
                                    r.RelativeItem().AlignRight().AlignMiddle().Text(text => 
                                    {
                                        text.Span($"{CurrencyHelper.GetSymbol(order.GrandTotal.Currency)} ").FontSize(10).Bold();
                                        text.Span($"{order.GrandTotal.Amount:N2}").Bold().FontSize(14);
                                    });
                                });

                                // Payments Made
                                if (order.TotalPaid.Amount > 0)
                                {
                                    sumCol.Item().PaddingTop(4).Row(r =>
                                    {
                                        r.RelativeItem().Text("Amount Paid:").FontColor(Colors.Grey.Darken1);
                                        r.RelativeItem().AlignRight().Text($"- {order.TotalPaid.Amount:N2}").FontColor(Colors.Grey.Darken1);
                                    });
                                }

                                // BALANCE DUE / COD
                                if (order.BalanceAmount.Amount > 0)
                                {
                                    sumCol.Item().PaddingTop(10).Background(order.IsCashOnDelivery ? Colors.Yellow.Lighten3 : Colors.Grey.Lighten3).Padding(8).Row(r =>
                                    {
                                        r.RelativeItem().AlignMiddle().Text(order.IsCashOnDelivery ? "CASH ON DELIVERY:" : "BALANCE DUE:").Bold().FontSize(12).FontColor(Colors.Black);
                                        r.RelativeItem().AlignRight().AlignMiddle().Text(text => 
                                        {
                                            text.Span($"{CurrencyHelper.GetSymbol(order.BalanceAmount.Currency)} ").FontSize(10).FontColor(Colors.Black);
                                            text.Span($"{order.BalanceAmount.Amount:N2}").Bold().FontSize(14).FontColor(Colors.Black);
                                        });
                                    });
                                }
                            });
                        });

                        // 4. NOTES
                        //if (!string.IsNullOrWhiteSpace(order.Notes))
                        //{
                        //    col.Item().PaddingTop(30).Column(noteCol =>
                        //    {
                        //        noteCol.Item().Text("Order Notes:").SemiBold().FontColor(Colors.Grey.Medium);
                        //        noteCol.Item().Text(order.Notes).FontSize(9);
                        //    });
                        //}
                    });

                    // --- FOOTER ---
                    page.Footer().BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(10).Column(col =>
                    {
                        if (!string.IsNullOrWhiteSpace(tenant.InvoiceFooterText))
                        {
                            col.Item().AlignCenter().Text(tenant.InvoiceFooterText).FontSize(9).FontColor(Colors.Grey.Medium).Italic();
                        }

                        col.Item().PaddingTop(5).AlignCenter().Text(x =>
                        {
                            x.Span("Generated on ");
                            x.Span(DateTime.Now.ToString("MMM dd, yyyy HH:mm")).SemiBold();
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
