namespace NetApi.Application.Common.Exceptions;

public class InternalErrorException(string message = "Internal server error") : Exception(message)
{
}
