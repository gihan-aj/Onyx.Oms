using MediatR;
using Onyx.Oms.Core.Common.Models;

namespace Onyx.Oms.Core.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
