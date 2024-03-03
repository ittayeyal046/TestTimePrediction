using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TTPService.Configuration;
using TTPService.Dtos.Notification;
using TTPService.Dtos.Orchestrator.ExperimentGroupCreationDtos;
using TTPService.Dtos.Orchestrator.ExperimentGroupUpdateDtos;
using TTPService.Helpers;
using TTPService.Services;
using RestSharp;

namespace TTPService.Tests.Services
{
    [TestClass]
    public class NotificationApiFixtureL0
    {
        private NotificationApi _sut;
        private Mock<INotificationClient> _clientMock;
        private ExperimentProgressNotificationDto _experimentProgressUpdatedDto;
        private CriticalEventDto _criticalEventDto;
        private Mock<IHttpContextTokenFetcher> _httpContextTokenFetcherMock;

        [TestInitialize]
        public void TestInitialize()
        {
            const string token = "token-for-all";
            var optionsMock = new Mock<IOptions<RetryOptions>>();
            _clientMock = new Mock<INotificationClient>();
            _httpContextTokenFetcherMock = new Mock<IHttpContextTokenFetcher>();

            // TODO:[Team]<-[Golan] - uncomment when authentication is enabled
            // _clientMock.Setup(s => s.AddJwtAuthenticator(token))
            //    .Returns(_clientMock.Object);
            _httpContextTokenFetcherMock.Setup(s => s.GetToken())
                .ReturnsAsync(Result.Ok(token));

            _experimentProgressUpdatedDto = It.IsAny<ExperimentProgressNotificationDto>();
            _criticalEventDto = It.IsAny<CriticalEventDto>();

            optionsMock.Setup(s => s.Value).Returns(() => new RetryOptions() { DelayInSeconds = 0, NumberOfRetries = 2 });

            _sut = new NotificationApi(_httpContextTokenFetcherMock.Object, Mock.Of<ILogger<NotificationApi>>(), _clientMock.Object, optionsMock.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _clientMock.VerifyAll();

            // TODO:[Team]<-[Golan] - uncomment when authentication is enabled
            // _httpContextTokenFetcherMock.VerifyAll();
        }

        [TestMethod]
        public async Task NotifyExperimentUpdated_HappyFlow()
        {
            // Arrange
            var responseMock = new Mock<IRestResponse>();
            responseMock.Setup(s => s.StatusCode).Returns(HttpStatusCode.Created);
            responseMock.Setup(s => s.IsSuccessful).Returns(true);

            _clientMock.Setup(s => s.NotifyExperimentUpdated(_experimentProgressUpdatedDto))
              .Returns(Task.FromResult(responseMock.Object));

            // Act
            var actual = await _sut.NotifyExperimentUpdated(_experimentProgressUpdatedDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
        }

        [Ignore("TODO:[Team]<-[Golan] - uncomment when authentication is enabled")]
        [TestMethod]
        public async Task NotifyExperimentUpdated_FailToken_Fail()
        {
            // Arrange
            const string expectedError = "bla-bla";
            var responseMock = new Mock<IRestResponse>();
            responseMock.Setup(s => s.StatusCode).Returns(HttpStatusCode.Created);
            responseMock.Setup(s => s.IsSuccessful).Returns(true);

            _clientMock.Reset();
            _httpContextTokenFetcherMock.Setup(s => s.GetToken())
              .ReturnsAsync(Result.Fail<string>(expectedError));

            // Act
            var actual = await _sut.NotifyExperimentUpdated(_experimentProgressUpdatedDto);

            // Assert
            actual.IsSuccess.Should().BeFalse();
            actual.Error.Should().Be(expectedError);
        }

        [TestMethod]
        public async Task NotifyExperimentUpdated_NotRetryableStatusCode_NoRetry()
        {
            // Arrange
            var responseMock = new Mock<IRestResponse>();
            responseMock.Setup(s => s.StatusCode).Returns(HttpStatusCode.UnprocessableEntity);
            responseMock.Setup(s => s.IsSuccessful).Returns(false);

            _clientMock.Setup(s => s.NotifyExperimentUpdated(_experimentProgressUpdatedDto))
              .Returns(Task.FromResult(responseMock.Object));

            // Act
            var actual = await _sut.NotifyExperimentUpdated(_experimentProgressUpdatedDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.Should().Contain(HttpStatusCode.UnprocessableEntity.ToString());
            _clientMock.Verify(v => v.NotifyExperimentUpdated(_experimentProgressUpdatedDto), Times.Once);
            responseMock.VerifyAll();
        }

        [DataRow(HttpStatusCode.ServiceUnavailable)]
        [DataRow(HttpStatusCode.TooManyRequests)]
        [DataRow(HttpStatusCode.NotFound)]
        [TestMethod]
        public async Task NotifyExperimentUpdated_RetryableStatusCode_Success(HttpStatusCode httpStatusCode)
        {
            // Arrange
            var failResponseMock = new Mock<IRestResponse>();
            failResponseMock.Setup(s => s.StatusCode).Returns(httpStatusCode);

            var successResponseMock = new Mock<IRestResponse>();
            successResponseMock.Setup(s => s.StatusCode).Returns(HttpStatusCode.Created);
            successResponseMock.Setup(s => s.IsSuccessful).Returns(true);

            _clientMock.SetupSequence(s => s.NotifyExperimentUpdated(_experimentProgressUpdatedDto))
              .Returns(Task.FromResult(failResponseMock.Object))
              .Returns(Task.FromResult(successResponseMock.Object));

            // Act
            var actual = await _sut.NotifyExperimentUpdated(_experimentProgressUpdatedDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            _clientMock.Verify(x => x.NotifyExperimentUpdated(_experimentProgressUpdatedDto), Times.Exactly(2));
            failResponseMock.VerifyAll();
            successResponseMock.VerifyAll();
        }

        [TestMethod]
        public async Task NotifyExperimentUpdated_RetryableStatusCode_Fail()
        {
            // Arrange
            var failResponseMock = new Mock<IRestResponse>();
            failResponseMock.Setup(s => s.StatusCode).Returns(HttpStatusCode.ServiceUnavailable);

            _clientMock.SetupSequence(s => s.NotifyExperimentUpdated(_experimentProgressUpdatedDto))
              .Returns(Task.FromResult(failResponseMock.Object))
              .Returns(Task.FromResult(failResponseMock.Object));

            // Act
            var actual = await _sut.NotifyExperimentUpdated(_experimentProgressUpdatedDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.Should().NotBeNullOrWhiteSpace();
            _clientMock.Verify(x => x.NotifyExperimentUpdated(_experimentProgressUpdatedDto), Times.Exactly(3));
            failResponseMock.VerifyAll();
        }

        [TestMethod]
        public async Task NotifyCriticalEvent_HappyFlow()
        {
            // Arrange
            var responseMock = new Mock<IRestResponse>();
            responseMock.Setup(s => s.StatusCode).Returns(HttpStatusCode.Created);
            responseMock.Setup(s => s.IsSuccessful).Returns(true);

            _clientMock.Setup(s => s.NotifyCriticalEvent(_criticalEventDto))
              .Returns(Task.FromResult(responseMock.Object));

            // Act
            var actual = await _sut.NotifyCriticalEvent(_criticalEventDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
        }

        [TestMethod]
        public async Task NotifyCriticalEvent_NotRetryableStatusCode_NoRetry()
        {
            // Arrange
            var responseMock = new Mock<IRestResponse>();
            responseMock.Setup(s => s.StatusCode).Returns(HttpStatusCode.UnprocessableEntity);
            responseMock.Setup(s => s.IsSuccessful).Returns(false);

            _clientMock.Setup(s => s.NotifyCriticalEvent(_criticalEventDto))
              .Returns(Task.FromResult(responseMock.Object));

            // Act
            var actual = await _sut.NotifyCriticalEvent(_criticalEventDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.Should().Contain(HttpStatusCode.UnprocessableEntity.ToString());
            _clientMock.Verify(v => v.NotifyCriticalEvent(_criticalEventDto), Times.Once);
            responseMock.VerifyAll();
        }

        [Ignore("TODO:[Team]<-[Golan] - uncomment when authentication is enabled")]
        [TestMethod]
        public async Task NotifyCriticalEvent_FailToken_Fail()
        {
            // Arrange
            const string expectedError = "bla-bla";
            var responseMock = new Mock<IRestResponse>();
            responseMock.Setup(s => s.StatusCode).Returns(HttpStatusCode.Created);
            responseMock.Setup(s => s.IsSuccessful).Returns(true);

            _clientMock.Reset();
            _httpContextTokenFetcherMock.Setup(s => s.GetToken())
              .ReturnsAsync(Result.Fail<string>(expectedError));

            // Act
            var actual = await _sut.NotifyCriticalEvent(_criticalEventDto);

            // Assert
            actual.IsSuccess.Should().BeFalse();
            actual.Error.Should().Be(expectedError);
        }

        [DataRow(HttpStatusCode.ServiceUnavailable)]
        [DataRow(HttpStatusCode.TooManyRequests)]
        [DataRow(HttpStatusCode.NotFound)]
        [TestMethod]
        public async Task NotifyCriticalEvent_RetryableStatusCode_Success(HttpStatusCode httpStatusCode)
        {
            // Arrange
            var failResponseMock = new Mock<IRestResponse>();
            failResponseMock.Setup(s => s.StatusCode).Returns(httpStatusCode);

            var successResponseMock = new Mock<IRestResponse>();
            successResponseMock.Setup(s => s.StatusCode).Returns(HttpStatusCode.Created);
            successResponseMock.Setup(s => s.IsSuccessful).Returns(true);

            _clientMock.SetupSequence(s => s.NotifyCriticalEvent(_criticalEventDto))
              .Returns(Task.FromResult(failResponseMock.Object))
              .Returns(Task.FromResult(successResponseMock.Object));

            // Act
            var actual = await _sut.NotifyCriticalEvent(_criticalEventDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            _clientMock.Verify(x => x.NotifyCriticalEvent(_criticalEventDto), Times.Exactly(2));
            failResponseMock.VerifyAll();
            successResponseMock.VerifyAll();
        }

        [TestMethod]
        public async Task NotifyCriticalEvent_RetryableStatusCode_Fail()
        {
            // Arrange
            var failResponseMock = new Mock<IRestResponse>();
            failResponseMock.Setup(s => s.StatusCode).Returns(HttpStatusCode.ServiceUnavailable);

            _clientMock.SetupSequence(s => s.NotifyCriticalEvent(_criticalEventDto))
              .Returns(Task.FromResult(failResponseMock.Object))
              .Returns(Task.FromResult(failResponseMock.Object));

            // Act
            var actual = await _sut.NotifyCriticalEvent(_criticalEventDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.Should().NotBeNullOrWhiteSpace();
            _clientMock.Verify(x => x.NotifyCriticalEvent(_criticalEventDto), Times.Exactly(3));
            failResponseMock.VerifyAll();
        }
    }
}
