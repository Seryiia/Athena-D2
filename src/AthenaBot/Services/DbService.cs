#nullable disable
using LinqToDB.Common;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AthenaBot.Services.Database;

namespace AthenaBot.Services;

public class DbService
{
    private readonly IBotCredsProvider _creds;

    // these are props because creds can change at runtime
    private string DbType => _creds.GetCreds().Db.Type.ToLowerInvariant().Trim();
    private string ConnString => _creds.GetCreds().Db.ConnectionString;
    
    public DbService(IBotCredsProvider creds)
    {
        LinqToDBForEFTools.Initialize();
        Configuration.Linq.DisableQueryCache = true;

        _creds = creds;
    }

    public async Task SetupAsync()
    {
        var dbType = DbType;
        var connString = ConnString;

        await using var context = CreateRawDbContext(dbType, connString);
        await context.Database.MigrateAsync();
    }

    private static AthenaContext CreateRawDbContext(string dbType, string connString)
    {
        switch (dbType)
        {
            case "mysql":
                return new MysqlContext(connString);
            default:
                throw new NotSupportedException($"The database provide type of '{dbType}' is not supported.");
        }
    }
    
    private AthenaContext GetDbContextInternal()
    {
        var dbType = DbType;
        var connString = ConnString;

        var context = CreateRawDbContext(dbType, connString);
        return context;
    }

    public AthenaContext GetDbContext()
        => GetDbContextInternal();
}