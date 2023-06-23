using System.Runtime.CompilerServices;

namespace AthenaBot.Common.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class AliasesAttribute : AliasAttribute
{
    public AliasesAttribute([CallerMemberName] string memberName = "")
        : base(CommandNameLoadHelper.GetAliasesFor(memberName))
    {
    }
}