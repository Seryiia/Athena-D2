namespace AthenaBot;

/// <summary>
/// Represents essential interacation data
/// </summary>
/// <param name="Emote">Emote which will show on a button</param>
/// <param name="CustomId">Custom interaction id</param>
public record AthenaInteractionData(IEmote Emote, string CustomId, string? Text = null);