using AuthenticationService.Entities;

namespace AuthenticationService.Interface.ServiceInterface
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
