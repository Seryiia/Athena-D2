﻿#nullable disable

namespace AthenaBot.Common.TypeReaders;

[MeansImplicitUse(ImplicitUseTargetFlags.Default | ImplicitUseTargetFlags.WithInheritors)]
public abstract class AthenaTypeReader<T> : TypeReader
{
    public abstract ValueTask<TypeReaderResult<T>> ReadAsync(ICommandContext ctx, string input);

    public override async Task<Discord.Commands.TypeReaderResult> ReadAsync(
        ICommandContext ctx,
        string input,
        IServiceProvider services)
        => await ReadAsync(ctx, input);
}