using HackerNews.Api.Controllers;
using HackerNews.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HackerNews.Api.Library.Tests.IntegrationTests;

public class AuthControllerTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly AuthController _authController;

    // Test constants for JWT settings
    private const string TestSecretKey = "ThisIsMySuperSecretKeyForHackerNewsApi12345";
    private const string TestIssuer = "https://nextech.com/jasonanderson/hackernews/issuer";
    private const string TestAudience = "https://nextech.com/jasonanderson/hackernews/audience";

    public AuthControllerTests()
    {
        // --- Arrange: Mock IConfiguration ---
        _mockConfiguration = new Mock<IConfiguration>();

        // Mock the "Jwt" section
        var mockJwtSection = new Mock<IConfigurationSection>();
        mockJwtSection.Setup(x => x["Key"]).Returns(TestSecretKey);
        mockJwtSection.Setup(x => x["Issuer"]).Returns(TestIssuer);
        mockJwtSection.Setup(x => x["Audience"]).Returns(TestAudience);

        _mockConfiguration.Setup(x => x.GetSection("Jwt")).Returns(mockJwtSection.Object);

        // Instantiate the controller with the mocked dependency
        _authController = new AuthController(_mockConfiguration.Object);
    }

    [Fact]
    public void Login_WithValidCredentials_ReturnsOkResultWithJwtToken()
    {
        // Arrange
        var loginRequest = new LoginRequest("testuser", "Password123!");

        // Act
        var result = _authController.Login(loginRequest);

        // Assert
        // 1. Check if the result is a 200 OK
        var okResult = Assert.IsType<OkObjectResult>(result);

        // 2. Check if the value of the result is a LoginResponse
        var loginResponse = Assert.IsType<LoginResponse>(okResult.Value);

        // 3. Check that the token is not null or empty
        Assert.False(string.IsNullOrWhiteSpace(loginResponse.Token));

        // 4. (Optional but good practice) Validate the token's contents
        var handler = new JwtSecurityTokenHandler();
        var decodedToken = handler.ReadJwtToken(loginResponse.Token);

        Assert.Equal(TestIssuer, decodedToken.Issuer);
        Assert.Equal(TestAudience, decodedToken.Audiences.First());

        var usernameClaim = decodedToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
        Assert.NotNull(usernameClaim);
        Assert.Equal(loginRequest.Username, usernameClaim.Value);
    }

    [Theory]
    [InlineData("wronguser", "Password123!")]
    [InlineData("testuser", "wrongpassword")]
    [InlineData("", "")]
    [InlineData(null, null)]
    [InlineData(null, "")]
    [InlineData("", null)]
    [InlineData("testuser", null)]
    [InlineData(null, "Password123!")]
    public void Login_WithInvalidCredentials_ReturnsUnauthorizedResult(string username, string password)
    {
        // Arrange
        var loginRequest = new LoginRequest(username, password);

        // Act
        var result = _authController.Login(loginRequest);

        // Assert
        // 1. Check if the result is a 401 Unauthorized
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);

        // 2. Check the error message
        Assert.Equal("Invalid credentials.", unauthorizedResult.Value);
    }

    [Fact]
    public void Login_WhenJwtKeyIsNotConfigured_ThrowsInvalidOperationException()
    {
        // Arrange
        // Create a new mock configuration that returns a null key
        var faultyMockConfig = new Mock<IConfiguration>();
        var faultyJwtSection = new Mock<IConfigurationSection>();
        faultyJwtSection.Setup(x => x["Key"]).Returns((string?)null); // Return null for the key
        faultyMockConfig.Setup(x => x.GetSection("Jwt")).Returns(faultyJwtSection.Object);

        var faultyController = new AuthController(faultyMockConfig.Object);
        var loginRequest = new LoginRequest("testuser", "Password123!");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => faultyController.Login(loginRequest));
        Assert.Equal("JWT Key is not configured.", exception.Message);
    }
}
