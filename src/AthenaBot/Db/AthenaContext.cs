#nullable disable
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AthenaBot.Db.Models;
using AthenaBot.Services.Database.Models;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace AthenaBot.Services.Database;

public abstract class AthenaContext : DbContext
{
    public DbSet<GuildConfig> GuildConfigs { get; set; }

    public DbSet<Quote> Quotes { get; set; }
    public DbSet<Reminder> Reminders { get; set; }
    public DbSet<AthenaExpression> Expressions { get; set; }
    public DbSet<DiscordUser> DiscordUser { get; set; }
    
    public DbSet<StreamOnlineMessage> StreamOnlineMessages { get; set; }


    #region Mandatory Provider-Specific Values

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region QUOTES

        var quoteEntity = modelBuilder.Entity<Quote>();
        quoteEntity.HasIndex(x => x.GuildId);
        quoteEntity.HasIndex(x => x.Keyword);

        #endregion

        #region GuildConfig

        var configEntity = modelBuilder.Entity<GuildConfig>();
        configEntity.HasIndex(c => c.GuildId)
                    .IsUnique();

        configEntity.Property(x => x.VerboseErrors)
                    .HasDefaultValue(true);

        modelBuilder.Entity<FeedSub>()
                    .HasAlternateKey(x => new
                    {
                        x.GuildConfigId,
                        x.Url
                    });

        #endregion

        #region DiscordUser

        modelBuilder.Entity<DiscordUser>(du =>
        {
            du.HasAlternateKey(w => w.UserId);
            du.HasIndex(x => x.UserId);
        });

        #endregion

        #region Reminders

        modelBuilder.Entity<Reminder>().HasIndex(x => x.When);

        #endregion

        #region GroupName

        modelBuilder.Entity<GroupName>()
                    .HasIndex(x => new
                    {
                        x.GuildConfigId,
                        x.Number
                    })
                    .IsUnique();

        modelBuilder.Entity<GroupName>()
                    .HasOne(x => x.GuildConfig);
        #endregion
    }

#if DEBUG
    private static readonly ILoggerFactory _debugLoggerFactory = LoggerFactory.Create(x => x.AddConsole());

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseLoggerFactory(_debugLoggerFactory);
#endif
}