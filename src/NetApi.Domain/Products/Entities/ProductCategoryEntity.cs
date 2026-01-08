using NetApi.Domain.Common.Contracts;
using NetApi.Domain.Products.ValueObjects;

namespace NetApi.Domain.Products.Entities;

public class ProductCategoryEntity : IEntity<ProductCategoryId>, ITimestamp, ISoftDelete
{
    public ProductCategoryId? Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public ProductCategoryId? ParentId { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = "";
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public virtual ProductCategoryEntity? Parent { get; set; }
}
