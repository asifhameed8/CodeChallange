using CodeChallange.Models;
using Microsoft.AspNetCore.Mvc;
using CodeChallange.Context;
using CodeChallange.Helpers;
using Microsoft.AspNetCore.Authorization;
using CodeChallange.Models.Dto;
using CodeChallange.Services;

namespace CodeChallange.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IAuthService _authService;

        public UserController(AppDbContext context, IAuthService service)
        {
            _authService = service;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] User userObj)
        {
            return ReturnFormattedResponse(await _authService.Authenticate(userObj));
        }

        [HttpPost("register")]
        public async Task<IActionResult> AddUser([FromBody] User userObj)
        {
            var result = await _authService.AddUser(userObj);
            if (result.Success)
            {
                return Ok(new
                {
                    Status = 200,
                    Message = "User Added!"
                });
            }
            return BadRequest(new { Message = string.Join(',', result.Errors) });
        }
        [Authorize]
        [HttpGet]
        public async Task<List<User>> GetAllUsers()
        {
            return await _authService.GetAllUsers();
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenApiDto tokenApiDto)
        {
            return ReturnFormattedResponse(await _authService.Refresh(tokenApiDto));
        }

        public IActionResult ReturnFormattedResponse<T>(ServiceResponse<T> response)
        {
            if (response.Success)
            {
                return Ok(response.Data);
            }
            return BadRequest(new { Message = string.Join(',', response.Errors) });
        }
    }

}
