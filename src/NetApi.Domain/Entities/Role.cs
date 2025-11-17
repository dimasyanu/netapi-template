using NetApi.Domain.Abstractions;

namespace NetApi.Domain.Entities;

public class Role : Int16BaseEntity
{
    public string Name { get; set; } = "";

    public List<User>? Users { get; set; }
}
