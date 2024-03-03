using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using Moq;
using Newtonsoft.Json;
using TTPService.Configuration;
using TTPService.Entities;
using TTPService.Repositories;

namespace TTPService.IntegrationTests
{
    [Ignore("Authentication is disabled in MVP")]
    [DoNotParallelize]
    [TestClass]
    public class AuthenticationIntegrationTests
    {
        private const string ApplicationJson = "application/json";
        private static Mock<IOptions<RepositoryOptions>> _optionsMock;
        private static Mock<ILogger<Repository>> _repositoryLoggerMock;
        private static MongoInMemory _mongoInMemory;
        private static RequestContext _requestContext;
        private static Repository _repository;
        private static HttpClient _client;
        private readonly string _version = "v1";
        private TestDataGenerator _testDataGenerator;

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            _repositoryLoggerMock = new Mock<ILogger<Repository>>();
            _optionsMock = new Mock<IOptions<RepositoryOptions>>();
            _optionsMock.Setup(x => x.Value.ConnectionString).Returns(MongoInMemory.ConnectionString);
            _optionsMock.Setup(x => x.Value.Database).Returns(MongoInMemory.DbName);
            _optionsMock.Setup(x => x.Value.ShouldSeedEmptyRepository).Returns(false);
            _optionsMock.Setup(x => x.Value.Collection).Returns("ExperimentGroups");

            _requestContext = new RequestContext(_optionsMock.Object);
            _mongoInMemory = new MongoInMemory();
            _repository = new Repository(_repositoryLoggerMock.Object, _requestContext);

            _client = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.Replace(new ServiceDescriptor(typeof(IRepository), _repository));
                });
            })
                .CreateClient();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _mongoInMemory.Dispose();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _requestContext.ExperimentGroups.DeleteMany(FilterDefinition<ExperimentGroup>.Empty);
        }

        [TestInitialize]
        public void Init()
        {
            _testDataGenerator = new TestDataGenerator();
        }

        [TestMethod]
        public async Task GetExperiments_UnAuthorized()
        {
            // Arrange
            var experimentIdsToGet = new[] { Guid.NewGuid(), Guid.NewGuid() };
            var experimentGroupId = Guid.NewGuid();
            var query = TestDataGenerator.ExperimentsIdsFromQueryBuilder(experimentIdsToGet);

            // Act
            var response = await _client.GetAsync($"api/{_version}/experimentGroups/{experimentGroupId}/experiments?{query}");

            // Assert
            response.Should().BeOfType<HttpResponseMessage>().Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task AddNewExperimentsToExperimentGroup_UnAuthorized()
        {
            // Arrange
            var experimentGroupId = Guid.NewGuid();

            var experimentsForCreationDtos = _testDataGenerator.GenerateListOfExperimentsForCreationDtos(2);

            var json = JsonConvert.SerializeObject(experimentsForCreationDtos);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{_version}/experimentGroups/{experimentGroupId}/experiments/", stringContent);

            // Assert
            response.Should().BeOfType<HttpResponseMessage>().Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task DeleteExperimentsFromExperimentGroup_UnAuthorized()
        {
            // Arrange
            var experimentIdsToGet = new[] { Guid.NewGuid(), Guid.NewGuid() };
            var experimentGroupId = Guid.NewGuid();
            var query = TestDataGenerator.ExperimentsIdsFromQueryBuilder(experimentIdsToGet);

            // Act
            var response = await _client.DeleteAsync($"api/{_version}/experimentGroups/{experimentGroupId}/experiments?{query}");

            // Assert
            response.Should().BeOfType<HttpResponseMessage>().Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task GetExperimentGroupById_UnAuthorized()
        {
            // Arrange
            var experimentGroupId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"api/{_version}/experimentGroups/{experimentGroupId}");

            // Assert
            response.Should().BeOfType<HttpResponseMessage>().Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task GetExperimentGroups_UnAuthorized()
        {
            // Act
            var response = await _client.GetAsync($"api/{_version}/experimentGroups");

            // Assert
            response.Should().BeOfType<HttpResponseMessage>().Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_UnAuthorized()
        {
            // Arrange
            var json = string.Empty;
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{_version}/experimentGroups", stringContent);

            // Assert
            response.Should().BeOfType<HttpResponseMessage>().Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task GetTopCommonLocationCodes_UnAuthorized()
        {
            // Arrange
            var programFamily = "ICL";
            var amount = 3;

            // Act
            var response = await _client.GetAsync($"api/{_version}/experimentGroups/commonLocationCodes/{programFamily}?amount={amount}");

            // Assert
            response.Should().BeOfType<HttpResponseMessage>().Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task GetTopCommonEngineeringIds_UnAuthorized()
        {
            // Arrange
            var programFamily = "ICL";
            var amount = 3;

            // Act
            var response = await _client.GetAsync($"api/{_version}/experimentGroups/commonEngineeringIds/{programFamily}?amount={amount}");

            // Assert
            response.Should().BeOfType<HttpResponseMessage>().Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task UpdateConditionStatus_UnAuthorized()
        {
            // Arrange
            var json = string.Empty;
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{_version}/callbacks/UpdateConditionStatus", stringContent);

            // Assert
            response.Should().BeOfType<HttpResponseMessage>().Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task UpdateConditionVpo_UnAuthorized()
        {
            // Arrange
            var json = string.Empty;
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{_version}/callbacks/UpdateVpoNumber", stringContent);

            // Assert
            response.Should().BeOfType<HttpResponseMessage>().Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task UpdateConditionResult_UnAuthorized()
        {
            // Arrange
            var json = string.Empty;
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{_version}/callbacks/Result", stringContent);

            // Assert
            response.Should().BeOfType<HttpResponseMessage>().Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}