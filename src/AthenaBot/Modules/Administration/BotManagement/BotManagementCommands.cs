#nullable disable
using AthenaBot.Modules.Administration.Services;

namespace AthenaBot.Modules.Administration;

public partial class Administration
{
    [Group]
    public partial class BotManagementCommands : AthenaModule<BotManagement>
    {
        public enum SettableUserStatus
        {
            Online,
            Invisible,
            Idle,
            Dnd
        }

        private readonly DiscordSocketClient _client;
        private readonly IBotStrings _strings;
        private readonly ICoordinator _coord;

        public BotManagementCommands(
            DiscordSocketClient client,
            IBotStrings strings,
            ICoordinator coord)
        {
            _client = client;
            _strings = strings;
            _coord = coord;
        }

        [Cmd]
        public async Task ShardStats(int page = 1)
        {
            if (--page < 0)
                return;

            var statuses = _coord.GetAllShardStatuses();

            var status = string.Join(" : ",
                statuses.Select(x => (ConnectionStateToEmoji(x), x))
                        .GroupBy(x => x.Item1)
                        .Select(x => $"`{x.Count()} {x.Key}`")
                        .ToArray());

            var allShardStrings = statuses.Select(st =>
                                          {
                                              var timeDiff = DateTime.UtcNow - st.LastUpdate;
                                              var stateStr = ConnectionStateToEmoji(st);
                                              var maxGuildCountLength =
                                                  statuses.Max(x => x.GuildCount).ToString().Length;
                                              return $"`{stateStr} "
                                                     + $"| #{st.ShardId.ToString().PadBoth(3)} "
                                                     + $"| {timeDiff:mm\\:ss} "
                                                     + $"| {st.GuildCount.ToString().PadBoth(maxGuildCountLength)} `";
                                          })
                                          .ToArray();
            await ctx.SendPaginatedConfirmAsync(page,
                curPage =>
                {
                    var str = string.Join("\n", allShardStrings.Skip(25 * curPage).Take(25));

                    if (string.IsNullOrWhiteSpace(str))
                        str = GetText(strs.no_shards_on_page);

                    return _eb.Create().WithOkColor().WithDescription($"{status}\n\n{str}");
                },
                allShardStrings.Length,
                25);
        }

        private static string ConnectionStateToEmoji(ShardStatus status)
        {
            var timeDiff = DateTime.UtcNow - status.LastUpdate;
            return status.ConnectionState switch
            {
                ConnectionState.Disconnected => "🔻",
                _ when timeDiff > TimeSpan.FromSeconds(30) => " ❗ ",
                ConnectionState.Connected => "✅",
                _ => " ⏳"
            };
        }

        [Cmd]
        [OwnerOnly]
        public async Task RestartShard(int shardId)
        {
            var success = _coord.RestartShard(shardId);
            if (success)
                await ReplyConfirmLocalizedAsync(strs.shard_reconnecting(Format.Bold("#" + shardId)));
            else
                await ReplyErrorLocalizedAsync(strs.no_shard_id);
        }

        [Cmd]
        [OwnerOnly]
        public Task Leave([Leftover] string guildStr)
            => _service.LeaveGuild(guildStr);

        [Cmd]
        [OwnerOnly]
        public async Task Die(bool graceful = false)
        {
            try
            {
                await _client.SetStatusAsync(UserStatus.Invisible);
                _ =  _client.StopAsync();
                await ReplyConfirmLocalizedAsync(strs.shutting_down);
            }
            catch
            {
                // ignored
            }

            await Task.Delay(2000);
            _coord.Die(graceful);
        }

        [Cmd]
        [OwnerOnly]
        public async Task Restart()
        {
            var success = _coord.RestartBot();
            if (!success)
            {
                await ReplyErrorLocalizedAsync(strs.restart_fail);
                return;
            }

            try { await ReplyConfirmLocalizedAsync(strs.restarting); }
            catch { }
        }

        [Cmd]
        [OwnerOnly]
        public async Task StringsReload()
        {
            _strings.Reload();
            await ReplyConfirmLocalizedAsync(strs.bot_strings_reloaded);
        }

        [Cmd]
        [OwnerOnly]
        public async Task CoordReload()
        {
            await _coord.Reload();
            await ctx.OkAsync();
        }
    }
}