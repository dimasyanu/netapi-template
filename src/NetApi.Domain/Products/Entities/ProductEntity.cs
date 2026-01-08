using NetApi.Domain.Common.Contracts;
using NetApi.Domain.Products.ValueObjects;

namespace NetApi.Domain.Products.Entities;

public class ProductEntity : IEntity<ProductId>, ITimestamp, ISoftDelete
{
    public ProductId? Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public ProductCategoryId? ProductCategoryId { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = "";
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public virtual ProductCategoryEntity? Category { get; set; }
    public virtual List<ProductTagEntity>? Tags { get; set; }
}
