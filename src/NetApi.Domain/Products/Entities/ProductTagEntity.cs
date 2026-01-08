using NetApi.Domain.Common.Contracts;
using NetApi.Domain.Products.ValueObjects;

namespace NetApi.Domain.Products.Entities;

public class ProductTagEntity : IEntity<ProductTagId>, ITimestamp
{
    public ProductTagId? Id { get; set; }
    public string Name { get; set; } = "";
    public string Color { get; set; } = "";

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = "";
}
