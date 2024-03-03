using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TTPService.Configuration;
using TTPService.Dtos.Orchestrator.ExperimentGroupCreationDtos;
using TTPService.Dtos.Orchestrator.ExperimentGroupUpdateDtos;
using TTPService.Helpers;
using TTPService.Services;
using RestSharp;

namespace TTPService.Tests.Services
{
    [TestClass]
    public class OrchestratorApiFixtureL0
    {
        private OrchestratorApi _sut;
        private Mock<IOrchestratorClient> _clientMock;
        private ExperimentGroupCreationDto _experimentGroupCreationDto;
        private ExperimentGroupForUpdateDto _experimentGroupForUpdateDto;
        private Mock<IHttpContextTokenFetcher> _httpContextTokenFetcherMock;

        [TestInitialize]
        public void TestInitialize()
        {
            const string token = "token-for-all";
            var optionsMock = new Mock<IOptions<RetryOptions>>();
            _clientMock = new Mock<IOrchestratorClient>();
            _httpContextTokenFetcherMock = new Mock<IHttpContextTokenFetcher>();

            // TODO:[Team]<-[Golan] - uncomment when authentication is enabled
            // _clientMock.Setup(s => s.AddJwtAuthenticator(token))
            //    .Returns(_clientMock.Object);
            _httpContextTokenFetcherMock.Setup(s => s.GetToken())
                .ReturnsAsync(Result.Ok(token));

            _experimentGroupCreationDto = It.IsAny<ExperimentGroupCreationDto>();
            _experimentGroupForUpdateDto = It.IsAny<ExperimentGroupForUpdateDto>();

            optionsMock.Setup(s => s.Value).Returns(() => new RetryOptions() { DelayInSeconds = 0, NumberOfRetries = 2 });

            _sut = new OrchestratorApi(_httpContextTokenFetcherMock.Object, Mock.Of<ILogger<OrchestratorApi>>(), _clientMock.Object, optionsMock.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _clientMock.VerifyAll();

            // TODO:[Team]<-[Golan] - uncomment when authentication is enabled
            // _httpContextTokenFetcherMock.VerifyAll();
        }

        [TestMethod]
        public async Task CreateExperimentGroup_HappyFlow()
        {
            // Arrange
            var responseMock = new Mock<IRestResponse>();
            responseMock.Setup(s => s.StatusCode).Returns(HttpStatusCode.Created);
            responseMock.Setup(s => s.IsSuccessful).Returns(true);

            _clientMock.Setup(s => s.CreateExperimentGroup(_experimentGroupCreationDto))
                .Returns(Task.FromResult(responseMock.Object));

            // Act
            var actual = await _sut.CreateExperimentGroup(_experimentGroupCreationDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
        }

        [Ignore("TODO:[Team]<-[Golan] - uncomment when authentication is enabled")]
        [TestMethod]
        public async Task CreateExperimentGroup_FailToken_Fail()
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
            var actual = await _sut.CreateExperimentGroup(_experimentGroupCreationDto);

            // Assert
            actual.IsSuccess.Should().BeFalse();
            actual.Error.Should().Be(expectedError);
        }

        [TestMethod]
        public async Task CreateExperimentGroup_NotRetryableStatusCode_NoRetry()
        {
            // Arrange
            var responseMock = new Mock<IRestResponse>();
            responseMock.Setup(s => s.StatusCode).Returns(HttpStatusCode.UnprocessableEntity);
            responseMock.Setup(s => s.IsSuccessful).Returns(false);

            _clientMock.Setup(s => s.CreateExperimentGroup(_experimentGroupCreationDto))
                .Returns(Task.FromResult(responseMock.Object));

            // Act
            var actual = await _sut.CreateExperimentGroup(_experimentGroupCreationDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.Should().Contain(HttpStatusCode.UnprocessableEntity.ToString());
            _clientMock.Verify(v => v.CreateExperimentGroup(_experimentGroupCreationDto), Times.Once);
            responseMock.VerifyAll();
        }

        [DataRow(HttpStatusCode.ServiceUnavailable)]
        [DataRow(HttpStatusCode.TooManyRequests)]
        [DataRow(HttpStatusCode.NotFound)]
        [TestMethod]
        public async Task CreateExperimentGroup_RetryableStatusCode_Success(HttpStatusCode httpStatusCode)
        {
            // Arrange
            var failResponseMock = new Mock<IRestResponse>();
            failResponseMock.Setup(s => s.StatusCode).Returns(httpStatusCode);

            var successResponseMock = new Mock<IRestResponse>();
            successResponseMock.Setup(s => s.StatusCode).Returns(HttpStatusCode.Created);
            successResponseMock.Setup(s => s.IsSuccessful).Returns(true);

            _clientMock.SetupSequence(s => s.CreateExperimentGroup(_experimentGroupCreationDto))
                .Returns(Task.FromResult(failResponseMock.Object))
                .Returns(Task.FromResult(successResponseMock.Object));

            // Act
            var actual = await _sut.CreateExperimentGroup(_experimentGroupCreationDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            _clientMock.Verify(x => x.CreateExperimentGroup(_experimentGroupCreationDto), Times.Exactly(2));
            failResponseMock.VerifyAll();
            successResponseMock.VerifyAll();
        }

        [TestMethod]
        public async Task CreateExperimentGroup_RetryableStatusCode_Fail()
        {
            // Arrange
            var failResponseMock = new Mock<IRestResponse>();
            failResponseMock.Setup(s => s.StatusCode).Returns(HttpStatusCode.ServiceUnavailable);

            _clientMock.SetupSequence(s => s.CreateExperimentGroup(_experimentGroupCreationDto))
                .Returns(Task.FromResult(failResponseMock.Object))
                .Returns(Task.FromResult(failResponseMock.Object));

            // Act
            var actual = await _sut.CreateExperimentGroup(_experimentGroupCreationDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.Should().NotBeNullOrWhiteSpace();
            _clientMock.Verify(x => x.CreateExperimentGroup(_experimentGroupCreationDto), Times.Exactly(3));
            failResponseMock.VerifyAll();
        }

        [TestMethod]
        public async Task UpdateExperimentGroup_HappyFlow()
        {
            // Arrange
            var responseMock = new Mock<IRestResponse>();
            responseMock.Setup(s => s.StatusCode).Returns(HttpStatusCode.Created);
            responseMock.Setup(s => s.IsSuccessful).Returns(true);

            _clientMock.Setup(s => s.UpdateExperimentGroup(_experimentGroupForUpdateDto))
                .Returns(Task.FromResult(responseMock.Object));

            // Act
            var actual = await _sut.UpdateExperimentGroup(_experimentGroupForUpdateDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
        }

        [Ignore("TODO:[Team]<-[Golan] - uncomment when authentication is enabled")]
        [TestMethod]
        public async Task UpdateExperimentGroup_FailToken_Fail()
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
            var actual = await _sut.UpdateExperimentGroup(_experimentGroupForUpdateDto);

            // Assert
            actual.IsSuccess.Should().BeFalse();
            actual.Error.Should().Be(expectedError);
        }

        [TestMethod]
        public async Task UpdateExperimentGroup_NotRetryableStatusCode_NoRetry()
        {
            // Arrange
            var responseMock = new Mock<IRestResponse>();
            responseMock.Setup(s => s.StatusCode).Returns(HttpStatusCode.UnprocessableEntity);
            responseMock.Setup(s => s.IsSuccessful).Returns(false);

            _clientMock.Setup(s => s.UpdateExperimentGroup(_experimentGroupForUpdateDto))
                .Returns(Task.FromResult(responseMock.Object));

            // Act
            var actual = await _sut.UpdateExperimentGroup(_experimentGroupForUpdateDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.Should().Contain(HttpStatusCode.UnprocessableEntity.ToString());
            _clientMock.Verify(v => v.UpdateExperimentGroup(_experimentGroupForUpdateDto), Times.Once);
            responseMock.VerifyAll();
        }

        [DataRow(HttpStatusCode.ServiceUnavailable)]
        [DataRow(HttpStatusCode.TooManyRequests)]
        [DataRow(HttpStatusCode.NotFound)]
        [TestMethod]
        public async Task UpdateExperimentGroup_RetryableStatusCode_Success(HttpStatusCode httpStatusCode)
        {
            // Arrange
            var failResponseMock = new Mock<IRestResponse>();
            failResponseMock.Setup(s => s.StatusCode).Returns(httpStatusCode);

            var successResponseMock = new Mock<IRestResponse>();
            successResponseMock.Setup(s => s.StatusCode).Returns(HttpStatusCode.Created);
            successResponseMock.Setup(s => s.IsSuccessful).Returns(true);

            _clientMock.SetupSequence(s => s.UpdateExperimentGroup(_experimentGroupForUpdateDto))
                .Returns(Task.FromResult(failResponseMock.Object))
                .Returns(Task.FromResult(successResponseMock.Object));

            // Act
            var actual = await _sut.UpdateExperimentGroup(_experimentGroupForUpdateDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            _clientMock.Verify(x => x.UpdateExperimentGroup(_experimentGroupForUpdateDto), Times.Exactly(2));
            failResponseMock.VerifyAll();
            successResponseMock.VerifyAll();
        }

        [TestMethod]
        public async Task UpdateExperimentGroup_RetryableStatusCode_Fail()
        {
            // Arrange
            var failResponseMock = new Mock<IRestResponse>();
            failResponseMock.Setup(s => s.StatusCode).Returns(HttpStatusCode.ServiceUnavailable);

            _clientMock.SetupSequence(s => s.UpdateExperimentGroup(_experimentGroupForUpdateDto))
                .Returns(Task.FromResult(failResponseMock.Object))
                .Returns(Task.FromResult(failResponseMock.Object));

            // Act
            var actual = await _sut.UpdateExperimentGroup(_experimentGroupForUpdateDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            _clientMock.Verify(x => x.UpdateExperimentGroup(_experimentGroupForUpdateDto), Times.Exactly(3));
            actual.Error.Should().NotBeNullOrWhiteSpace();
            failResponseMock.VerifyAll();
        }
    }
}
