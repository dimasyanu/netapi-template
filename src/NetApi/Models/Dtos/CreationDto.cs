namespace NetApi.Models.Dtos;

public class CreationDto<TKey>(TKey id)
{
    public TKey Id { get; } = id;
}
