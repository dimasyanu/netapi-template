using MediatR;

namespace NetApi.Application.Common.Contracts;

public interface ICommand : IRequest
{
}

public interface ICommand<IResponse> : IRequest<IResponse>
{
}
