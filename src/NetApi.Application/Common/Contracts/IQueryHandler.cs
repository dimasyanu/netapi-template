using MediatR;

namespace NetApi.Application.Common.Contracts;

public interface IQueryHandler<TQuery> : IRequestHandler<TQuery> where TQuery : IQuery
{
}

public interface IQueryHandler<TQuery, TResult> : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
}
