using MediatR;
using Onyx.Oms.Core.Common.Models;

namespace Onyx.Oms.Core.Messaging
{
    public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
        where TQuery : IQuery<TResponse>
    {
    }
}
