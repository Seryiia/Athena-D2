#nullable disable
using CommandLine;
using AthenaBot.Common.ModuleBehaviors;
using AthenaBot.Modules.Administration.Services;

namespace AthenaBot.Modules.Help.Services;

public class HelpService : IExecNoCommand, INService
{
    private readonly CommandHandler _ch;
    private readonly IBotStrings _strings;
    private readonly BotConfigService _bss;
    private readonly IEmbedBuilderService _eb;
    private readonly ILocalization _loc;

    public HelpService(
        CommandHandler ch,
        IBotStrings strings,
        BotConfigService bss,
        IEmbedBuilderService eb,
        ILocalization loc)
    {
        _ch = ch;
        _strings = strings;
        _bss = bss;
        _eb = eb;
        _loc = loc;
    }

    public Task ExecOnNoCommandAsync(IGuild guild, IUserMessage msg)
    {
        var settings = _bss.Data;
        if (guild is null)
        {
            if (string.IsNullOrWhiteSpace(settings.DmHelpText) || settings.DmHelpText == "-")
                return Task.CompletedTask;

            // only send dm help text if it contains one of the keywords, if they're specified
            // if they're not, then reply to every DM
            if (settings.DmHelpTextKeywords is not null && !settings.DmHelpTextKeywords.Any(k => msg.Content.Contains(k)))
                return Task.CompletedTask;

            var rep = new ReplacementBuilder().WithOverride("%prefix%", () => _bss.Data.Prefix)
                                              .WithOverride("%bot.prefix%", () => _bss.Data.Prefix)
                                              .WithUser(msg.Author)
                                              .Build();

            var text = SmartText.CreateFrom(settings.DmHelpText);
            text = rep.Replace(text);

            return msg.Channel.SendAsync(text);
        }

        return Task.CompletedTask;
    }

    public IEmbedBuilder GetCommandHelp(CommandInfo com, IGuild guild)
    {
        var prefix = _ch.GetPrefix(guild);
        
        var str = $"**`{prefix + com.Aliases.First()}`**";
        var alias = com.Aliases.Skip(1).FirstOrDefault();
        if (alias is not null)
            str += $" **/ `{prefix + alias}`**";

        var culture = _loc.GetCultureInfo(guild);
        
        var em = _eb.Create()
                    .AddField(str, $"{com.RealSummary(_strings, culture,  prefix)}", true);

        var reqs = GetCommandRequirements(com);
        if (reqs.Any())
            em.AddField(GetText(strs.requires, guild), string.Join("\n", reqs));

        em.AddField(_strings.GetText(strs.usage),
              string.Join("\n", com.RealRemarksArr(_strings, culture, prefix).Map(arg => Format.Code(arg))))
          .WithFooter(GetText(strs.module(com.Module.GetTopLevelModule().Name), guild))
          .WithOkColor();

        var opt = ((AthenaOptionsAttribute)com.Attributes.FirstOrDefault(x => x is AthenaOptionsAttribute))?.OptionType;
        if (opt is not null)
        {
            var hs = GetCommandOptionHelp(opt);
            if (!string.IsNullOrWhiteSpace(hs))
                em.AddField(GetText(strs.options, guild), hs);
        }

        return em;
    }

    public static string GetCommandOptionHelp(Type opt)
    {
        var strs = GetCommandOptionHelpList(opt);

        return string.Join("\n", strs);
    }

    public static List<string> GetCommandOptionHelpList(Type opt)
    {
        var strs = opt.GetProperties()
                      .Select(x => x.GetCustomAttributes(true).FirstOrDefault(a => a is OptionAttribute))
                      .Where(x => x is not null)
                      .Cast<OptionAttribute>()
                      .Select(x =>
                      {
                          var toReturn = $"`--{x.LongName}`";

                          if (!string.IsNullOrWhiteSpace(x.ShortName))
                              toReturn += $" (`-{x.ShortName}`)";

                          toReturn += $"   {x.HelpText}  ";
                          return toReturn;
                      })
                      .ToList();

        return strs;
    }


    public static string[] GetCommandRequirements(CommandInfo cmd)
    {
        var toReturn = new List<string>();

        if (cmd.Preconditions.Any(x => x is OwnerOnlyAttribute))
            toReturn.Add("Bot Owner Only");
        
        if(cmd.Preconditions.Any(x => x is NoPublicBotAttribute)
           || cmd.Module
                  .Preconditions
                  .Any(x => x is NoPublicBotAttribute)
            || cmd.Module.GetTopLevelModule()
                  .Preconditions
                  .Any(x => x is NoPublicBotAttribute))
            toReturn.Add("No Public Bot");

        if (cmd.Preconditions
               .Any(x => x is OnlyPublicBotAttribute)
            || cmd.Module
                  .Preconditions
                  .Any(x => x is OnlyPublicBotAttribute)
            || cmd.Module.GetTopLevelModule()
                  .Preconditions
                  .Any(x => x is OnlyPublicBotAttribute))
            toReturn.Add("Only Public Bot");

        var userPermString = cmd.Preconditions
                                .Where(ca => ca is RequireUserPermissionAttribute)
                                .Cast<RequireUserPermissionAttribute>()
                                .Select(userPerm =>
                                {
                                    if (userPerm.ChannelPermission is { } cPerm)
                                        return GetPreconditionString(cPerm);

                                    if (userPerm.GuildPermission is { } gPerm)
                                        return GetPreconditionString(gPerm);

                                    return string.Empty;
                                })
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .Join('\n');
        
        if (!string.IsNullOrWhiteSpace(userPermString))
            toReturn.Add(userPermString);

        return toReturn.ToArray();
    }

    public static string GetPreconditionString(ChannelPerm perm)
        => (perm + " Channel Permission").Replace("Guild", "Server");

    public static string GetPreconditionString(GuildPerm perm)
        => (perm + " Server Permission").Replace("Guild", "Server");

    private string GetText(LocStr str, IGuild guild)
        => _strings.GetText(str, guild?.Id);
}
