using System.ComponentModel.DataAnnotations;

namespace NetApi.Domain.Contracts;

public interface ITimestamp
{
    DateTime CreatedAt { get; set; }

    [MaxLength(100)]
    string CreatedBy { get; set; }

    DateTime UpdatedAt { get; set; }

    [MaxLength(100)]
    string UpdatedBy { get; set; }
}
