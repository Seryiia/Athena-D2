#nullable disable
using Microsoft.EntityFrameworkCore;
using AthenaBot.Db.Models;
using LinqToDB.EntityFrameworkCore;
using LinqToDB;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using StackExchange.Redis;
using AthenaBot.Modules.StreamNotification.Models;
using AthenaBot.Services.Database.Models;

namespace AthenaBot.Modules.AthenaVanityRole;

public sealed class AthenaVanityRoleService : INService
{
    private const ulong TWENTY_FOUR_HOURS_MS = 1000 * 60 * 60 * 24;
    private const ulong EIGHT_FOUR_HOURS_MS = 1000 * 60 * 60 * 8;

    private readonly DiscordSocketClient _client;
    private readonly DbService _db;

    private readonly Dictionary<ulong, Dictionary<VanityRole.VanityRoleType, System.Timers.Timer>> _roleTasks = [];
    private readonly System.Timers.Timer _birthdayTimer = new();

    public enum ErrorCodes
    {
        None = 0,

        // Configuration
        RoleConfigurationFailed = 1,
        NoRoleConfigured = 2,
        RoleMisconfigured = 3,

        // Invalid entries
        RoleNotFoundInGuild = 5,
        RoleTypeNotFound = 6,

        // Runtime errors
        RoleAssignmentFailed = 7,

        // Birthday specific
        InvalidDate = 8,
        BirthdayNotFound = 9,
        BirthdayDeleteFailed = 10,
        BirthdayConfigFailed = 11
    }

    public AthenaVanityRoleService(DbService db,
        DiscordSocketClient client)
    {
        _db = db;
        _client = client;

        BirthdayTimerExpired();
    }

    private void StartBirthdayTimer()
    {
        // Get current time
        DateTime now = DateTime.Now;

        // Run at 9am every day
        DateTime targetTime;
        if (now.Hour > 9)
        {
            // This doesn't account for DST but running one day at 8am and one day at 10am isn't a big deal
            targetTime = DateTime.Today.AddDays(1).AddHours(9);
        }
        else
        {
            targetTime = DateTime.Today.AddHours(9);
        }

        TimeSpan diff = targetTime - now;
        double timeToDelay = diff.TotalMilliseconds;

        Log.Information($"Starting birthday time with delay {timeToDelay}");

        _birthdayTimer.Stop();
        _birthdayTimer.Interval = timeToDelay;
        _birthdayTimer.Elapsed += (sender, e) => BirthdayTimerExpired();
        _birthdayTimer.AutoReset = false;
        _birthdayTimer.Start();
    }

