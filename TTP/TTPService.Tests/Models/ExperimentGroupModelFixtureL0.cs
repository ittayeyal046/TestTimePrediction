using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TTPService.Dtos;
using TTPService.Dtos.Orchestrator.ExperimentGroupCreationDtos;
using TTPService.Entities;
using TTPService.Enums;
using TTPService.FunctionalExtensions;
using TTPService.Models;
using TTPService.Models.Validators;
using TTPService.Queue;
using TTPService.Repositories;

namespace TTPService.Tests.Models
{
    [TestClass]
    public class ExperimentGroupModelFixtureL0
    {
        private static IMapper _mapper;
        private Mock<IRepository> _repositoryMock;
        private TTPModel _model;
        private TestDataGenerator _testDataGenerator;
        private Mock<ISubmitter> _submitter;
        private Mock<IExperimentGroupModificationValidator> _experimentGroupModificationValidator;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _mapper =
                new MapperConfiguration(config =>
                    config.AddProfile(new MapProfile())).CreateMapper();
        }

        [TestInitialize]
        public void Init()
        {
            _repositoryMock = new Mock<IRepository>();
            _testDataGenerator = new TestDataGenerator();
            _submitter = new Mock<ISubmitter>();
            _experimentGroupModificationValidator = new Mock<IExperimentGroupModificationValidator>();
            _model = new ExperimentGroupsModel(
                _mapper,
                _repositoryMock.Object,
                Mock.Of<ILogger<TTPModel>>(),
                _submitter.Object,
                _experimentGroupModificationValidator.Object);
        }

        [TestMethod]
        public async Task GetExperimentGroups_HappyPath()
        {
            // Arrange
            var username = TestDataGenerationHelper.GenerateUserName();
            var experimentGroups = _testDataGenerator.GenerateListOfExperimentGroups();
            var experimentGroupDtos = _mapper.Map<IEnumerable<PredictionRecordDto>>(experimentGroups);
            var experimentGroupsResult = Result.Ok(experimentGroups);
            _repositoryMock.Setup(m => m.GetExperimentGroups(
                                It.IsAny<string>(),
                                It.IsAny<ExperimentGroupStatus?>(),
                                It.IsAny<DateTime?>(),
                                It.IsAny<DateTime?>(),
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<IEnumerable<string>>()))
                           .Returns(Task.FromResult(experimentGroupsResult));

            // Act
            var actual = await _model.GetExperimentGroups(username, ExperimentGroupStatus.Rolling, DateTime.Today, DateTime.UtcNow, "team", "segment", "vpo", "lot", "visualId", new string[] { "tag1", "tag2" });

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Should().BeOfType<Result<IEnumerable<PredictionRecordDto>, ErrorResult>>()
                  .Which.Value.Should().BeEquivalentTo(experimentGroupDtos);
        }

