using System.ComponentModel.DataAnnotations;

namespace NetApi.Domain.Common.Contracts;

public interface IBaseEntity
{
    object GetId();
}

public interface IBaseEntity<TId> : IBaseEntity
{
    [Key]
    TId Id { get; set; }

    object IBaseEntity.GetId()
    {
        return (TId)(object)Id!;
    }
}
