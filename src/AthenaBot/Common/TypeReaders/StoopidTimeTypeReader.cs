#nullable disable
using AthenaBot.Common.TypeReaders.Models;

namespace AthenaBot.Common.TypeReaders;

public sealed class StoopidTimeTypeReader : AthenaTypeReader<StoopidTime>
{
    public override ValueTask<TypeReaderResult<StoopidTime>> ReadAsync(ICommandContext context, string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new(TypeReaderResult.FromError<StoopidTime>(CommandError.Unsuccessful, "Input is empty."));
        try
        {
            var time = StoopidTime.FromInput(input);
            return new(TypeReaderResult.FromSuccess(time));
        }
        catch (Exception ex)
        {
            return new(TypeReaderResult.FromError<StoopidTime>(CommandError.Exception, ex.Message));
        }
    }
}