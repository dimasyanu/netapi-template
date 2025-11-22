using MediatR;

namespace NetApi.Application.Common.Contracts;

public class ICommand : IRequest
{
}

public interface ICommand<IResponse> : IRequest<IResponse>
{
}
