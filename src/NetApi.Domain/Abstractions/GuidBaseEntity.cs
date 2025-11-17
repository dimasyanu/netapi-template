using System.ComponentModel.DataAnnotations;
using NetApi.Domain.Contracts;

namespace NetApi.Domain.Abstractions;

public abstract class GuidBaseEntity : IBaseEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }
}
