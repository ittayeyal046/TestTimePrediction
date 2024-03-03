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
using TTPService.Dtos.Creation;
using TTPService.Dtos.Update;
using TTPService.Entities;
using TTPService.Enums;
using TTPService.FunctionalExtensions;
using TTPService.Models;
using TTPService.Models.Validators;
using TTPService.Queue;
using TTPService.Repositories;
using ExperimentForUpdateDto = TTPService.Dtos.Update.ExperimentForUpdateOperationsDto;
using VoidResult = TTPService.FunctionalExtensions.VoidResult;

namespace TTPService.Tests.Models
{
    [TestClass]
    public class ExperimentsModelFixtureL0
    {
        private static IMapper _mapper;
        private Mock<IRepository> _repositoryMock;
        private ExperimentsModel _model;
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
            _experimentGroupModificationValidator =
                new Mock<IExperimentGroupModificationValidator>();
            _submitter = new Mock<ISubmitter>();

            _model = new ExperimentsModel(
                _repositoryMock.Object,
                Mock.Of<ILogger<ExperimentsModel>>(),
                _mapper,
                _submitter.Object,
                _experimentGroupModificationValidator.Object);
        }

        [TestMethod]
        public async Task GetExperiments_HappyPath()
        {
            // Arrange
            var experimentGroupFromRepo = _testDataGenerator.GenerateExperimentGroup();
            _repositoryMock.Setup(m => m.GetExperimentGroup(experimentGroupFromRepo.Id))
                           .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.From(experimentGroupFromRepo))));
            var experimentGroupDto = _mapper.Map<PredictionRecordDto>(experimentGroupFromRepo);

            // Act
            var actual = await _model.GetExperiments(
                experimentGroupFromRepo.Id,
                new List<Guid>()
                {
                    experimentGroupFromRepo.Experiments.First().Id,
                });

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Should().BeOfType<Result<IEnumerable<ExperimentDto>, ErrorResult>>()
                  .Which.Value.Should().BeEquivalentTo(experimentGroupDto.Experiments);
        }

        [TestMethod]
        public async Task GetExperiments_2Experiments1ExistingRequested_Only1ShouldBeReturned()
        {
            // Arrange
            var experimentGroupFromRepo = _testDataGenerator.GenerateExperimentGroup(2);
            _repositoryMock.Setup(m => m.GetExperimentGroup(experimentGroupFromRepo.Id))
                           .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.From(experimentGroupFromRepo))));
            var experimentGroupDto = _mapper.Map<PredictionRecordDto>(experimentGroupFromRepo);

            // Act
            var actual = await _model.GetExperiments(
                experimentGroupFromRepo.Id,
                new List<Guid>()
                {
                    experimentGroupFromRepo.Experiments.Skip(1).First().Id,
                });

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Should().BeOfType<Result<IEnumerable<ExperimentDto>, ErrorResult>>()
                  .Which.Value.Should().BeEquivalentTo(
                       experimentGroupDto.Experiments.Except(new List<ExperimentDto>() { experimentGroupDto.Experiments.First() }));
        }

        [TestMethod]
        public async Task GetExperiments_2ExistingExperimentsRequested_2ExperimentsReturned()
        {
            // Arrange
            var experimentGroupFromRepo = _testDataGenerator.GenerateExperimentGroup(3);
            _repositoryMock.Setup(m => m.GetExperimentGroup(experimentGroupFromRepo.Id))
                           .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.From(experimentGroupFromRepo))));
            var experimentGroupDto = _mapper.Map<PredictionRecordDto>(experimentGroupFromRepo);

            // Act
            var actual = await _model.GetExperiments(
                experimentGroupFromRepo.Id,
                experimentGroupFromRepo.Experiments.Take(2).Select(e => e.Id));

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Should().BeOfType<Result<IEnumerable<ExperimentDto>, ErrorResult>>()
                  .Which.Value.Should().BeEquivalentTo(
                       experimentGroupDto.Experiments.Take(2));
        }

        [TestMethod]
        public async Task GetExperiments_2ExperimentsNoIdsRequested_AllShouldBeReturned()
        {
            // Arrange
            var experimentGroupFromRepo = _testDataGenerator.GenerateExperimentGroup(2);
            _repositoryMock.Setup(m => m.GetExperimentGroup(experimentGroupFromRepo.Id))
                           .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.From(experimentGroupFromRepo))));
            var experimentGroupDto = _mapper.Map<PredictionRecordDto>(experimentGroupFromRepo);

            // Act
            var actual = await _model.GetExperiments(
                experimentGroupFromRepo.Id,
                new List<Guid>());

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Should().BeOfType<Result<IEnumerable<ExperimentDto>, ErrorResult>>()
                  .Which.Value.Should().BeEquivalentTo(experimentGroupDto.Experiments);
        }

        [TestMethod]
        public async Task GetExperiments_2ExperimentsRequestedOnly1Found_1ExperimentReturned()
        {
            // Arrange
            var experimentGroupFromRepo = _testDataGenerator.GenerateExperimentGroup(2);
            _repositoryMock.Setup(m => m.GetExperimentGroup(experimentGroupFromRepo.Id))
                           .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.From(experimentGroupFromRepo))));
            var experimentGroupDto = _mapper.Map<PredictionRecordDto>(experimentGroupFromRepo);

            // Act
            var actual = await _model.GetExperiments(
                experimentGroupFromRepo.Id,
                new List<Guid>()
                {
                    experimentGroupFromRepo.Experiments.First().Id,
                    Guid.NewGuid(),
                });

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Should().BeOfType<Result<IEnumerable<ExperimentDto>, ErrorResult>>()
                  .Which.Value.Should().BeEquivalentTo(experimentGroupDto.Experiments.Take(1));
        }

        [TestMethod]
        public async Task GetExperiments_2ExperimentsRequestedNoneFound_EmptyCollectionReturned()
        {
            // Arrange
            var experimentGroupFromRepo = _testDataGenerator.GenerateExperimentGroup();
            _repositoryMock.Setup(m => m.GetExperimentGroup(experimentGroupFromRepo.Id))
                           .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.From(experimentGroupFromRepo))));

            // Act
            var actual = await _model.GetExperiments(
                experimentGroupFromRepo.Id,
                new List<Guid>()
                {
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                });

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Should().BeOfType<Result<IEnumerable<ExperimentDto>, ErrorResult>>()
                  .Which.Value.Should().BeEquivalentTo(new List<Experiment>());
        }

        [TestMethod]
        public async Task GetExperiments_NoExperiments1Requested_EmptyCollectionReturned()
        {
            // Arrange
            var experimentGroupFromRepo = _testDataGenerator.GenerateExperimentGroup(0);
            _repositoryMock.Setup(m => m.GetExperimentGroup(experimentGroupFromRepo.Id))
                           .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.From(experimentGroupFromRepo))));

            // Act
            var actual = await _model.GetExperiments(experimentGroupFromRepo.Id, new List<Guid>() { Guid.NewGuid() });

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Should().BeOfType<Result<IEnumerable<ExperimentDto>, ErrorResult>>()
                  .Which.Value.Should().BeEquivalentTo(new List<ExperimentGroup>());
        }

        [TestMethod]
        public async Task GetExperiments_GetFromRepositoryFailed_ReturnsRepositoryError()
        {
            // Arrange
            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                    .Returns(Task.FromResult(Result.Fail<Maybe<ExperimentGroup>>("Failed to get from repo")));

            // Act
            var actual = await _model.GetExperiments(Guid.NewGuid(), new List<Guid>());

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<IEnumerable<ExperimentDto>, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.RepositoryError);
        }

        [TestMethod]
        public async Task GetExperiments_ExperimentGroupNotFoundInRepository_ReturnsNotFound()
        {
            // Arrange
            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.None)));

            // Act
            var actual = await _model.GetExperiments(Guid.NewGuid(), new List<Guid>());

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<IEnumerable<ExperimentDto>, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.NotFound);
        }

        [TestMethod]
        public async Task AddNewExperiments_HappyPath()
        {
            // Arrange
            var experimentGroupFromRepo = _testDataGenerator.GenerateExperimentGroup(1);
            var experimentsToCreate = _testDataGenerator.GenerateListOfExperimentsForCreationDtos(2);
            var experimentGroupWorkFlowUpdateDto = _testDataGenerator.GenerateExperimentGroupWorkFlowUpdateDto();

            _repositoryMock.Setup(m => m.GetExperimentGroup(experimentGroupFromRepo.Id))
                           .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.From(experimentGroupFromRepo))));

            _experimentGroupModificationValidator.Setup(m => m.CanAddExperiments(experimentGroupFromRepo))
                                             .Returns(Result.Ok());

            _repositoryMock.Setup(m => m.UpdateExperimentGroup(experimentGroupFromRepo))
                           .Returns(Task.FromResult(Result.Ok()));

            _submitter.Setup(m => m.SubmitUpdateExperimentGroup(It.IsAny<Dtos.Orchestrator.ExperimentGroupUpdateDtos.ExperimentGroupForUpdateDto>()))
                      .Returns(Task.FromResult(Result.Ok()));

            _repositoryMock.Setup(m => m.UpdateExperimentGroupSubmissionToQueueState(experimentGroupFromRepo.Id, true))
                           .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.AddNewExperiments(experimentGroupFromRepo.Id, experimentsToCreate);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Should().BeOfType<Result<IEnumerable<ExperimentDto>, ErrorResult>>()
                  .Which.Value.Count().Should().Be(2);

            _repositoryMock.Verify(m => m.GetExperimentGroup(experimentGroupFromRepo.Id), Times.Once);
            _experimentGroupModificationValidator.Verify(m => m.CanAddExperiments(experimentGroupFromRepo), Times.Once);
            _repositoryMock.Verify(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()), Times.Once);
            _submitter.Verify(m => m.SubmitUpdateExperimentGroup(It.IsAny<Dtos.Orchestrator.ExperimentGroupUpdateDtos.ExperimentGroupForUpdateDto>()), Times.Once);
            _repositoryMock.Verify(m => m.UpdateExperimentGroupSubmissionToQueueState(experimentGroupFromRepo.Id, true), Times.Once);
        }

        [TestMethod]
        public async Task AddNewExperiments_GroupWith1ExperimentsAdd1Experiment_Returns2Experiment()
        {
            // Arrange
            var experimentGroupFromRepo = _testDataGenerator.GenerateExperimentGroup(1);
            var experimentsToCreate = _testDataGenerator.GenerateListOfExperimentsForCreationDtos(1);

            _repositoryMock.Setup(m => m.GetExperimentGroup(experimentGroupFromRepo.Id))
                           .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.From(experimentGroupFromRepo))));

            _experimentGroupModificationValidator.Setup(m => m.CanAddExperiments(experimentGroupFromRepo))
                                             .Returns(Result.Ok());

            _repositoryMock.Setup(m => m.UpdateExperimentGroup(experimentGroupFromRepo))
                           .Returns(Task.FromResult(Result.Ok()));

            _submitter.Setup(m => m.SubmitUpdateExperimentGroup(It.IsAny<Dtos.Orchestrator.ExperimentGroupUpdateDtos.ExperimentGroupForUpdateDto>()))
                      .Returns(Task.FromResult(Result.Ok()));

            _repositoryMock.Setup(m => m.UpdateExperimentGroupSubmissionToQueueState(experimentGroupFromRepo.Id, true))
                           .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.AddNewExperiments(experimentGroupFromRepo.Id, experimentsToCreate);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Should().BeOfType<Result<IEnumerable<ExperimentDto>, ErrorResult>>()
                  .Which.Value.Count().Should().Be(1);

            _repositoryMock.Verify(m => m.GetExperimentGroup(experimentGroupFromRepo.Id), Times.Once);
            _experimentGroupModificationValidator.Verify(m => m.CanAddExperiments(experimentGroupFromRepo), Times.Once);
            _repositoryMock.Verify(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()), Times.Once);
            _submitter.Verify(m => m.SubmitUpdateExperimentGroup(It.IsAny<Dtos.Orchestrator.ExperimentGroupUpdateDtos.ExperimentGroupForUpdateDto>()), Times.Once);
            _repositoryMock.Verify(m => m.UpdateExperimentGroupSubmissionToQueueState(experimentGroupFromRepo.Id, true), Times.Once);
        }

        [TestMethod]
        public async Task AddNewExperiments_Add0Experiments_ReturnsBadRequest()
        {
            // Arrange
            var experimentGroupFromRepo = _testDataGenerator.GenerateExperimentGroup(1);

            // Act
            var actual = await _model.AddNewExperiments(experimentGroupFromRepo.Id, new List<ExperimentForCreationDto>());

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<IEnumerable<ExperimentDto>, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.BadRequest);

            _repositoryMock.Verify(m => m.GetExperimentGroup(It.IsAny<Guid>()), Times.Never);
            _experimentGroupModificationValidator.Verify(m => m.CanAddExperiments(It.IsAny<ExperimentGroup>()), Times.Never);
            _repositoryMock.Verify(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()), Times.Never);
            _submitter.Verify(m => m.SubmitUpdateExperimentGroup(It.IsAny<Dtos.Orchestrator.ExperimentGroupUpdateDtos.ExperimentGroupForUpdateDto>()), Times.Never);
            _repositoryMock.Verify(m => m.UpdateExperimentGroupSubmissionToQueueState(It.IsAny<Guid>(), true), Times.Never);
        }

        [TestMethod]
        public async Task AddNewExperiments_FindExperimentGroupFailed_ReturnsRepositoryError()
        {
            // Arrange
            var experimentGroupFromRepo = _testDataGenerator.GenerateExperimentGroup();
            var experimentsToCreate = _testDataGenerator.GenerateListOfExperimentsForCreationDtos();

            _repositoryMock.Setup(m => m.GetExperimentGroup(experimentGroupFromRepo.Id))
                           .Returns(Task.FromResult(Result.Fail<Maybe<ExperimentGroup>>("failed to get from repo")));

            // Act
            var actual = await _model.AddNewExperiments(experimentGroupFromRepo.Id, experimentsToCreate);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<IEnumerable<ExperimentDto>, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.RepositoryError);

            _repositoryMock.Verify(m => m.GetExperimentGroup(experimentGroupFromRepo.Id), Times.Once);
            _experimentGroupModificationValidator.Verify(m => m.CanAddExperiments(experimentGroupFromRepo), Times.Never);
            _repositoryMock.Verify(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()), Times.Never);
            _submitter.Verify(m => m.SubmitUpdateExperimentGroup(It.IsAny<Dtos.Orchestrator.ExperimentGroupUpdateDtos.ExperimentGroupForUpdateDto>()), Times.Never);
            _repositoryMock.Verify(m => m.UpdateExperimentGroupSubmissionToQueueState(It.IsAny<Guid>(), true), Times.Never);
        }

        [TestMethod]
        public async Task AddNewExperiments_ExperimentGroupNotFound_ReturnsNotFound()
        {
            // Arrange
            var experimentGroupFromRepo = _testDataGenerator.GenerateExperimentGroup();
            var experimentsToCreate = _testDataGenerator.GenerateListOfExperimentsForCreationDtos();

            _repositoryMock.Setup(m => m.GetExperimentGroup(experimentGroupFromRepo.Id))
                           .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.None)));

            // Act
            var actual = await _model.AddNewExperiments(experimentGroupFromRepo.Id, experimentsToCreate);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<IEnumerable<ExperimentDto>, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.NotFound);

            _repositoryMock.Verify(m => m.GetExperimentGroup(experimentGroupFromRepo.Id), Times.Once);
            _experimentGroupModificationValidator.Verify(m => m.CanAddExperiments(experimentGroupFromRepo), Times.Never);
            _repositoryMock.Verify(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()), Times.Never);
            _submitter.Verify(m => m.SubmitUpdateExperimentGroup(It.IsAny<Dtos.Orchestrator.ExperimentGroupUpdateDtos.ExperimentGroupForUpdateDto>()), Times.Never);
            _repositoryMock.Verify(m => m.UpdateExperimentGroupSubmissionToQueueState(It.IsAny<Guid>(), true), Times.Never);
        }

        [TestMethod]
        public async Task AddNewExperiments_ExperimentsCannotBeAdded_ReturnsValidationFailed()
        {
            // Arrange
            var experimentGroupFromRepo = _testDataGenerator.GenerateExperimentGroup();
            var experimentsToCreate = _testDataGenerator.GenerateListOfExperimentsForCreationDtos();

            _repositoryMock.Setup(m => m.GetExperimentGroup(experimentGroupFromRepo.Id))
                           .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.From(experimentGroupFromRepo))));

            _experimentGroupModificationValidator.Setup(m => m.CanAddExperiments(experimentGroupFromRepo))
                                                 .Returns(Result.Fail("can't add to experiment group"));

            // Act
            var actual = await _model.AddNewExperiments(experimentGroupFromRepo.Id, experimentsToCreate);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<IEnumerable<ExperimentDto>, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.ValidationFailed);

            _repositoryMock.Verify(m => m.GetExperimentGroup(experimentGroupFromRepo.Id), Times.Once);
            _experimentGroupModificationValidator.Verify(m => m.CanAddExperiments(experimentGroupFromRepo), Times.Once);
            _repositoryMock.Verify(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()), Times.Never);
            _submitter.Verify(m => m.SubmitUpdateExperimentGroup(It.IsAny<Dtos.Orchestrator.ExperimentGroupUpdateDtos.ExperimentGroupForUpdateDto>()), Times.Never);
            _repositoryMock.Verify(m => m.UpdateExperimentGroupSubmissionToQueueState(It.IsAny<Guid>(), true), Times.Never);
        }

        [TestMethod]
        public async Task AddNewExperiments_UpdateRepositoryFailed_ReturnsRepositoryError()
        {
            // Arrange
            var experimentGroupFromRepo = _testDataGenerator.GenerateExperimentGroup();
            var experimentsToCreate = _testDataGenerator.GenerateListOfExperimentsForCreationDtos();

            _repositoryMock.Setup(m => m.GetExperimentGroup(experimentGroupFromRepo.Id))
                           .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.From(experimentGroupFromRepo))));

            _experimentGroupModificationValidator.Setup(m => m.CanAddExperiments(experimentGroupFromRepo))
                                                 .Returns(Result.Ok());

            _repositoryMock.Setup(m => m.UpdateExperimentGroup(experimentGroupFromRepo))
                           .Returns(Task.FromResult(Result.Fail("failed to update repo")));

            // Act
            var actual = await _model.AddNewExperiments(experimentGroupFromRepo.Id, experimentsToCreate);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<IEnumerable<ExperimentDto>, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.RepositoryError);

            _repositoryMock.Verify(m => m.GetExperimentGroup(experimentGroupFromRepo.Id), Times.Once);
            _experimentGroupModificationValidator.Verify(m => m.CanAddExperiments(experimentGroupFromRepo), Times.Once);
            _repositoryMock.Verify(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()), Times.Once);
            _submitter.Verify(m => m.SubmitUpdateExperimentGroup(It.IsAny<Dtos.Orchestrator.ExperimentGroupUpdateDtos.ExperimentGroupForUpdateDto>()), Times.Never);
            _repositoryMock.Verify(m => m.UpdateExperimentGroupSubmissionToQueueState(It.IsAny<Guid>(), true), Times.Never);
        }

        [TestMethod]
        public async Task AddNewExperiments_SubmissionToQueueFailed_ReturnsQueueError()
        {
            // Arrange
            var experimentGroupFromRepo = _testDataGenerator.GenerateExperimentGroup();
            var experimentsToCreate = _testDataGenerator.GenerateListOfExperimentsForCreationDtos();

            _repositoryMock.Setup(m => m.GetExperimentGroup(experimentGroupFromRepo.Id))
                           .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.From(experimentGroupFromRepo))));

            _experimentGroupModificationValidator.Setup(m => m.CanAddExperiments(experimentGroupFromRepo))
                                             .Returns(Result.Ok());

            _repositoryMock.Setup(m => m.UpdateExperimentGroup(experimentGroupFromRepo))
                           .Returns(Task.FromResult(Result.Ok()));

            _submitter.Setup(m => m.SubmitUpdateExperimentGroup(It.IsAny<Dtos.Orchestrator.ExperimentGroupUpdateDtos.ExperimentGroupForUpdateDto>()))
                      .Returns(Task.FromResult(Result.Fail("failed to submit to queue")));

            // Act
            var actual = await _model.AddNewExperiments(experimentGroupFromRepo.Id, experimentsToCreate);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<IEnumerable<ExperimentDto>, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.QueueError);

            _repositoryMock.Verify(m => m.GetExperimentGroup(experimentGroupFromRepo.Id), Times.Once);
            _experimentGroupModificationValidator.Verify(m => m.CanAddExperiments(experimentGroupFromRepo), Times.Once);
            _repositoryMock.Verify(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()), Times.Once);
            _submitter.Verify(m => m.SubmitUpdateExperimentGroup(It.IsAny<Dtos.Orchestrator.ExperimentGroupUpdateDtos.ExperimentGroupForUpdateDto>()), Times.Once);
            _repositoryMock.Verify(m => m.UpdateExperimentGroupSubmissionToQueueState(It.IsAny<Guid>(), true), Times.Never);
        }

        [TestMethod]
        public async Task AddNewExperiments_UpdateRepositorySubmissionToQueueStateFailed_ReturnsRepositoryError()
        {
            // Arrange
            var experimentGroupFromRepo = _testDataGenerator.GenerateExperimentGroup();
            var experimentsToCreate = _testDataGenerator.GenerateListOfExperimentsForCreationDtos();

            _repositoryMock.Setup(m => m.GetExperimentGroup(experimentGroupFromRepo.Id))
                           .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.From(experimentGroupFromRepo))));

            _experimentGroupModificationValidator.Setup(m => m.CanAddExperiments(experimentGroupFromRepo))
                                             .Returns(Result.Ok());

            _repositoryMock.Setup(m => m.UpdateExperimentGroup(experimentGroupFromRepo))
                           .Returns(Task.FromResult(Result.Ok()));

            _submitter.Setup(m => m.SubmitUpdateExperimentGroup(It.IsAny<Dtos.Orchestrator.ExperimentGroupUpdateDtos.ExperimentGroupForUpdateDto>()))
                      .Returns(Task.FromResult(Result.Ok()));

            _repositoryMock.Setup(m => m.UpdateExperimentGroupSubmissionToQueueState(experimentGroupFromRepo.Id, true))
                           .Returns(Task.FromResult(Result.Fail("failed to update submission state in repo")));

            // Act
            var actual = await _model.AddNewExperiments(experimentGroupFromRepo.Id, experimentsToCreate);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<IEnumerable<ExperimentDto>, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.RepositoryError);

            _repositoryMock.Verify(m => m.GetExperimentGroup(experimentGroupFromRepo.Id), Times.Once);
            _experimentGroupModificationValidator.Verify(m => m.CanAddExperiments(experimentGroupFromRepo), Times.Once);
            _repositoryMock.Verify(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()), Times.Once);
            _submitter.Verify(m => m.SubmitUpdateExperimentGroup(It.IsAny<Dtos.Orchestrator.ExperimentGroupUpdateDtos.ExperimentGroupForUpdateDto>()), Times.Once);
            _repositoryMock.Verify(m => m.UpdateExperimentGroupSubmissionToQueueState(experimentGroupFromRepo.Id, true), Times.Once);
        }

        [TestMethod]
        public async Task Cancel_HappyPath_UpdatesStatusToCancelingOnlyForCancelableConditions()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentId = Guid.NewGuid();
            var statusComment = "some comment";
            var updateExperimentDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = new[] { experimentId },
                Comment = statusComment,
            };
            var group = new ExperimentGroup()
            {
                Id = groupId,
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        Id = experimentId,
                        Stages = new List<Stage>()
                        {
                            new Stage()
                            {
                                StageType = StageType.Class,
                                StageData = new ClassStageData()
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition() { Status = ProcessStatus.NotStarted},
                                        new Condition() { Status = ProcessStatus.PendingCommit},
                                        new Condition() { Status = ProcessStatus.Committed},
                                        new Condition() { Status = ProcessStatus.Running},
                                        new Condition() { Status = ProcessStatus.Paused},
                                        new Condition() { Status = ProcessStatus.Resuming},
                                        new Condition() { Status = ProcessStatus.Canceling},
                                        new Condition() { Status = ProcessStatus.Canceled},
                                        new Condition() { Status = ProcessStatus.Completed},
                                    },
                                },
                            },
                            new Stage()
                            {
                                StageType = StageType.Olb,
                                StageData = new OLBStageData()
                                {
                                   Status = ProcessStatus.NotStarted,
                                   Comment = "Superman and Spiderman are best friends",
                                   MoveUnits = MoveUnits.All,
                                   Operation = "operation",
                                   Qdf = "qdf1",
                                   Recipe = "recipe",
                                },
                            },
                            new Stage()
                            {
                                StageType = StageType.Ppv,
                                StageData = new PPVStageData()
                                {
                                    Status = ProcessStatus.NotStarted,
                                    Comment = "captain America and Iron Man are very popular",
                                    Operation = "operation",
                                    Qdf = "qdf1",
                                    Recipe = "recipe",
                                },
                            },
                            new Stage()
                            {
                                StageType = StageType.Maestro,
                                StageData = new MaestroStageData()
                                {
                                    Status = ProcessStatus.NotStarted,
                                    Comment = "captain America and Iron Man are very popular",
                                    Operation = "operation",
                                },
                            },
                        },
                    },
                },
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok<Maybe<ExperimentGroup>>(group)));
            _repositoryMock.Setup(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()))
                           .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.Cancel(groupId, updateExperimentDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            _repositoryMock.Verify(m => m.GetExperimentGroup(groupId), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentGroup(
                    It.Is<ExperimentGroup>(
                        e => e.Id == groupId &&
                        e.Experiments.First().Id == experimentId &&
                        e.Experiments.First().Stages.First().StageData.As<ClassStageData>().Conditions.Take(6).Select(c => c.StatusChangeComment)
                         .All(comment => comment == statusComment) &&
                        e.Experiments.First().Stages.First().StageData.As<ClassStageData>().Conditions.Select(c => c.Status).SequenceEqual(
                            new List<ProcessStatus>
                            {
                                ProcessStatus.Canceling,
                                ProcessStatus.Canceling,
                                ProcessStatus.Canceling,
                                ProcessStatus.Canceling,
                                ProcessStatus.Canceling,
                                ProcessStatus.Canceling,
                                ProcessStatus.Canceling,
                                ProcessStatus.Canceled,
                                ProcessStatus.Completed,
                            }))),
                Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentGroup(
                    It.Is<ExperimentGroup>(e => e.Id == groupId &&
                    e.Experiments.First().Id == experimentId &&
                        e.Experiments.First().Stages.First(s => s.StageType == StageType.Olb).StageData.As<OLBStageData>().StatusChangeComment == statusComment &&
                        e.Experiments.First().Stages.First(s => s.StageType == StageType.Olb).StageData.As<OLBStageData>().Status == ProcessStatus.Canceling)),
                Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentGroup(
                    It.Is<ExperimentGroup>(e => e.Id == groupId &&
                    e.Experiments.First().Id == experimentId &&
                        e.Experiments.First().Stages.First(s => s.StageType == StageType.Ppv).StageData.As<PPVStageData>().StatusChangeComment == statusComment &&
                        e.Experiments.First().Stages.First(s => s.StageType == StageType.Ppv).StageData.As<PPVStageData>().Status == ProcessStatus.Canceling)),
                Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentGroup(
                    It.Is<ExperimentGroup>(e => e.Id == groupId &&
                    e.Experiments.First().Id == experimentId &&
                        e.Experiments.First().Stages.First(s => s.StageType == StageType.Maestro).StageData.As<MaestroStageData>().StatusChangeComment == statusComment &&
                        e.Experiments.First().Stages.First(s => s.StageType == StageType.Maestro).StageData.As<MaestroStageData>().Status == ProcessStatus.Canceling)),
                Times.Once);
        }

        [TestMethod]
        public async Task Cancel_SeveralExperimentsHappyPath_UpdatesStatusToCancelingOnlyForCancelableConditions()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentId1 = Guid.Parse("50007b5a-22dc-4703-9411-4c0433555af2");
            var experimentId2 = Guid.Parse("bf8dce69-b5c2-47e9-adaf-520d7ea0c6cc");
            var experimentId3 = Guid.Parse("8b9cd7af-0e89-420e-99f1-d3488577671f");
            var experimentId4 = Guid.Parse("f5e5d094-93d0-4576-bc28-32be068d3f12");
            var statusComment = "some comment";
            var updateExperimentDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = new[] { experimentId1, experimentId2, experimentId3, experimentId4 },
                Comment = statusComment,
            };
            var group = new ExperimentGroup()
            {
                Id = groupId,
                Experiments = new List<Experiment>
                {
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId1, ProcessStatus.Canceling, ProcessStatus.Resuming, ProcessStatus.Canceled, ProcessStatus.Completed),
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId2, ProcessStatus.Resuming),
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId3, ProcessStatus.PendingCommit, ProcessStatus.Committed, ProcessStatus.Completed),
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId4, ProcessStatus.Canceled, ProcessStatus.Canceled, ProcessStatus.Completed), // no update expected here.
                },
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok<Maybe<ExperimentGroup>>(group)));
            _repositoryMock.Setup(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()))
                           .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.Cancel(groupId, updateExperimentDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            _repositoryMock.Verify(m => m.GetExperimentGroup(groupId), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentGroup(
                    It.Is<ExperimentGroup>(
                        e => e.Id == groupId &&
                        MatchExperimentAndConditionsStatus(e, experimentId1, statusComment, new[] { 1 }, ProcessStatus.Canceling, ProcessStatus.Canceling, ProcessStatus.Canceled, ProcessStatus.Completed) &&
                        MatchExperimentAndConditionsStatus(e, experimentId2, statusComment, new[] { 0 }, ProcessStatus.Canceling) &&
                        MatchExperimentAndConditionsStatus(e, experimentId3, statusComment, new[] { 0, 1 }, ProcessStatus.Canceling, ProcessStatus.Canceling, ProcessStatus.Completed) &&
                        MatchExperimentAndConditionsStatus(e, experimentId4, statusComment, new int[] { }, ProcessStatus.Canceled, ProcessStatus.Canceled, ProcessStatus.Completed))),
                Times.Once);
        }

        [TestMethod]
        public async Task Cancel_NoneOfTheRequestCanBeCancelled_ValidationErrorReturned()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentId1 = Guid.Parse("50007b5a-22dc-4703-9411-4c0433555af2");
            var experimentId2 = Guid.Parse("bf8dce69-b5c2-47e9-adaf-520d7ea0c6cc");
            var experimentId3 = Guid.Parse("8b9cd7af-0e89-420e-99f1-d3488577671f");
            var experimentId4 = Guid.Parse("f5e5d094-93d0-4576-bc28-32be068d3f12");
            var statusComment = "some comment";
            var updateExperimentDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = new[] { experimentId1, experimentId2, experimentId3, experimentId4 },
                Comment = statusComment,
            };
            var group = new ExperimentGroup()
            {
                Id = groupId,
                Experiments = new List<Experiment>
                {
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId1, ProcessStatus.Canceling, ProcessStatus.Canceled),
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId2, ProcessStatus.Canceled),
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId3, ProcessStatus.Completed, ProcessStatus.Completed, ProcessStatus.Completed),
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId4, ProcessStatus.Canceled, ProcessStatus.Canceled, ProcessStatus.Completed),
                },
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok<Maybe<ExperimentGroup>>(group)));

            // Act
            var actual = await _model.Cancel(groupId, updateExperimentDto);

            // Assert
            actual.IsSuccess.Should().BeFalse();
            actual.Should().BeOfType<Result<VoidResult, ErrorResult>>()
                 .Which.Error.ErrorType.Should().Be(ErrorTypes.ValidationFailed);
            actual.Should().BeOfType<Result<VoidResult, ErrorResult>>()
                 .Which.Error.Error.Should().Be($"No experiments were updated in group {groupId}.");
            _repositoryMock.Verify(m => m.GetExperimentGroup(groupId), Times.Once);
            _repositoryMock.Verify(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()), Times.Never);
        }

        [TestMethod]
        public async Task Cancel_NoneOfTheExperimentsFound_NotFoundErrorReturned()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentId1 = Guid.Parse("50007b5a-22dc-4703-9411-4c0433555af2");
            var experimentId2 = Guid.Parse("bf8dce69-b5c2-47e9-adaf-520d7ea0c6cc");
            var experimentId3 = Guid.Parse("8b9cd7af-0e89-420e-99f1-d3488577671f");
            var experimentId4 = Guid.Parse("f5e5d094-93d0-4576-bc28-32be068d3f12");
            var statusComment = "some comment";
            var updateExperimentDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = new[] { Guid.NewGuid(), Guid.NewGuid(), },
                Comment = statusComment,
            };
            var group = new ExperimentGroup()
            {
                Id = groupId,
                Experiments = new List<Experiment>
                {
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId1, ProcessStatus.Canceling, ProcessStatus.Canceled),
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId2, ProcessStatus.Canceled),
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId3, ProcessStatus.Completed, ProcessStatus.Completed, ProcessStatus.Completed),
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId4, ProcessStatus.Canceled, ProcessStatus.Canceled, ProcessStatus.Completed),
                },
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok<Maybe<ExperimentGroup>>(group)));

            // Act
            var actual = await _model.Cancel(groupId, updateExperimentDto);

            // Assert
            actual.IsSuccess.Should().BeFalse();
            actual.Should().BeOfType<Result<VoidResult, ErrorResult>>()
                 .Which.Error.ErrorType.Should().Be(ErrorTypes.NotFound);
            _repositoryMock.Verify(m => m.GetExperimentGroup(groupId), Times.Once);
            _repositoryMock.Verify(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()), Times.Never);
        }

        [TestMethod]
        public async Task Cancel_PartOfTheExperimentsFound_ValidationErrorReturned()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentId1 = Guid.Parse("50007b5a-22dc-4703-9411-4c0433555af2");
            var experimentId2 = Guid.Parse("bf8dce69-b5c2-47e9-adaf-520d7ea0c6cc");
            var notFoundId = Guid.Parse("f5e5d094-93d0-4576-bc28-32be068d3f12");
            var statusComment = "some comment";
            var updateExperimentDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = new[] { experimentId1, notFoundId, },
                Comment = statusComment,
            };
            var group = new ExperimentGroup()
            {
                Id = groupId,
                Experiments = new List<Experiment>
                {
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId1, ProcessStatus.Canceling, ProcessStatus.Canceled),
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId2, ProcessStatus.Canceled),
                },
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok<Maybe<ExperimentGroup>>(group)));

            // Act
            var actual = await _model.Cancel(groupId, updateExperimentDto);

            // Assert
            actual.IsSuccess.Should().BeFalse();
            actual.Should().BeOfType<Result<VoidResult, ErrorResult>>()
                 .Which.Error.ErrorType.Should().Be(ErrorTypes.ValidationFailed);
            actual.Should().BeOfType<Result<VoidResult, ErrorResult>>()
                 .Which.Error.Error.Should().Be($"Could not find all experiments {notFoundId} in group {groupId}.");
            _repositoryMock.Verify(m => m.GetExperimentGroup(groupId), Times.Once);
            _repositoryMock.Verify(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()), Times.Never);
        }

        [TestMethod]
        public async Task Cancel_GroupDoesntExist_ReturnsNotFound()
        {
            // Arrange
            var updateExperimentDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = new[] { Guid.NewGuid() },
                Comment = "some comment",
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.None)));
            _repositoryMock.Setup(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()))
                           .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.Cancel(Guid.NewGuid(), updateExperimentDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<VoidResult, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.NotFound);
            _repositoryMock.Verify(m => m.GetExperimentGroup(It.IsAny<Guid>()), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Cancel_ExperimentDoesntExist_ReturnsNotFound()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentId = Guid.NewGuid();
            var updateExperimentDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = new[] { Guid.NewGuid() },
                Comment = "some comment",
            };
            var group = new ExperimentGroup()
            {
                Id = groupId,
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        Id = experimentId,
                        Stages = new List<Stage>
                        {
                            new Stage()
                            {
                                StageData = new OLBStageData
                                {
                                    Status = ProcessStatus.PendingCommit,
                                },
                            },
                        },
                    },
                },
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok<Maybe<ExperimentGroup>>(group)));
            _repositoryMock.Setup(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()))
                           .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.Cancel(groupId, updateExperimentDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<VoidResult, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.NotFound);
            _repositoryMock.Verify(m => m.GetExperimentGroup(It.IsAny<Guid>()), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Cancel_RepositoryGetFailure_ReturnsRepositoryError()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentId = Guid.NewGuid();
            var updateExperimentDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = new[] { experimentId },
                Comment = "some comment",
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Fail<Maybe<ExperimentGroup>>("failed to get from repo")));
            _repositoryMock.Setup(m => m.UpdateExperiment(
                                It.IsAny<Guid>(),
                                It.IsAny<Experiment>()))
                           .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.Cancel(groupId, updateExperimentDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<VoidResult, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.RepositoryError);
            _repositoryMock.Verify(m => m.GetExperimentGroup(It.IsAny<Guid>()), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Cancel_RepositoryUpdateFailure_ReturnsRepositoryError()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentId = Guid.NewGuid();
            var updateExperimentDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = new[] { experimentId },
                Comment = "some comment",
            };
            var group = new ExperimentGroup()
            {
                Id = groupId,
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        Id = experimentId,
                        Stages = new List<Stage>
                        {
                            new Stage()
                            {
                                StageType = StageType.Olb,
                                StageData = new OLBStageData
                                {
                                    Status = ProcessStatus.PendingCommit,
                                },
                            },
                        },
                    },
                },
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok<Maybe<ExperimentGroup>>(group)));
            _repositoryMock.Setup(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()))
                           .Returns(Task.FromResult(Result.Fail("fail to update")));

            // Act
            var actual = await _model.Cancel(groupId, updateExperimentDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<VoidResult, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.RepositoryError);
            _repositoryMock.Verify(m => m.GetExperimentGroup(It.IsAny<Guid>()), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()),
                Times.Once);
        }

        [TestMethod]
        public async Task Cancel_AllStagesNotCancelable_ReturnsValidationError()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentId = Guid.NewGuid();
            var updateExperimentDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = new[] { experimentId },
                Comment = "some comment",
            };
            var group = new ExperimentGroup()
            {
                Id = groupId,
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        Id = experimentId,
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition
                                        {
                                            Status = ProcessStatus.Canceling,
                                        },
                                    },
                                },
                            },
                            new Stage
                            {
                                StageType = StageType.Ppv,
                                StageData = new PPVStageData
                                {
                                    Status = ProcessStatus.Canceled,
                                },
                            },
                        },
                    },
                },
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok<Maybe<ExperimentGroup>>(group)));
            _repositoryMock.Setup(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()))
                           .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.Cancel(groupId, updateExperimentDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<VoidResult, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.ValidationFailed);
            _repositoryMock.Verify(m => m.GetExperimentGroup(It.IsAny<Guid>()), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperiment(It.IsAny<Guid>(), It.IsAny<Experiment>()),
                Times.Never);
        }

        [TestMethod]
        public async Task UpdateState_HappyPath_UpdatesStateToDraft()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentsId = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() };
            var updateExperimentStateDto = new ExperimentsStateForUpdateDto()
            {
                ExperimentIds = experimentsId,
                ExperimentState = ExperimentState.Draft,
            };
            var group = new ExperimentGroup()
            {
                Id = groupId,
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        Id = updateExperimentStateDto.ExperimentIds.ToArray()[0],
                        ExperimentState = ExperimentState.Draft,
                    },
                    new Experiment
                    {
                        Id = updateExperimentStateDto.ExperimentIds.ToArray()[1],
                        ExperimentState = ExperimentState.Draft,
                    },
                },
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok<Maybe<ExperimentGroup>>(group)));
            _repositoryMock.Setup(m => m.UpdateExperimentGroup(
                               It.IsAny<ExperimentGroup>()))
                          .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.UpdateExperimentsState(groupId, updateExperimentStateDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            _repositoryMock.Verify(m => m.GetExperimentGroup(groupId), Times.Once);
            _repositoryMock.Verify(m => m.UpdateExperimentGroup(It.Is<ExperimentGroup>(eg => eg.Experiments.FirstOrDefault().ExperimentState == ExperimentState.Draft)), Times.Once);
        }

        [TestMethod]
        public async Task UpdateState_ReturnsError_ExperimentsIdsNotFoundInGroup()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentsId = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() };
            var updateExperimentStateDto = new ExperimentsStateForUpdateDto()
            {
                ExperimentIds = experimentsId,
                ExperimentState = ExperimentState.Draft,
            };
            var group = new ExperimentGroup()
            {
                Id = groupId,
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        Id = Guid.NewGuid(),
                        ExperimentState = ExperimentState.Draft,
                    },
                    new Experiment
                    {
                        Id = Guid.NewGuid(),
                        ExperimentState = ExperimentState.Draft,
                    },
                },
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok<Maybe<ExperimentGroup>>(group)));
            _repositoryMock.Setup(m => m.UpdateExperimentGroup(
                               It.IsAny<ExperimentGroup>()))
                          .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.UpdateExperimentsState(groupId, updateExperimentStateDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.Error.Should().Be("_");
            _repositoryMock.Verify(m => m.GetExperimentGroup(groupId), Times.Once);
            _repositoryMock.Verify(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateState_ReturnsError_ExperimentsFoundCount_DoesNotMatchInputIdsCount()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentsId = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() };
            var updateExperimentStateDto = new ExperimentsStateForUpdateDto()
            {
                ExperimentIds = experimentsId,
                ExperimentState = ExperimentState.Draft,
            };
            var group = new ExperimentGroup()
            {
                Id = groupId,
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        Id = updateExperimentStateDto.ExperimentIds.ToArray()[0],
                        ExperimentState = ExperimentState.Draft,
                    },
                    new Experiment
                    {
                        Id = Guid.NewGuid(),
                        ExperimentState = ExperimentState.Draft,
                    },
                },
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok<Maybe<ExperimentGroup>>(group)));
            _repositoryMock.Setup(m => m.UpdateExperimentGroup(
                               It.IsAny<ExperimentGroup>()))
                          .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.UpdateExperimentsState(groupId, updateExperimentStateDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.Error.Should().StartWith("Could not find all experiments");
            _repositoryMock.Verify(m => m.GetExperimentGroup(groupId), Times.Once);
            _repositoryMock.Verify(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateState_ExperimentGroupNotFound()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentsId = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() };
            var updateExperimentStateDto = new ExperimentsStateForUpdateDto()
            {
                ExperimentIds = experimentsId,
            };
            var group = new ExperimentGroup()
            {
                Id = groupId,
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        Id = updateExperimentStateDto.ExperimentIds.ToArray()[0],
                        ExperimentState = ExperimentState.Draft,
                    },
                    new Experiment
                    {
                        Id = updateExperimentStateDto.ExperimentIds.ToArray()[1],
                        ExperimentState = ExperimentState.Draft,
                    },
                },
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.None)));
            _repositoryMock.Setup(m => m.UpdateExperimentGroup(
                               It.IsAny<ExperimentGroup>()))
                          .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.UpdateExperimentsState(groupId, updateExperimentStateDto);

            // Assert
            actual.IsSuccess.Should().BeFalse();
            _repositoryMock.Verify(m => m.GetExperimentGroup(groupId), Times.Once);
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<VoidResult, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.NotFound);
            _repositoryMock.Verify(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()), Times.Never);
        }

        [TestMethod]
        public async Task Delete_HappyPath_ArchivesAndUpdatesStatusToCancelingOnlyForCancelableConditions()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentId = Guid.NewGuid();
            var statusComment = "some comment";
            var updateExperimentDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = new[] { experimentId },
                Comment = "some comment",
            };
            var group = new ExperimentGroup()
            {
                Id = groupId,
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        Id = experimentId,
                        Stages = new List<Stage>()
                        {
                            new Stage()
                            {
                                StageType = StageType.Class,
                                StageData = new ClassStageData()
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition() { Status = ProcessStatus.NotStarted},
                                        new Condition() { Status = ProcessStatus.PendingCommit},
                                        new Condition() { Status = ProcessStatus.Committed},
                                        new Condition() { Status = ProcessStatus.Running},
                                        new Condition() { Status = ProcessStatus.Paused},
                                        new Condition() { Status = ProcessStatus.Resuming},
                                        new Condition() { Status = ProcessStatus.Canceling},
                                        new Condition() { Status = ProcessStatus.Canceled},
                                        new Condition() { Status = ProcessStatus.Completed},
                                    },
                                },
                            },
                            new Stage()
                            {
                                StageType = StageType.Olb,
                                StageData = new OLBStageData()
                                {
                                   Status = ProcessStatus.NotStarted,
                                   Comment = "Superman and Spiderman are best friends",
                                   MoveUnits = MoveUnits.All,
                                   Operation = "operation",
                                   Qdf = "qdf1",
                                   Recipe = "recipe",
                                },
                            },
                            new Stage()
                            {
                                StageType = StageType.Ppv,
                                StageData = new PPVStageData()
                                {
                                    Status = ProcessStatus.NotStarted,
                                    Comment = "captain America and Iron Man are very popular",
                                    Operation = "operation",
                                    Qdf = "qdf1",
                                    Recipe = "recipe",
                                },
                            },
                            new Stage()
                            {
                                StageType = StageType.Maestro,
                                StageData = new MaestroStageData()
                                {
                                    Status = ProcessStatus.NotStarted,
                                    Comment = "captain America and Iron Man are very popular",
                                    Operation = "operation",
                                },
                            },
                        },
                    },
                },
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok<Maybe<ExperimentGroup>>(group)));
            _repositoryMock.Setup(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()))
                           .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.Delete(groupId, updateExperimentDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            _repositoryMock.Verify(m => m.GetExperimentGroup(groupId), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentGroup(
                    It.Is<ExperimentGroup>(e => e.Id == groupId &&
                    e.Experiments.First().Id == experimentId &&
                    e.Experiments.First().IsArchived &&
                        e.Experiments.First().Stages.First().StageData.As<ClassStageData>().Conditions.Take(6).Select(c => c.StatusChangeComment)
                         .All(comment => comment == statusComment) &&
                        e.Experiments.First().Stages.First().StageData.As<ClassStageData>().Conditions.Select(c => c.Status).SequenceEqual(
                            new List<ProcessStatus>
                            {
                                ProcessStatus.Canceling,
                                ProcessStatus.Canceling,
                                ProcessStatus.Canceling,
                                ProcessStatus.Canceling,
                                ProcessStatus.Canceling,
                                ProcessStatus.Canceling,
                                ProcessStatus.Canceling,
                                ProcessStatus.Canceled,
                                ProcessStatus.Completed,
                            }))),
                Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentGroup(
                    It.Is<ExperimentGroup>(e => e.Id == groupId &&
                    e.Experiments.First().Id == experimentId &&
                    e.Experiments.First().IsArchived &&
                        e.Experiments.First().Stages.First(s => s.StageType == StageType.Olb).StageData.As<OLBStageData>().StatusChangeComment == statusComment &&
                        e.Experiments.First().Stages.First(s => s.StageType == StageType.Olb).StageData.As<OLBStageData>().Status == ProcessStatus.Canceling)),
                Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentGroup(
                    It.Is<ExperimentGroup>(e => e.Id == groupId &&
                    e.Experiments.First().Id == experimentId &&
                    e.Experiments.First().IsArchived &&
                        e.Experiments.First().Stages.First(s => s.StageType == StageType.Ppv).StageData.As<PPVStageData>().StatusChangeComment == statusComment &&
                        e.Experiments.First().Stages.First(s => s.StageType == StageType.Ppv).StageData.As<PPVStageData>().Status == ProcessStatus.Canceling)),
                Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentGroup(
                    It.Is<ExperimentGroup>(e => e.Id == groupId &&
                    e.Experiments.First().Id == experimentId &&
                    e.Experiments.First().IsArchived &&
                        e.Experiments.First().Stages.First(s => s.StageType == StageType.Maestro).StageData.As<MaestroStageData>().StatusChangeComment == statusComment &&
                        e.Experiments.First().Stages.First(s => s.StageType == StageType.Maestro).StageData.As<MaestroStageData>().Status == ProcessStatus.Canceling)),
                Times.Once);
        }

        [TestMethod]
        public async Task Delete_GroupDoesntExist_ReturnsNotFound()
        {
            // Arrange
            var updateExperimentDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = new[] { Guid.NewGuid() },
                Comment = "some comment",
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.None)));
            _repositoryMock.Setup(m => m.UpdateExperiment(
                                It.IsAny<Guid>(),
                                It.IsAny<Experiment>()))
                           .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.Delete(Guid.NewGuid(), updateExperimentDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<VoidResult, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.NotFound);
            _repositoryMock.Verify(m => m.GetExperimentGroup(It.IsAny<Guid>()), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperiment(It.IsAny<Guid>(), It.IsAny<Experiment>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Delete_ExperimentDoesntExist_ReturnsNotFound()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentId = Guid.NewGuid();
            var updateExperimentDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = new[] { Guid.NewGuid() },
                Comment = "some comment",
            };
            var group = new ExperimentGroup()
            {
                Id = groupId,
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        Id = experimentId,
                        Stages = new List<Stage>
                        {
                            new Stage()
                            {
                                StageData = new OLBStageData
                                {
                                    Status = ProcessStatus.PendingCommit,
                                },
                            },
                        },
                    },
                },
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok<Maybe<ExperimentGroup>>(group)));
            _repositoryMock.Setup(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()))
                           .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.Delete(groupId, updateExperimentDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<VoidResult, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.NotFound);
            _repositoryMock.Verify(m => m.GetExperimentGroup(It.IsAny<Guid>()), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperiment(It.IsAny<Guid>(), It.IsAny<Experiment>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Delete_RepositoryGetFailure_ReturnsRepositoryError()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentId = Guid.NewGuid();
            var updateExperimentDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = new[] { experimentId },
                Comment = "some comment",
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Fail<Maybe<ExperimentGroup>>("failed to get from repo")));
            _repositoryMock.Setup(m => m.UpdateExperiment(
                                It.IsAny<Guid>(),
                                It.IsAny<Experiment>()))
                           .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.Delete(groupId, updateExperimentDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<VoidResult, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.RepositoryError);
            _repositoryMock.Verify(m => m.GetExperimentGroup(It.IsAny<Guid>()), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperiment(It.IsAny<Guid>(), It.IsAny<Experiment>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Delete_RepositoryUpdateFailure_ReturnsRepositoryError()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentId = Guid.NewGuid();
            var updateExperimentDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = new[] { experimentId },
                Comment = "some comment",
            };
            var group = new ExperimentGroup()
            {
                Id = groupId,
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        Id = experimentId,
                        Stages = new List<Stage>
                        {
                            new Stage()
                            {
                                StageType = StageType.Olb,
                                StageData = new OLBStageData
                                {
                                    Status = ProcessStatus.PendingCommit,
                                },
                            },
                        },
                    },
                },
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok<Maybe<ExperimentGroup>>(group)));
            _repositoryMock.Setup(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()))
                           .Returns(Task.FromResult(Result.Fail("fail to update")));

            // Act
            var actual = await _model.Delete(groupId, updateExperimentDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<VoidResult, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.RepositoryError);
            _repositoryMock.Verify(m => m.GetExperimentGroup(It.IsAny<Guid>()), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()),
                Times.Once);
        }

        [TestMethod]
        public async Task Delete_AllConditionsNotCancelable_ShouldArchiveAndReturnOk()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentId = Guid.NewGuid();
            var updateExperimentDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = new[] { experimentId },
                Comment = "some comment",
            };
            var group = new ExperimentGroup()
            {
                Id = groupId,
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        Id = experimentId,
                        Stages = new List<Stage>()
                        {
                            new Stage()
                            {
                                StageType = StageType.Class,
                                StageData = new ClassStageData()
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition {Status = ProcessStatus.Canceling},
                                        new Condition {Status = ProcessStatus.Canceled},
                                        new Condition {Status = ProcessStatus.Completed},
                                    },
                                },
                            },
                        },
                    },
                },
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok<Maybe<ExperimentGroup>>(group)));
            _repositoryMock.Setup(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()))
                           .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.Delete(groupId, updateExperimentDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            _repositoryMock.Verify(m => m.GetExperimentGroup(groupId), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentGroup(
                    It.Is<ExperimentGroup>(e => e.Id == groupId &&
                    e.Experiments.First().Id == experimentId && e.Experiments.First().IsArchived &&
                        e.Experiments.First().Stages.First().StageData.As<ClassStageData>().Conditions.Select(c => c.Status).SequenceEqual(
                            new List<ProcessStatus>
                            {
                                ProcessStatus.Canceling,
                                ProcessStatus.Canceled,
                                ProcessStatus.Completed,
                            }))),
                Times.Once);
        }

        [TestMethod]
        public async Task Delete_SeveralExperimentsHappyPath_ArchivesAndUpdatesStatusToCancelingOnlyForCancelableConditions()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentId1 = Guid.Parse("50007b5a-22dc-4703-9411-4c0433555af2");
            var experimentId2 = Guid.Parse("bf8dce69-b5c2-47e9-adaf-520d7ea0c6cc");
            var experimentId3 = Guid.Parse("8b9cd7af-0e89-420e-99f1-d3488577671f");
            var experimentId4 = Guid.Parse("f5e5d094-93d0-4576-bc28-32be068d3f12");
            var statusComment = "some comment";
            var updateExperimentDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = new[] { experimentId1, experimentId2, experimentId3, experimentId4 },
                Comment = statusComment,
            };
            var group = new ExperimentGroup()
            {
                Id = groupId,
                Experiments = new List<Experiment>
                {
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId1, ProcessStatus.Canceling, ProcessStatus.Resuming, ProcessStatus.Canceled, ProcessStatus.Completed),
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId2, ProcessStatus.Resuming),
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId3, ProcessStatus.PendingCommit, ProcessStatus.Committed, ProcessStatus.Completed),
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId4, ProcessStatus.Canceled, ProcessStatus.Canceled, ProcessStatus.Completed),
                },
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok<Maybe<ExperimentGroup>>(group)));
            _repositoryMock.Setup(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()))
                           .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.Delete(groupId, updateExperimentDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            _repositoryMock.Verify(m => m.GetExperimentGroup(groupId), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentGroup(
                    It.Is<ExperimentGroup>(
                        e => e.Id == groupId &&
                        e.Experiments.All(ex => ex.IsArchived) &&
                        MatchExperimentAndConditionsStatus(e, experimentId1, statusComment, new[] { 1 }, ProcessStatus.Canceling, ProcessStatus.Canceling, ProcessStatus.Canceled, ProcessStatus.Completed) &&
                        MatchExperimentAndConditionsStatus(e, experimentId2, statusComment, new[] { 0 }, ProcessStatus.Canceling) &&
                        MatchExperimentAndConditionsStatus(e, experimentId3, statusComment, new[] { 0, 1 }, ProcessStatus.Canceling, ProcessStatus.Canceling, ProcessStatus.Completed) &&
                        MatchExperimentAndConditionsStatus(e, experimentId4, statusComment, new int[] { }, ProcessStatus.Canceled, ProcessStatus.Canceled, ProcessStatus.Completed))),
                Times.Once);
        }

        [TestMethod]
        public async Task Delete_NoneOfTheRequestCanBeCancelled_ShouldArchiveAndReturnOk()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentId1 = Guid.Parse("50007b5a-22dc-4703-9411-4c0433555af2");
            var experimentId2 = Guid.Parse("bf8dce69-b5c2-47e9-adaf-520d7ea0c6cc");
            var experimentId3 = Guid.Parse("8b9cd7af-0e89-420e-99f1-d3488577671f");
            var experimentId4 = Guid.Parse("f5e5d094-93d0-4576-bc28-32be068d3f12");
            var statusComment = "some comment";
            var updateExperimentDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = new[] { experimentId1, experimentId2, experimentId3, experimentId4 },
                Comment = statusComment,
            };
            var group = new ExperimentGroup()
            {
                Id = groupId,
                Experiments = new List<Experiment>
                {
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId1, ProcessStatus.Canceling, ProcessStatus.Canceled),
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId2, ProcessStatus.Canceled),
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId3, ProcessStatus.Completed, ProcessStatus.Completed, ProcessStatus.Completed),
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId4, ProcessStatus.Canceled, ProcessStatus.Canceled, ProcessStatus.Completed),
                },
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok<Maybe<ExperimentGroup>>(group)));
            _repositoryMock.Setup(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()))
                           .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.Delete(groupId, updateExperimentDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            _repositoryMock.Verify(m => m.GetExperimentGroup(groupId), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentGroup(
                    It.Is<ExperimentGroup>(
                        e => e.Id == groupId &&
                        e.Experiments.All(ex => ex.IsArchived) &&
                        MatchExperimentAndConditionsStatus(e, experimentId1, statusComment, new int[] { }, ProcessStatus.Canceling, ProcessStatus.Canceled) &&
                        MatchExperimentAndConditionsStatus(e, experimentId2, statusComment, new int[] { }, ProcessStatus.Canceled) &&
                        MatchExperimentAndConditionsStatus(e, experimentId3, statusComment, new int[] { }, ProcessStatus.Completed, ProcessStatus.Completed, ProcessStatus.Completed) &&
                        MatchExperimentAndConditionsStatus(e, experimentId4, statusComment, new int[] { }, ProcessStatus.Canceled, ProcessStatus.Canceled, ProcessStatus.Completed))),
                Times.Once);
        }

        [TestMethod]
        public async Task Delete_NoneOfTheExperimentsFound_NotFoundErrorReturned()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentId1 = Guid.Parse("50007b5a-22dc-4703-9411-4c0433555af2");
            var experimentId2 = Guid.Parse("bf8dce69-b5c2-47e9-adaf-520d7ea0c6cc");
            var experimentId3 = Guid.Parse("8b9cd7af-0e89-420e-99f1-d3488577671f");
            var experimentId4 = Guid.Parse("f5e5d094-93d0-4576-bc28-32be068d3f12");
            var statusComment = "some comment";
            var updateExperimentDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = new[] { Guid.NewGuid(), Guid.NewGuid(), },
                Comment = statusComment,
            };
            var group = new ExperimentGroup()
            {
                Id = groupId,
                Experiments = new List<Experiment>
                {
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId1, ProcessStatus.Canceling, ProcessStatus.Canceled),
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId2, ProcessStatus.Canceled),
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId3, ProcessStatus.Completed, ProcessStatus.Completed, ProcessStatus.Completed),
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId4, ProcessStatus.Canceled, ProcessStatus.Canceled, ProcessStatus.Completed),
                },
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok<Maybe<ExperimentGroup>>(group)));

            // Act
            var actual = await _model.Delete(groupId, updateExperimentDto);

            // Assert
            actual.IsSuccess.Should().BeFalse();
            actual.Should().BeOfType<Result<VoidResult, ErrorResult>>()
                 .Which.Error.ErrorType.Should().Be(ErrorTypes.NotFound);
            _repositoryMock.Verify(m => m.GetExperimentGroup(groupId), Times.Once);
            _repositoryMock.Verify(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()), Times.Never);
        }

        [TestMethod]
        public async Task Delete_PartOfTheExperimentsFound_ValidationErrorReturned()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentId1 = Guid.Parse("50007b5a-22dc-4703-9411-4c0433555af2");
            var experimentId2 = Guid.Parse("bf8dce69-b5c2-47e9-adaf-520d7ea0c6cc");
            var notFoundId = Guid.Parse("f5e5d094-93d0-4576-bc28-32be068d3f12");
            var statusComment = "some comment";
            var updateExperimentDto = new ExperimentsForUpdateOperationsDto()
            {
                ExperimentIds = new[] { experimentId1, notFoundId, },
                Comment = statusComment,
            };
            var group = new ExperimentGroup()
            {
                Id = groupId,
                Experiments = new List<Experiment>
                {
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId1, ProcessStatus.Canceling, ProcessStatus.Canceled),
                    _testDataGenerator.GenerateExperimentWithStatusCondition(experimentId2, ProcessStatus.Canceled),
                },
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok<Maybe<ExperimentGroup>>(group)));

            // Act
            var actual = await _model.Delete(groupId, updateExperimentDto);

            // Assert
            actual.IsSuccess.Should().BeFalse();
            actual.Should().BeOfType<Result<VoidResult, ErrorResult>>()
                 .Which.Error.ErrorType.Should().Be(ErrorTypes.ValidationFailed);
            actual.Should().BeOfType<Result<VoidResult, ErrorResult>>()
                 .Which.Error.Error.Should().Be($"Could not find all experiments {notFoundId} in group {groupId}.");
            _repositoryMock.Verify(m => m.GetExperimentGroup(groupId), Times.Once);
            _repositoryMock.Verify(m => m.UpdateExperimentGroup(It.IsAny<ExperimentGroup>()), Times.Never);
        }

        [TestMethod]
        public async Task Resume_HappyPath_UpdatesStatusToResumingOnlyForPausedConditions()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentId = Guid.NewGuid();
            var statusChangeComment = "some comment";
            var updateExperimentDto = new ExperimentForUpdateDto()
            {
                ExperimentId = experimentId,
                Comment = statusChangeComment,
            };
            var group = new ExperimentGroup()
            {
                Id = groupId,
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        Id = updateExperimentDto.ExperimentId,
                        Stages = new List<Stage>()
                        {
                            new Stage()
                            {
                                StageType = StageType.Class,
                                StageData = new ClassStageData()
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition() { Status = ProcessStatus.NotStarted},
                                        new Condition() { Status = ProcessStatus.PendingCommit},
                                        new Condition() { Status = ProcessStatus.Committed},
                                        new Condition() { Status = ProcessStatus.Running},
                                        new Condition() { Status = ProcessStatus.Paused},
                                        new Condition() { Status = ProcessStatus.Canceling},
                                        new Condition() { Status = ProcessStatus.Canceled},
                                        new Condition() { Status = ProcessStatus.Completed},
                                        new Condition() { Status = ProcessStatus.Resuming},
                                    },
                                },
                            },
                        },
                    },
                },
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok<Maybe<ExperimentGroup>>(group)));
            _repositoryMock.Setup(m => m.UpdateExperiment(
                                It.IsAny<Guid>(),
                                It.IsAny<Experiment>()))
                           .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.Resume(groupId, updateExperimentDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            _repositoryMock.Verify(m => m.GetExperimentGroup(groupId), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperiment(
                    groupId,
                    It.Is<Experiment>(e => e.Id == experimentId &&
                        e.Stages.First().StageData.As<ClassStageData>().Conditions.ElementAt(4).StatusChangeComment == statusChangeComment &&
                        e.Stages.First().StageData.As<ClassStageData>().Conditions.Select(c => c.Status).SequenceEqual(
                            new List<ProcessStatus>
                            {
                                ProcessStatus.NotStarted,
                                ProcessStatus.PendingCommit,
                                ProcessStatus.Committed,
                                ProcessStatus.Running,
                                ProcessStatus.Resuming,
                                ProcessStatus.Canceling,
                                ProcessStatus.Canceled,
                                ProcessStatus.Completed,
                                ProcessStatus.Resuming,
                            }))),
                Times.Once);
        }

        [TestMethod]
        public async Task Resume_GroupDoesntExist_ReturnsNotFound()
        {
            // Arrange
            var updateExperimentDto = new ExperimentForUpdateDto()
            {
                ExperimentId = Guid.NewGuid(),
                Comment = "some comment",
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.None)));
            _repositoryMock.Setup(m => m.UpdateExperiment(
                                It.IsAny<Guid>(),
                                It.IsAny<Experiment>()))
                           .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.Resume(Guid.NewGuid(), updateExperimentDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<VoidResult, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.NotFound);
            _repositoryMock.Verify(m => m.GetExperimentGroup(It.IsAny<Guid>()), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperiment(It.IsAny<Guid>(), It.IsAny<Experiment>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Resume_ExperimentDoesntExist_ReturnsNotFound()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentId = Guid.NewGuid();
            var updateExperimentDto = new ExperimentForUpdateDto()
            {
                ExperimentId = Guid.NewGuid(),
                Comment = "some comment",
            };
            var group = new ExperimentGroup()
            {
                Id = groupId,
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        Id = experimentId,
                        Stages = new List<Stage>
                        {
                            new Stage()
                            {
                                StageData = new OLBStageData
                                {
                                    Status = ProcessStatus.Paused,
                                },
                            },
                        },
                    },
                },
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok<Maybe<ExperimentGroup>>(group)));
            _repositoryMock.Setup(m => m.UpdateExperiment(
                                It.IsAny<Guid>(),
                                It.IsAny<Experiment>()))
                           .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.Resume(groupId, updateExperimentDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<VoidResult, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.NotFound);
            _repositoryMock.Verify(m => m.GetExperimentGroup(It.IsAny<Guid>()), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperiment(It.IsAny<Guid>(), It.IsAny<Experiment>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Resume_RepositoryGetFailure_ReturnsRepositoryError()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentId = Guid.NewGuid();
            var updateExperimentDto = new ExperimentForUpdateDto()
            {
                ExperimentId = experimentId,
                Comment = "some comment",
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Fail<Maybe<ExperimentGroup>>("failed to get from repo")));
            _repositoryMock.Setup(m => m.UpdateExperiment(
                                It.IsAny<Guid>(),
                                It.IsAny<Experiment>()))
                           .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.Resume(groupId, updateExperimentDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<VoidResult, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.RepositoryError);
            _repositoryMock.Verify(m => m.GetExperimentGroup(It.IsAny<Guid>()), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperiment(It.IsAny<Guid>(), It.IsAny<Experiment>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Resume_RepositoryUpdateFailure_ReturnsRepositoryError()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentId = Guid.NewGuid();
            var updateExperimentDto = new ExperimentForUpdateDto()
            {
                ExperimentId = experimentId,
                Comment = "some comment",
            };
            var group = new ExperimentGroup()
            {
                Id = groupId,
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        Id = updateExperimentDto.ExperimentId,
                        Stages = new List<Stage>
                        {
                            new Stage()
                            {
                                StageType = StageType.Olb,
                                StageData = new OLBStageData
                                {
                                    Status = ProcessStatus.Paused,
                                },
                            },
                        },
                    },
                },
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok<Maybe<ExperimentGroup>>(group)));
            _repositoryMock.Setup(m => m.UpdateExperiment(
                                It.IsAny<Guid>(),
                                It.IsAny<Experiment>()))
                           .Returns(Task.FromResult(Result.Fail("fail to update")));

            // Act
            var actual = await _model.Resume(groupId, updateExperimentDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<VoidResult, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.RepositoryError);
            _repositoryMock.Verify(m => m.GetExperimentGroup(It.IsAny<Guid>()), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperiment(groupId, It.IsAny<Experiment>()),
                Times.Once);
        }

        [DataRow(ProcessStatus.NotStarted)]
        [DataRow(ProcessStatus.Committed)]
        [DataRow(ProcessStatus.Running)]
        [DataRow(ProcessStatus.Completed)]
        [DataRow(ProcessStatus.Canceled)]
        [DataRow(ProcessStatus.Canceling)]
        [DataRow(ProcessStatus.Resuming)]
        [DataTestMethod]
        public async Task Resume_NoPausedStages_ReturnsValidationError(ProcessStatus status)
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var experimentId = Guid.NewGuid();
            var updateExperimentDto = new ExperimentForUpdateDto()
            {
                ExperimentId = experimentId,
                Comment = "some comment",
            };
            var group = new ExperimentGroup()
            {
                Id = groupId,
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        Id = updateExperimentDto.ExperimentId,
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Olb,
                                StageData = new OLBStageData
                                {
                                    Status = ProcessStatus.PendingCommit,
                                },
                            },
                            new Stage
                            {
                                StageType = StageType.Olb,
                                StageData = new OLBStageData
                                {
                                    Status = status,
                                },
                            },
                        },
                    },
                },
            };

            _repositoryMock.Setup(m => m.GetExperimentGroup(It.IsAny<Guid>()))
                           .Returns(Task.FromResult(Result.Ok<Maybe<ExperimentGroup>>(group)));
            _repositoryMock.Setup(m => m.UpdateExperiment(
                                It.IsAny<Guid>(),
                                It.IsAny<Experiment>()))
                           .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.Resume(groupId, updateExperimentDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Should().BeOfType<Result<VoidResult, ErrorResult>>()
                  .Which.Error.ErrorType.Should().Be(ErrorTypes.ValidationFailed);
            _repositoryMock.Verify(m => m.GetExperimentGroup(It.IsAny<Guid>()), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperiment(It.IsAny<Guid>(), It.IsAny<Experiment>()),
                Times.Never);
        }

        private bool MatchExperimentAndConditionsStatus(ExperimentGroup experimentGroup, Guid experimentId, string updateComment, int[] relevantUpdatedIndexes, params ProcessStatus[] status)
        {
            return experimentGroup.Experiments.FirstOrDefault(e => e.Id == experimentId) != null &&
                       experimentGroup.Experiments.First(e => e.Id == experimentId).Stages.First().StageData.As<ClassStageData>().Conditions.Select((c, i) => new { Comment = c.StatusChangeComment, Index = i })
                         .All(c => !relevantUpdatedIndexes.Any(i => i == c.Index) || c.Comment == updateComment) &&
                        experimentGroup.Experiments.First(e => e.Id == experimentId).Stages.First().StageData.As<ClassStageData>().Conditions.Select(c => c.Status).SequenceEqual(status);
        }
    }
}