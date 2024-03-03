using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
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
using TTPService.Dtos;
using TTPService.Dtos.Orchestrator.ExperimentGroupCreationDtos;
using TTPService.Dtos.Update;
using TTPService.Entities;
using TTPService.Enums;
using TTPService.IntegrationTests.Authorization;
using TTPService.Repositories;
using TTPService.Services;

namespace TTPService.IntegrationTests
{
    [DoNotParallelize]
    [TestClass]
    public class ExperimentGroupControllerIntegrationTestsFlows
    {
        private const string ApplicationJson = "application/json";
        private const string Version = "v1";
        private static Mock<IOptions<RepositoryOptions>> _optionsMock;
        private static Mock<ILogger<Repository>> _repositoryLoggerMock;
        private static Mock<IOrchestratorApi> _orchestratorApi;
        private static MongoInMemory _mongoInMemory;
        private static RequestContext _requestContext;
        private static Repository _repository;
        private static HttpClient _client;
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
            _orchestratorApi = new Mock<IOrchestratorApi>();
            _orchestratorApi
                .Setup(x => x.CreateExperimentGroup(It.IsAny<ExperimentGroupCreationDto>())).Returns(
                    Task.FromResult(Result.Ok()));
            _orchestratorApi
                .Setup(x => x.UpdateExperimentGroup(It.IsAny<Dtos.Orchestrator.ExperimentGroupUpdateDtos.ExperimentGroupForUpdateDto>())).Returns(
                    Task.FromResult(Result.Ok()));

            _client = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.Replace(new ServiceDescriptor(typeof(IRepository), _repository));
                        services.Replace(new ServiceDescriptor(typeof(IOrchestratorApi), _orchestratorApi.Object));
                    });
                })
                .CreateClient()
                .WithAuthorization();
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

        // TOOD: Fix next methods, during https://dev.azure.com/mit-is/SPARK/_workitems/edit/70007
        /*[TestMethod]
        [DataRow(ExperimentState.Ready, ExperimentState.UpdateInProgress)]
        [DataRow(ExperimentState.Draft, ExperimentState.DraftUpdateInProgress)]
        public async Task UpdateExperimentGroup_WithValidDuplicatedConditions_HappyPath(ExperimentState experimentState, ExperimentState expectedExperimentState)
        {
            // Arrange
            var objectForCreation = JsonConvert.DeserializeObject<Dtos.Update.ExperimentGroupForUpdateDto>(File.ReadAllText(@"HelperFiles\DtoPayloads\WithDuplicatedConditions.json"));
            objectForCreation.Experiments.First().ExperimentState = experimentState;
            var objectForGroupUpdate = JsonConvert.DeserializeObject<Dtos.Update.ExperimentGroupForUpdateDto>(File.ReadAllText(@"HelperFiles\DtoPayloads\UpdateWithDuplicatedConditions.json"));
            var objectForVpoUpdate = new VpoForUpdateDto()
            {
                IsFinishedSuccessfully = true,
                Vpo = "someVpoToMakeItWork",
            };

            // Act
            var responseForCreation = await _client.PostAsync($"api/{Version}/experimentGroups", new StringContent(JsonConvert.SerializeObject(objectForCreation), Encoding.Default, ApplicationJson));
            responseForCreation.EnsureSuccessStatusCode();

            var stringResponseForCreation = await responseForCreation.Content.ReadAsStringAsync();

            var actualCreationObject = JsonConvert.DeserializeObject<ExperimentGroupDto>(stringResponseForCreation);
            objectForGroupUpdate.Experiments.First().Id = actualCreationObject.Experiments.First().Id;
            objectForVpoUpdate.CorrelationId = actualCreationObject.Experiments.First().Id;

            var updateVpoNumberResult = await _client.PostAsync($"api/{Version}/callbacks/UpdateVpoNumber", new StringContent(JsonConvert.SerializeObject(objectForVpoUpdate), Encoding.Default, ApplicationJson));
            updateVpoNumberResult.EnsureSuccessStatusCode();

            var responseForUpdate = await _client.PatchAsync($"api/{Version}/experimentGroups/{actualCreationObject.Id}", new StringContent(JsonConvert.SerializeObject(objectForGroupUpdate), Encoding.Default, ApplicationJson));
            objectForGroupUpdate.Experiments.First().ExperimentState = expectedExperimentState;

            // Assert
            responseForCreation.EnsureSuccessStatusCode();
            responseForUpdate.EnsureSuccessStatusCode();

            // Examine results for the creation
            actualCreationObject.Should().BeEquivalentTo(JsonConvert.SerializeObject(objectForCreation), opt =>
            {
                opt.ExcludingMissingMembers();
                opt.IncludingAllRuntimeProperties();
                opt.IncludingNestedObjects();
                opt.AllowingInfiniteRecursion();
                opt
                .Excluding(e => e.SelectedMemberInfo.MemberType == typeof(Guid))
                .Excluding(e => e.SelectedMemberInfo.MemberType == typeof(DateTime));
                return opt;
            });

            // Examine results for the creation
            var stringResponseForUpdate = await responseForUpdate.Content.ReadAsStringAsync();
            var actualUpdateObject = JsonConvert.DeserializeObject<ExperimentGroupDto>(stringResponseForUpdate);
            actualUpdateObject.Should().NotBeNull();
            actualUpdateObject.Id.Should().NotBeEmpty();
            actualUpdateObject.Experiments = actualUpdateObject.Experiments.Select(e =>
            {
                e.Id.Should().NotBeEmpty();
                e.Stages.SelectMany(s => s.StageData.As<ClassStageData>().Conditions).Select(c =>
                {
                    c.Id.Should().NotBeEmpty();
                    return c;
                });
                e.Stages.SelectMany(s => s.StageData.As<ClassStageDataDto>().ConditionsWithResults = Enumerable.Empty<ConditionWithResultsDto>());
                e.Vpo.Should().Be(objectForVpoUpdate.Vpo);
                e.ExperimentState.Should().Be(expectedExperimentState);
                return e;
            }).ToList();
        }*/
    }
}