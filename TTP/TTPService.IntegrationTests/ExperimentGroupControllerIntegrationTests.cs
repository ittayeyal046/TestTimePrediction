using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
using TTPService.Configuration;
using TTPService.Dtos;
using TTPService.Dtos.Orchestrator.ExperimentGroupCreationDtos;
using TTPService.Dtos.Orchestrator.ExperimentGroupUpdateDtos;
using TTPService.Entities;
using TTPService.Enums;
using TTPService.Helpers;
using TTPService.IntegrationTests.Authorization;
using TTPService.Repositories;
using TTPService.Services;
using ClassStageDataForCreationDto = TTPService.Dtos.Creation.ClassStageDataForCreationDto;

namespace TTPService.IntegrationTests
{
    [DoNotParallelize]
    [TestClass]
    public class ExperimentGroupControllerIntegrationTests
    {
        private const string ApplicationJson = "application/json";
        private const string Version = "v1";
        private static readonly Guid Id = Guid.NewGuid();
        private static Mock<IOptions<RepositoryOptions>> _optionsMock;
        private static Mock<ILogger<Repository>> _repositoryLoggerMock;
        private static Mock<IOrchestratorApi> _orchestratorApi;
        private static MongoInMemory _mongoInMemory;
        private static RequestContext _requestContext;
        private static Repository _repository;
        private static HttpClient _client;
        private static JsonSerializerOptions _jsonOptions;
        private TestDataGenerator _testDataGenerator;

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            _jsonOptions.Converters.Add(new JsonStringEnumConverter());
            _jsonOptions.Converters.Add(new StageForCreationDtoJsonConverter());
            _jsonOptions.Converters.Add(new StageDtoJsonConverter());

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
                .Setup(x => x.UpdateExperimentGroup(It.IsAny<ExperimentGroupForUpdateDto>())).Returns(
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

        [TestMethod]
        public async Task GetExperimentGroupById_HappyFlow()
        {
            // Arrange
            var rollingExperimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();

            await _repository.AddExperimentGroup(rollingExperimentGroup);
            var experimentGroupId = rollingExperimentGroup.Id;

            // Act
            var response = await _client.GetAsync($"api/{Version}/experimentGroups/{experimentGroupId}");

            // Assert
            response.EnsureSuccessStatusCode();

            // Deserialize and examine results.
            var stringResponse = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<PredictionRecordDto>(stringResponse, _jsonOptions);
            actual.Username.Should().Be(rollingExperimentGroup.Username);
        }

        [TestMethod]
        public async Task GetExperimentGroupById_WithSplitExperiment_HappyFlow()
        {
            // Arrange
            var experimentGroup = _testDataGenerator.GenerateExperimentGroupWithSplitExperiment();
            await _repository.AddExperimentGroup(experimentGroup);
            var experimentGroupId = experimentGroup.Id;
            var expected = _testDataGenerator.GenerateExperimentGroupDtoWithSplitExperiment();

            // Act
            var response = await _client.GetAsync($"api/{Version}/experimentGroups/{experimentGroupId}");

            // Assert
            response.EnsureSuccessStatusCode();

            // Deserialize and examine results.
            var stringResponse = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<PredictionRecordDto>(stringResponse, _jsonOptions);
            actual.Should().BeEquivalentTo(expected, opt =>
            {
                opt.IncludingAllRuntimeProperties();
                opt.IncludingNestedObjects();
                opt.AllowingInfiniteRecursion();
                opt
                .Excluding(e => e.Type == typeof(Guid))
                .Excluding(e => e.Type == typeof(DateTime));
                return opt;
            });
        }

        [TestMethod]
        public async Task GetExperimentGroupById_WithDuplicatedConditions_HappyFlow()
        {
            // Arrange
            var experimentGroup = _testDataGenerator.GenerateExperimentGroupWithDuplicatedConditions();
            await _repository.AddExperimentGroup(experimentGroup);
            var experimentGroupId = experimentGroup.Id;
            var expected = _testDataGenerator.GenerateExperimentGroupDtoWithDuplicatedConditions();

            // Act
            var response = await _client.GetAsync($"api/{Version}/experimentGroups/{experimentGroupId}");

            // Assert
            response.EnsureSuccessStatusCode();

            // Deserialize and examine results.
            var stringResponse = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<PredictionRecordDto>(stringResponse, _jsonOptions);
            actual.Should().BeEquivalentTo(expected, opt =>
            {
                opt.IncludingAllRuntimeProperties();
                opt.IncludingNestedObjects();
                opt.AllowingInfiniteRecursion();
                opt
                .Excluding(e => e.Type == typeof(Guid))
                .Excluding(e => e.Type == typeof(DateTime));
                return opt;
            });
        }

        [TestMethod]
        public async Task GetExperimentGroupByIdWithoutTestProgramRootDirectory_HappyFlow()
        {
            // Arrange
            var rollingExperimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperimentWithoutTestProgramRootDirectory();

            await _repository.AddExperimentGroup(rollingExperimentGroup);
            var experimentGroupId = rollingExperimentGroup.Id;

            // Act
            var response = await _client.GetAsync($"api/{Version}/experimentGroups/{experimentGroupId}");

            // Assert
            response.EnsureSuccessStatusCode();

            // Deserialize and examine results.
            var stringResponse = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<PredictionRecordDto>(stringResponse, _jsonOptions);
            actual.Username.Should().Be(rollingExperimentGroup.Username);
            actual.TestProgramData.TestProgramRootDirectory.Should().Be(actual.TestProgramData.TestProgramRootDirectory);
            actual.Experiments
                .SelectMany(e => e.Stages)
                .Where(s => s.StageType == StageType.Class)
                .All(x => x.StageData.As<ClassStageDataDto>().Conditions.All(y => y.FuseEnabled ?? false));
            actual.Experiments.First().Stages.First().StageData.As<ClassStageDataDto>().Conditions.First().DieSelection.Should().Be("DummyDieSelection");
            actual.Experiments.First().Stages.First().StageData.As<ClassStageDataDto>().Conditions.First().MoveUnits.Should().Be(MoveUnits.All);
        }

        [TestMethod]
        public async Task GetExperimentGroupById_IdNotFound()
        {
            // Arrange
            var rollingExperimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            await _repository.AddExperimentGroup(rollingExperimentGroup);
            var id = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"api/{Version}/experimentGroups/{id}");

            // Assert
            response.Should().BeOfType<HttpResponseMessage>().Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task GetExperimentGroups_HappyFlow()
        {
            // Arrange
            var rollingExperimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            var completedExperimentGroup = _testDataGenerator.CreateCompletedExperimentGroup();

            await _repository.AddExperimentGroup(rollingExperimentGroup);
            await _repository.AddExperimentGroup(completedExperimentGroup);

            // Act
            var response = await _client.GetAsync($"api/{Version}/experimentGroups");

            // Assert
            response.EnsureSuccessStatusCode();

            // Deserialize and examine results.
            var stringResponse = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<List<PredictionRecordDto>>(stringResponse, _jsonOptions);
            actual.Count.Should().Be(2);
        }

        [TestMethod]
        public async Task GetExperimentGroups_userValidationFailed()
        {
            // Arrange
            var rollingExperimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            var completedExperimentGroup = _testDataGenerator.CreateCompletedExperimentGroup();

            await _repository.AddExperimentGroup(rollingExperimentGroup);
            await _repository.AddExperimentGroup(completedExperimentGroup);

            // Act
            var response = await _client.GetAsync($"api/{Version}/experimentGroups?username=$$$$");

            // Assert
            response.Should().BeOfType<HttpResponseMessage>().Which.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        [TestMethod]
        public async Task GetExperimentGroups_endDateTimeIsNotValid()
        {
            // Arrange
            var rollingExperimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            var completedExperimentGroup = _testDataGenerator.CreateCompletedExperimentGroup();

            await _repository.AddExperimentGroup(rollingExperimentGroup);
            await _repository.AddExperimentGroup(completedExperimentGroup);

            var today = DateTime.UtcNow;
            var duration = new TimeSpan(1, 0, 0, 0);
            var notValidDateTime = today.Add(duration);

            // Act
            var response = await _client.GetAsync($"api/{Version}/experimentGroups?startDate={notValidDateTime:MM/dd/yyyy HH:mm:ss}");

            // Assert
            response.Should().BeOfType<HttpResponseMessage>().Which.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        [TestMethod]
        public async Task GetExperimentGroups_startDateTimeIsNotValid()
        {
            // Arrange
            var rollingExperimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            var completedExperimentGroup = _testDataGenerator.CreateCompletedExperimentGroup();

            await _repository.AddExperimentGroup(rollingExperimentGroup);
            await _repository.AddExperimentGroup(completedExperimentGroup);

            var today = DateTime.UtcNow;
            var duration = new TimeSpan(1, 0, 0, 0);
            var notValidDateTime = today.Add(duration);

            // Act
            var response = await _client.GetAsync($"api/{Version}/experimentGroups?endDate={notValidDateTime:MM/dd/yyyy HH:mm:ss}");

            // Assert
            response.Should().BeOfType<HttpResponseMessage>().Which.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        [TestMethod]
        public async Task GetExperimentGroups_StatusIsNotValid()
        {
            // Arrange
            var rollingExperimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            var completedExperimentGroup = _testDataGenerator.CreateCompletedExperimentGroup();

            await _repository.AddExperimentGroup(rollingExperimentGroup);
            await _repository.AddExperimentGroup(completedExperimentGroup);

            // Act
            var response = await _client.GetAsync($"api/{Version}/experimentGroups?status=NotValidStatus");

            // Assert
            response.Should().BeOfType<HttpResponseMessage>().Which.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_NoFuseData_HappyFlow()
        {
            // Arrange
            var dto = _testDataGenerator.GenerateExperimentGroupForCreationDto();

            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups", stringContent);

            // Assert
            response.EnsureSuccessStatusCode();

            // Deserialize and examine results.
            var stringResponse = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<PredictionRecordDto>(stringResponse, _jsonOptions);
            actual.Wwid.Should().Be(dto.Wwid);
            actual.Username.Should().Be(dto.Username.ToLower());
            actual.Id.Should().NotBeEmpty();
            actual.Experiments
                .SelectMany(e => e.Stages)
                .Where(s => s.StageType == StageType.Class && s.StageData != null)
                .SelectMany(s => s.StageData.As<ClassStageDataDto>().Conditions.Select(c => c.Fuse))
                .All(f => f == null).Should().BeTrue();
            actual.Experiments.First().Stages.First().StageData.As<ClassStageDataDto>().Conditions.First().FuseEnabled.Should().BeFalse();
            actual.Experiments.First().Stages.First().StageData.As<ClassStageDataDto>().Conditions.First().DieSelection.Should().Be("DummyDieSelection");
            actual.Experiments.First().Stages.First().StageData.As<ClassStageDataDto>().Conditions.First().MoveUnits.Should().Be(MoveUnits.All);
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_WTL_HappyFlow()
        {
            // Arrange
            var dto = _testDataGenerator.GenerateWalkTheLotExperimentGroupForCreationDto();

            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups", stringContent);

            // Assert
            response.EnsureSuccessStatusCode();

            // Deserialize and examine results.
            var stringResponse = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<PredictionRecordDto>(stringResponse, _jsonOptions);
            actual.Wwid.Should().Be(dto.Wwid);
            actual.Username.Should().Be(dto.Username.ToLower());
            actual.Id.Should().NotBeEmpty();
            actual.Experiments.Should().BeEquivalentTo(dto.Experiments, opt =>
            {
                opt.IncludingAllRuntimeProperties();
                opt.IncludingNestedObjects();
                opt.AllowingInfiniteRecursion();
                return opt;
            });
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_CustomAttributes_HappyFlow()
        {
            // Arrange
            var dto = _testDataGenerator.GenerateExperimentGroupForCreationDto();

            var expectedCustomAttribute = new List<CustomAttributeDto>
            {
                new CustomAttributeDto
                {
                    AttributeName = "Attribute1",
                    AttributeValue = "Value for Attribute 1",
                },
            };

            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups", stringContent);

            // Assert
            response.EnsureSuccessStatusCode();

            // Deserialize and examine results.
            var stringResponse = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<PredictionRecordDto>(stringResponse, _jsonOptions);
            actual.Wwid.Should().Be(dto.Wwid);
            actual.Username.Should().Be(dto.Username.ToLower());
            actual.Id.Should().NotBeEmpty();
            actual.Experiments.SelectMany(e => e.Stages)
                .Where(s => s.StageType == StageType.Class)
                .SelectMany(s => s.StageData.As<ClassStageDataDto>().Conditions)
                .Select(c => c.Fuse).All(f => f == null).Should().BeTrue();
            actual.Experiments.First().Stages
                .Select(s => s.StageData.As<ClassStageDataDto>().Conditions)
                .First().First().CustomAttributes
                .Should().BeEquivalentTo(expectedCustomAttribute);
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_WithThermalByPassed_HappyPath()
        {
            // Arrange
            var dto = _testDataGenerator.GenerateExperimentGroupForCreationDto();
            dto.Experiments.First().Stages.First().StageData.As<ClassStageDataForCreationDto>().Conditions.First().Thermals = new[]
            {
                new Dtos.Creation.ThermalForCreationDto() { Name = "test", SequenceId = 1, IsByPassed = true},
                new Dtos.Creation.ThermalForCreationDto() { Name = "test2", SequenceId = 2, IsByPassed = false},
            };

            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups", stringContent);

            // Assert
            response.EnsureSuccessStatusCode();

            // Deserialize and examine results.
            var stringResponse = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<PredictionRecordDto>(stringResponse, _jsonOptions);
            actual.Wwid.Should().Be(dto.Wwid);
            actual.Username.Should().Be(dto.Username.ToLower());
            actual.Id.Should().NotBeEmpty();
            actual.Experiments
                .SelectMany(e => e.Stages)
                .Where(s => s.StageType == StageType.Class && s.StageData != null)
                .SelectMany(s => s.StageData.As<ClassStageDataDto>().Conditions.Select(c => c.Fuse)).All(f => f == null).Should().BeTrue();
            actual.Experiments.First().Stages.First().StageData.As<ClassStageDataDto>().Conditions.First().FuseEnabled.Should().BeFalse();
            actual.Experiments.First().Stages.First().StageData.As<ClassStageDataDto>().Conditions.First().DieSelection.Should().Be("DummyDieSelection");
            actual.Experiments.First().Stages.First().StageData.As<ClassStageDataDto>().Conditions.First().MoveUnits.Should().Be(MoveUnits.All);
            dto.Experiments.First().Stages.First().StageData.As<ClassStageDataForCreationDto>().Conditions.First().Thermals.Should().BeEquivalentTo(new[]
            {
                new Dtos.Creation.ThermalForCreationDto() { Name = "test", SequenceId = 1, IsByPassed = true},
                new Dtos.Creation.ThermalForCreationDto() { Name = "test2", SequenceId = 2, IsByPassed = false},
            });
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_WithOneStepFuseData_HappyFlow()
        {
            // Arrange
            var dto = _testDataGenerator.GenerateExperimentGroupForCreationDto();
            var fuseDto = _testDataGenerator.GenerateOneStepFuseForCreationDto();
            dto.Experiments.First().Stages.First().StageData.As<ClassStageDataForCreationDto>().Conditions.First().Fuse = fuseDto;

            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups", stringContent);

            // Assert
            response.EnsureSuccessStatusCode();

            // Deserialize and examine results.
            var stringResponse = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<PredictionRecordDto>(stringResponse, _jsonOptions);
            actual.Wwid.Should().Be(dto.Wwid);
            actual.Username.Should().Be(dto.Username.ToLower());
            actual.Id.Should().NotBeEmpty();
            actual.Experiments.First().Stages.First().StageData.As<ClassStageDataDto>().Conditions.First().Fuse.Should().BeEquivalentTo(fuseDto);
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_WithDuplicatedConditions_HappyPath()
        {
            // Arrange
            var dtoJsonContent = File.ReadAllText(@"HelperFiles\DtoPayloads\WithDuplicatedConditions.json");
            var stringContent = new StringContent(dtoJsonContent, Encoding.Default, ApplicationJson);
            var expected = _testDataGenerator.GenerateExperimentGroupDtoWithDuplicatedConditionsInSameStage();

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups", stringContent);

            // Assert
            response.EnsureSuccessStatusCode();

            // Deserialize and examine results.
            var stringResponse = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<PredictionRecordDto>(stringResponse, _jsonOptions);
            actual.Should().BeEquivalentTo(expected, opt =>
            {
                opt.IncludingAllRuntimeProperties();
                opt.IncludingNestedObjects();
                opt.AllowingInfiniteRecursion();
                opt
                .Excluding(e => e.Type == typeof(Guid))
                .Excluding(e => e.Type == typeof(DateTime));
                return opt;
            });
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_WithInvalidDuplicatedConditions_HappyPath()
        {
            // Arrange
            var dtoJsonContent = File.ReadAllText(@"HelperFiles\DtoPayloads\WithInvalidDuplicatedConditions.json");
            var stringContent = new StringContent(dtoJsonContent, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups", stringContent);

            // Assert
            response.StatusCode.Should().Be((HttpStatusCode)422);

            // Deserialize and examine results.
            var stringResponse = await response.Content.ReadAsStringAsync();
            stringResponse.Should().Be("{\"Experiments[0].Stages\":[\"VPO suffix should be provided in case of multiple conditions with same operation. It must have exactly one empty value, and the rest unique values in correct format.\"]}");
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_WithNotProvidedDuplicatedConditions_HappyPath()
        {
            // Arrange
            var dtoJsonContent = File.ReadAllText(@"HelperFiles\DtoPayloads\WithNotProvidedDuplicatedConditions.json");
            var stringContent = new StringContent(dtoJsonContent, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups", stringContent);

            // Assert
            response.StatusCode.Should().Be((HttpStatusCode)422);

            // Deserialize and examine results.
            var stringResponse = await response.Content.ReadAsStringAsync();
            stringResponse.Should().Be("{\"Experiments[0].Stages\":[\"VPO suffix should be provided in case of multiple conditions with same operation. It must have exactly one empty value, and the rest unique values in correct format.\"]}");
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_WithIssueStep_HappyFlow()
        {
            // Arrange
            var dto = _testDataGenerator.GenerateExperimentGroupForCreationDto();
            var fuseDto = _testDataGenerator.GenerateOneStepFuseForCreationDto();
            dto.Experiments.First().Stages.First().StageData.As<ClassStageDataForCreationDto>().Conditions.First().Fuse = fuseDto;
            dto.Experiments.First().Material.MaterialIssue = new Dtos.Creation.MaterialIssueForCreationDto() { MaterialIssueIsRequired = true };

            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups", stringContent);

            // Assert
            response.EnsureSuccessStatusCode();

            // Deserialize and examine results.
            var stringResponse = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<PredictionRecordDto>(stringResponse, _jsonOptions);
            actual.Wwid.Should().Be(dto.Wwid);
            actual.Username.Should().Be(dto.Username.ToLower());
            actual.Id.Should().NotBeEmpty();
            actual.Experiments.First().Material.MaterialIssue.MaterialIssueIsRequired.Should().BeTrue();
            actual.Experiments.First().Stages.First().StageData.As<ClassStageDataDto>().Conditions.First().Fuse.Should().BeEquivalentTo(fuseDto);
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_WithTwoStepLrfFuseData_HappyFlow()
        {
            // Arrange
            var dto = _testDataGenerator.GenerateExperimentGroupForCreationDto();
            var fuseDto = _testDataGenerator.GenerateTwoStepLrfFuseForCreationDto();
            dto.Experiments.First().Stages.First().StageData.As<ClassStageDataForCreationDto>().Conditions.First().Fuse = fuseDto;

            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups", stringContent);

            // Assert
            response.EnsureSuccessStatusCode();

            // Deserialize and examine results.
            var stringResponse = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<PredictionRecordDto>(stringResponse, _jsonOptions);
            actual.Wwid.Should().Be(dto.Wwid);
            actual.Username.Should().Be(dto.Username.ToLower());
            actual.Id.Should().NotBeEmpty();
            actual.Experiments.First().Stages.First().StageData.As<ClassStageDataDto>().Conditions.First().Fuse.Should().BeEquivalentTo(fuseDto);
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_WithTwoStepQdfFuseData_HappyFlow()
        {
            // Arrange
            var dto = _testDataGenerator.GenerateExperimentGroupForCreationDto();
            var fuseDto = _testDataGenerator.GenerateTwoStepQdfFuseForCreationDto();
            dto.Experiments.First().Stages.First().StageData.As<ClassStageDataForCreationDto>().Conditions.First().Fuse = fuseDto;

            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups", stringContent);

            // Assert
            response.EnsureSuccessStatusCode();

            // Deserialize and examine results.
            var stringResponse = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<PredictionRecordDto>(stringResponse, _jsonOptions);
            actual.Wwid.Should().Be(dto.Wwid);
            actual.Username.Should().Be(dto.Username.ToLower());
            actual.Id.Should().NotBeEmpty();
            actual.Experiments.First().Stages.First().StageData.As<ClassStageDataDto>().Conditions.First().Fuse.Should().BeEquivalentTo(fuseDto);
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_Fail_WithInvalidTwoStepQdfFuseData()
        {
            // Arrange
            var dto = _testDataGenerator.GenerateExperimentGroupForCreationDto();
            var fuseDto = _testDataGenerator.GenerateTwoStepQdfFuseForCreationDto();
            fuseDto.Qdf = null;
            dto.Experiments.First().Stages.First().StageData.As<ClassStageDataForCreationDto>().Conditions.First().Fuse = fuseDto;

            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups", stringContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            response.Content.ReadAsStringAsync().Result.Should().Be("{\"Experiments[0].Stages[0].StageData.Conditions[0].Fuse.Qdf\":[\"Qdf should be provided when two step qdf mode is used\"]}");
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_InvalidWTL_ValidationFailure()
        {
            // Arrange
            var dto = _testDataGenerator.GenerateWTLMultipleMaestroExperimentGroupForCreationDto();
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups", stringContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            response.Content.ReadAsStringAsync().Result.Should().Be("{\"Experiments[0].Stages\":[\"There can only be 1 Maestro stage in an experiment.\"]}");
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_Fail_WithInvalidTwoStepQdf()
        {
            // Arrange
            var dto = _testDataGenerator.GenerateExperimentGroupForCreationDto();
            var fuseDto = _testDataGenerator.GenerateTwoStepQdfFuseForCreationDto();
            fuseDto.Qdf = "INVALID";
            dto.Experiments.First().Stages.First().StageData.As<ClassStageDataForCreationDto>().Conditions.First().Fuse = fuseDto;

            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups", stringContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            response.Content.ReadAsStringAsync().Result.Should().Be("{\"Experiments[0].Stages[0].StageData.Conditions[0].Fuse.Qdf\":[\"Qdf should have lenght of 4 characters.\"]}");
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_Fail_WithInvalidTwoStepLrfFuseData()
        {
            // Arrange
            var dto = _testDataGenerator.GenerateExperimentGroupForCreationDto();
            var fuseDto = _testDataGenerator.GenerateTwoStepLrfFuseForCreationDto();
            fuseDto.Lrf = null;
            dto.Experiments.First().Stages.First().StageData.As<ClassStageDataForCreationDto>().Conditions.First().Fuse = fuseDto;

            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups", stringContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            response.Content.ReadAsStringAsync().Result.Should().Be("{\"Experiments[0].Stages[0].StageData.Conditions[0].Fuse.Lrf\":[\"Lrf should be provided when two step lrf or one step fact mode is used\"]}");
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_Fail_WithInvalidOneStepFuseData()
        {
            // Arrange
            var dto = _testDataGenerator.GenerateExperimentGroupForCreationDto();
            var fuseDto = _testDataGenerator.GenerateOneStepFuseForCreationDto();
            fuseDto.Lrf = null;
            dto.Experiments.First().Stages.First().StageData.As<ClassStageDataForCreationDto>().Conditions.First().Fuse = fuseDto;

            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups", stringContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            response.Content.ReadAsStringAsync().Result.Should().Be("{\"Experiments[0].Stages[0].StageData.Conditions[0].Fuse.Lrf\":[\"Lrf should be provided when two step lrf or one step fact mode is used\"]}");
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_Fail_WithInvalidCustomAttributeData()
        {
            // Arrange
            var dto = _testDataGenerator.GenerateExperimentGroupForCreationDto();

            dto.Experiments.First().Stages.Select(s => s.StageData.As<ClassStageDataForCreationDto>().Conditions).First().First().CustomAttributes.First().AttributeValue = string.Empty;

            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups", stringContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            response.Content.ReadAsStringAsync().Result.Should().Be("{\"Experiments[0].Stages[0].StageData.Conditions[0].CustomAttributes[0].AttributeValue\":[\"'Attribute Value' must not be empty.\",\"The specified condition was not met for 'Attribute Value'.\"]}");
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_EmptyOriginalPartType()
        {
            // Arrange
            var dto = _testDataGenerator.GenerateExperimentGroupForCreationDto_EmptyOriginalPartType();

            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups", stringContent);

            // Assert
            response.EnsureSuccessStatusCode();

            // Deserialize and examine results.
            var stringResponse = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<PredictionRecordDto>(stringResponse, _jsonOptions)!;
            actual.Wwid.Should().Be(dto.Wwid);
            actual.Username.Should().Be(dto.Username.ToLower());
            actual.Id.Should().NotBeEmpty();
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_ValidOriginalPartType()
        {
            // Arrange
            var dto = _testDataGenerator.GenerateExperimentGroupForCreationDto_ValidOriginalPartType();

            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups", stringContent);

            // Assert
            response.EnsureSuccessStatusCode();

            // Deserialize and examine results.
            var stringResponse = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<PredictionRecordDto>(stringResponse, _jsonOptions);
            actual.Experiments.FirstOrDefault().Material.Units.FirstOrDefault().OriginalPartType.Should().NotBeNullOrEmpty();
            actual.Experiments.FirstOrDefault().Material.Units.FirstOrDefault().OriginalPartType.Should().Be("HH 6HY789 A G");
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_NotUniqueLots()
        {
            // Arrange
            var dto = _testDataGenerator.GenerateExperimentGroupForCreationDto();
            dto.Experiments.First().Material.Lots = _testDataGenerator.GenerateNotUniqueLotForCreationDto();

            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups", stringContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_NotUniqueUnits()
        {
            // Arrange
            var dto = _testDataGenerator.GenerateExperimentGroupForCreationDto();
            dto.Experiments.First().Material.Units = _testDataGenerator.GenerateNotUniqueUnitForCreationDto();

            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups", stringContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_ValidationIsTriggered()
        {
            // Arrange
            var dto = _testDataGenerator.GenerateExperimentGroupForCreationDto();
            dto.TestProgramData.BaseTestProgramName = string.Empty; // a required field

            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups", stringContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            response.Should().BeOfType<HttpResponseMessage>();
            var stringResponse = await response.Content.ReadAsStringAsync();
            stringResponse.Should().Contain(nameof(dto.TestProgramData.BaseTestProgramName));
        }

        [TestMethod]
        public async Task GetTopCommonLocationCodes_HappyFlow()
        {
            // Arrange
            await _repository.AddExperimentGroup(_testDataGenerator.CreateIclExperimentGroupForAggregationUser1());
            await _repository.AddExperimentGroup(_testDataGenerator.CreateIclExperimentGroupForAggregationUser2());
            await _repository.AddExperimentGroup(_testDataGenerator.CreateIclExperimentGroupForAggregationUser3());
            await _repository.AddExperimentGroup(_testDataGenerator.CreateClxExperimentGroupForAggregationUser4());
            var programFamily = "ICL";
            var amount = 3;

            // Act
            var response = await _client.GetAsync($"api/{Version}/experimentGroups/commonLocationCodes/{programFamily}?amount={amount}");

            // Assert
            response.EnsureSuccessStatusCode();

            // Deserialize and examine results.
            var stringResponse = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<IEnumerable<int>>(stringResponse, _jsonOptions).ToList();

            actual.Should().NotBeNull();
            actual.Count().Should().Be(amount);
            actual.Contains(6262).Should().BeTrue();    // 6262 : 8
            actual.Contains(7712).Should().BeTrue();    // 7712 : 11
            actual.Contains(6881).Should().BeTrue();    // 6881 : 6
        }

        [TestMethod]
        public async Task GetTopCommonLocationCodes_TriggerRequiredProgramFamily()
        {
            // Arrange
            var programFamily = string.Empty;
            var amount = 3;

            // Act
            var response = await _client.GetAsync($"api/{Version}/experimentGroups/commonLocationCodes/{programFamily}?amount={amount}");

            // Assert
            response.Should().BeOfType<HttpResponseMessage>().Which.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        [TestMethod]
        public async Task GetTopCommonLocationCodes_TriggerNegativeAmount()
        {
            // Arrange
            var programFamily = "ICL";
            var amount = -1;

            // Act
            var response = await _client.GetAsync($"api/{Version}/experimentGroups/commonLocationCodes/{programFamily}?amount={amount}");

            // Assert
            response.Should().BeOfType<HttpResponseMessage>().Which.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        [TestMethod]
        public async Task GetTopCommonEngineeringIds_HappyFlow()
        {
            // Arrange
            await _repository.AddExperimentGroup(_testDataGenerator.CreateIclExperimentGroupForAggregationUser1());
            await _repository.AddExperimentGroup(_testDataGenerator.CreateIclExperimentGroupForAggregationUser2());
            await _repository.AddExperimentGroup(_testDataGenerator.CreateIclExperimentGroupForAggregationUser3());
            await _repository.AddExperimentGroup(_testDataGenerator.CreateClxExperimentGroupForAggregationUser4());
            var programFamily = "ICL";
            var amount = 3;

            // Act
            var response = await _client.GetAsync($"api/{Version}/experimentGroups/commonEngineeringIds/{programFamily}?amount={amount}");

            // Assert
            response.EnsureSuccessStatusCode();

            // Deserialize and examine results.
            var stringResponse = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<IEnumerable<string>>(stringResponse, _jsonOptions);

            actual.Should().NotBeNull();
            actual.Count().Should().Be(amount);
            actual.Contains("EngineeringId12").Should().BeTrue();  // EngineeringId12 : 5
            actual.Contains("EngineeringId15").Should().BeTrue();  // EngineeringId15 : 4
            actual.Contains("EngineeringId17").Should().BeTrue();  // EngineeringId17 : 4
        }

        [TestMethod]
        public async Task GetTopCommonEngineeringIds_TriggerRequiredProgramFamily()
        {
            // Arrange
            var programFamily = string.Empty;
            var amount = 3;

            // Act
            var response = await _client.GetAsync($"api/{Version}/experimentGroups/commonEngineeringIds/{programFamily}?amount={amount}");

            // Assert
            response.Should().BeOfType<HttpResponseMessage>().Which.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        [TestMethod]
        public async Task GetTopCommonEngineeringIds_TriggerNegativeAmount()
        {
            // Arrange
            var programFamily = "ICL";
            var amount = -1;

            // Act
            var response = await _client.GetAsync($"api/{Version}/experimentGroups/commonEngineeringIds/{programFamily}?amount={amount}");

            // Assert
            response.Should().BeOfType<HttpResponseMessage>().Which.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        // TOOD: Fix next methods, during https://dev.azure.com/mit-is/SPARK/_workitems/edit/70007
        /*[TestMethod]
        public async Task UpdateExperimentGroup_HappyFlow()
        {
            // Arrange
            var experimentGroup = _testDataGenerator.GenerateExperimentGroup();
            var experimentGroupForEdit = _testDataGenerator.GenerateExperimentGroupForTpEditDto();

            var experimentGroupWithId = await _repository.AddExperimentGroup(experimentGroup);
            experimentGroupForEdit.Experiments.First().Id = experimentGroupWithId.Value.Experiments.First().Id;

            var json = JsonSerializer.Serialize(experimentGroupForEdit, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PatchAsync($"api/{Version}/experimentGroups/{experimentGroup.Id}", stringContent);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [TestMethod]
        [DataRow(ExperimentState.Ready, ExperimentState.UpdateInProgress)]
        [DataRow(ExperimentState.Draft, ExperimentState.DraftUpdateInProgress)]
        public async Task UpdateExperimentGroup_WithDuplicatedConditions_HappyPath(ExperimentState experimentState, ExperimentState expectedExperimentState)
        {
            // Arrange
            var experimentGroup = JsonSerializer.Deserialize<ExperimentGroup>(File.ReadAllText(@"HelperFiles\DtoPayloads\WithDuplicatedConditions.json"), _jsonOptions);
            experimentGroup.Experiments.First().Vpo = "SomeVpoToMakeItWork";
            experimentGroup.Experiments.First().ExperimentState = experimentState;
            var objectForGroupUpdateAndExpectedValue = JsonSerializer.Deserialize<Dtos.Update.ExperimentGroupForUpdateDto>(File.ReadAllText(@"HelperFiles\DtoPayloads\UpdateWithDuplicatedConditions.json"), _jsonOptions);

            var experimentGroupWithId = await _repository.AddExperimentGroup(experimentGroup);
            objectForGroupUpdateAndExpectedValue.Experiments.First().Id = experimentGroupWithId.Value.Experiments.First().Id;

            // Act
            var responseForUpdate = await _client.PatchAsync($"api/{Version}/experimentGroups/{experimentGroupWithId.Value.Id}", new StringContent(JsonSerializer.Serialize(objectForGroupUpdateAndExpectedValue, _jsonOptions), Encoding.Default, ApplicationJson));
            objectForGroupUpdateAndExpectedValue.Experiments.First().ExperimentState = expectedExperimentState;

            // Assert
            responseForUpdate.EnsureSuccessStatusCode();

            // Deserialize and examine results for the creation
            var stringResponseForUpdate = await responseForUpdate.Content.ReadAsStringAsync();
            var actualUpdateObject = JsonSerializer.Deserialize<ExperimentGroupDto>(stringResponseForUpdate, _jsonOptions);
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
                e.Vpo.Should().Be("SomeVpoToMakeItWork");
                e.ExperimentState.Should().Be(expectedExperimentState);
                return e;
            }).ToList();
        }

        [TestMethod]
        public async Task UpdateExperimentGroup_WithInvalidDuplicatedConditions()
        {
            // Arrange
            var experimentGroup = JsonSerializer.Deserialize<ExperimentGroup>(File.ReadAllText(@"HelperFiles\DtoPayloads\WithInvalidDuplicatedConditions.json"), _jsonOptions);
            experimentGroup.Experiments.First().Vpo = "SomeVpoToMakeItWork";
            var objectForGroupUpdateAndExpectedValue = JsonSerializer.Deserialize<Dtos.Update.ExperimentGroupForUpdateDto>(File.ReadAllText(@"HelperFiles\DtoPayloads\UpdateWithInvalidDuplicatedConditions.json"), _jsonOptions);

            var experimentGroupWithId = await _repository.AddExperimentGroup(experimentGroup);
            objectForGroupUpdateAndExpectedValue.Experiments.First().Id = experimentGroupWithId.Value.Experiments.First().Id;

            // Act
            var response = await _client.PatchAsync($"api/{Version}/experimentGroups/{experimentGroupWithId.Value.Id}", new StringContent(JsonSerializer.Serialize(objectForGroupUpdateAndExpectedValue, _jsonOptions), Encoding.Default, ApplicationJson));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            response.Should().BeOfType<HttpResponseMessage>();
            var stringResponse = await response.Content.ReadAsStringAsync();
            stringResponse.Should().Be("{\"Experiments[0].Conditions\":[\"Non duplicated conditions should not have a custom vpo suffix.\"]}");
        }

        [TestMethod]
        public async Task UpdateExperimentGroup_WithoutVpo_ErrorReturned()
        {
            // Arrange
            var experimentGroup = JsonSerializer.Deserialize<ExperimentGroup>(File.ReadAllText(@"HelperFiles\DtoPayloads\WithDuplicatedConditions.json"), _jsonOptions);
            var objectForGroupUpdateAndExpectedValue = JsonSerializer.Deserialize<Dtos.Update.ExperimentGroupForUpdateDto>(File.ReadAllText(@"HelperFiles\DtoPayloads\UpdateWithDuplicatedConditions.json"), _jsonOptions);

            var experimentGroupWithId = await _repository.AddExperimentGroup(experimentGroup);
            objectForGroupUpdateAndExpectedValue.Experiments.First().Id = experimentGroupWithId.Value.Experiments.First().Id;

            // Act
            var response = await _client.PatchAsync($"api/{Version}/experimentGroups/{experimentGroupWithId.Value.Id}", new StringContent(JsonSerializer.Serialize(objectForGroupUpdateAndExpectedValue, _jsonOptions), Encoding.Default, ApplicationJson));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            response.Should().BeOfType<HttpResponseMessage>();
            var stringResponse = await response.Content.ReadAsStringAsync();
            stringResponse.Should().Be("Some experiments don't have vpo number associated with it");
        }

        [TestMethod]
        public async Task UpdateExperimentGroup_NoMatchingExperiments_ErrorReturned()
        {
            // Arrange
            var experimentGroup = JsonSerializer.Deserialize<ExperimentGroup>(File.ReadAllText(@"HelperFiles\DtoPayloads\WithDuplicatedConditions.json"), _jsonOptions);
            experimentGroup.Experiments.First().Vpo = "SomeVpoToMakeItWork";
            var objectForGroupUpdateAndExpectedValue = JsonSerializer.Deserialize<Dtos.Update.ExperimentGroupForUpdateDto>(File.ReadAllText(@"HelperFiles\DtoPayloads\UpdateWithDuplicatedConditions.json"), _jsonOptions);

            var experimentGroupWithId = await _repository.AddExperimentGroup(experimentGroup);

            // Act
            var response = await _client.PatchAsync($"api/{Version}/experimentGroups/{experimentGroupWithId.Value.Id}", new StringContent(JsonSerializer.Serialize(objectForGroupUpdateAndExpectedValue, _jsonOptions), Encoding.Default, ApplicationJson));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            response.Should().BeOfType<HttpResponseMessage>();
            var stringResponse = await response.Content.ReadAsStringAsync();
            stringResponse.Should().Be("Not all the given experiments exist under the experiment group.");
        }

        [TestMethod]
        public async Task UpdateExperimentGroup_GroupNotFound_ErrorReturned()
        {
            // Arrange
            var experimentGroup = JsonSerializer.Deserialize<ExperimentGroup>(File.ReadAllText(@"HelperFiles\DtoPayloads\WithDuplicatedConditions.json"), _jsonOptions);
            var objectForGroupUpdateAndExpectedValue = JsonSerializer.Deserialize<Dtos.Update.ExperimentGroupForUpdateDto>(File.ReadAllText(@"HelperFiles\DtoPayloads\UpdateWithDuplicatedConditions.json"), _jsonOptions);

            var experimentGroupWithId = await _repository.AddExperimentGroup(experimentGroup);
            objectForGroupUpdateAndExpectedValue.Experiments.First().Id = experimentGroupWithId.Value.Experiments.First().Id;

            // Act
            var response = await _client.PatchAsync($"api/{Version}/experimentGroups/{Guid.Empty}", new StringContent(JsonSerializer.Serialize(objectForGroupUpdateAndExpectedValue, _jsonOptions), Encoding.Default, ApplicationJson));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            response.Should().BeOfType<HttpResponseMessage>();
            var stringResponse = await response.Content.ReadAsStringAsync();
            stringResponse.Should().BeNullOrEmpty();
        }*/
    }
}