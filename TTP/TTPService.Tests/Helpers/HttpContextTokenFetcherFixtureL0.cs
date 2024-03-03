using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TTPService.Helpers;

namespace TTPService.Tests.Helpers
{
    [TestClass]
    public class HttpContextTokenFetcherFixtureL0
    {
        private const string Token = "tic token";
        private HttpContextTokenFetcher _sut;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private Mock<IAuthenticationService> _authenticationServiceMock;

        [TestInitialize]
        public void TestInitialize()
        {
            // TODO:[Team]<-[Golan] - keep the ugliness in one place, better than multi mock wrapping classes
            var items = new Dictionary<string, string>() { { ".Token.access_token", Token } };
            var authenticationProperties = new AuthenticationProperties(items);
            var identity = new ClaimsIdentity(new Claim[] { }, "test");
            var authenticationTicket = new AuthenticationTicket(new ClaimsPrincipal(identity), authenticationProperties, "Test");

            var serviceProviderMock = new Mock<IServiceProvider>();
            var httpContextMock = new Mock<HttpContext>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _authenticationServiceMock = new Mock<IAuthenticationService>();

            _authenticationServiceMock.Setup(s => s.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
                .Returns(Task.FromResult(AuthenticateResult.Success(authenticationTicket)));

            serviceProviderMock.Setup(s => s.GetService(It.IsAny<Type>()))
                .Returns(_authenticationServiceMock.Object);

            httpContextMock.Setup(s => s.TTPServices)
                .Returns(serviceProviderMock.Object);

            _httpContextAccessorMock.Setup(s => s.HttpContext)
                .Returns(httpContextMock.Object);

            _sut = new HttpContextTokenFetcher(_httpContextAccessorMock.Object);
        }

        [TestMethod]
        public async Task GetToken_HappyPath()
        {
            // Arrange
            // Act
            var token = await _sut.GetToken();

            // Assert
            token.IsSuccess.Should().BeTrue();
            token.Value.Should().Be(Token);
        }

        [TestMethod]
        public async Task GetToken_NullContext_Fail()
        {
            // Arrange
            _httpContextAccessorMock.Setup(s => s.HttpContext)
                .Returns(() => null);

            // Act
            var token = await _sut.GetToken();

            // Assert
            token.IsSuccess.Should().BeFalse();
        }

        [TestMethod]
        public async Task GetToken_TokenNotInContext_Fail()
        {
            // Arrange
            var authenticationTicket = new AuthenticationTicket(new ClaimsPrincipal(), new AuthenticationProperties(), string.Empty);
            _authenticationServiceMock.Setup(s => s.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
                .Returns(Task.FromResult(AuthenticateResult.Success(authenticationTicket)));

            // Act
            var token = await _sut.GetToken();

            // Assert
            token.IsSuccess.Should().BeFalse();
        }
    }
}