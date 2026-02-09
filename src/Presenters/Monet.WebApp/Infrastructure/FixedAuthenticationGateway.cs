using Monet.Application.Ports;
using Monet.Domain;
using Monet.Domain.Shared;

namespace Monet.WebApp.Infrastructure;

public class FixedAuthenticationGateway : IAuthenticationGateway
{
    private static readonly ConnectedUser DemoUser = new()
    {
        Id = Guid.Parse("6f798809-b7c1-4d3f-a21b-e1d3390a0b2e"),
        Email = "import@monet.local",
    };

    public Maybe<ConnectedUser> GetConnectedUser()
    {
        return Maybe<ConnectedUser>.Of(DemoUser);
    }
}
