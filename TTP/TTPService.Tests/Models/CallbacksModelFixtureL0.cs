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
using TTPService.Dtos.Notification.Enums;
using TTPService.Dtos.Update;
using TTPService.Entities;
using TTPService.Entities.Extensions;
using TTPService.Enums;
using TTPService.FunctionalExtensions;
using TTPService.Models;
using TTPService.Models.Validators;
using TTPService.Repositories;
using TTPService.Services;

namespace TTPService.Tests.Models
{
    // TODO:[Team]<-[Golan] - reduce code, use mock setup Verifiable
    [TestClass]
    public class CallbacksModelFixtureL0
    {
        private Mock<IRepository> _repositoryMock;
        private CallbacksModel _model;
        private TestDataGenerator _testDataGenerator;
        private Mock<ICallbacksValidator> _callbacksValidator;
        private Mock<INotifier> _notifier;

        [TestInitialize]
        public void Init()
        {
            _repositoryMock = new Mock<IRepository>();
            _testDataGenerator = new TestDataGenerator();
            _callbacksValidator = new Mock<ICallbacksValidator>();
            _notifier = new Mock<INotifier>();
            _model = new CallbacksModel(
                Mock.Of<IMapper>(),
                _repositoryMock.Object,
                _callbacksValidator.Object,
                Mock.Of<ILogger<CallbacksModel>>(),
                _notifier.Object);
        }

        [DataRow(ProcessStatus.NotStarted, ProcessStatus.PendingMaterialIssue, true)]
        [DataRow(ProcessStatus.NotStarted, ProcessStatus.PendingCommit, true)]
        [DataRow(ProcessStatus.PendingCommit, ProcessStatus.Committed, true)]
        [DataRow(ProcessStatus.Committed, ProcessStatus.Running, true)]
        [DataRow(ProcessStatus.Running, ProcessStatus.Completed, false)]
        [DataRow(ProcessStatus.PendingCommit, ProcessStatus.Paused, true)]
        [DataRow(ProcessStatus.Paused, ProcessStatus.Completed, false)]
        [DataRow(ProcessStatus.Committed, ProcessStatus.Canceling, true)]
        [DataRow(ProcessStatus.Canceling, ProcessStatus.Canceled, false)]
        [DataRow(ProcessStatus.Paused, ProcessStatus.Completed, false)]
        [DataRow(ProcessStatus.Paused, ProcessStatus.Running, true)]
        [DataTestMethod]
        public async Task UpdateConditionOrStageStatus_With_Condition_HappyPath(ProcessStatus originalStatus, ProcessStatus statusToUpdate, bool isCompleteDateNull)
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup = new ExperimentGroup()
            {
                Id = Guid.NewGuid(),
                TestProgramData = new TestProgramData(),
                Experiments = new List<Experiment>()
                {
                    new Experiment()
                    {
                        Id = Guid.NewGuid(),
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
                                            Status = originalStatus,
                                        },
                                        new Condition
                                        {
                                            Id = Guid.NewGuid(),
                                            Status = ProcessStatus.Completed,
                                            Results = new ConditionResult(),
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            };
            var firstExperiment = experimentGroup.Value.Experiments.First();
            var firstStage = firstExperiment.Stages.First();
            var firstCondition = ((ClassStageData)firstStage.StageData).Conditions.First();
            var statusForUpdateDto = new StatusForUpdateDto()
            {
                CorrelationId = Guid.NewGuid(),
                Status = statusToUpdate,
                Comment = "progress change comment",
            };
            statusForUpdateDto.CorrelationId = firstCondition.Id;
            _repositoryMock.Setup(m => m.GetExperimentGroupByConditionId(statusForUpdateDto.CorrelationId))
                           .Returns(Task.FromResult(Result.Ok(experimentGroup)));
            _repositoryMock.Setup(m => m.UpdateExperimentCondition(experimentGroup.Value.Id, firstExperiment.Id, firstStage.Id, firstCondition))
                           .Returns(Task.FromResult(Result.Ok()));
            _callbacksValidator.Setup(m =>
                                    m.ValidateUpdateStatusIsAllowed(It.IsAny<ProcessStatus>(), It.IsAny<ProcessStatus>()))
                               .Returns(Result.Ok());

            // Act
            var actual = await _model.UpdateConditionOrStageStatus(statusForUpdateDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            _repositoryMock.Verify(
                m => m.GetExperimentGroupByConditionId(firstCondition.Id), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentCondition(experimentGroup.Value.Id, firstExperiment.Id, firstStage.Id, firstCondition), Times.Once);
            _callbacksValidator.Verify(
                m => m.ValidateUpdateStatusIsAllowed(
                    originalStatus,
                    statusForUpdateDto.Status.Value), Times.Once);
            _repositoryMock.Verify(
               m => m.UpdateExperimentMaterialIssue(experimentGroup.Value.Id, firstExperiment.Id, It.IsAny<MaterialIssue>()), Times.Never);

            Assert.AreEqual(firstCondition.CompletionTime == null, isCompleteDateNull);
        }

        [DataRow(ProcessStatus.NotStarted, ProcessStatus.PendingMaterialIssue, true)]
        [DataRow(ProcessStatus.NotStarted, ProcessStatus.PendingCommit, true)]
        [DataRow(ProcessStatus.PendingCommit, ProcessStatus.Committed, true)]
        [DataRow(ProcessStatus.Committed, ProcessStatus.Running, true)]
        [DataRow(ProcessStatus.Running, ProcessStatus.Completed, false)]
        [DataRow(ProcessStatus.PendingCommit, ProcessStatus.Paused, true)]
        [DataRow(ProcessStatus.Paused, ProcessStatus.Completed, false)]
        [DataRow(ProcessStatus.Committed, ProcessStatus.Canceling, true)]
        [DataRow(ProcessStatus.Canceling, ProcessStatus.Canceled, false)]
        [DataRow(ProcessStatus.Paused, ProcessStatus.Completed, false)]
        [DataRow(ProcessStatus.Paused, ProcessStatus.Running, true)]
        [DataTestMethod]
        public async Task UpdateConditionOrStageStatus_WithStage_HappyPath(ProcessStatus originalStatus, ProcessStatus statusToUpdate, bool isCompleteDateNull)
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup = new ExperimentGroup()
            {
                Id = Guid.NewGuid(),
                TestProgramData = new TestProgramData(),
                Experiments = new List<Experiment>()
                {
                    new Experiment()
                    {
                        Id = Guid.NewGuid(),
                        Stages = new List<Stage>
                        {
                            new Stage()
                            {
                                Id = Guid.NewGuid(),
                                StageType = StageType.Olb,
                                StageData = new OLBStageData
                                {
                                    Status = originalStatus,
                                },
                            },
                        },
                    },
                },
            };

            var firstExperiment = experimentGroup.Value.Experiments.First();
            var firstStage = firstExperiment.Stages.First();
            var statusForUpdateDto = new StatusForUpdateDto()
            {
                CorrelationId = Guid.NewGuid(),
                Status = statusToUpdate,
                Comment = "progress change comment",
            };
            statusForUpdateDto.CorrelationId = firstStage.Id;

            _repositoryMock.Setup(m => m.GetExperimentGroupByConditionId(statusForUpdateDto.CorrelationId))
                    .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.None)));
            _repositoryMock.Setup(m => m.GetExperimentGroupByNonClassStageId(statusForUpdateDto.CorrelationId))
                           .Returns(Task.FromResult(Result.Ok(experimentGroup)));
            _repositoryMock.Setup(m => m.UpdateExperimentStage(experimentGroup.Value.Id, firstExperiment.Id, firstStage))
                           .Returns(Task.FromResult(Result.Ok()));
            _callbacksValidator.Setup(m =>
                                    m.ValidateUpdateStatusIsAllowed(It.IsAny<ProcessStatus>(), It.IsAny<ProcessStatus>()))
                               .Returns(Result.Ok());

