using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.GenerateProductSheet
{
    public class GenerateProductSheetHandler : IQueryHandler<GenerateProductSheetQuery, byte[]>
    {
        private readonly IProductSheetGenerator _pdfGenerator;

        public GenerateProductSheetHandler(IProductSheetGenerator pdfGenerator)
        {
            _pdfGenerator = pdfGenerator;
        }

        public async Task<Result<byte[]>> Handle(GenerateProductSheetQuery request, CancellationToken cancellationToken)
        {
            var pdfResult = await _pdfGenerator.GenerateAsync(request.ProductId, request.ImageStoragePath, cancellationToken);

            if (pdfResult.IsFailure)
            {
                return Result.Failure<byte[]>(pdfResult.Error);
            }

            return Result.Success(pdfResult.Value);
        }
    }
}
