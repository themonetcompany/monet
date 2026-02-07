using Monet.Application.Ports;

namespace Monet.Application.Tests.Shared;

public class DeterministicClock : IClock
{
    public DateTimeOffset Now { get; private set; } = DateTimeOffset.Now;

    public DeterministicClock SetDate(int year, int month, int day)
    {
        Now = new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero);
        return this;
    }
}