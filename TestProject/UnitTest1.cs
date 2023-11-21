using System.Threading.Tasks;
using CodeChallange.Controllers;
using CodeChallange.Models;
using CodeChallange.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Moq;
using Xunit;

public class UnitTest1
{
    [Fact]
    public async Task Authenticate_ReturnsOkObjectResult_WhenAuthenticationSucceeds()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        //mockAuthService.Setup(service => service.Authenticate(It.IsAny<User>()))
        //    .ReturnsAsync(new AuthenticationResult {});

        //var controller = new UserController(mockAuthService.Object);

        //// Act
        //var result = await controller.Authenticate(new User());

        //// Assert
        //Assert.IsType<OkObjectResult>(result);
    }
}