        [TestMethod]
        public async Task GetExperimentGroups_GroupWith1ArchivedExperiment_ReturnsTheGroupWithoutTheExperiment()
        {
            // Arrange
            var username = TestDataGenerationHelper.GenerateUserName();
            var experimentGroupId = Guid.NewGuid();
            var experiment1Id = Guid.NewGuid();
            var experiment2Id = Guid.NewGuid();
            var archivedExperiment = new Experiment
            {
                Id = experiment1Id,
                IsArchived = true,
                Stages = new List<Stage>
                {
                    new Stage()
                    {
                        Id = Guid.NewGuid(),
                        StageData = new ClassStageData
                        {
                            Conditions = new List<Condition>
                            {
                                new Condition
                                {
                                    Id = Guid.NewGuid(),
                                    Status = ProcessStatus.Running,
                                },
                            },
                        },
                    },
                },
            };
            var nonArchivedExperiment = new Experiment
            {
                Id = experiment2Id,
                IsArchived = false,
                Stages = new List<Stage>
                {
                    new Stage()
                    {
                        Id = Guid.NewGuid(),
                        StageData = new ClassStageData
                        {
                            Conditions = new List<Condition>
                            {
                                new Condition
                                {
                                    Id = Guid.NewGuid(),
                                    Status = ProcessStatus.PendingCommit,
                                },
                            },
                        },
                    },
                },
            };
            IEnumerable<ExperimentGroup> experimentGroups = new List<ExperimentGroup>
            {
                new ExperimentGroup
                {
                    Id = experimentGroupId,
                    Username = "someone",
                    Experiments = new List<Experiment>
                    {
                        archivedExperiment,
                        nonArchivedExperiment,
                    },
                    SubmissionTime = new DateTime(2019, 4, 1, 13, 00, 00).ToUniversalTime(),
                },
            };
            IEnumerable<ExperimentGroup> nonArchivedExperimentGroups = new List<ExperimentGroup>
            {
                new ExperimentGroup
                {
                    Id = experimentGroupId,
                    Username = "someone",
                    Experiments = new List<Experiment>
                    {
                        nonArchivedExperiment,
                    },
                    SubmissionTime = new DateTime(2019, 4, 1, 13, 00, 00).ToUniversalTime(),
                },
            };
            var nonArchivedExperimentGroupDtos = _mapper.Map<IEnumerable<PredictionRecordDto>>(nonArchivedExperimentGroups);
            _repositoryMock.Setup(m => m.GetExperimentGroups(
                                It.IsAny<string>(),
                                It.IsAny<ExperimentGroupStatus?>(),
                                It.IsAny<DateTime?>(),
                                It.IsAny<DateTime?>(),
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<IEnumerable<string>>()))
                           .Returns(Task.FromResult(Result.Ok(experimentGroups)));

            // Act
            var actual = await _model.GetExperimentGroups(username, ExperimentGroupStatus.Rolling);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Should().BeOfType<Result<IEnumerable<PredictionRecordDto>, ErrorResult>>()
                  .Which.Value.Should().BeEquivalentTo(nonArchivedExperimentGroupDtos);
        }

        [TestMethod]
        public async Task GetExperimentGroups_GroupWithAllArchivedExperiment_ReturnsTheGroupWithoutTheExperiment()
        {
            // Arrange
            var username = TestDataGenerationHelper.GenerateUserName();
            var experimentGroupId = Guid.NewGuid();
            var experiment1Id = Guid.NewGuid();
            var archivedExperiment = new Experiment
            {
                Id = experiment1Id,
                IsArchived = true,
                Stages = new List<Stage>
                {
                    new Stage()
                    {
                        Id = Guid.NewGuid(),
                        StageData = new ClassStageData
                        {
                            Conditions = new List<Condition>
                            {
                                new Condition
                                {
                                    Id = Guid.NewGuid(),
                                    Status = ProcessStatus.Running,
                                },
                            },
                        },
                    },
                },
            };
            IEnumerable<ExperimentGroup> experimentGroups = new List<ExperimentGroup>
            {
                new ExperimentGroup
                {
                    Id = experimentGroupId,
                    Username = "someone",
                    Experiments = new List<Experiment>
                    {
                        archivedExperiment,
                    },
                },
            };

            _repositoryMock.Setup(m => m.GetExperimentGroups(
                                It.IsAny<string>(),
                                It.IsAny<ExperimentGroupStatus?>(),
                                It.IsAny<DateTime?>(),
                                It.IsAny<DateTime?>(),
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<IEnumerable<string>>()))
                           .Returns(Task.FromResult(Result.Ok(experimentGroups)));

            // Act
            var actual = await _model.GetExperimentGroups(username, ExperimentGroupStatus.Rolling);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Should().BeOfType<Result<IEnumerable<PredictionRecordDto>, ErrorResult>>()
                  .Which.Value.Should().BeEquivalentTo(new List<PredictionRecordDto>());
        }

