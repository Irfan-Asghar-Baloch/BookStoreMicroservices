using AuthenticationService.DTO;
using Microsoft.AspNetCore.Identity.Data;
using RegisterSignupRequest = AuthenticationService.DTO.RegisterSignupRequest;

namespace AuthenticationService.Interface.ServiceInterface
{
    public interface IAuthService
    {
        Task<ApiResponse> RegisterAsync(RegisterSignupRequest request);
        Task<ApiResponse> LoginAsync(LoginRequest request);
    }
}
