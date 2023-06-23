namespace AthenaBot.Common.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class AthenaOptionsAttribute : Attribute
{
    public Type OptionType { get; set; }

    public AthenaOptionsAttribute(Type t)
        => OptionType = t;
}