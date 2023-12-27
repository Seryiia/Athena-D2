#nullable disable

using StackExchange.Redis;
using System.Text.RegularExpressions;

namespace AthenaBot.Modules.AthenaVanityRole;

public partial class AthenaVanityRole : AthenaModule<INService>
{
    [Group]
    public partial class AthenaVanityRoleCommands : AthenaModule<AthenaVanityRoleService>
    {
        private readonly DbService _db;

        public AthenaVanityRoleCommands(DbService db)
            => _db = db;

        [Cmd]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPerm.Administrator)]
        public async Task ConfigureVanityRole(int type, ulong role)
        {
            AthenaVanityRoleService.ErrorCodes retCode = await _service.ConfigureVanityRole(ctx.Guild.Id, type, role);


            switch (retCode)
            {
                case AthenaVanityRoleService.ErrorCodes.RoleConfigurationFailed:
                    await ReplyErrorLocalizedAsync(strs.role_configuration_failed);
                    return;
                case AthenaVanityRoleService.ErrorCodes.RoleTypeNotFound:
                    await ReplyErrorLocalizedAsync(strs.role_type_not_found);
                    return;
            }

            await ReplyConfirmLocalizedAsync(strs.vanity_role_configured(role));
        }

        [Cmd]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPerm.Administrator)]
        public async Task BirthdayAdd(IGuildUser user, IChannel channel, string date)
        {
            AthenaVanityRoleService.ErrorCodes retCode = await _service.AddBirthday(ctx.Guild.Id, user, date, channel);


            switch (retCode)
            {
                case AthenaVanityRoleService.ErrorCodes.InvalidDate:
                    await ReplyErrorLocalizedAsync(strs.invalid_date);
                    return;
                case AthenaVanityRoleService.ErrorCodes.BirthdayConfigFailed:
                    await ReplyErrorLocalizedAsync(strs.birthday_config_failed);
                    return;
            }

            await ReplyConfirmLocalizedAsync(strs.birthday_added(user.Username));
        }

        [Cmd]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPerm.Administrator)]
        public async Task BirthdayRemove(IGuildUser user)
        {
            AthenaVanityRoleService.ErrorCodes retCode = await _service.RemoveBirthday(ctx.Guild.Id, user);
            switch (retCode)
            {
                case AthenaVanityRoleService.ErrorCodes.BirthdayNotFound:
                    await ReplyErrorLocalizedAsync(strs.birthday_not_found);
                    return;
                case AthenaVanityRoleService.ErrorCodes.InvalidDate:
                    await ReplyErrorLocalizedAsync(strs.birthday_delete_failed);
                    return;
            }

            await ReplyConfirmLocalizedAsync(strs.birthday_removed(user.Username));
        }

        [Cmd]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPerm.Administrator)]
        public async Task Peach(IGuildUser user)
        {
            AthenaVanityRoleService.ErrorCodes retCode = await _service.Peach(ctx.Guild.Id, user);
            switch (retCode)
            {
                case AthenaVanityRoleService.ErrorCodes.NoRoleConfigured:
                    await ReplyErrorLocalizedAsync(strs.no_role_configured);
                    return;
                case AthenaVanityRoleService.ErrorCodes.RoleMisconfigured:
                    await ReplyErrorLocalizedAsync(strs.role_misconfigured);
                    return;
                case AthenaVanityRoleService.ErrorCodes.RoleAssignmentFailed:
                    await ReplyErrorLocalizedAsync(strs.could_not_add_role(user.Username));
                    return;
                case AthenaVanityRoleService.ErrorCodes.RoleNotFoundInGuild:
                    await ReplyErrorLocalizedAsync(strs.vanity_role_not_found);
                    return;
            }

            await ReplyAsync(GetText(strs.peached(user.Nickname)));
        }

        [Cmd]
        [RequireContext(ContextType.Guild)]
        public async Task GoToBed(IGuildUser user)
        {
            AthenaVanityRoleService.ErrorCodes retCode = await _service.BapToSleep(ctx.Guild.Id, user);
            switch (retCode)
            {
                case AthenaVanityRoleService.ErrorCodes.NoRoleConfigured:
                    await ReplyErrorLocalizedAsync(strs.no_role_configured);
                    return;
                case AthenaVanityRoleService.ErrorCodes.RoleMisconfigured:
                    await ReplyErrorLocalizedAsync(strs.role_misconfigured);
                    return;
                case AthenaVanityRoleService.ErrorCodes.RoleAssignmentFailed:
                    await ReplyErrorLocalizedAsync(strs.could_not_add_role(user.Username));
                    return;
                case AthenaVanityRoleService.ErrorCodes.RoleNotFoundInGuild:
                    await ReplyErrorLocalizedAsync(strs.vanity_role_not_found);
                    return;
            }

            await ReplyAsync(GetText(strs.go_to_bed(user.Nickname)).ToUpper());
        }
    }
}