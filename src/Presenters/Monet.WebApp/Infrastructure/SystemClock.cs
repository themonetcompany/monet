using Monet.Application.Ports;

namespace Monet.WebApp.Infrastructure;

public class SystemClock : IClock
{
    public DateTimeOffset Now => DateTimeOffset.UtcNow;
}
