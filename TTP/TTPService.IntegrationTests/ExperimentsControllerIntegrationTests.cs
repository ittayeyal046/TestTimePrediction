using System;
using System.Collections.Generic;
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
using TTPService.Dtos.Update;
using TTPService.Entities;
using TTPService.Enums;
using TTPService.Helpers;
using TTPService.IntegrationTests.Authorization;
using TTPService.Repositories;
using TTPService.Services;
using ExperimentForUpdateOperationsDto = TTPService.Dtos.Update.ExperimentForUpdateOperationsDto;

namespace TTPService.IntegrationTests
{
    [DoNotParallelize]
    [TestClass]
    public class ExperimentsControllerIntegrationTests
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

            _orchestratorApi = new Mock<IOrchestratorApi>();
            _orchestratorApi
                .Setup(x => x.CreateExperimentGroup(It.IsAny<ExperimentGroupCreationDto>())).Returns(
                    Task.FromResult(Result.Ok()));
            _orchestratorApi
                .Setup(x => x.UpdateExperimentGroup(It.IsAny<Dtos.Orchestrator.ExperimentGroupUpdateDtos.ExperimentGroupForUpdateDto>())).Returns(
                    Task.FromResult(Result.Ok()));

            _requestContext = new RequestContext(_optionsMock.Object);
            _mongoInMemory = new MongoInMemory();
            _repository = new Repository(_repositoryLoggerMock.Object, _requestContext);

