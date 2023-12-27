using Microsoft.EntityFrameworkCore;

namespace AthenaBot.Services.Database;

public sealed class MysqlContext : AthenaContext
{
    private readonly string _connStr;
    private readonly string _version;

    public MysqlContext(string connStr = "Server=localhost;Port=3306;Uid=local;Pwd=replacePass;Database=athena", string version = "8.0")
    {
        _connStr = connStr;
        _version = version;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder
            .UseLowerCaseNamingConvention()
            .UseMySql(_connStr, ServerVersion.Parse(_version));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}