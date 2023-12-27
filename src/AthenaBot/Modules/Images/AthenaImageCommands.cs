#nullable disable

using AthenaBot.Db.Models;

namespace AthenaBot.Modules.AthenaImages;

public partial class AthenaImages : AthenaModule<INService>
{
    [Group]
    public partial class AthenaImageCommands : AthenaModule<AthenaImagesService>
    {
        private readonly DbService _db;
        private readonly Random _random = new();

        public AthenaImageCommands(DbService db)
            => _db = db;

        [Cmd]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPerm.Administrator)]
        public async Task ImageAdd(string name)
        {
            var attachments = Context.Message.Attachments;

            // Ensure only one attachemnt
            if(attachments == null || attachments.Count == 0)
            {
                await ReplyErrorLocalizedAsync(strs.images_no_attachment);
                return;
            }

            if(attachments.Count > 1)
            {
                await ReplyErrorLocalizedAsync(strs.images_too_many);
                return;
            }

            // Pass attachemnt to service
            AthenaImagesService.ImageErrorCodes retCode = await _service.AddImage(ctx.Guild.Id, name, attachments.ElementAt(0));

            switch (retCode)
            {
                case AthenaImagesService.ImageErrorCodes.NotImage:
                    await ReplyErrorLocalizedAsync(strs.images_not_image);
                    return;
                case AthenaImagesService.ImageErrorCodes.UnsupportedFormat:
                    await ReplyErrorLocalizedAsync(strs.images_unsupported);
                    return;
                case AthenaImagesService.ImageErrorCodes.SaveFailed:
                    await ReplyErrorLocalizedAsync(strs.images_not_saved);
                    return;
                case AthenaImagesService.ImageErrorCodes.DownloadFailed:
                    await ReplyErrorLocalizedAsync(strs.images_not_downloaded);
                    return;
            }

            await ReplyConfirmLocalizedAsync(strs.images_added(name));
        }

        [Cmd]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPerm.Administrator)]
        public async Task ImageRemove(string name)
        {
            AthenaImagesService.ImageErrorCodes retCode = await _service.RemoveImage(ctx.Guild.Id, name);
            switch (retCode)
            {
                case AthenaImagesService.ImageErrorCodes.DeleteFailed:
                    await ReplyErrorLocalizedAsync(strs.images_not_deleted);
                    return;
            }

            await ReplyConfirmLocalizedAsync(strs.images_removed(name));
        }

        [Cmd]
        [RequireContext(ContextType.Guild)]
        public async Task ImageShow(string name)
        {
            try
            {
                AthenaImage image = await _service.FetchImage(ctx.Guild.Id, name);
                if (image is null)
                {
                    await ReplyErrorLocalizedAsync(strs.images_not_found(name));
                    return;
                }

                // Attempt to convert to image
                try
                {
                    MemoryStream stream = new MemoryStream();
                    stream.Write(image.Image, 0, image.Image.Length);

                    // Attempt to display image
                    await Context.Channel.SendFileAsync(stream, name + "." + image.Extension);
                }
                catch (Exception)
                {
                    await ReplyErrorLocalizedAsync(strs.images_unsupported);
                    return;
                }
            }
            catch
            {
                await ReplyErrorLocalizedAsync(strs.images_could_not_be_displayed(name));
            }
        }

        [Cmd]
        [RequireContext(ContextType.Guild)]
        public async Task ImageList()
        {
            List<string> imageNames = await _service.FetchImageList(ctx.Guild.Id);
            if (imageNames is null || imageNames.Count == 0)
            {
                await ReplyErrorLocalizedAsync(strs.images_none);
                return;
            }

            string textList = "";
            foreach (string imageName in imageNames)
            {
                textList += $"{imageName}\n\n";
            }

            var embed = new EmbedBuilder
            {
                Title = "Images",
                Color = Color.Green,
                Description = textList
            }.Build();

            await ReplyAsync("", false, embed);
        }

        [Cmd]
        [RequireContext(ContextType.Guild)]
        public async Task ImageRandom()
        {
            // First get all names
            List<string> imageNames = await _service.FetchImageList(ctx.Guild.Id);
            if (imageNames is null || imageNames.Count == 0)
            {
                await ReplyErrorLocalizedAsync(strs.images_none);
                return;
            }

            // Grab a random index to display
            int index = _random.Next(imageNames.Count);
            await ImageShow(imageNames[index]);
        }
    }
}