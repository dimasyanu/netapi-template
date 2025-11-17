using System.ComponentModel.DataAnnotations;

namespace NetApi.Domain.Abstractions;

public abstract class Int16BaseEntity
{
    [Key]
    public short Id { get; set; }
}
