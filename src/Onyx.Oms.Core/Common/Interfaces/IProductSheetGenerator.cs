using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Core.Common.Interfaces
{
    public interface IProductSheetGenerator
    {
        byte[] Generate(Product product, List<SpecDefinition>? allSpecDefs, Tenant tenant, string imageStoragePath, string logoStoragePath);
    }
}
