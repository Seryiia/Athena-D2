#nullable disable
using AthenaBot.Common.TypeReaders.Models;
using AthenaBot.Modules.Administration.Services;

namespace AthenaBot.Modules.Administration;

public partial class Administration : AthenaModule<AdministrationService>
{
    public enum Channel
    {
        Channel,
        Ch,
        Chnl,
        Chan
    }

    public enum List
    {
        List = 0,
        Ls = 0
    }

    public enum Server
    {
        Server
    }

    public enum State
    {
        Enable,
        Disable,
        Inherit
    }

    [Cmd]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPerm.Administrator)]
    [BotPerm(GuildPerm.ManageMessages)]
    [Priority(2)]
    public async Task Delmsgoncmd(List _)
    {
        var guild = (SocketGuild)ctx.Guild;
        var (enabled, channels) = _service.GetDelMsgOnCmdData(ctx.Guild.Id);

        var embed = _eb.Create()
                       .WithOkColor()
                       .WithTitle(GetText(strs.server_delmsgoncmd))
                       .WithDescription(enabled ? "✅" : "❌");

        var str = string.Join("\n",
            channels.Select(x =>
            {
                var ch = guild.GetChannel(x.ChannelId)?.ToString() ?? x.ChannelId.ToString();
                var prefixSign = x.State ? "✅ " : "❌ ";
                return prefixSign + ch;
            }));

        if (string.IsNullOrWhiteSpace(str))
            str = "-";

        embed.AddField(GetText(strs.channel_delmsgoncmd), str);

        await ctx.Channel.EmbedAsync(embed);
    }

    [Cmd]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPerm.Administrator)]
    [BotPerm(GuildPerm.ManageMessages)]
    [Priority(1)]
    public async Task Delmsgoncmd(Server _ = Server.Server)
    {
        if (_service.ToggleDeleteMessageOnCommand(ctx.Guild.Id))
        {
            _service.DeleteMessagesOnCommand.Add(ctx.Guild.Id);
            await ReplyConfirmLocalizedAsync(strs.delmsg_on);
        }
        else
        {
            _service.DeleteMessagesOnCommand.TryRemove(ctx.Guild.Id);
            await ReplyConfirmLocalizedAsync(strs.delmsg_off);
        }
    }

    [Cmd]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPerm.Administrator)]
    [BotPerm(GuildPerm.ManageMessages)]
    [Priority(0)]
    public Task Delmsgoncmd(Channel _, State s, ITextChannel ch)
        => Delmsgoncmd(_, s, ch.Id);

    [Cmd]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPerm.Administrator)]
    [BotPerm(GuildPerm.ManageMessages)]
    [Priority(1)]
    public async Task Delmsgoncmd(Channel _, State s, ulong? chId = null)
    {
        var actualChId = chId ?? ctx.Channel.Id;
        await _service.SetDelMsgOnCmdState(ctx.Guild.Id, actualChId, s);

        if (s == State.Disable)
            await ReplyConfirmLocalizedAsync(strs.delmsg_channel_off);
        else if (s == State.Enable)
            await ReplyConfirmLocalizedAsync(strs.delmsg_channel_on);
        else
            await ReplyConfirmLocalizedAsync(strs.delmsg_channel_inherit);
    }
}