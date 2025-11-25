namespace NetApi.Application.Common.Contracts;

public interface IAuthorizedCommand : ICommand
{
}

public interface IAuthorizedCommand<TResponse> : ICommand<TResponse>
{
}