        [TestMethod]
        public async Task GetExperimentGroups_DateTimeHappyPath()
        {
            // Arrange
            var username = TestDataGenerationHelper.GenerateUserName();
            var experimentGroups = _testDataGenerator.GenerateListOfExperimentGroups();
            var experimentGroupDtos = _mapper.Map<IEnumerable<PredictionRecordDto>>(experimentGroups);
            var experimentGroupsResult = Result.Ok(experimentGroups);
            _repositoryMock.Setup(m => m.GetExperimentGroups(
                    It.IsAny<string>(),
                    It.IsAny<ExperimentGroupStatus?>(),
                    DateTime.MinValue,
                    DateTime.MaxValue,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(experimentGroupsResult));

            // Act
            var actual = await _model.GetExperimentGroups(username, ExperimentGroupStatus.Rolling, DateTime.MinValue, DateTime.MaxValue);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Should().BeOfType<Result<IEnumerable<PredictionRecordDto>, ErrorResult>>()
                .Which.Value.Should().BeEquivalentTo(experimentGroupDtos);
        }

        [TestMethod]
        public async Task GetExperimentGroups_DateTimeIsNull()
        {
            // Arrange
            var username = TestDataGenerationHelper.GenerateUserName();
            var experimentGroups = _testDataGenerator.GenerateListOfExperimentGroups();
            var experimentGroupDtos = _mapper.Map<IEnumerable<PredictionRecordDto>>(experimentGroups);
            var experimentGroupsResult = Result.Ok(experimentGroups);
            _repositoryMock.Setup(m => m.GetExperimentGroups(
                    It.IsAny<string>(),
                    It.IsAny<ExperimentGroupStatus?>(),
                    null,
                    null,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(experimentGroupsResult));

            // Act
            var actual = await _model.GetExperimentGroups(username, ExperimentGroupStatus.Rolling);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Should().BeOfType<Result<IEnumerable<PredictionRecordDto>, ErrorResult>>()
                .Which.Value.Should().BeEquivalentTo(experimentGroupDtos);
        }

        [TestMethod]
        public async Task GetExperimentGroups_StartDateIsNull()
        {
            // Arrange
            var username = TestDataGenerationHelper.GenerateUserName();
            var experimentGroups = _testDataGenerator.GenerateListOfExperimentGroups();
            var experimentGroupDtos = _mapper.Map<IEnumerable<PredictionRecordDto>>(experimentGroups);
            var experimentGroupsResult = Result.Ok(experimentGroups);
            _repositoryMock.Setup(m => m.GetExperimentGroups(
                    It.IsAny<string>(),
                    It.IsAny<ExperimentGroupStatus?>(),
                    null,
                    It.IsAny<DateTime?>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(experimentGroupsResult));

            // Act
            var actual = await _model.GetExperimentGroups(username, ExperimentGroupStatus.Rolling, null, DateTime.UtcNow);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Should().BeOfType<Result<IEnumerable<PredictionRecordDto>, ErrorResult>>()
                .Which.Value.Should().BeEquivalentTo(experimentGroupDtos);
        }

        [TestMethod]
        public async Task GetExperimentGroups_EndDateIsNull()
        {
            // Arrange
            var username = TestDataGenerationHelper.GenerateUserName();
            var experimentGroups = _testDataGenerator.GenerateListOfExperimentGroups();
            var experimentGroupDtos = _mapper.Map<IEnumerable<PredictionRecordDto>>(experimentGroups);
            var experimentGroupsResult = Result.Ok(experimentGroups);
            _repositoryMock.Setup(m => m.GetExperimentGroups(
                    It.IsAny<string>(),
                    It.IsAny<ExperimentGroupStatus?>(),
                    It.IsAny<DateTime?>(),
                    null,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(experimentGroupsResult));

            // Act
            var actual = await _model.GetExperimentGroups(username, ExperimentGroupStatus.Rolling, DateTime.UtcNow);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Should().BeOfType<Result<IEnumerable<PredictionRecordDto>, ErrorResult>>()
                .Which.Value.Should().BeEquivalentTo(experimentGroupDtos);
        }

        [TestMethod]
        public async Task GetExperimentGroups_AllParametersAreOptional_ReturnsOK()
        {
            // Arrange
            var experimentGroups =
                _testDataGenerator.GenerateListOfExperimentGroups();
            var experimentGroupDtos =
                _mapper.Map<IEnumerable<PredictionRecordDto>>(experimentGroups);
            var experimentGroupsResult = Result.Ok(experimentGroups);
            _repositoryMock.Setup(m => m.GetExperimentGroups(
                                It.IsAny<string>(),
                                It.IsAny<ExperimentGroupStatus?>(),
                                It.IsAny<DateTime?>(),
                                It.IsAny<DateTime?>(),
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<IEnumerable<string>>()))
                           .Returns(
                                Task.FromResult(experimentGroupsResult));

            // Act
            var actual =
                await _model.GetExperimentGroups();

            // Assert
            actual.IsFailure.Should().BeFalse();
            actual.Should()
                  .BeOfType<Result<IEnumerable<PredictionRecordDto>, ErrorResult>
                   >()
                  .Which.Value.Should().BeEquivalentTo(experimentGroupDtos);
        }

        [TestMethod]
        public async Task GetExperimentGroups_RepositoryFailure_ReturnsRepositoryError()
        {
            // Arrange
            var username = TestDataGenerationHelper.GenerateUserName();
            _repositoryMock.Setup(m => m.GetExperimentGroups(
                                It.IsAny<string>(),
                                It.IsAny<ExperimentGroupStatus?>(),
                                It.IsAny<DateTime?>(),
                                It.IsAny<DateTime?>(),
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<IEnumerable<string>>()))
                           .Returns(Task.FromResult(
                                Result.Fail<IEnumerable<ExperimentGroup>>(
                                    "DB error")));

            // Act
            var actual = await _model.GetExperimentGroups(username, ExperimentGroupStatus.Rolling);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.ErrorType.Should().Be(ErrorTypes.RepositoryError);
        }

        [TestMethod]
        public async Task GetExperimentGroupById_HappyPath()
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup =
                _testDataGenerator.GenerateExperimentGroup();
            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(
                                Task.FromResult(Result.Ok(experimentGroup)));

            // Act
            var actual =
                await _model.GetExperimentGroupById(experimentGroup.Value.Id);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Should()
                  .BeOfType<Result<PredictionRecordDto, ErrorResult>>()
                  .Which.Value.Username.Should()
                  .Be(experimentGroup.Value.Username);
        }

        [TestMethod]
        public async Task GetExperimentGroupById_HappyPath_WithSplitExperiments()
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup = _testDataGenerator.GenerateExperimentGroup(includeExperimentSplitId: true);
            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(
                                Task.FromResult(Result.Ok(experimentGroup)));

            // Act
            var actual =
                await _model.GetExperimentGroupById(experimentGroup.Value.Id);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Should()
                  .BeOfType<Result<PredictionRecordDto, ErrorResult>>()
                  .Which.Value.Username.Should()
                  .Be(experimentGroup.Value.Username);
        }

        [TestMethod]
        public async Task GetExperimentGroupById_AllParametersAreOptional_ReturnsOK()
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup =
                _testDataGenerator.GenerateExperimentGroup();
            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(
                                Task.FromResult(Result.Ok(experimentGroup)));

            // Act
            var actual =
                await _model.GetExperimentGroupById(experimentGroup.Value.Id);

            // Assert
            actual.IsFailure.Should().BeFalse();
            actual.Should()
                  .BeOfType<Result<PredictionRecordDto, ErrorResult>>()
                  .Which.Value.Username.Should()
                  .Be(experimentGroup.Value.Username);
        }

        [TestMethod]
        public async Task GetExperimentGroupById_RepositoryFailure_ReturnsRepositoryError()
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup =
                _testDataGenerator.GenerateExperimentGroup();
            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(
                                Result.Fail<Maybe<ExperimentGroup>>(
                                    "DB error")));

