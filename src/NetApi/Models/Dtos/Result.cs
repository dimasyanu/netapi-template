namespace NetApi.Models.Dtos;

public class Result<T>
{
    public T? Data { get; set; }
    public string Message { get; set; } = "Success";
    public bool Success { get; set; } = true;
}
