using CodeChallange.Helpers;
using CodeChallange.Models;
using CodeChallange.Models.Dto;

namespace CodeChallange.Services
{
    public interface IAuthService
    {
        Task<ServiceResponse<TokenApiDto>> Authenticate(User userObj);
        Task<ServiceResponse<string>> AddUser(User userObj);
        Task<List<User>> GetAllUsers();
        Task<ServiceResponse<TokenApiDto>> Refresh(TokenApiDto tokenApiDto);
    }
}