            // Act
            var actual = await _model.GetExperimentGroupById(Guid.NewGuid());

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.ErrorType.Should().Be(ErrorTypes.RepositoryError);
        }

        [TestMethod]
        public async Task GetExperimentGroupById_GroupNotFound_ReturnsNotFound()
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup =
                _testDataGenerator.GenerateExperimentGroup();
            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.None)));

            // Act
            var actual = await _model.GetExperimentGroupById(Guid.NewGuid());

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.ErrorType.Should().Be(ErrorTypes.NotFound);
        }

        [TestMethod]
        public async Task CreateExperimentGroup_HappyPath_WithSplitExperiments()
        {
            // Arrange
            var createdExperimentGroup = _testDataGenerator.GenerateExperimentGroup(includeExperimentSplitId: true);

            _repositoryMock.Setup(m => m.AddExperimentGroup(It.IsAny<ExperimentGroup>()))
                           .Returns(Task.FromResult(Result.Ok(createdExperimentGroup)));

            _submitter.Setup(m => m.SubmitExperimentGroup(It.IsAny<ExperimentGroupCreationDto>()))
                      .Returns(Task.FromResult(Result.Ok()));

            _repositoryMock.Setup(m => m.UpdateExperimentGroupSubmissionToQueueState(createdExperimentGroup.Id, true))
                           .Returns(Task.FromResult(Result.Ok()));

            var experimentGroupToCreate = _testDataGenerator.GenerateExperimentGroupForCreationDto();

            // Act
            var actual = await _model.CreateExperimentGroup(experimentGroupToCreate);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Should().BeOfType<Result<PredictionRecordDto, ErrorResult>>()
                  .Which.Value.Id.Should().Be(createdExperimentGroup.Id);
            _repositoryMock.Verify(m => m.AddExperimentGroup(It.IsAny<ExperimentGroup>()), Times.Once);
            _submitter.Verify(m => m.SubmitExperimentGroup(It.IsAny<ExperimentGroupCreationDto>()), Times.Once);
            _repositoryMock.Verify(m => m.UpdateExperimentGroupSubmissionToQueueState(createdExperimentGroup.Id, true), Times.Once);
            _repositoryMock.Verify(m => m.RemoveExperimentGroup(It.IsAny<Guid>()), Times.Never);
        }

        [TestMethod]
        public async Task CreateExperimentGroup_AddToRepositoryFailed_ReturnsRepositoryError()
        {
            // Arrange
            _repositoryMock.Setup(m => m.AddExperimentGroup(It.IsAny<ExperimentGroup>()))
                           .Returns(Task.FromResult(Result.Fail<ExperimentGroup>("failed to add to repository")));

            var experimentGroupToCreate = _testDataGenerator.GenerateExperimentGroupForCreationDto();

            // Act
            var actual = await _model.CreateExperimentGroup(experimentGroupToCreate);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<PredictionRecordDto, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.RepositoryError);
            _repositoryMock.Verify(m => m.AddExperimentGroup(It.IsAny<ExperimentGroup>()), Times.Once);
            _submitter.Verify(m => m.SubmitExperimentGroup(It.IsAny<ExperimentGroupCreationDto>()), Times.Never);
            _repositoryMock.Verify(m => m.UpdateExperimentGroupSubmissionToQueueState(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
            _repositoryMock.Verify(m => m.RemoveExperimentGroup(It.IsAny<Guid>()), Times.Never);
        }

        [TestMethod]
        public async Task CreateExperimentGroup_SubmissionToQueueFailed_ReturnsQueueError()
        {
            // Arrange
            var createdExperimentGroup = _testDataGenerator.GenerateExperimentGroup();

            _repositoryMock.Setup(m => m.AddExperimentGroup(It.IsAny<ExperimentGroup>()))
                           .Returns(Task.FromResult(Result.Ok(createdExperimentGroup)));

            _submitter.Setup(m => m.SubmitExperimentGroup(It.IsAny<ExperimentGroupCreationDto>()))
                      .Returns(Task.FromResult(Result.Fail("failed to submit")));

            _repositoryMock.Setup(m => m.RemoveExperimentGroup(createdExperimentGroup.Id))
                           .Returns(Task.FromResult(Result.Ok()));

            var experimentGroupToCreate = _testDataGenerator.GenerateExperimentGroupForCreationDto();

            // Act
            var actual = await _model.CreateExperimentGroup(experimentGroupToCreate);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<PredictionRecordDto, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.QueueError);
            _repositoryMock.Verify(m => m.AddExperimentGroup(It.IsAny<ExperimentGroup>()), Times.Once);
            _submitter.Verify(m => m.SubmitExperimentGroup(It.IsAny<ExperimentGroupCreationDto>()), Times.Once);
            _repositoryMock.Verify(m => m.UpdateExperimentGroupSubmissionToQueueState(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
            _repositoryMock.Verify(m => m.RemoveExperimentGroup(createdExperimentGroup.Id), Times.Once);
        }

        [TestMethod]
        public async Task CreateExperimentGroup_SubmissionToQueueFailedRemoveFromRepositoryFailed_ReturnsRepositoryError()
        {
            // Arrange
            var createdExperimentGroup = _testDataGenerator.GenerateExperimentGroup();

            _repositoryMock.Setup(m => m.AddExperimentGroup(It.IsAny<ExperimentGroup>()))
                           .Returns(Task.FromResult(Result.Ok(createdExperimentGroup)));

            _submitter.Setup(m => m.SubmitExperimentGroup(It.IsAny<ExperimentGroupCreationDto>()))
                      .Returns(Task.FromResult(Result.Fail("failed to submit")));

            _repositoryMock.Setup(m => m.RemoveExperimentGroup(createdExperimentGroup.Id))
                           .Returns(Task.FromResult(Result.Fail("failed to remove from repo")));

            var experimentGroupToCreate = _testDataGenerator.GenerateExperimentGroupForCreationDto();

            // Act
            var actual = await _model.CreateExperimentGroup(experimentGroupToCreate);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<PredictionRecordDto, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.RepositoryError);
            _repositoryMock.Verify(m => m.AddExperimentGroup(It.IsAny<ExperimentGroup>()), Times.Once);
            _submitter.Verify(m => m.SubmitExperimentGroup(It.IsAny<ExperimentGroupCreationDto>()), Times.Once);
            _repositoryMock.Verify(m => m.UpdateExperimentGroupSubmissionToQueueState(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
            _repositoryMock.Verify(m => m.RemoveExperimentGroup(createdExperimentGroup.Id), Times.Once);
        }

        [TestMethod]
        public async Task CreateExperimentGroup_UpdateSubmissionToQueueStateFailed_ReturnsRepositoryError()
        {
            // Arrange
            var createdExperimentGroup = _testDataGenerator.GenerateExperimentGroup();

            _repositoryMock.Setup(m => m.AddExperimentGroup(It.IsAny<ExperimentGroup>()))
                           .Returns(Task.FromResult(Result.Ok(createdExperimentGroup)));

            _submitter.Setup(m => m.SubmitExperimentGroup(It.IsAny<ExperimentGroupCreationDto>()))
                      .Returns(Task.FromResult(Result.Ok()));

            _repositoryMock.Setup(m => m.UpdateExperimentGroupSubmissionToQueueState(createdExperimentGroup.Id, true))
                           .Returns(Task.FromResult(Result.Fail("failed to update submission to queue state")));

            var experimentGroupToCreate = _testDataGenerator.GenerateExperimentGroupForCreationDto();

            // Act
            var actual = await _model.CreateExperimentGroup(experimentGroupToCreate);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<PredictionRecordDto, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.RepositoryError);
            _repositoryMock.Verify(m => m.AddExperimentGroup(It.IsAny<ExperimentGroup>()), Times.Once);
            _submitter.Verify(m => m.SubmitExperimentGroup(It.IsAny<ExperimentGroupCreationDto>()), Times.Once);
            _repositoryMock.Verify(m => m.UpdateExperimentGroupSubmissionToQueueState(createdExperimentGroup.Id, true), Times.Once);
            _repositoryMock.Verify(m => m.RemoveExperimentGroup(It.IsAny<Guid>()), Times.Never);
        }

        [TestMethod]
        public async Task GetTopCommonLocationCodes_HappyFlow()
        {
            // Arrange
            var result = new List<uint>();
            _repositoryMock.Setup(m => m.GetTopCommonLocationCodes(
                    It.IsAny<string>(),
                    It.IsAny<uint>()))
                .Returns(Task.FromResult(Result.Ok<IEnumerable<uint>>(result)));

            // Act
            var actual = await _model.GetTopCommonLocationCodes("programFamily", 2);

            // Assert
            actual.IsFailure.Should().BeFalse();
            actual.Should()
                .BeOfType<Result<IEnumerable<uint>, ErrorResult>>()
                .Which.Value.Should().BeEquivalentTo(result);
        }

        [TestMethod]
        public async Task GetTopCommonLocationCodes_RepositoryFailure_ReturnsRepositoryError()
        {
            // Arrange
            var result = new List<string>();
            _repositoryMock.Setup(m => m.GetTopCommonLocationCodes(
                    It.IsAny<string>(),
                    It.IsAny<uint>()))
                .Returns(() => Task.FromResult(Result.Fail<IEnumerable<uint>>("DB error")));

            // Act
            var actual = await _model.GetTopCommonLocationCodes("programFamily", 2);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.ErrorType.Should().Be(ErrorTypes.RepositoryError);
        }

        [TestMethod]
        public async Task GetTopCommonEngineeringIds_HappyFlow()
        {
            // Arrange
            var result = new List<string>();
            _repositoryMock.Setup(m => m.GetTopCommonEngineeringIds(
                    It.IsAny<string>(),
                    It.IsAny<uint>()))
                .Returns(Task.FromResult(Result.Ok<IEnumerable<string>>(result)));

            // Act
            var actual = await _model.GetTopCommonEngineeringIds("programFamily", 2);

            // Assert
            actual.IsFailure.Should().BeFalse();
            actual.Should()
                .BeOfType<Result<IEnumerable<string>, ErrorResult>>()
                .Which.Value.Should().BeEquivalentTo(result);
        }

        [TestMethod]
        public async Task GetTopCommonEngineeringIds_RepositoryFailure_ReturnsRepositoryError()
        {
            // Arrange
            var result = new List<string>();
            _repositoryMock.Setup(m => m.GetTopCommonEngineeringIds(
                    It.IsAny<string>(),
                    It.IsAny<uint>()))
                .Returns(() => Task.FromResult(Result.Fail<IEnumerable<string>>("DB error")));

            // Act
            var actual = await _model.GetTopCommonEngineeringIds("programFamily", 2);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.ErrorType.Should().Be(ErrorTypes.RepositoryError);
        }

        [TestMethod]
        [DataRow(ExperimentState.Draft, ExperimentState.Draft)]
        [DataRow(ExperimentState.Ready, ExperimentState.Ready)]
        public async Task UpdateExperimentGroup_HappyPath(ExperimentState experimentState, ExperimentState expectedStateForOrchestratorCall)
        {
            // Arrange
            var experimentGroupForEdit = _testDataGenerator.GenerateExperimentGroupForEdit();
            var groupFromRepo = _testDataGenerator.GenerateExperimentGroup();
            groupFromRepo.Experiments = groupFromRepo.Experiments.Select(e =>
            {
                e.ExperimentState = experimentState;
                return e;
            });

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.From(groupFromRepo))));

            _experimentGroupModificationValidator.Setup(m => m.CanUpdateExperimentGroup(It.IsAny<ExperimentGroup>(), It.IsAny<IEnumerable<Guid>>()))
                                             .Returns(Result.Ok());

            _submitter.Setup(m => m.SubmitUpdateExperimentGroup(It.IsAny<Dtos.Orchestrator.ExperimentGroupUpdateDtos.ExperimentGroupForUpdateDto>()))
                      .Returns(Task.FromResult(Result.Ok()));

            _repositoryMock.Setup(m => m.UpdateExperimentGroup(It.IsAny<Guid>(), It.IsAny<ExperimentGroup>()))
                           .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.UpdateExperimentGroup(groupFromRepo.Id, experimentGroupForEdit);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Should().BeOfType<Result<PredictionRecordDto, ErrorResult>>()
                  .Which.Value.Id.Should().Be(groupFromRepo.Id);
            _repositoryMock.Verify(m => m.UpdateExperimentGroup(It.IsAny<Guid>(), It.IsAny<ExperimentGroup>()), Times.Once);
            _submitter.Verify(
                m => m.SubmitUpdateExperimentGroup(
                It.Is<Dtos.Orchestrator.ExperimentGroupUpdateDtos.ExperimentGroupForUpdateDto>(
                    eg => eg.Experiments.All(e => e.ExperimentState == expectedStateForOrchestratorCall))), Times.Once);
        }

        [TestMethod]
        public async Task UpdateExperimentGroup_FailedToGetExperimentGroup_ThrowsRepositoryError()
        {
            // Arrange
            var experimentGroupForEdit = _testDataGenerator.GenerateExperimentGroupForEdit();
            Maybe<ExperimentGroup> groupFromRepo = _testDataGenerator.GenerateExperimentGroup();

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Fail<Maybe<ExperimentGroup>>(
                                    "DB error")));

            // Act
            var actual = await _model.UpdateExperimentGroup(groupFromRepo.Value.Id, experimentGroupForEdit);

            // Assert
            actual.IsSuccess.Should().BeFalse();
            actual.Error.Should().BeOfType<ErrorResult>().Which.ErrorType.Should().Be(ErrorTypes.RepositoryError);
            _experimentGroupModificationValidator.Verify(e => e.CanUpdateExperimentGroup(It.IsAny<ExperimentGroup>(), It.IsAny<IEnumerable<Guid>>()), Times.Never);
            _submitter.Verify(m => m.SubmitUpdateExperimentGroup(It.IsAny<Dtos.Orchestrator.ExperimentGroupUpdateDtos.ExperimentGroupForUpdateDto>()), Times.Never);
            _repositoryMock.Verify(m => m.UpdateExperimentGroup(It.IsAny<Guid>(), It.IsAny<ExperimentGroup>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateExperimentGroup_CantUpdateExperimentGroup_ThrowsValidationFailed()
        {
            // Arrange
            var experimentGroupForEdit = _testDataGenerator.GenerateExperimentGroupForEdit();
            Maybe<ExperimentGroup> groupFromRepo = _testDataGenerator.GenerateExperimentGroup();

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok(groupFromRepo)));

            _experimentGroupModificationValidator.Setup(m => m.CanUpdateExperimentGroup(It.IsAny<ExperimentGroup>(), It.IsAny<IEnumerable<Guid>>()))
                                             .Returns(Result.Fail("Some conditions are not"));

            // Act
            var actual = await _model.UpdateExperimentGroup(groupFromRepo.Value.Id, experimentGroupForEdit);

            // Assert
            actual.IsSuccess.Should().BeFalse();
            actual.Error.Should().BeOfType<ErrorResult>().Which.ErrorType.Should().Be(ErrorTypes.ValidationFailed);
            _submitter.Verify(m => m.SubmitUpdateExperimentGroup(It.IsAny<Dtos.Orchestrator.ExperimentGroupUpdateDtos.ExperimentGroupForUpdateDto>()), Times.Never);
            _repositoryMock.Verify(m => m.UpdateExperimentGroup(It.IsAny<Guid>(), It.IsAny<ExperimentGroup>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateExperimentGroup_SubmitPatchFailed_ThrowsQueueError()
        {
            // Arrange
            var experimentGroupForEdit = _testDataGenerator.GenerateExperimentGroupForEdit();
            Maybe<ExperimentGroup> groupFromRepo = _testDataGenerator.GenerateExperimentGroup();

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok(groupFromRepo)));

            _experimentGroupModificationValidator.Setup(m => m.CanUpdateExperimentGroup(It.IsAny<ExperimentGroup>(), It.IsAny<IEnumerable<Guid>>()))
                                             .Returns(Result.Ok());

            _submitter.Setup(m => m.SubmitUpdateExperimentGroup(It.IsAny<Dtos.Orchestrator.ExperimentGroupUpdateDtos.ExperimentGroupForUpdateDto>()))
                      .Returns(Task.FromResult(Result.Fail("Failed to send")));

            // Act
            var actual = await _model.UpdateExperimentGroup(groupFromRepo.Value.Id, experimentGroupForEdit);

            // Assert
            actual.IsSuccess.Should().BeFalse();
            actual.Error.Should().BeOfType<ErrorResult>().Which.ErrorType.Should().Be(ErrorTypes.ExternalServerError);
            _repositoryMock.Verify(m => m.UpdateExperimentGroup(It.IsAny<Guid>(), It.IsAny<ExperimentGroup>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateExperimentGroup_UpdateOnRepositoryFailed_ThrowsRepositoryError()
        {
            // Arrange
            var experimentGroupForEdit = _testDataGenerator.GenerateExperimentGroupForEdit();
            Maybe<ExperimentGroup> groupFromRepo = _testDataGenerator.GenerateExperimentGroup();

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok(groupFromRepo)));

            _experimentGroupModificationValidator.Setup(m => m.CanUpdateExperimentGroup(It.IsAny<ExperimentGroup>(), It.IsAny<IEnumerable<Guid>>()))
                                             .Returns(Result.Ok());

            _submitter.Setup(m => m.SubmitUpdateExperimentGroup(It.IsAny<Dtos.Orchestrator.ExperimentGroupUpdateDtos.ExperimentGroupForUpdateDto>()))
                      .Returns(Task.FromResult(Result.Ok()));

            _repositoryMock.Setup(m => m.UpdateExperimentGroup(It.IsAny<Guid>(), It.IsAny<ExperimentGroup>()))
                           .Returns(Task.FromResult(Result.Fail("Failed to update experiment")));

            // Act
            var actual = await _model.UpdateExperimentGroup(groupFromRepo.Value.Id, experimentGroupForEdit);

            // Assert
            actual.IsSuccess.Should().BeFalse();
            actual.Error.Should().BeOfType<ErrorResult>().Which.Error.StartsWith("Failed to update experiment");
            actual.Error.Should().BeOfType<ErrorResult>().Which.ErrorType.Should().Be(ErrorTypes.ExternalServerError);
        }
    }
}