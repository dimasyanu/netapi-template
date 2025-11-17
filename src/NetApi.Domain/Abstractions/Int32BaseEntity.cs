using System.ComponentModel.DataAnnotations;

namespace NetApi.Domain.Abstractions;

public abstract class Int32BaseEntity
{
    [Key]
    public int Id { get; set; }
}
