using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
using TTPService.Configuration;
using TTPService.Dtos;
using TTPService.Dtos.Update;
using TTPService.Entities;
using TTPService.Enums;
using TTPService.Helpers;
using TTPService.IntegrationTests.Authorization;
using TTPService.Repositories;

namespace TTPService.IntegrationTests
{
    [DoNotParallelize]
    [TestClass]
    public class CallbacksControllerIntegrationTests
    {
        private const string ApplicationJson = "application/json";
        private const string Version = "v1";
        private static Mock<IOptions<RepositoryOptions>> _optionsMock;
        private static Mock<ILogger<Repository>> _repositoryLoggerMock;
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

            _client = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
                                                           {
                                                               builder.ConfigureTestServices(services =>
                                                               {
                                                                   ServiceCollectionDescriptorExtensions.Replace(services, new ServiceDescriptor(typeof(IRepository), _repository));
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
        public async Task UpdateConditionOrStageStatus_WithCondition_HappyFlow()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().ToList().First();
            var condition = ((ClassStageData)group.Experiments.First().Stages.First().StageData).Conditions.First();
            condition.Status = ProcessStatus.Running;
            await _repository.AddExperimentGroup(group);

            var dto = new StatusForUpdateDto()
            {
                CorrelationId = condition.Id,
                Status = ProcessStatus.Completed,
            };
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var postResponse = await _client.PostAsync($"api/{Version}/callbacks/UpdateConditionOrStageStatus", stringContent);
            var getResponse = await _client.GetAsync($"api/{Version}/experimentGroups/{group.Id}");
            var getStringResponse = await getResponse.Content.ReadAsStringAsync();
            var actualGroup = JsonSerializer.Deserialize<PredictionRecordDto>(getStringResponse, _jsonOptions);
            var actualCondition = actualGroup
                                 .Experiments.First().Stages.First().StageData.As<ClassStageDataDto>().ConditionsWithResults
                                 .First(c => c.Id == condition.Id);

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            getResponse.EnsureSuccessStatusCode();
            actualCondition.Status.Should().Be(ProcessStatus.Completed);
        }

        [TestMethod]
        public async Task UpdateConditionOrStageStatus_WithOLBStage_HappyFlow()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroupWithOlbStage();
            var stage = group.Experiments.First().Stages.First();
            var stageData = (OLBStageData)stage.StageData;
            stageData.Status = ProcessStatus.Running;
            await _repository.AddExperimentGroup(group);

            var dto = new StatusForUpdateDto()
            {
                CorrelationId = stage.Id,
                Status = ProcessStatus.Completed,
            };
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var postResponse = await _client.PostAsync($"api/{Version}/callbacks/UpdateConditionOrStageStatus", stringContent);
            var getResponse = await _client.GetAsync($"api/{Version}/experimentGroups/{group.Id}");
            var getStringResponse = await getResponse.Content.ReadAsStringAsync();
            var actualGroup = JsonSerializer.Deserialize<PredictionRecordDto>(getStringResponse, _jsonOptions);
            var actualStage = actualGroup
                .Experiments.First().Stages.First(c => c.Id == stage.Id);

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            getResponse.EnsureSuccessStatusCode();
            ((OlbStageDataDto)actualStage.StageData).Status.Should().Be(ProcessStatus.Completed);
        }

        [TestMethod]
        public async Task UpdateConditionOrStageStatus_WithPpvStage_HappyFlow()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroupWithPpvStage();
            var stage = group.Experiments.First().Stages.First();
            var stageData = (PPVStageData)stage.StageData;
            stageData.Status = ProcessStatus.Running;
            await _repository.AddExperimentGroup(group);

            var dto = new StatusForUpdateDto()
            {
                CorrelationId = stage.Id,
                Status = ProcessStatus.Completed,
            };
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var postResponse = await _client.PostAsync($"api/{Version}/callbacks/UpdateConditionOrStageStatus", stringContent);
            var getResponse = await _client.GetAsync($"api/{Version}/experimentGroups/{group.Id}");
            var getStringResponse = await getResponse.Content.ReadAsStringAsync();
            var actualGroup = JsonSerializer.Deserialize<PredictionRecordDto>(getStringResponse, _jsonOptions);
            var actualStage = actualGroup
                .Experiments.First().Stages.First(c => c.Id == stage.Id);

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            getResponse.EnsureSuccessStatusCode();
            ((PpvStageDataDto)actualStage.StageData).Status.Should().Be(ProcessStatus.Completed);
        }

