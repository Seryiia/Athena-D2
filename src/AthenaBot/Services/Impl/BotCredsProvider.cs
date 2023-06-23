#nullable disable
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using AthenaBot.Common.Yml;
using Newtonsoft.Json;

namespace AthenaBot.Services;

public interface IBotCredsProvider
{
    public void Reload();
    public IBotCredentials GetCreds();
    public void ModifyCredsFile(Action<Creds> func);
}

public sealed class BotCredsProvider : IBotCredsProvider
{
    private const string CREDS_FILE_NAME = "creds.yml";
    private const string CREDS_EXAMPLE_FILE_NAME = "creds_example.yml";

    private string CredsPath { get; }

    private string CredsExamplePath { get; }

    private readonly int? _totalShards;


    private readonly Creds _creds = new();
    private readonly IConfigurationRoot _config;


    private readonly object _reloadLock = new();
    private readonly IDisposable _changeToken;

    public BotCredsProvider(int? totalShards = null, string credPath = null)
    {
        _totalShards = totalShards;

        if (!string.IsNullOrWhiteSpace(credPath))
        {
            CredsPath = credPath;
            CredsExamplePath = Path.Combine(Path.GetDirectoryName(credPath), CREDS_EXAMPLE_FILE_NAME);
        }
        else
        {
            CredsPath = Path.Combine(Directory.GetCurrentDirectory(), CREDS_FILE_NAME);
            CredsExamplePath = Path.Combine(Directory.GetCurrentDirectory(), CREDS_EXAMPLE_FILE_NAME);
        }

        try
        {
            if (!File.Exists(CredsExamplePath))
                File.WriteAllText(CredsExamplePath, Yaml.Serializer.Serialize(_creds));
        }
        catch
        {
            // this can fail in docker containers
        }


        if (!File.Exists(CredsPath))
        {
            Log.Warning(
                "{CredsPath} is missing. Attempting to load creds from environment variables prefixed with 'AthenaBot_'. Example is in {CredsExamplePath}",
                CredsPath,
                CredsExamplePath);
        }

        _config = new ConfigurationBuilder().AddYamlFile(CredsPath, false, true)
                                            .AddEnvironmentVariables("AthenaBot_")
                                            .Build();

        _changeToken = ChangeToken.OnChange(() => _config.GetReloadToken(), Reload);
        Reload();
    }

    public void Reload()
    {
        lock (_reloadLock)
        {
            _creds.OwnerIds.Clear();
            _config.Bind(_creds);

            if (string.IsNullOrWhiteSpace(_creds.Token))
            {
                Log.Error("Token is missing from creds.yml or Environment variables.\nAdd it and restart the program");
                Helpers.ReadErrorAndExit(5);
                return;
            }

            if (string.IsNullOrWhiteSpace(_creds.RestartCommand?.Cmd)
                || string.IsNullOrWhiteSpace(_creds.RestartCommand?.Args))
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    _creds.RestartCommand = new()
                    {
                        Args = "dotnet",
                        Cmd = "AthenaBot.dll -- {0}"
                    };
                }
                else
                {
                    _creds.RestartCommand = new()
                    {
                        Args = "AthenaBot.exe",
                        Cmd = "{0}"
                    };
                }
            }

            if (string.IsNullOrWhiteSpace(_creds.RedisOptions))
                _creds.RedisOptions = "127.0.0.1,syncTimeout=3000";

            _creds.TotalShards = _totalShards ?? _creds.TotalShards;
        }
    }

    public void ModifyCredsFile(Action<Creds> func)
    {
        var ymlData = File.ReadAllText(CREDS_FILE_NAME);
        var creds = Yaml.Deserializer.Deserialize<Creds>(ymlData);

        func(creds);

        ymlData = Yaml.Serializer.Serialize(creds);
        File.WriteAllText(CREDS_FILE_NAME, ymlData);
    }

    public IBotCredentials GetCreds()
    {
        lock (_reloadLock)
        {
            return _creds;
        }
    }
}