namespace NetApi.Application.Common.Exceptions;

public class BadRequestException : Exception
{
    private readonly List<KeyValuePair<string, string[]>> _errors;
    public IReadOnlyList<KeyValuePair<string, string[]>> Errors => _errors.AsReadOnly();

    public BadRequestException(string message = "Bad request") : base(message)
    {
        _errors = [new KeyValuePair<string, string[]>("Error", [message])];
    }

    public BadRequestException(List<KeyValuePair<string, string[]>>? errors = null, string message = "Bad Request") : base(message)
    {
        _errors = errors ?? new();
    }
}
