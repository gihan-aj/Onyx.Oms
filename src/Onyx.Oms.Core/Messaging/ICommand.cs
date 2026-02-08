using MediatR;
using Onyx.Oms.Core.Common.Models;

namespace Onyx.Oms.Core.Messaging;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