            _client = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.Replace(new ServiceDescriptor(typeof(IRepository), _repository));
                        services.Replace(new ServiceDescriptor(typeof(IOrchestratorApi), _orchestratorApi.Object));
                    });
                }).CreateClient().WithAuthorization();
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
        public async Task GetExperiments_HappyFlow()
        {
            // Arrange
            var rollingExperimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            await _repository.AddExperimentGroup(rollingExperimentGroup);
            var experimentIdsToGet = rollingExperimentGroup.Experiments.Select(e => e.Id).ToList();
            var experimentGroupId = rollingExperimentGroup.Id;
            var count = experimentIdsToGet.Count;
            var query = TestDataGenerator.ExperimentsIdsFromQueryBuilder(experimentIdsToGet);

            // Act
            var response = await _client.GetAsync($"api/{Version}/experimentGroups/{experimentGroupId}/experiments?{query}");

            // Assert
            response.EnsureSuccessStatusCode();

            // Deserialize and examine results.
            var stringResponse = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<List<ExperimentDto>>(stringResponse, _jsonOptions);
            actual.Count.Should().Be(count);
            for (var i = 0; i < count; i++)
            {
                actual[i].Id.Should().Be(experimentIdsToGet.ElementAt(i));
            }
        }

        [TestMethod]
        public async Task AddNewExperimentsToExperimentGroup_HappyFlow()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().First();
            await _repository.AddExperimentGroup(group);
            var experimentGroupId = group.Id;
            var experimentsBeforeAddition = group.Experiments.ToList();
            var count = experimentsBeforeAddition.Count;

            var experimentsForCreationDtos = _testDataGenerator.GenerateListOfExperimentsForCreationDtos(2);

            var json = JsonSerializer.Serialize(experimentsForCreationDtos, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups/{experimentGroupId}/experiments/", stringContent);

            // Assert
            response.EnsureSuccessStatusCode();

            var updatedGroup = await _repository.GetExperimentGroup(experimentGroupId);
            updatedGroup.IsSuccess.Should().BeTrue();
            updatedGroup.Value.HasValue.Should().BeTrue();
            updatedGroup.Value.Value.Experiments.Count().Should().Be(count + 2);
        }

        [TestMethod]
        public async Task AddNewExperimentsToExperimentGroup_ValidationIsTriggered()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().First();
            await _repository.AddExperimentGroup(group);
            var experimentGroupId = group.Id;
            var count = group.Experiments.Count();

            var experimentsForCreationDtos = _testDataGenerator.GenerateListOfExperimentsForCreationDtos();
            experimentsForCreationDtos.First().BomGroupName = string.Empty;

            var json = JsonSerializer.Serialize(experimentsForCreationDtos, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups/{experimentGroupId}/experiments/", stringContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            response.Should().BeOfType<HttpResponseMessage>();
            var stringResponse = await response.Content.ReadAsStringAsync();
            stringResponse.Should().Contain(nameof(Experiment.BomGroupName));

            var groupResult = await _repository.GetExperimentGroup(experimentGroupId);
            groupResult.IsSuccess.Should().BeTrue();
            groupResult.Value.HasValue.Should().BeTrue();
            groupResult.Value.Value.Experiments.Count().Should().Be(count);
        }

        [TestMethod]
        public async Task Cancel_HappyFlow()
        {
            // Arrange
            var experimentGroup = new ExperimentGroup
            {
                Wwid = 1234,
                Username = "userName1",
                TestProgramData = new TestProgramData
                {
                    BaseTestProgramName = "BaseTestProgramName",
                    ProgramFamily = "ProgramFamily",
                    ProgramSubFamily = "ProgramSubFamily",
                    StplDirectory = @"\\amr.corp.intel.com\ThisIs\StplDirectory",
                    DirectoryChecksum = "DirectoryChecksum",
                    TestProgramRootDirectory = "TestProgramRootDirectory",
                },
                Experiments = new List<Experiment>
                {
                    new Experiment()
                    {
                        BomGroupName = "BomGroup",
                        Step = "A0",
                        TplFileName = "TplFileName",
                        StplFileName = "StplFileName",
                        EnvironmentFileName = "EnvironmentFileName",
                        PlistAllFileName = "PlistAllFileName",
                        Material = new Material
                        {
                            Units = new List<Unit>
                            {
                                new Unit
                                {
                                    VisualId = "VisualId11",
                                    PartType = "HH 6HY789 A J",
                                    Lot = "LotName",
                                    Lab = "LabName",
                                    Location = new Location
                                    {
                                        LocationType = "LocationType",
                                        LocationName = "LocationName",
                                    },
                                },
                            },
                            Lots = new List<Lot>(),
                        },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                SequenceId = 1,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition
                                        {
                                            Thermals = new List<Thermal>
                                            {
                                                new Thermal { Name = "HOT", SequenceId = 1 },
                                            },
                                            EngineeringId = "9A",
                                            LocationCode = "19",
                                            SequenceId = 1,
                                        },
                                    },
                                },
                            },
                        },
                        SequenceId = 1,
                    },
                },
            };
            var addedGroupResult = await _repository.AddExperimentGroup(experimentGroup);
            var addedGroup = addedGroupResult.Value;
            var experimentId = addedGroup.Experiments.First().Id;
            var conditionId = addedGroup.Experiments.First().Stages.First().StageData.As<ClassStageData>().Conditions.First().Id;
            string comment = "some comment";
            var experimentForUpdateDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = new[] { experimentId },
                Comment = comment,
            };
            var json = JsonSerializer.Serialize(experimentForUpdateDto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups/{addedGroup.Id}/experiments/cancel", stringContent);

            // Assert
            response.EnsureSuccessStatusCode();

            var updatedGroup = await _repository.GetExperimentGroup(addedGroup.Id);
            updatedGroup.IsSuccess.Should().BeTrue();
            updatedGroup.Value.HasValue.Should().BeTrue();
            updatedGroup.Value.Value.Experiments.Count().Should().Be(1);
            var actualExperiment = updatedGroup.Value.Value.Experiments.First();
            actualExperiment.Id.Should().Be(experimentId);
            actualExperiment.Stages.First().StageData.As<ClassStageData>().Conditions.Count().Should().Be(1);
            var actualCondition = actualExperiment.Stages.First().StageData.As<ClassStageData>().Conditions.First();
            actualCondition.Id.Should().Be(conditionId);
            actualCondition.Status.Should().Be(ProcessStatus.Canceling);
        }

        [TestMethod]
        public async Task Cancel_SeveralExperiments_HappyFlow()
        {
            // Arrange
            var experimentGroup = new ExperimentGroup
            {
                Experiments = new List<Experiment>
                {
                    new Experiment()
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition {Status = ProcessStatus.NotStarted},
                                        new Condition {Status = ProcessStatus.NotStarted},
                                    },
                                },
                            },
                        },
                        SequenceId = 1,
                    },
                    new Experiment()
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition {Status = ProcessStatus.NotStarted},
                                        new Condition {Status = ProcessStatus.NotStarted},
                                    },
                                },
                            },
                        },
                        SequenceId = 2,
                    },
                    new Experiment()
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition {Status = ProcessStatus.NotStarted},
                                        new Condition {Status = ProcessStatus.NotStarted},
                                    },
                                },
                            },
                        },
                        SequenceId = 3,
                    },
                },
            };
            var addedGroupResult = await _repository.AddExperimentGroup(experimentGroup);
            var addedGroup = addedGroupResult.Value;
            var experimentIds = addedGroup.Experiments.Select(e => e.Id);
            string comment = "some comment";
            var experimentForUpdateDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = experimentIds,
                Comment = comment,
            };
            var json = JsonSerializer.Serialize(experimentForUpdateDto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups/{addedGroup.Id}/experiments/cancel", stringContent);

            // Assert
            response.EnsureSuccessStatusCode();

            var updatedGroup = await _repository.GetExperimentGroup(addedGroup.Id);
            updatedGroup.IsSuccess.Should().BeTrue();
            updatedGroup.Value.HasValue.Should().BeTrue();
            updatedGroup.Value.Value.Experiments.Count().Should().Be(3);
            var actualExperiments = updatedGroup.Value.Value.Experiments;
            actualExperiments.All(e => e.Stages.First().StageData.As<ClassStageData>().Conditions.All(c => c.Status == ProcessStatus.Canceling && c.StatusChangeComment == comment)).Should().BeTrue();
        }

        [TestMethod]
        public async Task Cancel_NoItemsFound_NotFoundErrorReturned()
        {
            // Arrange
            string comment = "some comment";
            var groupId = Guid.NewGuid();
            var experimentForUpdateDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = new[] { Guid.NewGuid() },
                Comment = comment,
            };
            var json = JsonSerializer.Serialize(experimentForUpdateDto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups/{groupId}/experiments/cancel", stringContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            response.Should().BeOfType<HttpResponseMessage>();
        }

        [TestMethod]
        public async Task Cancel_SomeOfTheItemsNotFound_ValidationErrorReturned()
        {
            // Arrange
            var experimentGroup = new ExperimentGroup
            {
                Experiments = new List<Experiment>
                {
                    new Experiment()
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition {Status = ProcessStatus.NotStarted},
                                        new Condition {Status = ProcessStatus.NotStarted},
                                    },
                                },
                            },
                        },
                        SequenceId = 1,
                    },
                    new Experiment()
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition {Status = ProcessStatus.NotStarted},
                                        new Condition {Status = ProcessStatus.NotStarted},
                                    },
                                },
                            },
                        },
                        SequenceId = 2,
                    },
                    new Experiment()
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition {Status = ProcessStatus.NotStarted},
                                        new Condition {Status = ProcessStatus.NotStarted},
                                    },
                                },
                            },
                        },
                        SequenceId = 3,
                    },
                },
            };
            var addedGroupResult = await _repository.AddExperimentGroup(experimentGroup);
            var addedGroup = addedGroupResult.Value;
            var experimentIds = addedGroup.Experiments.Select(e => e.Id);
            string comment = "some comment";
            var notFoundId = Guid.NewGuid();
            var experimentForUpdateDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = experimentIds.Append(notFoundId),
                Comment = comment,
            };
            var json = JsonSerializer.Serialize(experimentForUpdateDto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups/{addedGroup.Id}/experiments/cancel", stringContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            response.Should().BeOfType<HttpResponseMessage>();
            var stringResponse = await response.Content.ReadAsStringAsync();
            stringResponse.Should().Be($"Could not find all experiments {notFoundId} in group {addedGroup.Id}.");
        }

        [TestMethod]
        public async Task Cancel_ValidationIsTriggered()
        {
            // Arrange
            var experimentForUpdateDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = new[] { Guid.Empty },
                Comment = "some comment",
            };
            var json = JsonSerializer.Serialize(experimentForUpdateDto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups/{Guid.NewGuid()}/experiments/cancel", stringContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            response.Should().BeOfType<HttpResponseMessage>();
            var stringResponse = await response.Content.ReadAsStringAsync();
            stringResponse.Should().Contain(nameof(ExperimentForUpdateOperationsDto.ExperimentId));
        }

        [TestMethod]
        [DataRow(true, DisplayName = "From Draft to Ready")]
        [DataRow(false, DisplayName = "From Ready to Draft")]
        public async Task UpdateExperimentState_HappyFlow(bool fromDraftToReady)
        {
            // Arrange
            var experimentGroup = new ExperimentGroup
            {
                Wwid = 1234,
                Username = "userName1",
                TestProgramData = new TestProgramData
                {
                    BaseTestProgramName = "BaseTestProgramName",
                    ProgramFamily = "ProgramFamily",
                    ProgramSubFamily = "ProgramSubFamily",
                    StplDirectory = @"\\amr.corp.intel.com\ThisIs\StplDirectory",
                    DirectoryChecksum = "DirectoryChecksum",
                    TestProgramRootDirectory = "TestProgramRootDirectory",
                },
                Experiments = new List<Experiment>
                {
                    new Experiment()
                    {
                        BomGroupName = "BomGroup",
                        Step = "A0",
                        TplFileName = "TplFileName",
                        StplFileName = "StplFileName",
                        EnvironmentFileName = "EnvironmentFileName",
                        PlistAllFileName = "PlistAllFileName",
                        ExperimentState = fromDraftToReady ? ExperimentState.Draft : ExperimentState.Ready,
                        Material = new Material
                        {
                            Units = new List<Unit>
                            {
                                new Unit
                                {
                                    VisualId = "VisualId11",
                                    PartType = "HH 6HY789 A J",
                                    Lot = "LotName",
                                    Lab = "LabName",
                                    Location = new Location
                                    {
                                        LocationType = "LocationType",
                                        LocationName = "LocationName",
                                    },
                                },
                            },
                            Lots = new List<Lot>(),
                        },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition
                                        {
                                            Thermals = new List<Thermal>
                                            {
                                                new Thermal { Name = "HOT", SequenceId = 1 },
                                            },
                                            EngineeringId = "9A",
                                            LocationCode = "19",
                                            SequenceId = 1,
                                        },
                                    },
                                },
                            },
                        },
                        SequenceId = 1,
                    },
                },
            };
            var addedGroupResult = await _repository.AddExperimentGroup(experimentGroup);
            var addedGroup = addedGroupResult.Value;
            var experimentId = addedGroup.Experiments.First().Id;
            var experimentsStateForUpdateDto = new ExperimentsStateForUpdateDto()
            {
                ExperimentIds = new List<Guid>() { experimentId },
                ExperimentState = fromDraftToReady ? ExperimentState.Ready : ExperimentState.Draft,
            };
            var json = JsonSerializer.Serialize(experimentsStateForUpdateDto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups/{addedGroup.Id}/experiments/UpdateExperimentsState", stringContent);

            // Assert
            response.EnsureSuccessStatusCode();

            var updatedGroup = await _repository.GetExperimentGroup(addedGroup.Id);
            updatedGroup.IsSuccess.Should().BeTrue();
            updatedGroup.Value.HasValue.Should().BeTrue();
            updatedGroup.Value.Value.Experiments.Count().Should().Be(1);
            var actualExperiment = updatedGroup.Value.Value.Experiments.First();
            actualExperiment.Id.Should().Be(experimentId);
            actualExperiment.ExperimentState.Should().Be(fromDraftToReady ? ExperimentState.Ready : ExperimentState.Draft);
        }

        [TestMethod]
        public async Task UpdateExperimentState_ExperimentNotFound_ErrorReturned()
        {
            // Arrange
            var experimentGroup = new ExperimentGroup
            {
                Wwid = 1234,
                Username = "userName1",
                TestProgramData = new TestProgramData
                {
                    BaseTestProgramName = "BaseTestProgramName",
                    ProgramFamily = "ProgramFamily",
                    ProgramSubFamily = "ProgramSubFamily",
                    StplDirectory = @"\\amr.corp.intel.com\ThisIs\StplDirectory",
                    DirectoryChecksum = "DirectoryChecksum",
                    TestProgramRootDirectory = "TestProgramRootDirectory",
                },
                Experiments = new List<Experiment>
                {
                    new Experiment()
                    {
                        BomGroupName = "BomGroup",
                        Step = "A0",
                        TplFileName = "TplFileName",
                        StplFileName = "StplFileName",
                        EnvironmentFileName = "EnvironmentFileName",
                        PlistAllFileName = "PlistAllFileName",
                        Material = new Material
                        {
                            Units = new List<Unit>
                            {
                                new Unit
                                {
                                    VisualId = "VisualId11",
                                    PartType = "HH 6HY789 A J",
                                    Lot = "LotName",
                                    Lab = "LabName",
                                    Location = new Location
                                    {
                                        LocationType = "LocationType",
                                        LocationName = "LocationName",
                                    },
                                },
                            },
                            Lots = new List<Lot>(),
                        },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition
                                        {
                                            Thermals = new List<Thermal>
                                            {
                                                new Thermal { Name = "HOT", SequenceId = 1 },
                                            },
                                            EngineeringId = "9A",
                                            LocationCode = "19",
                                            SequenceId = 1,
                                        },
                                    },
                                },
                            },
                        },
                        SequenceId = 1,
                    },
                },
            };
            var addedGroupResult = await _repository.AddExperimentGroup(experimentGroup);
            var addedGroup = addedGroupResult.Value;
            var experimentsStateForUpdateDto = new ExperimentsStateForUpdateDto()
            {
                ExperimentIds = new List<Guid>() { Guid.NewGuid()},
                ExperimentState = ExperimentState.Draft,
            };
            var json = JsonSerializer.Serialize(experimentsStateForUpdateDto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups/{addedGroup.Id}/experiments/UpdateExperimentsState", stringContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            response.Should().BeOfType<HttpResponseMessage>();
        }

        [TestMethod]
        public async Task UpdateExperimentState_ValidationIsTriggered()
        {
            // Arrange
            var experimentsStateForUpdateDto = new ExperimentsStateForUpdateDto()
            {
                ExperimentIds = new List<Guid>(),
                ExperimentState = ExperimentState.Draft,
            };
            var json = JsonSerializer.Serialize(experimentsStateForUpdateDto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups/{Guid.NewGuid().ToString()}/experiments/UpdateExperimentsState", stringContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            response.Should().BeOfType<HttpResponseMessage>();
            var stringResponse = await response.Content.ReadAsStringAsync();
            stringResponse.Should().Be("{\"ExperimentIds\":[\"'Experiment Ids' must not be empty.\"]}");
        }

        [TestMethod]
        public async Task Delete_HappyFlow()
        {
            // Arrange
            var experimentGroup = new ExperimentGroup
            {
                Wwid = 1234,
                Username = "userName1",
                TestProgramData = new TestProgramData
                {
                    BaseTestProgramName = "BaseTestProgramName",
                    ProgramFamily = "ProgramFamily",
                    ProgramSubFamily = "ProgramSubFamily",
                    StplDirectory = @"\\amr.corp.intel.com\ThisIs\StplDirectory",
                    DirectoryChecksum = "DirectoryChecksum",
                    TestProgramRootDirectory = "TestProgramRootDirectory",
                },
                Experiments = new List<Experiment>
                {
                    new Experiment()
                    {
                        BomGroupName = "BomGroup",
                        Step = "A0",
                        TplFileName = "TplFileName",
                        StplFileName = "StplFileName",
                        EnvironmentFileName = "EnvironmentFileName",
                        PlistAllFileName = "PlistAllFileName",
                        Material = new Material
                        {
                            Units = new List<Unit>
                            {
                                new Unit
                                {
                                    VisualId = "VisualId11",
                                    PartType = "HH 6HY789 A J",
                                    Lot = "LotName",
                                    Lab = "LabName",
                                    Location = new Location
                                    {
                                        LocationType = "LocationType",
                                        LocationName = "LocationName",
                                    },
                                },
                            },
                            Lots = new List<Lot>(),
                        },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition
                                        {
                                            Thermals = new List<Thermal>
                                            {
                                                new Thermal { Name = "HOT", SequenceId = 1 },
                                            },
                                            EngineeringId = "9A",
                                            LocationCode = "19",
                                            SequenceId = 1,
                                        },
                                    },
                                },
                            },
                        },
                        SequenceId = 1,
                    },
                },
            };
            var addedGroupResult = await _repository.AddExperimentGroup(experimentGroup);
            var addedGroup = addedGroupResult.Value;
            var experimentId = addedGroup.Experiments.First().Id;
            var conditionId = addedGroup.Experiments.First().Stages.First().StageData.As<ClassStageData>().Conditions.First().Id;
            string comment = "some comment";
            var experimentForUpdateDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = new[] { experimentId },
                Comment = comment,
            };
            var json = JsonSerializer.Serialize(experimentForUpdateDto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups/{addedGroup.Id}/experiments/delete", stringContent);

            // Assert
            response.EnsureSuccessStatusCode();

            var updatedGroup = await _repository.GetExperimentGroup(addedGroup.Id);
            updatedGroup.IsSuccess.Should().BeTrue();
            updatedGroup.Value.HasValue.Should().BeTrue();
            updatedGroup.Value.Value.Experiments.Count().Should().Be(1);
            var actualExperiment = updatedGroup.Value.Value.Experiments.First();
            actualExperiment.Id.Should().Be(experimentId);
            actualExperiment.IsArchived.Should().BeTrue();
            actualExperiment.Stages.First().StageData.As<ClassStageData>().Conditions.Count().Should().Be(1);
            var actualCondition = actualExperiment.Stages.First().StageData.As<ClassStageData>().Conditions.First();
            actualCondition.Id.Should().Be(conditionId);
            actualCondition.Status.Should().Be(ProcessStatus.Canceling);
        }

        [TestMethod]
        public async Task Delete_SeveralExperiments_HappyFlow()
        {
            // Arrange
            var experimentGroup = new ExperimentGroup
            {
                Experiments = new List<Experiment>
                {
                    new Experiment()
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                SequenceId = 1,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition {Status = ProcessStatus.NotStarted},
                                        new Condition {Status = ProcessStatus.NotStarted},
                                    },
                                },
                            },
                        },
                        SequenceId = 1,
                    },
                    new Experiment()
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                SequenceId = 1,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition {Status = ProcessStatus.NotStarted},
                                        new Condition {Status = ProcessStatus.NotStarted},
                                    },
                                },
                            },
                        },
                        SequenceId = 2,
                    },
                    new Experiment()
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                SequenceId = 1,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition {Status = ProcessStatus.NotStarted},
                                        new Condition {Status = ProcessStatus.NotStarted},
                                    },
                                },
                            },
                        },
                        SequenceId = 3,
                    },
                },
            };
            var addedGroupResult = await _repository.AddExperimentGroup(experimentGroup);
            var addedGroup = addedGroupResult.Value;
            var experimentIds = addedGroup.Experiments.Select(e => e.Id);
            string comment = "some comment";
            var experimentForUpdateDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = experimentIds,
                Comment = comment,
            };
            var json = JsonSerializer.Serialize(experimentForUpdateDto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups/{addedGroup.Id}/experiments/delete", stringContent);

            // Assert
            response.EnsureSuccessStatusCode();

            var updatedGroup = await _repository.GetExperimentGroup(addedGroup.Id);
            updatedGroup.IsSuccess.Should().BeTrue();
            updatedGroup.Value.HasValue.Should().BeTrue();
            updatedGroup.Value.Value.Experiments.Count().Should().Be(3);
            var actualExperiments = updatedGroup.Value.Value.Experiments;
            actualExperiments.All(e => e.IsArchived && e.Stages.First().StageData.As<ClassStageData>().Conditions.All(c => c.Status == ProcessStatus.Canceling && c.StatusChangeComment == comment)).Should().BeTrue();
        }

        [TestMethod]
        public async Task Delete_SomeOfTheItemsNotFound_ValidationErrorReturned()
        {
            // Arrange
            var experimentGroup = new ExperimentGroup
            {
                Experiments = new List<Experiment>
                {
                    new Experiment()
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition {Status = ProcessStatus.NotStarted},
                                        new Condition {Status = ProcessStatus.NotStarted},
                                    },
                                },
                            },
                        },
                        SequenceId = 1,
                    },
                    new Experiment()
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition {Status = ProcessStatus.NotStarted},
                                        new Condition {Status = ProcessStatus.NotStarted},
                                    },
                                },
                            },
                        },
                        SequenceId = 2,
                    },
                    new Experiment()
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition {Status = ProcessStatus.NotStarted},
                                        new Condition {Status = ProcessStatus.NotStarted},
                                    },
                                },
                            },
                        },
                        SequenceId = 3,
                    },
                },
            };
            var addedGroupResult = await _repository.AddExperimentGroup(experimentGroup);
            var addedGroup = addedGroupResult.Value;
            var experimentIds = addedGroup.Experiments.Select(e => e.Id);
            string comment = "some comment";
            var notFoundId = Guid.NewGuid();
            var experimentForUpdateDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = experimentIds.Append(notFoundId),
                Comment = comment,
            };
            var json = JsonSerializer.Serialize(experimentForUpdateDto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups/{addedGroup.Id}/experiments/delete", stringContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            response.Should().BeOfType<HttpResponseMessage>();
            var stringResponse = await response.Content.ReadAsStringAsync();
            stringResponse.Should().Be($"Could not find all experiments {notFoundId} in group {addedGroup.Id}.");
        }

        [TestMethod]
        public async Task Delete_NoItemsFound_NotFoundErrorReturned()
        {
            // Arrange
            string comment = "some comment";
            var groupId = Guid.NewGuid();
            var experimentForUpdateDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = new[] { Guid.NewGuid() },
                Comment = comment,
            };
            var json = JsonSerializer.Serialize(experimentForUpdateDto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups/{groupId}/experiments/delete", stringContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            response.Should().BeOfType<HttpResponseMessage>();
        }

        [TestMethod]
        public async Task Delete_ValidationIsTriggered()
        {
            // Arrange
            var experimentForUpdateDto = new ExperimentForUpdateOperationsDto()
            {
                ExperimentId = Guid.Empty,
                Comment = "some comment",
            };
            var json = JsonSerializer.Serialize(experimentForUpdateDto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var response = await _client.PostAsync($"api/{Version}/experimentGroups/{Guid.NewGuid()}/experiments/delete", stringContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            response.Should().BeOfType<HttpResponseMessage>();
            var stringResponse = await response.Content.ReadAsStringAsync();
            stringResponse.Should().Contain(nameof(ExperimentForUpdateOperationsDto.ExperimentId));
        }
    }
}