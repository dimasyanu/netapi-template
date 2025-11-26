namespace NetApi.Application.Common.Contracts;

public interface IJobService
{
    void Enqueue(ICommand command);

    void Enqueue<T>(ICommand<T> command);
}
