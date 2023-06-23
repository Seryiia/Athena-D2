namespace AthenaBot;

public class AthenaInteractionService : IAthenaInteractionService, INService
{
    private readonly DiscordSocketClient _client;

    public AthenaInteractionService(DiscordSocketClient client)
    {
        _client = client;
    }

    public AthenaInteraction Create<T>(
        ulong userId,
        SimpleInteraction<T> inter)
        => new AthenaInteraction(_client,
            userId,
            inter.Button,
            inter.TriggerAsync,
            onlyAuthor: true);
}