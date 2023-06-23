#nullable disable
namespace AthenaBot.Common;

public interface ICloneable<T>
    where T : new()
{
    public T Clone();
}