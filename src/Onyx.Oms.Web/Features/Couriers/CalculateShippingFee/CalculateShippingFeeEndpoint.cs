using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Couriers.CalculateShippingFee
{
    public class CalculateShippingFeeEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/couriers")
                .WithApiVersionSet(app.NewApiVersionSet("Couriers").Build())
                .HasApiVersion(1);
            group.MapGet("{courierId:guid}/calculate-shipping-fee", async (
                ISender sender, 
                Guid courierId, 
                [FromQuery] string district, 
                [FromQuery] decimal totalWeightKg, 
                [FromQuery] decimal codAmount) =>
            {
                var query = new CalculateShippingFeeQuery(courierId, district, totalWeightKg, codAmount);
                Result<decimal> result = await sender.Send(query);
                return result.ToMinimalApiResult();
            })
            .WithTags("Couriers")
            .WithName("CalculateShippingFee")
            .WithSummary("Calculate shipping fee")
            .WithDescription("Calculates the shipping fee for a given courier based on the district, total weight, and COD amount.")
            .HasPermission(Permissions.Couriers.View);
        }
    }
}