#nullable disable
using AthenaBot;

namespace AthenaBot.Modules.StreamNotification.Exceptions;

public class StreamNotFoundException : Exception
{
    public StreamNotFoundException()
    {
    }

    public StreamNotFoundException(string message)
        : base(message)
    {
    }

    public StreamNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}