using MediatR;
namespace NetApi.Application.Common.Contracts;

public interface IQuery : IRequest
{
}

public interface IQuery<TResult> : IRequest<TResult>
{ }