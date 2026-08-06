using AuthenticationService.DTO;
using AuthenticationService.Entities;
using AuthenticationService.Exceptions;
using AuthenticationService.Interface.RepositoryInterface;
using AuthenticationService.Interface.ServiceInterface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using RegisterSignupRequest = AuthenticationService.DTO.RegisterSignupRequest;
namespace AuthenticationService.ServiceImplementation
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthService> _logger;
        public AuthService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher, IJwtService jwtService, ILogger<AuthService> logger) 
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<ApiResponse> RegisterAsync(RegisterSignupRequest request)
        {
            _logger.LogInformation("User registration started for {Email}", request.Email);
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);

            if (existingUser != null)
            {
              _logger.LogWarning("User registration failed. Email {Email} already exists.", request.Email);
                throw new BadRequestException("Email already exists.");
            }

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = request.Password,

                Role = "User"
            };
            user.PasswordHash= _passwordHasher.HashPassword(user, request.Password);
            await _userRepository.AddAsync(user);

            await _userRepository.SaveChangesAsync();
            _logger.LogInformation("User {Email} registered successfully.", request.Email);
            return new ApiResponse
            {
                Success = true,
                Message = "User registered successfully."
            };
        }

        public async Task<ApiResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null)
            {
                throw new NotFoundException("User not found");
            }
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Failed login attempt for {Email}", request.Email);
                throw new UnauthorizedException("Invalid email or password.");
            }
            var token = _jwtService.GenerateToken(user);
            _logger.LogInformation("User {Email} logged in successfully.", request.Email);
            return new ApiResponse
            {
                Success = true,
                Message = "Login successful.",
                Data = new
                {
                    Token = token,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role
                }
            };
        }
    }
}