        [TestMethod]
        public async Task UpdateConditionOrStageStatus_WithMaestroStage_HappyFlow()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroupWithMaestroStage();
            var stage = group.Experiments.First().Stages.First();
            var stageData = (MaestroStageData)stage.StageData;
            stageData.Status = ProcessStatus.Running;
            await _repository.AddExperimentGroup(group);

            var dto = new StatusForUpdateDto()
            {
                CorrelationId = stage.Id,
                Status = ProcessStatus.Completed,
            };
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var postResponse = await _client.PostAsync($"api/{Version}/callbacks/UpdateConditionOrStageStatus", stringContent);
            var getResponse = await _client.GetAsync($"api/{Version}/experimentGroups/{group.Id}");
            var getStringResponse = await getResponse.Content.ReadAsStringAsync();
            var actualGroup = JsonSerializer.Deserialize<PredictionRecordDto>(getStringResponse, _jsonOptions);
            var actualStage = actualGroup
                .Experiments.First().Stages.First(c => c.Id == stage.Id);

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            getResponse.EnsureSuccessStatusCode();
            ((MaestroStageDataDto)actualStage.StageData).Status.Should().Be(ProcessStatus.Completed);
        }

        [TestMethod]
        public async Task UpdateConditionOrStageStatus_WithMaterialIssueStepFailures_MaterialUpdated()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().ToList().First();
            var condition = group.Experiments.First().Stages.First().StageData.As<ClassStageData>().Conditions.First();
            condition.Status = ProcessStatus.PendingMaterialIssue;
            await _repository.AddExperimentGroup(group);

