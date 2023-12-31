﻿#nullable disable
using System.Diagnostics.CodeAnalysis;

namespace AthenaBot.Common;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
[SuppressMessage("Style", "IDE0022:Use expression body for methods")]
public sealed class NoPublicBotAttribute : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckPermissionsAsync(
        ICommandContext context,
        CommandInfo command,
        IServiceProvider services)
    {
#if GLOBAL_ATHENA
        return Task.FromResult(PreconditionResult.FromError("Not available on the public bot. To learn how to selfhost a private bot, click [here](https://athenabot.readthedocs.io/en/latest/)."));
#else
        return Task.FromResult(PreconditionResult.FromSuccess());
#endif
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
[SuppressMessage("Style", "IDE0022:Use expression body for methods")]
public sealed class OnlyPublicBotAttribute : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckPermissionsAsync(
        ICommandContext context,
        CommandInfo command,
        IServiceProvider services)
    {
#if GLOBAL_ATHENA || DEBUG
        return Task.FromResult(PreconditionResult.FromSuccess());
#else
        return Task.FromResult(PreconditionResult.FromError("Only available on the public bot."));
#endif
    }
}