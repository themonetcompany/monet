using Monet.Domain;
using Monet.Domain.Shared;

namespace Monet.Application.Ports;

public interface IAuthenticationGateway
{
    Option<ConnectedUser> GetConnectedUser();
}