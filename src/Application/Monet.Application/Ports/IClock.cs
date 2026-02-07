namespace Monet.Application.Ports;

public interface IClock
{
    DateTimeOffset Now { get; }
}