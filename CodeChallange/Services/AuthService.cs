using CodeChallange.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text;
using CodeChallange.Context;
using Microsoft.EntityFrameworkCore;
using CodeChallange.Models.Dto;
using CodeChallange.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace CodeChallange.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _authContext;
        public AuthService(AppDbContext context)
        {
            _authContext = context;
        }


        public async Task<ServiceResponse<TokenApiDto>> Authenticate(User userObj)
        {
            if (userObj == null) return ServiceResponse<TokenApiDto>.Return422("Invalid Data");
            var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Username == userObj.Username);
            if (user == null) return ServiceResponse<TokenApiDto>.Return422("User not found!"); 
            if (!PasswordHasher.VerifyPassword(userObj.Password, user.Password))
            {
                return ServiceResponse<TokenApiDto>.Return422("Password is Incorrect");
            }
            user.Token = CreateJwt(user);
            var newAccessToken = user.Token;
            var newRefreshToken = CreateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(5);
            await _authContext.SaveChangesAsync();

            var result  = new TokenApiDto()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
            return ServiceResponse<TokenApiDto>.ReturnResultWith200(result);
        }

        public async Task<ServiceResponse<string>> AddUser(User userObj)
        {
            if (userObj == null) return ServiceResponse<string>.Return422("Invalid Data");

            // check email
            if (await CheckEmailExistAsync(userObj.Email))
                return ServiceResponse<string>.Return422("Email Already Exist");

            //check username
            if (await CheckUsernameExistAsync(userObj.Username))
                return ServiceResponse<string>.Return422("Username Already Exist");


            var passMessage = CheckPasswordStrength(userObj.Password);
            if (!string.IsNullOrEmpty(passMessage))
                return ServiceResponse<string>.Return422(passMessage.ToString());

            userObj.Password = PasswordHasher.HashPassword(userObj.Password);
            userObj.Role = userObj.Role;
            userObj.Token = "";
            await _authContext.AddAsync(userObj);
            await _authContext.SaveChangesAsync();
            //return Ok(new
            //{
            //    Status = 200,
            //    Message = "User Added!"
            //});

            return ServiceResponse<string>.ReturnResultWith200("User Added!");
        }
        public async Task<List<User>> GetAllUsers()
        {
           return await _authContext.Users.ToListAsync();
        }
        public async Task<ServiceResponse<TokenApiDto>> Refresh(TokenApiDto tokenApiDto)
        {
            if (tokenApiDto is null)
                ServiceResponse<TokenApiDto>.Return422("Invalid Client Request");
            
            string accessToken = tokenApiDto.AccessToken;
            string refreshToken = tokenApiDto.RefreshToken;
            var principal = GetPrincipleFromExpiredToken(accessToken);
            var username = principal.Identity.Name;
            var user = await _authContext.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
                return ServiceResponse<TokenApiDto>.Return422("Invalid Request");

            var newAccessToken = CreateJwt(user);
            var newRefreshToken = CreateRefreshToken();
            user.RefreshToken = newRefreshToken;
            await _authContext.SaveChangesAsync();
            var result  = new TokenApiDto()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            };
            return ServiceResponse<TokenApiDto>.ReturnResultWith200(result);

        }


        #region Private Methods
        private Task<bool> CheckEmailExistAsync(string? email) => _authContext.Users.AnyAsync(x => x.Email == email);

        private Task<bool> CheckUsernameExistAsync(string? username)
            => _authContext.Users.AnyAsync(x => x.Email == username);

        private static string CheckPasswordStrength(string pass)
        {
            StringBuilder sb = new StringBuilder();
            if (pass.Length < 9)
                sb.Append("Minimum password length should be 8" + Environment.NewLine);
            if (!(Regex.IsMatch(pass, "[a-z]") && Regex.IsMatch(pass, "[A-Z]") && Regex.IsMatch(pass, "[0-9]")))
                sb.Append("Password should be AlphaNumeric" + Environment.NewLine);
            if (!Regex.IsMatch(pass, "[<,>,@,!,#,$,%,^,&,*,(,),_,+,\\[,\\],{,},?,:,;,|,',\\,.,/,~,`,-,=]"))
                sb.Append("Password should contain special charcter" + Environment.NewLine);
            return sb.ToString();
        }

        private string CreateJwt(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("Vz9agWrAMylOq3Al9UBKs42kYW6dENOttuhEhNYCSu6rmtX5xF4ntm_0dEclOy59");
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Name,$"{user.Username}")
            });

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.UtcNow.AddSeconds(100),
                SigningCredentials = credentials
            };
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }

        private string CreateRefreshToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var refreshToken = Convert.ToBase64String(tokenBytes);

            var tokenInUser = _authContext.Users
                .Any(a => a.RefreshToken == refreshToken);
            if (tokenInUser)
            {
                return CreateRefreshToken();
            }
            return refreshToken;
        }
        private ClaimsPrincipal GetPrincipleFromExpiredToken(string token)
        {
            var key = Encoding.ASCII.GetBytes("Vz9agWrAMylOq3Al9UBKs42kYW6dENOttuhEhNYCSu6rmtX5xF4ntm_0dEclOy59");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("This is Invalid Token");
            return principal;

        }

        #endregion

    }
}
