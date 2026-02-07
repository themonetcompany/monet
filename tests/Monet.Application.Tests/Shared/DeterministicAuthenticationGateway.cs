using Monet.Application.Ports;
using Monet.Domain;
using Monet.Domain.Shared;

namespace Monet.Application.Tests.Shared;

public class DeterministicAuthenticationGateway : IAuthenticationGateway
{
    private ConnectedUser? _connectedUser;

    public void ConnectedUser(ConnectedUser connectedUser)
    {
        _connectedUser = connectedUser;
    }

    public Option<ConnectedUser> GetConnectedUser()
    {
        return Option<ConnectedUser>.Of(_connectedUser);
    }
}