            var dto = new StatusForUpdateDto()
            {
                CorrelationId = condition.Id,
                Status = ProcessStatus.PendingMaterialIssue,
                Comment = "Of course something happened with vortex ... or wombat... or mole",
                IsIssueStep = false,
                MaterialIssueFailed = true,
            };
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var postResponse = await _client.PostAsync($"api/{Version}/callbacks/UpdateConditionOrStageStatus", stringContent);
            var getResponse = await _client.GetAsync($"api/{Version}/experimentGroups/{group.Id}");
            var getStringResponse = await getResponse.Content.ReadAsStringAsync();
            var actualGroup = JsonSerializer.Deserialize<PredictionRecordDto>(getStringResponse, _jsonOptions);
            var actualCondition = actualGroup
                                 .Experiments.First().Stages.First().StageData.As<ClassStageDataDto>().Conditions
                                 .First(c => c.Id == condition.Id);

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            getResponse.EnsureSuccessStatusCode();
            actualCondition.Status.Should().Be(ProcessStatus.PendingMaterialIssue);
            actualCondition.StatusChangeComment.Should().Be(string.Empty);
            actualGroup.Experiments.First().Material.MaterialIssue.MaterialIssueIsRequired.Should().BeTrue();
            actualGroup.Experiments.First().Material.MaterialIssue.MaterialIssueErrorComments.Should().Be(dto.Comment);
        }

        [TestMethod]
        public async Task UpdateConditionOrStageStatus_WithPreviousMaterialIssueStepFailed_MaterialIssueSectionErrorCommentsCleared()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().ToList().First();
            group.Experiments.First().Material.MaterialIssue = new MaterialIssue()
            {
                MaterialIssueIsRequired = true,
                MaterialIssueErrorComments = "some errors from previous data call",
            };
            var condition = group.Experiments.First().Stages.First().StageData.As<ClassStageData>().Conditions.First();
            condition.Status = ProcessStatus.PendingMaterialIssue;
            await _repository.AddExperimentGroup(group);

            var dto = new StatusForUpdateDto()
            {
                CorrelationId = condition.Id,
                Status = ProcessStatus.PendingMaterialIssue,
                Comment = "now is all fixed",
                IsIssueStep = false,
                MaterialIssueFailed = false,
            };
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var postResponse = await _client.PostAsync($"api/{Version}/callbacks/UpdateConditionOrStageStatus", stringContent);
            var getResponse = await _client.GetAsync($"api/{Version}/experimentGroups/{group.Id}");
            var getStringResponse = await getResponse.Content.ReadAsStringAsync();
            var actualGroup = JsonSerializer.Deserialize<PredictionRecordDto>(getStringResponse, _jsonOptions);
            var actualCondition = actualGroup
                                 .Experiments.First()
                                 .Stages.First().StageData.As<ClassStageDataDto>().Conditions
                                 .First(c => c.Id == condition.Id);

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            getResponse.EnsureSuccessStatusCode();
            actualCondition.Status.Should().Be(ProcessStatus.PendingMaterialIssue);
            actualCondition.StatusChangeComment.Should().Be(dto.Comment);
            actualGroup.Experiments.First().Material.MaterialIssue.MaterialIssueIsRequired.Should().BeTrue();
            actualGroup.Experiments.First().Material.MaterialIssue.MaterialIssueErrorComments.Should().BeNullOrEmpty();
        }

        [TestMethod]
        public async Task UpdateConditionOrStageStatus_PausedCondition_ConditionUpdated()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().ToList().First();
            var condition = group.Experiments.First().Stages.First().StageData.As<ClassStageData>().Conditions.First();
            condition.Status = ProcessStatus.Running;
            await _repository.AddExperimentGroup(group);

            var dto = new StatusForUpdateDto()
            {
                CorrelationId = condition.Id,
                Status = ProcessStatus.Paused,
                Comment = "is not a copy paste from another test case...",
                IsIssueStep = false,
                MaterialIssueFailed = false,
            };
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var postResponse = await _client.PostAsync($"api/{Version}/callbacks/UpdateConditionOrStageStatus", stringContent);
            var getResponse = await _client.GetAsync($"api/{Version}/experimentGroups/{group.Id}");
            var getStringResponse = await getResponse.Content.ReadAsStringAsync();
            var actualGroup = JsonSerializer.Deserialize<PredictionRecordDto>(getStringResponse, _jsonOptions);
            var actualCondition = actualGroup
                                 .Experiments.First()
                                 .Stages.First().StageData.As<ClassStageDataDto>().Conditions
                                 .First(c => c.Id == condition.Id);

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            getResponse.EnsureSuccessStatusCode();
            actualCondition.Status.Should().Be(ProcessStatus.Paused);
            actualCondition.StatusChangeComment.Should().Be(dto.Comment);
            actualGroup.Experiments.First().Material.MaterialIssue.Should().BeNull();
        }

        [TestMethod]
        public async Task UpdateConditionOrStageStatus_IssueStepsAreIgnored_NoContentReturned()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().ToList().First();
            var condition = group.Experiments.First().Stages.First().StageData.As<ClassStageData>().Conditions.First();
            condition.Status = ProcessStatus.Running;
            await _repository.AddExperimentGroup(group);

            var dto = new StatusForUpdateDto()
            {
                CorrelationId = Guid.NewGuid(),
                Status = ProcessStatus.Completed,
                IsIssueStep = true,
            };
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var postResponse = await _client.PostAsync($"api/{Version}/callbacks/UpdateConditionOrStageStatus", stringContent);
            var getResponse = await _client.GetAsync($"api/{Version}/experimentGroups/{group.Id}");
            var getStringResponse = await getResponse.Content.ReadAsStringAsync();
            var actualGroup = JsonSerializer.Deserialize<PredictionRecordDto>(getStringResponse, _jsonOptions);
            var actualCondition = actualGroup
                                 .Experiments.First()
                                 .Stages.First()
                                 .StageData.As<ClassStageDataDto>().Conditions
                                 .First(c => c.Id == condition.Id);

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            getResponse.EnsureSuccessStatusCode();
            actualCondition.Status.Should().Be(ProcessStatus.Running);
        }

        [TestMethod]
        public async Task UpdateConditionOrStageStatus_IdNotFound()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().ToList().First();
            await _repository.AddExperimentGroup(group);

            var dto = new StatusForUpdateDto()
            {
                CorrelationId = Guid.NewGuid(),
                Status = ProcessStatus.PendingCommit,
            };
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var postResponse = await _client.PostAsync($"api/{Version}/callbacks/UpdateConditionOrStageStatus", stringContent);

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task UpdateConditionOrStageStatus_ValidationIsTriggered()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().ToList().First();
            await _repository.AddExperimentGroup(group);

            var dto = new StatusForUpdateDto()
            {
                CorrelationId = Guid.NewGuid(),
                Status = null, // required!
            };
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var postResponse = await _client.PostAsync($"api/{Version}/callbacks/UpdateConditionOrStageStatus", stringContent);

            // Assert
            postResponse.Should().BeOfType<HttpResponseMessage>();
            postResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var stringResponse = await postResponse.Content.ReadAsStringAsync();
            stringResponse.Should().Contain(nameof(dto.Status));
        }

        [TestMethod]
        public async Task UpdateExperimentVpo_HappyFlow()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().ToList().First();
            var experiment = group.Experiments.First();
            experiment.Vpo = "oldVPO";
            await _repository.AddExperimentGroup(group);

            var newVpo = "newVPO";
            var dto = new VpoForUpdateDto()
            {
                CorrelationId = experiment.Id,
                Vpo = newVpo,
                IsFinishedSuccessfully = true,
            };
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var postResponse = await _client.PostAsync($"api/{Version}/callbacks/UpdateVpoNumber", stringContent);
            var getResponse = await _client.GetAsync($"api/{Version}/experimentGroups/{group.Id}");
            var getStringResponse = await getResponse.Content.ReadAsStringAsync();
            var actualGroup = JsonSerializer.Deserialize<PredictionRecordDto>(getStringResponse, _jsonOptions);

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            getResponse.EnsureSuccessStatusCode();
            actualGroup.Experiments.First(e => e.Id == experiment.Id).Vpo.Should()
                       .Be(newVpo);
        }

        [TestMethod]
        public async Task UpdateExperimentVpo_WithFailureStatus_HappyFlow()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().ToList().First();
            var experiment = group.Experiments.Last();
            await _repository.AddExperimentGroup(group);

            var dto = new VpoForUpdateDto()
            {
                CorrelationId = experiment.Id,
                Vpo = null,
                ErrorMessage = "some error",
            };
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var postResponse = await _client.PostAsync($"api/{Version}/callbacks/UpdateVpoNumber", stringContent);
            var getResponse = await _client.GetAsync($"api/{Version}/experimentGroups/{group.Id}");
            var getStringResponse = await getResponse.Content.ReadAsStringAsync();
            var actualGroup = JsonSerializer.Deserialize<PredictionRecordDto>(getStringResponse, _jsonOptions);

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            getResponse.EnsureSuccessStatusCode();
            actualGroup.Experiments.Last(e => e.Id == experiment.Id).Vpo.Should()
                       .BeNull();
        }

        [TestMethod]
        public async Task UpdateExperimentVpo_IdNotFound()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().ToList().First();

            await _repository.AddExperimentGroup(group);

            var dto = new VpoForUpdateDto()
            {
                CorrelationId = Guid.NewGuid(),
                Vpo = "newVpo",
                IsFinishedSuccessfully = true,
            };
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var postResponse = await _client.PostAsync($"api/{Version}/callbacks/UpdateVpoNumber", stringContent);

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task UpdateExperimentVpo_ValidationIsTriggered()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().ToList().First();

            await _repository.AddExperimentGroup(group);

            var dto = new VpoForUpdateDto()
            {
                CorrelationId = Guid.Empty,
                Vpo = "newVpo",
            };
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var postResponse = await _client.PostAsync($"api/{Version}/callbacks/UpdateVpoNumber", stringContent);

            // Assert
            postResponse.Should().BeOfType<HttpResponseMessage>();
            postResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var stringResponse = await postResponse.Content.ReadAsStringAsync();
            stringResponse.Should().Contain(nameof(dto.CorrelationId));
        }

        [TestMethod]
        [DataRow(ExperimentState.DraftUpdateInProgress, ExperimentState.Draft, "oldVPO")]
        [DataRow(ExperimentState.UpdateInProgress, ExperimentState.Ready, "oldVPO")]
        [DataRow(ExperimentState.Draft, ExperimentState.Draft, "newVPO")]
        [DataRow(ExperimentState.Ready, ExperimentState.Ready, "newVPO")]
        public async Task UpdateExperimentVpo_AfterUpdateOperation_HappyFlow(ExperimentState experimentState, ExperimentState stateAfterUpdate, string expectedVpo)
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().ToList().First();
            var experiment = group.Experiments.First();
            experiment.Vpo = "oldVPO";
            experiment.ExperimentState = experimentState;
            await _repository.AddExperimentGroup(group);

            var newVpo = "newVPO";
            var dto = new VpoForUpdateDto()
            {
                CorrelationId = experiment.Id,
                Vpo = newVpo,
                IsFinishedSuccessfully = true,
            };
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var postResponse = await _client.PostAsync($"api/{Version}/callbacks/UpdateVpoNumber", stringContent);
            var getResponse = await _client.GetAsync($"api/{Version}/experimentGroups/{group.Id}");
            var getStringResponse = await getResponse.Content.ReadAsStringAsync();
            var actualGroup = JsonSerializer.Deserialize<PredictionRecordDto>(getStringResponse, _jsonOptions);

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            getResponse.EnsureSuccessStatusCode();
            var relevantExperiment = actualGroup.Experiments.First(e => e.Id == experiment.Id);
            relevantExperiment.Vpo.Should().Be(expectedVpo);
            relevantExperiment.ExperimentState.Should().Be(stateAfterUpdate);
        }

        [TestMethod]
        [DataRow(ExperimentState.DraftUpdateInProgress)]
        [DataRow(ExperimentState.UpdateInProgress)]
        [DataRow(ExperimentState.Draft)]
        [DataRow(ExperimentState.Ready)]
        public async Task UpdateExperimentVpo_AfterUpdateOperationWithFailure_ShouldNotUpdateAnything(ExperimentState experimentState)
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().ToList().First();
            var experiment = group.Experiments.First();
            experiment.Vpo = "oldVPO";
            experiment.ExperimentState = experimentState;
            await _repository.AddExperimentGroup(group);

            var dto = new VpoForUpdateDto()
            {
                CorrelationId = experiment.Id,
                Vpo = "newVPO",
                IsFinishedSuccessfully = false,
                ErrorMessage = "Just wanted to give another workitem for the support team",
            };
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var postResponse = await _client.PostAsync($"api/{Version}/callbacks/UpdateVpoNumber", stringContent);
            var getResponse = await _client.GetAsync($"api/{Version}/experimentGroups/{group.Id}");
            var getStringResponse = await getResponse.Content.ReadAsStringAsync();
            var actualGroup = JsonSerializer.Deserialize<PredictionRecordDto>(getStringResponse, _jsonOptions);

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            getResponse.EnsureSuccessStatusCode();
            var relevantExperiment = actualGroup.Experiments.First(e => e.Id == experiment.Id);
            relevantExperiment.Vpo.Should().Be("oldVPO");
            relevantExperiment.ExperimentState.Should().Be(experimentState);
        }

        [TestMethod]
        public async Task UpdateConditionResult_HappyFlow()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().ToList().First();
            var condition = group.Experiments.First().Stages.First().StageData.As<ClassStageData>().Conditions.First(c => c.Status == ProcessStatus.Completed);
            condition.Results = null;
            await _repository.AddExperimentGroup(group);

            var dto = new ResultsForUpdateDto()
            {
                ConditionId = condition.Id,
                ConditionResultForUpdate = new ConditionResultForUpdateDto()
                {
                    NumberOfGoodBins = 1,
                    NumberOfBadBins = 2,
                },
            };
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var postResponse = await _client.PostAsync($"api/{Version}/callbacks/Result", stringContent);
            var getResponse = await _client.GetAsync($"api/{Version}/experimentGroups/{group.Id}");
            var getStringResponse = await getResponse.Content.ReadAsStringAsync();
            var actualGroup = JsonSerializer.Deserialize<PredictionRecordDto>(getStringResponse, _jsonOptions);

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            getResponse.EnsureSuccessStatusCode();
            var actualResult = actualGroup.Experiments.First().Stages.First().StageData.As<ClassStageDataDto>().ConditionsWithResults
                                           .First(c => c.Id == condition.Id).Results;
            actualResult.NumberOfGoodBins.Should().Be(1);
            actualResult.NumberOfBadBins.Should().Be(2);
        }

        [TestMethod]
        public async Task UpdateConditionResult_IdNotFound()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().ToList().First();
            await _repository.AddExperimentGroup(group);

            var dto = new ResultsForUpdateDto()
            {
                ConditionId = Guid.NewGuid(),
                ConditionResultForUpdate = new ConditionResultForUpdateDto()
                {
                    NumberOfGoodBins = 5,
                    NumberOfBadBins = 2,
                },
            };
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var postResponse = await _client.PostAsync($"api/{Version}/callbacks/Result", stringContent);

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task UpdateConditionResult_ValidationIsTriggered()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().ToList().First();
            await _repository.AddExperimentGroup(group);

            var dto = new ResultsForUpdateDto()
            {
                ConditionId = Guid.NewGuid(),
                ConditionResultForUpdate = null,
            };
            var json = JsonSerializer.Serialize(dto, _jsonOptions);
            var stringContent = new StringContent(json, Encoding.Default, ApplicationJson);

            // Act
            var postResponse = await _client.PostAsync($"api/{Version}/callbacks/Result", stringContent);

            // Assert
            postResponse.Should().BeOfType<HttpResponseMessage>();
            postResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var stringResponse = await postResponse.Content.ReadAsStringAsync();
            stringResponse.Should().Contain(nameof(dto.ConditionResultForUpdate));
        }
    }
}