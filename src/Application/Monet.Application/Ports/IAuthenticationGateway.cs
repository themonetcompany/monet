using Monet.Domain;
using Monet.Domain.Shared;

namespace Monet.Application.Ports;

public interface IAuthenticationGateway
{
    Maybe<ConnectedUser> GetConnectedUser();
}