    private async void BirthdayTimerExpired()
    {
        _birthdayTimer.Stop();

        // Check for birthdays
        DateTime now = DateTime.Now;

        // Run for each guild
        var ids = _client.GetGuildIds();

        foreach (var guildId in ids)
        {
            // Fetch from database
            List<Birthday> usersWithBirthdays = new List<Birthday>();
            try
            {
                using (var ctx = _db.GetDbContext())
                {
                    // First verify the name wasn't taken
                    var count = ctx.Birthdays
                        .Where(x => x.GuildId == guildId)
                        .Where(x => x.Month == now.Month)
                        .Where(x => x.Day == now.Day)
                        .Select(x => x.Id).Count();

                    if (count > 0)
                    {
                        Log.Information($"Found birthdays for {now.Month}/{now.Day} in guild {guildId}");

                        foreach (var birthday in ctx.Birthdays.Where(x => x.GuildId == guildId).Where(x => x.Month == now.Month).Where(x => x.Day == now.Day))
                        {
                            usersWithBirthdays.Add(birthday);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"Birthday fetching failed with error: {ex.Message}");
            }

            // Add birthday roles
            foreach (var birthday in usersWithBirthdays)
            {
                DateTime tomorrow = DateTime.Now.AddDays(1);
                TimeSpan timeTillTomorrow = tomorrow - DateTime.Now;

                IGuild guild = _client.GetGuild(birthday.GuildId);
                IGuildUser guildUser = guild.GetUserAsync(birthday.UserId).Result;

                if (guildUser != null)
                {
                    await AddRoleTypeForTime(guildId, guildUser, VanityRole.VanityRoleType.Birthday, (ulong)timeTillTomorrow.TotalMilliseconds);

                    // Send birthday message
                    IChannel channel = await guild.GetChannelAsync(birthday.ChannelId);
                    if(channel is IMessageChannel messageChannel)
                    {
                        await messageChannel.SendMessageAsync($"From the deepest pit of the seven hells to the pinnacle of the heavens, the world shall tremble! Unleash <@{birthday.UserId}>'s birthday!!");
                    }
                }
                else
                {
                    Log.Information($"Could not find userId {birthday.UserId} in guild {guildId}");
                }
            }
        }

        // Restart timer
        StartBirthdayTimer();
    }

    public async Task<ErrorCodes> ConfigureVanityRole(ulong guildId, int type, ulong roleId)
    {
        // Get vanity role type
        if (!Enum.IsDefined(typeof(VanityRole.VanityRoleType), type))
        {
            return ErrorCodes.RoleTypeNotFound;
        }

        VanityRole.VanityRoleType roleType = (VanityRole.VanityRoleType)type;

        // Save to database
        try
        {
            await using (var ctx = _db.GetDbContext())
            {
                // First verify the name wasn't taken
                var count = ctx.VanityRoles
                    .Where(x => x.GuildId == guildId)
                    .Where(x => x.Type == roleType)
                    .Select(x => x.Id).Count();

                if (count != 0)
                {
                    Log.Information($"Overwritting vanity role of type {type} with role id {roleId}");

                    foreach (var roleConfig in ctx.VanityRoles.Where(x => x.GuildId == guildId).Where(x => x.Type == roleType))
                    {
                        roleConfig.RoleId = roleId;
                    }
                    await ctx.SaveChangesAsync();
                }
                else
                {
                    ctx.VanityRoles.Add(new()
                    {
                        GuildId = guildId,
                        Type = roleType,
                        RoleId = roleId
                    });
                    ;
                    await ctx.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warning($"Vanity role configuration failed with error: {ex.Message}");
            return ErrorCodes.RoleConfigurationFailed;
        }

        return ErrorCodes.None;
    }

    public async Task<ErrorCodes> AddBirthday(ulong guildId, IGuildUser user, string date, IChannel channel)
    {
        // Validate date format
        var match = Regex.Match(date, "^\\d{1,2}/\\d{1,2}");
        if (!match.Success)
        {
            return ErrorCodes.InvalidDate;
        }

        string[] tokens = date.Split("/");
        if (!short.TryParse(tokens[0], out var month))
        {
            return ErrorCodes.InvalidDate;
        }

        if (!short.TryParse(tokens[1], out var day))
        {
            return ErrorCodes.InvalidDate;
        }

        // Validate date range
        if (month < 1 || month > 12 || day < 1 || day > 31)
        {
            return ErrorCodes.InvalidDate;
        }

        // 30 day months
        if (month is 4 or 6 or 9 or 11)
        {
            if (day > 30)
            {
                return ErrorCodes.InvalidDate;
            }
        }

        // Feburary
        if (month == 2)
        {
            if (day > 29)
            {
                return ErrorCodes.InvalidDate;
            }
        }

        // Date validated

        // Save to DB
        try
        {
            await using (var ctx = _db.GetDbContext())
            {
                // First verify the name wasn't taken
                var count = ctx.Birthdays
                    .Where(x => x.GuildId == guildId)
                    .Where(x => x.UserId == user.Id)
                    .Select(x => x.Id).Count();

                if (count != 0)
                {
                    Log.Information($"Overwritting birthday for user {user.Nickname} with month/day {month}/{day}");

                    foreach (var birthdayConfig in ctx.Birthdays.Where(x => x.GuildId == guildId).Where(x => x.UserId == user.Id))
                    {
                        birthdayConfig.Month = (ushort)month;
                        birthdayConfig.Day = (ushort)day;
                        birthdayConfig.ChannelId = channel.Id;
                    }
                    await ctx.SaveChangesAsync();
                }
                else
                {
                    ctx.Birthdays.Add(new()
                    {
                        GuildId = guildId,
                        UserId = user.Id,
                        Month = (ushort)month,
                        Day = (ushort)day,
                        ChannelId = channel.Id
                    });
                    await ctx.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warning($"Birthday configuration failed with error: {ex.Message}");
            return ErrorCodes.BirthdayConfigFailed;
        }

        return ErrorCodes.None;
    }

    public async Task<ErrorCodes> RemoveBirthday(ulong guildId, IGuildUser user)
    {
        // Delete from database
        try
        {
            await using (var ctx = _db.GetDbContext())
            {
                // First verify the name wasn't taken
                var birthdayQuery = ctx.Birthdays
                .Where(x => x.GuildId == guildId)
                .Where(x => x.UserId == user.Id)
                .Select(x => x.Id);

                if (birthdayQuery.Count() != 0)
                {
                    await ctx.GetTable<Birthday>()
                        .Where(x => x.GuildId == guildId && x.UserId == user.Id)
                        .DeleteAsync();
                }
                else
                {
                    return ErrorCodes.BirthdayNotFound;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warning($"Birthday delete failed with error: {ex.Message}");
            return ErrorCodes.BirthdayDeleteFailed;
        }

        return ErrorCodes.None;
    }

    public async Task<ErrorCodes> Peach(ulong guildId, IGuildUser user)
    {
        return await AddRoleTypeForTime(guildId, user, VanityRole.VanityRoleType.Peach, TWENTY_FOUR_HOURS_MS);
    }

    public async Task<ErrorCodes> BapToSleep(ulong guildId, IGuildUser user)
    {
        return await AddRoleTypeForTime(guildId, user, VanityRole.VanityRoleType.BapToSleep, EIGHT_FOUR_HOURS_MS);
    }

    private async Task<ErrorCodes> AddRoleTypeForTime(ulong guildId, IGuildUser user, VanityRole.VanityRoleType type, ulong timeMs)
    {
        // Fetch role from database
        ulong roleId;

        Log.Information($"Looking up role for type {type}");
        try
        {
            await using (var ctx = _db.GetDbContext())
            {
                // First verify the name wasn't taken
                var roleQuery = ctx.VanityRoles
                    .Where(x => x.GuildId == guildId)
                    .Where(x => x.Type == type)
                    .Select(x => x.Id);

                if (roleQuery.Count() < 1)
                {
                    return ErrorCodes.NoRoleConfigured;
                }
                else if (roleQuery.Count() > 1)
                {
                    // Something went really wrong, clear out this role in the database
                    await roleQuery.DeleteAsync();
                    await ctx.SaveChangesAsync();

                    return ErrorCodes.RoleMisconfigured;
                }
                else
                {
                    roleId = ctx.VanityRoles
                            .Where(x => x.GuildId == guildId)
                            .Where(x => x.Type == type)
                            .First().RoleId;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warning($"Vanity role fetch failed with error: {ex.Message}");
            return ErrorCodes.RoleAssignmentFailed;
        }

        // Verify role exists
        IRole role = user.Guild.GetRole(roleId);
        if (role == null)
        {
            return ErrorCodes.RoleNotFoundInGuild;
        }

        // Role exists, assign to user
        Log.Information($"Adding role {role.Name} to {user.Nickname}");
        await user.AddRoleAsync(role);

        // Create / Update task to remove role
        if (!_roleTasks.ContainsKey(user.Id))
        {
            _roleTasks.Add(user.Id, []);
        }

        var userTasks = _roleTasks[user.Id];
        if (userTasks.ContainsKey(type))
        {
            System.Timers.Timer userTask = userTasks[type];
            userTask.Stop();
            userTasks.Remove(type);
        }

        System.Timers.Timer timer = new System.Timers.Timer(timeMs);
        timer.Elapsed += (sender, e) => RemoveRole(user, role);
        timer.AutoReset = false;

        userTasks.Add(type, timer);
        timer.Start();

        return ErrorCodes.None;
    }

    private static void RemoveRole(IGuildUser user, IRole role)
    {
        Log.Information($"Removing role {role.Name} from {user.Nickname}");
        user.RemoveRoleAsync(role);
    }
}