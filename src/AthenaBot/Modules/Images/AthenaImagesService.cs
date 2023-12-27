#nullable disable
using AthenaBot.Db.Models;
using LinqToDB.EntityFrameworkCore;
using LinqToDB;

namespace AthenaBot.Modules.AthenaImages;

public sealed class AthenaImagesService : INService
{
    private readonly DbService _db;

    public enum ImageErrorCodes
    {
        None = 0,
        NotImage = 1,
        UnsupportedFormat = 2,
        SaveFailed = 3,
        DownloadFailed = 4,
        DeleteFailed = 5,
        ImageNotFound = 6,
        ImageNameAlreadyTaken = 7
    }

    public AthenaImagesService(DbService db)
    {
        _db = db;
    }

    public async Task<ImageErrorCodes> AddImage(ulong guildId, string name, IAttachment attachment)
    {
        // First we have to verify the attachment is an image
        if(!attachment.ContentType.Contains("image"))
        {
            return ImageErrorCodes.NotImage;
        }

        if(attachment.ContentType is not "image/jpeg" and not "image/png" and not "image/gif")
        {
            return ImageErrorCodes.UnsupportedFormat;
        }

        string type = attachment.ContentType.Replace("image/", "");
        if (type == "jpeg")
            type = "jpg";

        byte[] binaryData;

        // Download attachment
        try
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(attachment.Url);
            binaryData = await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            Log.Warning($"Failed to fetch attachment: {ex.Message}");
            return ImageErrorCodes.DownloadFailed;
        }

        // Save to database
        try
        {
            await using (var ctx = _db.GetDbContext())
            {
                // First verify the name wasn't taken
                var count = ctx.AthenaImages
                    .Where(x => x.Name == name)
                    .Select(x => x.Id).Count();

                if(count != 0)
                {
                    Log.Warning($"Tried to add image with duplicate name: {name}");
                    return ImageErrorCodes.ImageNameAlreadyTaken;
                }

                ctx.AthenaImages.Add(new()
                {
                    GuildId = guildId,
                    Name = name,
                    Extension = type,
                    Image = binaryData
                });
                ;
                await ctx.SaveChangesAsync();
            }
        }
        catch(Exception ex)
        {
            Log.Warning($"Image save failed with error: {ex.Message}");
            return ImageErrorCodes.SaveFailed;
        }

        return ImageErrorCodes.None;
    }

    public async Task<ImageErrorCodes> RemoveImage(ulong guildId, string name)
    {
        // Delete from database
        try
        {
            await using (var ctx = _db.GetDbContext())
            {
                await ctx.GetTable<AthenaImage>()
                    .Where(x => x.GuildId == guildId && x.Name == name)
                    .DeleteAsync();
            }
        }
        catch (Exception ex)
        {
            Log.Warning($"Image delete failed with error: {ex.Message}");
            return ImageErrorCodes.DeleteFailed;
        }

        return ImageErrorCodes.None;
    }

    public async Task<AthenaImage> FetchImage(ulong guildId, string name)
    {
        // Fetch from database
        try
        {
            await using (var ctx = _db.GetDbContext())
            {
                return ctx.AthenaImages
                    .Where(x => x.GuildId == guildId && x.Name == name).First();
            }
        }
        catch (Exception ex)
        {
            Log.Warning($"Image fetch failed with error: {ex.Message}");
            return null;
        }
    }

    public async Task<List<string>> FetchImageList(ulong guildId)
    {
        // Fetch from database
        try
        {
            await using (var ctx = _db.GetDbContext())
            {
                return ctx.AthenaImages
                    .Where(x => x.GuildId == guildId)
                    .Select(x => x.Name).ToList();
            }
        }
        catch (Exception ex)
        {
            Log.Warning($"Image list fetch failed with error: {ex.Message}");
            return null;
        }
    }
}