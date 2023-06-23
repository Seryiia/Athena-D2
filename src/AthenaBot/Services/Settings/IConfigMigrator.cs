#nullable disable
namespace AthenaBot.Services;

public interface IConfigMigrator
{
    public void EnsureMigrated();
}