using AthenaBot.Common.ModuleBehaviors;

namespace AthenaBot.Services;

public interface ICustomBehavior
    : IExecOnMessage,
        IInputTransformer,
        IExecPreCommand,
        IExecNoCommand,
        IExecPostCommand
{

}