namespace NetApi.Models;

public class Result<T>
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = "Success";
    public T? Data { get; set; }
    public Dictionary<string, List<string>>? Errors { get; set; }
}
