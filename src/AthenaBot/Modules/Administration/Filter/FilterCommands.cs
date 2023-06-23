﻿#nullable disable
using Microsoft.EntityFrameworkCore;
using AthenaBot.Db;
using AthenaBot.Services.Database.Models;

namespace AthenaBot.Modules.Administration;

public partial class Administration
{
    [Group]
    public partial class FilterCommands : AthenaModule<FilterService>
    {
        private readonly DbService _db;

        public FilterCommands(DbService db)
            => _db = db;

        [Cmd]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPerm.Administrator)]
        public async Task FwClear()
        {
            _service.ClearFilteredWords(ctx.Guild.Id);
            await ReplyConfirmLocalizedAsync(strs.fw_cleared);
        }

        [Cmd]
        [RequireContext(ContextType.Guild)]
        public async Task SrvrFilterWords()
        {
            var channel = (ITextChannel)ctx.Channel;

            bool enabled;
            await using (var uow = _db.GetDbContext())
            {
                var config = uow.GuildConfigsForId(channel.Guild.Id, set => set);
                enabled = config.FilterWords = !config.FilterWords;
                await uow.SaveChangesAsync();
            }

            if (enabled)
            {
                _service.WordFilteringServers.Add(channel.Guild.Id);
                await ReplyConfirmLocalizedAsync(strs.word_filter_server_on);
            }
            else
            {
                _service.WordFilteringServers.TryRemove(channel.Guild.Id);
                await ReplyConfirmLocalizedAsync(strs.word_filter_server_off);
            }
        }

        [Cmd]
        [RequireContext(ContextType.Guild)]
        public async Task FilterWord([Leftover] string word)
        {
            var channel = (ITextChannel)ctx.Channel;

            word = word?.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(word))
                return;

            FilteredWord removed;
            await using (var uow = _db.GetDbContext())
            {
                var config = uow.GuildConfigsForId(channel.Guild.Id, set => set.Include(gc => gc.FilteredWords));

                removed = config.FilteredWords.FirstOrDefault(fw => fw.Word.Trim().ToLowerInvariant() == word);

                if (removed is null)
                {
                    config.FilteredWords.Add(new()
                    {
                        Word = word
                    });
                }
                else
                    uow.Remove(removed);

                await uow.SaveChangesAsync();
            }

            var filteredWords =
                _service.ServerFilteredWords.GetOrAdd(channel.Guild.Id, new ConcurrentHashSet<string>());

            if (removed is null)
            {
                filteredWords.Add(word);
                await ReplyConfirmLocalizedAsync(strs.filter_word_add(Format.Code(word)));
            }
            else
            {
                filteredWords.TryRemove(word);
                await ReplyConfirmLocalizedAsync(strs.filter_word_remove(Format.Code(word)));
            }
        }

        [Cmd]
        [RequireContext(ContextType.Guild)]
        public async Task LstFilterWords(int page = 1)
        {
            page--;
            if (page < 0)
                return;

            var channel = (ITextChannel)ctx.Channel;

            _service.ServerFilteredWords.TryGetValue(channel.Guild.Id, out var fwHash);

            var fws = fwHash.ToArray();

            await ctx.SendPaginatedConfirmAsync(page,
                curPage => _eb.Create()
                              .WithTitle(GetText(strs.filter_word_list))
                              .WithDescription(string.Join("\n", fws.Skip(curPage * 10).Take(10)))
                              .WithOkColor(),
                fws.Length,
                10);
        }
    }
}