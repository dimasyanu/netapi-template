namespace NetApi.Application.Common.Exceptions;

public class BadRequestException(List<KeyValuePair<string, string[]>>? errors = null, string message = "Bad Request") : Exception(message)
{
    private readonly List<KeyValuePair<string, string[]>> _errors = errors ?? [];
    public IReadOnlyList<KeyValuePair<string, string[]>> Errors => _errors.AsReadOnly();
}
