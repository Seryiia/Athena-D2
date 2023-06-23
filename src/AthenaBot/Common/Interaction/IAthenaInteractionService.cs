namespace AthenaBot;

public interface IAthenaInteractionService
{
    public AthenaInteraction Create<T>(
        ulong userId,
        SimpleInteraction<T> inter);
}