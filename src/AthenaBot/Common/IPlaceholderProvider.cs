#nullable disable
namespace AthenaBot.Common;

public interface IPlaceholderProvider
{
    public IEnumerable<(string Name, Func<string> Func)> GetPlaceholders();
}