            // Act
            var actual = await _model.UpdateConditionOrStageStatus(statusForUpdateDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            _repositoryMock.Verify(
                m => m.GetExperimentGroupByNonClassStageId(firstStage.Id), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentStage(experimentGroup.Value.Id, firstExperiment.Id, firstStage), Times.Once);
            _callbacksValidator.Verify(
                m => m.ValidateUpdateStatusIsAllowed(
                    originalStatus,
                    statusForUpdateDto.Status.Value), Times.Once);
            _repositoryMock.Verify(
               m => m.UpdateExperimentMaterialIssue(experimentGroup.Value.Id, firstExperiment.Id, It.IsAny<MaterialIssue>()), Times.Never);

            Assert.AreEqual(firstStage.GetCompletionTime() == null, isCompleteDateNull);
        }

        [TestMethod]
        public async Task UpdateConditionOrStageStatus_With_Condition_CommittedToPendingCommitWithPreviousIssueStepFailure_CommentsRemovedFromMaterial()
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup = new ExperimentGroup()
            {
                Id = Guid.NewGuid(),
                TestProgramData = new TestProgramData(),
                Experiments = new List<Experiment>()
                {
                    new Experiment()
                    {
                        Id = Guid.NewGuid(),
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
                                            Status = ProcessStatus.Committed,
                                        },
                                        new Condition
                                        {
                                            Id = Guid.NewGuid(),
                                            Status = ProcessStatus.Committed,
                                            Results = new ConditionResult(),
                                        },
                                    },
                                },
                            },
                        },
                        Material = new Material()
                        {
                            MaterialIssue = new MaterialIssue()
                            {
                                MaterialIssueIsRequired = true,
                                MaterialIssueErrorComments = "previous error",
                            },
                            Lots = new[]
                            {
                                new Lot(),
                            },
                        },
                    },
                },
            };
            var firstExperiment = experimentGroup.Value.Experiments.First();
            var firstStage = firstExperiment.Stages.First();
            var firstCondition = ((ClassStageData)firstStage.StageData).Conditions.First();
            var statusForUpdateDto = new StatusForUpdateDto()
            {
                CorrelationId = Guid.NewGuid(),
                Status = ProcessStatus.PendingCommit,
                Comment = "Finally something good...",
            };
            statusForUpdateDto.CorrelationId = firstCondition.Id;
            _repositoryMock.Setup(m => m.GetExperimentGroupByConditionId(statusForUpdateDto.CorrelationId))
                .Returns(Task.FromResult(Result.Ok(experimentGroup)));
            _repositoryMock.Setup(m => m.UpdateExperimentCondition(experimentGroup.Value.Id, firstExperiment.Id, firstStage.Id, firstCondition))
                .Returns(Task.FromResult(Result.Ok()));
            _repositoryMock.Setup(m => m.UpdateExperimentMaterialIssue(experimentGroup.Value.Id, firstExperiment.Id, It.IsAny<MaterialIssue>()))
                .Returns(Task.FromResult(Result.Ok()));
            _callbacksValidator.Setup(m => m.ValidateUpdateStatusIsAllowed(It.IsAny<ProcessStatus>(), It.IsAny<ProcessStatus>()))
                .Returns(Result.Ok());

            // Act
            var actual = await _model.UpdateConditionOrStageStatus(statusForUpdateDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            _repositoryMock.Verify(
                m => m.GetExperimentGroupByConditionId(firstCondition.Id), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentCondition(experimentGroup.Value.Id, firstExperiment.Id, firstStage.Id, firstCondition), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentMaterialIssue(
                    experimentGroup.Value.Id,
                    firstExperiment.Id,
                    It.Is<MaterialIssue>(mi => mi.MaterialIssueIsRequired && string.IsNullOrEmpty(mi.MaterialIssueErrorComments))), Times.Once);
            _callbacksValidator.Verify(
                m => m.ValidateUpdateStatusIsAllowed(
                    ProcessStatus.Committed,
                    statusForUpdateDto.Status.Value), Times.Once);
            firstCondition.CompletionTime.Should().BeNull();
        }

        [DataTestMethod]
        public async Task UpdateConditionOrStageStatus_With_Condition_PausedWithIssueStep_CommentsAreSetInMaterial()
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup = new ExperimentGroup()
            {
                Id = Guid.NewGuid(),
                TestProgramData = new TestProgramData(),
                Experiments = new List<Experiment>()
                {
                    new Experiment()
                    {
                        Id = Guid.NewGuid(),
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
                                            Status = ProcessStatus.PendingMaterialIssue,
                                        },
                                        new Condition
                                        {
                                            Id = Guid.NewGuid(),
                                            Status = ProcessStatus.Completed,
                                            Results = new ConditionResult(),
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            };
            var firstExperiment = experimentGroup.Value.Experiments.First();
            var firstStage = firstExperiment.Stages.First();
            var firstCondition = ((ClassStageData)firstStage.StageData).Conditions.First();
            var statusForUpdateDto = new StatusForUpdateDto()
            {
                CorrelationId = Guid.NewGuid(),
                Status = ProcessStatus.Paused,
                Comment = "issue error...",
                MaterialIssueFailed = true,
            };
            statusForUpdateDto.CorrelationId = firstCondition.Id;
            _repositoryMock.Setup(m => m.GetExperimentGroupByConditionId(statusForUpdateDto.CorrelationId))
                           .Returns(Task.FromResult(Result.Ok(experimentGroup)));
            _repositoryMock.Setup(m => m.UpdateExperimentCondition(experimentGroup.Value.Id, firstExperiment.Id, firstStage.Id, firstCondition))
                           .Returns(Task.FromResult(Result.Ok()));
            _repositoryMock.Setup(m => m.UpdateExperimentMaterialIssue(experimentGroup.Value.Id, firstExperiment.Id, It.IsAny<MaterialIssue>()))
                           .Returns(Task.FromResult(Result.Ok()));
            _callbacksValidator.Setup(m =>
                                    m.ValidateUpdateStatusIsAllowed(It.IsAny<ProcessStatus>(), It.IsAny<ProcessStatus>()))
                               .Returns(Result.Ok());

            // Act
            var actual = await _model.UpdateConditionOrStageStatus(statusForUpdateDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            _repositoryMock.Verify(
                m => m.GetExperimentGroupByConditionId(firstCondition.Id), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentCondition(experimentGroup.Value.Id, firstExperiment.Id, firstStage.Id, It.Is<Condition>(c => string.IsNullOrEmpty(c.Comment))), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentMaterialIssue(experimentGroup.Value.Id, firstExperiment.Id, It.Is<MaterialIssue>(m2 => m2.MaterialIssueErrorComments == statusForUpdateDto.Comment)), Times.Once);
            _callbacksValidator.Verify(
                m => m.ValidateUpdateStatusIsAllowed(
                    ProcessStatus.PendingMaterialIssue,
                    statusForUpdateDto.Status.Value), Times.Once);
        }

        [DataRow(ProcessStatus.Canceling)]
        [DataRow(ProcessStatus.Paused)]
        [DataRow(ProcessStatus.Completed)]
        [DataRow(ProcessStatus.Canceled)]
        [DataTestMethod]
        public async Task UpdateConditionOrStageStatus_With_Condition_ExperimentIsArchived_ShouldUpdateAndReturnOk(ProcessStatus status)
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup = new ExperimentGroup()
            {
                Id = Guid.NewGuid(),
                TestProgramData = new TestProgramData(),
                Experiments = new List<Experiment>()
                {
                    new Experiment()
                    {
                        IsArchived = true,
                        Id = Guid.NewGuid(),
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
                                            Status = ProcessStatus.Canceling,
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            };
            var firstExperiment = experimentGroup.Value.Experiments.First();
            var firstStage = firstExperiment.Stages.First();
            var firstCondition = ((ClassStageData)firstStage.StageData).Conditions.First();
            var statusForUpdateDto = new StatusForUpdateDto()
            {
                CorrelationId = Guid.NewGuid(),
                Status = status,
                Comment = "progress change comment",
            };
            statusForUpdateDto.CorrelationId = firstCondition.Id;
            _repositoryMock.Setup(m => m.GetExperimentGroupByConditionId(statusForUpdateDto.CorrelationId))
                           .Returns(Task.FromResult(Result.Ok(experimentGroup)));
            _repositoryMock.Setup(m => m.UpdateExperimentCondition(experimentGroup.Value.Id, firstExperiment.Id, firstStage.Id, firstCondition))
                           .Returns(Task.FromResult(Result.Ok()));
            _callbacksValidator.Setup(m =>
                                    m.ValidateUpdateStatusIsAllowed(It.IsAny<ProcessStatus>(), It.IsAny<ProcessStatus>()))
                               .Returns(Result.Ok());

            // Act
            var actual = await _model.UpdateConditionOrStageStatus(statusForUpdateDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            _repositoryMock.Verify(
                m => m.GetExperimentGroupByConditionId(firstCondition.Id), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentCondition(experimentGroup.Value.Id, firstExperiment.Id, firstStage.Id, firstCondition), Times.Once);
            _callbacksValidator.Verify(
                m => m.ValidateUpdateStatusIsAllowed(
                    ProcessStatus.Canceling,
                    statusForUpdateDto.Status.Value), Times.Once);
        }

        [DataRow(ProcessStatus.Canceling)]
        [DataRow(ProcessStatus.Paused)]
        [DataRow(ProcessStatus.Completed)]
        [DataRow(ProcessStatus.Canceled)]
        [DataTestMethod]
        public async Task UpdateConditionOrStageStatus_With_Stage_ExperimentIsArchived_ShouldUpdateAndReturnOk(ProcessStatus status)
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup = new ExperimentGroup()
            {
                Id = Guid.NewGuid(),
                TestProgramData = new TestProgramData(),
                Experiments = new List<Experiment>()
                {
                    new Experiment()
                    {
                        IsArchived = true,
                        Id = Guid.NewGuid(),
                        Stages = new List<Stage>
                        {
                            new Stage()
                            {
                                Id = Guid.NewGuid(),
                                StageType = StageType.Olb,
                                StageData = new OLBStageData
                                {
                                    Status = ProcessStatus.Canceling,
                                },
                            },
                        },
                    },
                },
            };
            var firstExperiment = experimentGroup.Value.Experiments.First();
            var firstStage = firstExperiment.Stages.First();
            var statusForUpdateDto = new StatusForUpdateDto()
            {
                CorrelationId = Guid.NewGuid(),
                Status = status,
                Comment = "progress change comment",
            };
            statusForUpdateDto.CorrelationId = firstStage.Id;
            _repositoryMock.Setup(m => m.GetExperimentGroupByConditionId(statusForUpdateDto.CorrelationId))
                .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.None)));
            _repositoryMock.Setup(m => m.GetExperimentGroupByNonClassStageId(statusForUpdateDto.CorrelationId))
                .Returns(Task.FromResult(Result.Ok(experimentGroup)));
            _repositoryMock.Setup(m => m.UpdateExperimentStage(experimentGroup.Value.Id, firstExperiment.Id, firstStage))
                           .Returns(Task.FromResult(Result.Ok()));
            _callbacksValidator.Setup(m =>
                                    m.ValidateUpdateStatusIsAllowed(It.IsAny<ProcessStatus>(), It.IsAny<ProcessStatus>()))
                               .Returns(Result.Ok());

            // Act
            var actual = await _model.UpdateConditionOrStageStatus(statusForUpdateDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            _repositoryMock.Verify(
                m => m.GetExperimentGroupByNonClassStageId(firstStage.Id), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentStage(experimentGroup.Value.Id, firstExperiment.Id, firstStage), Times.Once);
            _callbacksValidator.Verify(
                m => m.ValidateUpdateStatusIsAllowed(
                    ProcessStatus.Canceling,
                    statusForUpdateDto.Status.Value), Times.Once);
        }

        [TestMethod]
        public async Task UpdateConditionOrStageStatus_With_Condition_ConditionCompleted_CompletionTimeWasSet()
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup = _testDataGenerator.GenerateExperimentGroup();
            var firstExperiment = experimentGroup.Value.Experiments.First();
            var firstStage = firstExperiment.Stages.First();
            var firstCondition = ((ClassStageData)firstStage.StageData).Conditions.First();
            var originalStatus = firstCondition.Status;
            var statusForUpdateDto = new StatusForUpdateDto()
            {
                Status = ProcessStatus.Completed,
                CorrelationId = firstCondition.Id,
            };

            _repositoryMock.Setup(m => m.GetExperimentGroupByConditionId(statusForUpdateDto.CorrelationId))
                           .Returns(Task.FromResult(Result.Ok(experimentGroup)));
            _repositoryMock.Setup(m => m.UpdateExperimentCondition(experimentGroup.Value.Id, firstExperiment.Id, firstStage.Id, firstCondition))
                .Returns(Task.FromResult(Result.Ok()));

            _callbacksValidator.Setup(m =>
                                    m.ValidateUpdateStatusIsAllowed(It.IsAny<ProcessStatus>(), It.IsAny<ProcessStatus>()))
                               .Returns(Result.Ok());

            // Act
            var actual = await _model.UpdateConditionOrStageStatus(statusForUpdateDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            _repositoryMock.Verify(
                m => m.GetExperimentGroupByConditionId(firstCondition.Id), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentCondition(experimentGroup.Value.Id, firstExperiment.Id, firstStage.Id, firstCondition), Times.Once);
            _callbacksValidator.Verify(
                m => m.ValidateUpdateStatusIsAllowed(
                    originalStatus,
                    statusForUpdateDto.Status.Value), Times.Once);
        }

        [TestMethod]
        public async Task UpdateConditionOrStageStatus_With_Condition_FailedToGetExperimentGroup_ReturnsRepositoryError()
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup = _testDataGenerator.GenerateExperimentGroup();
            var firstExperiment = experimentGroup.Value.Experiments.First();
            var firstStage = firstExperiment.Stages.First();
            var firstCondition = ((ClassStageData)firstStage.StageData).Conditions.First();
            var statusForUpdateDto = _testDataGenerator.GenerateStatusForUpdateDto();
            statusForUpdateDto.CorrelationId = firstCondition.Id;
            _repositoryMock.Setup(m => m.GetExperimentGroupByConditionId(statusForUpdateDto.CorrelationId))
                           .Returns(Task.FromResult(Result.Fail<Maybe<ExperimentGroup>>("fail to get from repository")));
            _repositoryMock.Setup(m => m.GetExperimentGroupByNonClassStageId(statusForUpdateDto.CorrelationId))
                .Returns(Task.FromResult(Result.Fail<Maybe<ExperimentGroup>>("fail to get from repository")));

            // Act
            var actual = await _model.UpdateConditionOrStageStatus(statusForUpdateDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.ErrorType.Should().Be(ErrorTypes.RepositoryError);
            _repositoryMock.Verify(
                m => m.GetExperimentGroupByConditionId(firstCondition.Id), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperiment(
                    It.IsAny<Guid>(),
                    It.IsAny<Experiment>()), Times.Never);
            _callbacksValidator.Verify(
                m => m.ValidateUpdateStatusIsAllowed(
                    It.IsAny<ProcessStatus>(),
                    It.IsAny<ProcessStatus>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateConditionOrStageStatus_With_Condition_WhenIssueStepProvided_NoContentReturned()
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup = _testDataGenerator.GenerateExperimentGroup();
            var firstExperiment = experimentGroup.Value.Experiments.First();
            var firstStage = firstExperiment.Stages.First();
            var firstCondition = ((ClassStageData)firstStage.StageData).Conditions.First();
            var statusForUpdateDto = _testDataGenerator.GenerateStatusForUpdateDto();
            statusForUpdateDto.IsIssueStep = true;

            // Act
            var actual = await _model.UpdateConditionOrStageStatus(statusForUpdateDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            _repositoryMock.Verify(
                m => m.GetExperimentGroupByConditionId(firstCondition.Id), Times.Never);
            _repositoryMock.Verify(
                m => m.UpdateExperiment(
                    It.IsAny<Guid>(),
                    It.IsAny<Experiment>()), Times.Never);
            _repositoryMock.Verify(
                m => m.UpdateExperimentMaterialIssue(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<MaterialIssue>()), Times.Never);
            _callbacksValidator.Verify(
                m => m.ValidateUpdateStatusIsAllowed(
                    It.IsAny<ProcessStatus>(),
                    It.IsAny<ProcessStatus>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateConditionOrStageStatus_With_Condition_ExperimentGroupNotFound_ReturnsNotFound()
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup = _testDataGenerator.GenerateExperimentGroup();
            var firstExperiment = experimentGroup.Value.Experiments.First();
            var firstStage = firstExperiment.Stages.First();
            var firstCondition = ((ClassStageData)firstStage.StageData).Conditions.First();
            var statusForUpdateDto = _testDataGenerator.GenerateStatusForUpdateDto();
            statusForUpdateDto.CorrelationId = firstCondition.Id;
            _repositoryMock.Setup(m => m.GetExperimentGroupByConditionId(statusForUpdateDto.CorrelationId))
                           .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.None)));
            _repositoryMock.Setup(m => m.GetExperimentGroupByNonClassStageId(statusForUpdateDto.CorrelationId))
                .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.None)));

            _callbacksValidator.Setup(m =>
                                    m.ValidateUpdateStatusIsAllowed(It.IsAny<ProcessStatus>(), It.IsAny<ProcessStatus>()))
                               .Returns(Result.Fail("fail to validate"));

            // Act
            var actual = await _model.UpdateConditionOrStageStatus(statusForUpdateDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.ErrorType.Should().Be(ErrorTypes.NotFound);
            _repositoryMock.Verify(
                m => m.GetExperimentGroupByConditionId(firstCondition.Id), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperiment(It.IsAny<Guid>(), It.IsAny<Experiment>()), Times.Never);
            _repositoryMock.Verify(
                m => m.UpdateExperimentMaterialIssue(experimentGroup.Value.Id, firstExperiment.Id, It.IsAny<MaterialIssue>()), Times.Never);
            _callbacksValidator.Verify(
                m => m.ValidateUpdateStatusIsAllowed(
                    It.IsAny<ProcessStatus>(),
                    It.IsAny<ProcessStatus>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateConditionOrStageStatus_With_Condition_ValidationFailed_ReturnsValidationError()
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup = _testDataGenerator.GenerateExperimentGroup();
            var firstExperiment = experimentGroup.Value.Experiments.First();
            var firstStage = firstExperiment.Stages.First();
            var firstCondition = ((ClassStageData)firstStage.StageData).Conditions.First();
            var originalStatus = firstCondition.Status;
            var statusForUpdateDto = _testDataGenerator.GenerateStatusForUpdateDto();
            statusForUpdateDto.CorrelationId = firstCondition.Id;
            _repositoryMock.Setup(m => m.GetExperimentGroupByConditionId(statusForUpdateDto.CorrelationId))
                           .Returns(Task.FromResult(Result.Ok(experimentGroup)));

            _callbacksValidator.Setup(m =>
                                    m.ValidateUpdateStatusIsAllowed(It.IsAny<ProcessStatus>(), It.IsAny<ProcessStatus>()))
                               .Returns(Result.Fail("fail to validate"));

            // Act
            var actual = await _model.UpdateConditionOrStageStatus(statusForUpdateDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.ErrorType.Should().Be(ErrorTypes.ValidationFailed);
            _repositoryMock.Verify(
                m => m.GetExperimentGroupByConditionId(firstCondition.Id), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperiment(It.IsAny<Guid>(), It.IsAny<Experiment>()), Times.Never);
            _repositoryMock.Verify(
                m => m.UpdateExperimentMaterialIssue(experimentGroup.Value.Id, firstExperiment.Id, It.IsAny<MaterialIssue>()), Times.Never);
            _callbacksValidator.Verify(
                m => m.ValidateUpdateStatusIsAllowed(
                    originalStatus,
                    statusForUpdateDto.Status.Value), Times.Once);
        }

        [TestMethod]
        public async Task UpdateConditionOrStageStatus_With_Condition_UpdateConditionFailed_ReturnsRepositoryError()
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup = _testDataGenerator.GenerateExperimentGroup();
            var firstExperiment = experimentGroup.Value.Experiments.First();
            var firstStage = firstExperiment.Stages.First();
            var firstCondition = ((ClassStageData)firstStage.StageData).Conditions.First();
            var originalStatus = firstCondition.Status;
            var statusForUpdateDto = _testDataGenerator.GenerateStatusForUpdateDto();
            statusForUpdateDto.CorrelationId = firstCondition.Id;
            _repositoryMock.Setup(m => m.GetExperimentGroupByConditionId(statusForUpdateDto.CorrelationId))
                           .Returns(Task.FromResult(Result.Ok(experimentGroup)));
            _repositoryMock.Setup(m => m.UpdateExperimentCondition(experimentGroup.Value.Id, firstExperiment.Id, firstStage.Id, firstCondition))
                           .Returns(Task.FromResult(Result.Fail("Fail to update")));

            _callbacksValidator.Setup(m =>
                                    m.ValidateUpdateStatusIsAllowed(It.IsAny<ProcessStatus>(), It.IsAny<ProcessStatus>()))
                               .Returns(Result.Ok());

            // Act
            var actual = await _model.UpdateConditionOrStageStatus(statusForUpdateDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.ErrorType.Should().Be(ErrorTypes.RepositoryError);
            _repositoryMock.Verify(
                m => m.GetExperimentGroupByConditionId(firstCondition.Id), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentCondition(experimentGroup.Value.Id, firstExperiment.Id, firstStage.Id, firstCondition), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentMaterialIssue(experimentGroup.Value.Id, firstExperiment.Id, It.IsAny<MaterialIssue>()), Times.Never);
            _callbacksValidator.Verify(
                m => m.ValidateUpdateStatusIsAllowed(
                    originalStatus,
                    statusForUpdateDto.Status.Value), Times.Once);
        }

        [TestMethod]
        public async Task UpdateExperimentVpo_HappyPath()
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup = _testDataGenerator.GenerateExperimentGroup();
            var firstExperiment = experimentGroup.Value.Experiments.First();
            var vpoForUpdateDto = _testDataGenerator.GenerateVpoForUpdateDto();
            vpoForUpdateDto.CorrelationId = firstExperiment.Id;
            _repositoryMock.Setup(m => m.GetExperimentGroupByExperimentId(vpoForUpdateDto.CorrelationId))
                           .Returns(Task.FromResult(Result.Ok(experimentGroup)));
            _repositoryMock.Setup(m => m.UpdateExperiment(experimentGroup.Value.Id, firstExperiment))
                           .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.UpdateExperimentVpo(vpoForUpdateDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            _repositoryMock.Verify(
                m => m.GetExperimentGroupByExperimentId(firstExperiment.Id), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperiment(experimentGroup.Value.Id, firstExperiment), Times.Once);
        }

        [TestMethod]
        public async Task UpdateExperimentVpo_FailureCallback_Ignored()
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup = _testDataGenerator.GenerateExperimentGroup();
            var firstExperiment = experimentGroup.Value.Experiments.First();
            var vpoForUpdateDto = _testDataGenerator.GenerateVpoFailureForUpdateDto();
            vpoForUpdateDto.CorrelationId = firstExperiment.Id;

            // Act
            var actual = await _model.UpdateExperimentVpo(vpoForUpdateDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            _repositoryMock.Verify(
                m => m.GetExperimentGroupByExperimentId(firstExperiment.Id), Times.Never);
            _repositoryMock.Verify(
                m => m.UpdateExperiment(experimentGroup.Value.Id, firstExperiment), Times.Never);
        }

        [TestMethod]
        public async Task UpdateExperimentVpo_FailedToGetGroup_ReturnsRepositoryError()
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup = _testDataGenerator.GenerateExperimentGroup();
            var firstExperiment = experimentGroup.Value.Experiments.First();
            var vpoForUpdateDto = _testDataGenerator.GenerateVpoForUpdateDto();
            vpoForUpdateDto.CorrelationId = firstExperiment.Id;
            _repositoryMock.Setup(m => m.GetExperimentGroupByExperimentId(vpoForUpdateDto.CorrelationId))
                           .Returns(Task.FromResult(Result.Fail<Maybe<ExperimentGroup>>("fail to get from repository")));

            // Act
            var actual = await _model.UpdateExperimentVpo(vpoForUpdateDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.ErrorType.Should().Be(ErrorTypes.RepositoryError);
            _repositoryMock.Verify(
                m => m.GetExperimentGroupByExperimentId(firstExperiment.Id), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperiment(
                    It.IsAny<Guid>(),
                    It.IsAny<Experiment>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateExperimentVpo_GroupNotFound_ReturnsNotFound()
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup = _testDataGenerator.GenerateExperimentGroup();
            var firstExperiment = experimentGroup.Value.Experiments.First();
            var vpoForUpdateDto = _testDataGenerator.GenerateVpoForUpdateDto();
            vpoForUpdateDto.CorrelationId = firstExperiment.Id;
            _repositoryMock.Setup(m => m.GetExperimentGroupByExperimentId(vpoForUpdateDto.CorrelationId))
                           .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.None)));

            // Act
            var actual = await _model.UpdateExperimentVpo(vpoForUpdateDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.ErrorType.Should().Be(ErrorTypes.NotFound);
            _repositoryMock.Verify(
                m => m.GetExperimentGroupByExperimentId(firstExperiment.Id), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperiment(It.IsAny<Guid>(), It.IsAny<Experiment>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateExperimentVpo_UpdateExperimentFailed_ReturnsRepositoryError()
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup = _testDataGenerator.GenerateExperimentGroup();
            var firstExperiment = experimentGroup.Value.Experiments.First();
            var vpoForUpdateDto = _testDataGenerator.GenerateVpoForUpdateDto();
            vpoForUpdateDto.CorrelationId = firstExperiment.Id;
            _repositoryMock.Setup(m => m.GetExperimentGroupByExperimentId(vpoForUpdateDto.CorrelationId))
                           .Returns(Task.FromResult(Result.Ok(experimentGroup)));
            _repositoryMock.Setup(m => m.UpdateExperiment(experimentGroup.Value.Id, firstExperiment))
                           .Returns(Task.FromResult(Result.Fail("Fail to update")));

            // Act
            var actual = await _model.UpdateExperimentVpo(vpoForUpdateDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.ErrorType.Should().Be(ErrorTypes.RepositoryError);
            _repositoryMock.Verify(
                m => m.GetExperimentGroupByExperimentId(firstExperiment.Id), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperiment(experimentGroup.Value.Id, firstExperiment), Times.Once);
        }

        [TestMethod]
        public async Task UpdateConditionResult_HappyPath()
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup = _testDataGenerator.GenerateExperimentGroup();
            var firstExperiment = experimentGroup.Value.Experiments.First();
            var firstStage = firstExperiment.Stages.First();
            var firstCondition = ((ClassStageData)firstStage.StageData).Conditions.First();
            var resultsForUpdateDto = _testDataGenerator.GenerateResultForUpdateDto();
            resultsForUpdateDto.ConditionId = firstCondition.Id;
            _repositoryMock.Setup(m => m.GetExperimentGroupByConditionId(resultsForUpdateDto.ConditionId))
                           .Returns(Task.FromResult(Result.Ok(experimentGroup)));
            _repositoryMock.Setup(m => m.UpdateExperimentCondition(experimentGroup.Value.Id, firstExperiment.Id, firstStage.Id, firstCondition))
                           .Returns(Task.FromResult(Result.Ok()));

            _callbacksValidator.Setup(m =>
                                    m.ValidateUpdateResultIsAllowed(It.IsAny<Condition>()))
                               .Returns(Result.Ok());

            // Act
            var actual = await _model.UpdateConditionResult(resultsForUpdateDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            _repositoryMock.Verify(
                m => m.GetExperimentGroupByConditionId(firstCondition.Id), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentCondition(experimentGroup.Value.Id, firstExperiment.Id, firstStage.Id, firstCondition), Times.Once);
            _callbacksValidator.Verify(m => m.ValidateUpdateResultIsAllowed(firstCondition), Times.Once);
        }

        [TestMethod]
        public async Task UpdateConditionResult_FailedToGetGroup_ReturnsRepositoryError()
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup = _testDataGenerator.GenerateExperimentGroup();
            var firstExperiment = experimentGroup.Value.Experiments.First();
            var firstStage = firstExperiment.Stages.First();
            var firstCondition = ((ClassStageData)firstStage.StageData).Conditions.First();
            var resultsForUpdateDto = _testDataGenerator.GenerateResultForUpdateDto();
            resultsForUpdateDto.ConditionId = firstCondition.Id;
            _repositoryMock.Setup(m => m.GetExperimentGroupByConditionId(resultsForUpdateDto.ConditionId))
                           .Returns(Task.FromResult(Result.Fail<Maybe<ExperimentGroup>>("fail to get from repository")));

            // Act
            var actual = await _model.UpdateConditionResult(resultsForUpdateDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.ErrorType.Should().Be(ErrorTypes.RepositoryError);
            _repositoryMock.Verify(
                m => m.GetExperimentGroupByConditionId(firstCondition.Id), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperiment(
                    It.IsAny<Guid>(),
                    It.IsAny<Experiment>()), Times.Never);
            _repositoryMock.Verify(
                m => m.UpdateExperimentMaterialIssue(experimentGroup.Value.Id, firstExperiment.Id, It.IsAny<MaterialIssue>()), Times.Never);
            _callbacksValidator.Verify(m => m.ValidateUpdateResultIsAllowed(It.IsAny<Condition>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateConditionResult_GroupNotFound_ReturnsNotFound()
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup = _testDataGenerator.GenerateExperimentGroup();
            var firstExperiment = experimentGroup.Value.Experiments.First();
            var firstStage = firstExperiment.Stages.First();
            var firstCondition = ((ClassStageData)firstStage.StageData).Conditions.First();
            var resultsForUpdateDto = _testDataGenerator.GenerateResultForUpdateDto();
            resultsForUpdateDto.ConditionId = firstCondition.Id;
            _repositoryMock.Setup(m => m.GetExperimentGroupByConditionId(resultsForUpdateDto.ConditionId))
                           .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.None)));
            _repositoryMock.Setup(m => m.GetExperimentGroupByNonClassStageId(resultsForUpdateDto.ConditionId))
                .Returns(Task.FromResult(Result.Ok(Maybe<ExperimentGroup>.None)));

            _callbacksValidator.Setup(m => m.ValidateUpdateResultIsAllowed(It.IsAny<Condition>()))
                               .Returns(Result.Ok());

            // Act
            var actual = await _model.UpdateConditionResult(resultsForUpdateDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.ErrorType.Should().Be(ErrorTypes.NotFound);
            _repositoryMock.Verify(
                m => m.GetExperimentGroupByConditionId(firstCondition.Id), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperiment(It.IsAny<Guid>(), It.IsAny<Experiment>()), Times.Never);
            _callbacksValidator.Verify(m => m.ValidateUpdateResultIsAllowed(It.IsAny<Condition>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateConditionResult_ValidationFailed_ReturnsValidationError()
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup = _testDataGenerator.GenerateExperimentGroup();
            var firstExperiment = experimentGroup.Value.Experiments.First();
            var firstStage = firstExperiment.Stages.First();
            var firstCondition = ((ClassStageData)firstStage.StageData).Conditions.First();
            var resultsForUpdateDto = _testDataGenerator.GenerateResultForUpdateDto();
            resultsForUpdateDto.ConditionId = firstCondition.Id;
            _repositoryMock.Setup(m => m.GetExperimentGroupByConditionId(resultsForUpdateDto.ConditionId))
                           .Returns(Task.FromResult(Result.Ok(experimentGroup)));

            _callbacksValidator.Setup(m => m.ValidateUpdateResultIsAllowed(It.IsAny<Condition>()))
                               .Returns(Result.Fail("fail to validate"));

            // Act
            var actual = await _model.UpdateConditionResult(resultsForUpdateDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.ErrorType.Should().Be(ErrorTypes.ValidationFailed);
            _repositoryMock.Verify(
                m => m.GetExperimentGroupByConditionId(firstCondition.Id), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperiment(It.IsAny<Guid>(), It.IsAny<Experiment>()), Times.Never);
            _callbacksValidator.Verify(m => m.ValidateUpdateResultIsAllowed(firstCondition), Times.Once);
        }

        [TestMethod]
        public async Task UpdateConditionResult_UpdateConditionFailed_ReturnsRepositoryError()
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup = _testDataGenerator.GenerateExperimentGroup();
            var firstExperiment = experimentGroup.Value.Experiments.First();
            var firstStage = firstExperiment.Stages.First();
            var firstCondition = ((ClassStageData)firstStage.StageData).Conditions.First();
            var resultsForUpdateDto = _testDataGenerator.GenerateResultForUpdateDto();
            resultsForUpdateDto.ConditionId = firstCondition.Id;
            _repositoryMock.Setup(m => m.GetExperimentGroupByConditionId(resultsForUpdateDto.ConditionId))
                           .Returns(Task.FromResult(Result.Ok(experimentGroup)));
            _repositoryMock.Setup(m => m.UpdateExperimentCondition(experimentGroup.Value.Id, firstExperiment.Id, firstStage.Id, firstCondition))
                           .Returns(Task.FromResult(Result.Fail("Fail to update")));

            _callbacksValidator.Setup(m => m.ValidateUpdateResultIsAllowed(It.IsAny<Condition>()))
                               .Returns(Result.Ok());

            // Act
            var actual = await _model.UpdateConditionResult(resultsForUpdateDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.ErrorType.Should().Be(ErrorTypes.RepositoryError);
            _repositoryMock.Verify(
                m => m.GetExperimentGroupByConditionId(firstCondition.Id), Times.Once);
            _repositoryMock.Verify(
                m => m.UpdateExperimentCondition(experimentGroup.Value.Id, firstExperiment.Id, firstStage.Id, firstCondition), Times.Once);
            _callbacksValidator.Verify(m => m.ValidateUpdateResultIsAllowed(firstCondition), Times.Once);
        }

        [DataTestMethod]
        public async Task ExperimentProgress_HappyPath()
        {
            Maybe<ExperimentGroup> experimentGroup = _testDataGenerator.GenerateExperimentGroup();
            var experimentProgressNotifyDto = _testDataGenerator.GenerateExperimentProgressNotifyDto();

            _repositoryMock.Setup(m => m.GetExperimentGroupByExperimentId(experimentProgressNotifyDto.CorrelationId))
                           .Returns(Task.FromResult(Result.Ok(experimentGroup)));
            _notifier.Setup(m =>
                      m.NotifyExperimentUpdated(experimentGroup.Value, It.IsAny<Experiment>(), experimentProgressNotifyDto.ExperimentStatus))
                     .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _model.NotifyExperimentProgress(experimentProgressNotifyDto);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            _repositoryMock.Verify(
                m => m.GetExperimentGroupByExperimentId(experimentProgressNotifyDto.CorrelationId), Times.Once);
            _notifier.Verify(
              m => m.NotifyExperimentUpdated(experimentGroup.Value, It.IsAny<Experiment>(), experimentProgressNotifyDto.ExperimentStatus), Times.Once);
        }

        [TestMethod]
        public async Task ExperimentProgress_FailedToGetGroup_ReturnsRepositoryError()
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup = _testDataGenerator.GenerateExperimentGroup();
            var firstExperiment = experimentGroup.Value.Experiments.First();

            var experimentProgressNotifyDto = _testDataGenerator.GenerateExperimentProgressNotifyDto();
            experimentProgressNotifyDto.CorrelationId = firstExperiment.Id;
            _repositoryMock.Setup(m => m.GetExperimentGroupByExperimentId(firstExperiment.Id))
              .Returns(Task.FromResult(Result.Fail<Maybe<ExperimentGroup>>("fail to get from repository")));

            // Act
            var actual = await _model.NotifyExperimentProgress(experimentProgressNotifyDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.ErrorType.Should().Be(ErrorTypes.RepositoryError);
            _repositoryMock.Verify(
              m => m.GetExperimentGroupByExperimentId(firstExperiment.Id), Times.Once);
            _notifier.Verify(
              m => m.NotifyExperimentUpdated(It.IsAny<ExperimentGroup>(), It.IsAny<Experiment>(), It.IsAny<ExperimentStatus>()), Times.Never);
        }

        [TestMethod]
        public async Task ExperimentProgress_ReturnsFail()
        {
            // Arrange
            Maybe<ExperimentGroup> experimentGroup = _testDataGenerator.GenerateExperimentGroup();
            var firstExperiment = experimentGroup.Value.Experiments.First();

            var experimentProgressNotifyDto = _testDataGenerator.GenerateExperimentProgressNotifyDto();
            experimentProgressNotifyDto.CorrelationId = firstExperiment.Id;

            _repositoryMock.Setup(m => m.GetExperimentGroupByExperimentId(firstExperiment.Id))
              .Returns(Task.FromResult(Result.Ok(experimentGroup)));

            _notifier.Setup(m =>
                m.NotifyExperimentUpdated(experimentGroup.Value, firstExperiment, It.IsAny<ExperimentStatus>()))
              .Returns(Task.FromResult(Result.Fail("Failed to notify experiment progress.")));

            // Act
            var actual = await _model.NotifyExperimentProgress(experimentProgressNotifyDto);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.ErrorType.Should().Be(ErrorTypes.ExternalServerError);
            _repositoryMock.Verify(
              m => m.GetExperimentGroupByExperimentId(firstExperiment.Id), Times.Once);
            _notifier.Verify(
              m => m.NotifyExperimentUpdated(It.IsAny<ExperimentGroup>(), It.IsAny<Experiment>(), It.IsAny<ExperimentStatus>()), Times.Once);
        }
    }
}
