using System.ComponentModel.DataAnnotations;

namespace NetApi.Domain.Common.Contracts;

public interface ISoftDelete
{
    DateTime? DeletedAt { get; set; }

    [MaxLength(100)]
    string? DeletedBy { get; set; }
}
