using System.ComponentModel.DataAnnotations;

namespace NetApi.Domain.Abstractions;

public abstract class Int64BaseEntity
{
    [Key]
    public long Id { get; set; }
}