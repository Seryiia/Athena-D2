// global using System.Collections.Concurrent;
global using NonBlocking;

// packages
global using Serilog;
global using Humanizer;

// athenabot
global using AthenaBot;
global using AthenaBot.Services;
global using Athena.Common; // new project
global using AthenaBot.Common; // old + athenabot specific things
global using AthenaBot.Common.Attributes;
global using AthenaBot.Extensions;

// discord
global using Discord;
global using Discord.Commands;
global using Discord.Net;
global using Discord.WebSocket;

// aliases
global using GuildPerm = Discord.GuildPermission;
global using ChannelPerm = Discord.ChannelPermission;
global using BotPermAttribute = Discord.Commands.RequireBotPermissionAttribute;
global using LeftoverAttribute = Discord.Commands.RemainderAttribute;

// non-essential
global using JetBrains.Annotations;