using NetApi.Domain.Common.Contracts;

namespace NetApi.Domain.Media.ValueObjects;

public record class MediaType(byte Value) : IValueObject
{
    #region Enums
    public static readonly MediaType Empty = 0;
    public static readonly MediaType Image = 1;
    public static readonly MediaType Video = 2;
    public static readonly MediaType Document = 3;
    public static readonly MediaType Audio = 4;
    public static readonly MediaType Other = 5;
    public static readonly MediaType Unknown = 99;
    #endregion

    public static MediaType FromByte(byte value) => new(value);

    public byte ToByte() => Value;

    public static implicit operator MediaType(byte value)
        => new(value);

    public bool IsEmpty()
        => Value < 0;
}
