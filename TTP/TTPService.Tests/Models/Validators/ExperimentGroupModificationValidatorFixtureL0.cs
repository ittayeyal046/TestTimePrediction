using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TTPService.Entities;
using TTPService.Enums;
using TTPService.Models.Validators;

namespace TTPService.Tests.Models.Validators
{
    [TestClass]
    public class ExperimentGroupModificationValidatorFixtureL0
    {
        [TestMethod]
        public void CanAddExperiments_ExperimentGroupNotCompleted_ReturnsSuccess()
        {
            // Arrange
            var experimentGroup = new Mock<IExperimentGroup>();
            experimentGroup.Setup(g => g.IsCompleted).Returns(false);
            var validator = new ExperimentGroupModificationValidator(
                Mock.Of<ILogger<ExperimentGroupModificationValidator>>());

            // Act
            var actual = validator.CanAddExperiments(experimentGroup.Object);

            // Assert
            actual.IsSuccess.Should().BeTrue();
        }

        [TestMethod]
        public void CanAddExperiments_ExperimentGroupCompleted_ReturnsFailure()
        {
            // Arrange
            var experimentGroup = new Mock<IExperimentGroup>();
            experimentGroup.Setup(g => g.IsCompleted).Returns(true);
            var validator = new ExperimentGroupModificationValidator(
                Mock.Of<ILogger<ExperimentGroupModificationValidator>>());

            // Act
            var actual = validator.CanAddExperiments(experimentGroup.Object);

            // Assert
            actual.IsFailure.Should().BeTrue();
        }

        [TestMethod]
        public void CanDeleteExperiments_ExperimentGroupNotCompleted_ReturnsSuccess()
        {
            // Arrange
            var experimentGroup = new Mock<IExperimentGroup>();
            experimentGroup.Setup(g => g.IsCompleted).Returns(false);

            var validator = new ExperimentGroupModificationValidator(
                Mock.Of<ILogger<ExperimentGroupModificationValidator>>());

            // Act
            var actual = validator.CanDeleteExperiments(
                experimentGroup.Object,
                new List<Guid>() { Guid.Empty });

            // Assert
            actual.IsSuccess.Should().BeTrue();
        }

        [TestMethod]
        public void CanDeleteExperiments_ExperimentGroupCompleted_ReturnsFailure()
        {
            // Arrange
            var experimentGroup = new Mock<IExperimentGroup>();
            experimentGroup.Setup(g => g.IsCompleted).Returns(true);

            var validator = new ExperimentGroupModificationValidator(
                Mock.Of<ILogger<ExperimentGroupModificationValidator>>());

            // Act
            var actual = validator.CanDeleteExperiments(
                experimentGroup.Object,
                new List<Guid>() { Guid.Empty });

            // Assert
            actual.IsFailure.Should().BeTrue();
        }

        [TestMethod]
        public void CanDeleteExperiments_ExperimentsNotStarted_ReturnsSuccess()
        {
            // Arrange
            var experimentGroup = new Mock<IExperimentGroup>();
            experimentGroup.Setup(g => g.IsCompleted).Returns(false);
            var idToDelete = Guid.NewGuid();
            var experiments = new List<Experiment>()
            {
                new Experiment()
                {
                    Id = idToDelete,
                    Stages = new List<Stage>()
                    {
                        new Stage()
                        {
                            StageType = StageType.Ppv,
                            StageData = new PPVStageData
                            {
                                Status = ProcessStatus.PendingCommit,
                            },
                        },
                    },
                },
            };
            experimentGroup.Setup(g => g.Experiments).Returns(experiments);
            var validator = new ExperimentGroupModificationValidator(
                Mock.Of<ILogger<ExperimentGroupModificationValidator>>());

            // Act
            var actual = validator.CanDeleteExperiments(
                experimentGroup.Object,
                new List<Guid>() { idToDelete });

            // Assert
            actual.IsSuccess.Should().BeTrue();
        }

        [TestMethod]
        public void CanDeleteExperiments_ExperimentStarted_ReturnsFailure()
        {
            // Arrange
            var experimentGroup = new Mock<IExperimentGroup>();
            experimentGroup.Setup(g => g.IsCompleted).Returns(false);
            var idToDelete = Guid.NewGuid();
            var experiments = new List<Experiment>()
            {
                new Experiment()
                {
                    Id = idToDelete,
                    Stages = new List<Stage>()
                    {
                        new Stage()
                        {
                            StageType = StageType.Ppv,
                            StageData = new PPVStageData
                            {
                                Status = ProcessStatus.Running,
                            },
                        },
                    },
                },
            };
            experimentGroup.Setup(g => g.Experiments).Returns(experiments);
            var validator = new ExperimentGroupModificationValidator(
                Mock.Of<ILogger<ExperimentGroupModificationValidator>>());

            // Act
            var actual = validator.CanDeleteExperiments(
                experimentGroup.Object,
                new List<Guid>() { idToDelete });

            // Assert
            actual.IsFailure.Should().BeTrue();
        }

        [TestMethod]
        public void CanDeleteExperiments_NoExperiments_ReturnsSuccess()
        {
            // Arrange
            var experimentGroup = new Mock<IExperimentGroup>();
            experimentGroup.Setup(g => g.IsCompleted).Returns(false);
            var idToDelete = Guid.NewGuid();

            experimentGroup.Setup(g => g.Experiments)
                           .Returns(new List<Experiment>());
            var validator = new ExperimentGroupModificationValidator(
                Mock.Of<ILogger<ExperimentGroupModificationValidator>>());

            // Act
            var actual = validator.CanDeleteExperiments(
                experimentGroup.Object,
                new List<Guid>() { idToDelete });

            // Assert
            actual.IsSuccess.Should().BeTrue();
        }

        [TestMethod]
        public void CanDeleteExperiments_NoIdsToDelete_ReturnsSuccess()
        {
            // Arrange
            var experimentGroup = new Mock<IExperimentGroup>();
            experimentGroup.Setup(g => g.IsCompleted).Returns(false);
            var experiments = new List<Experiment>()
            {
                new Experiment()
                {
                    Id = Guid.NewGuid(),
                    Stages = new List<Stage>()
                    {
                        new Stage()
                        {
                            StageType = StageType.Ppv,
                            StageData = new PPVStageData
                            {
                                Status = ProcessStatus.Running,
                            },
                        },
                    },
                },
            };
            experimentGroup.Setup(g => g.Experiments).Returns(experiments);
            var validator = new ExperimentGroupModificationValidator(
                Mock.Of<ILogger<ExperimentGroupModificationValidator>>());

            // Act
            var actual = validator.CanDeleteExperiments(
                experimentGroup.Object,
                new List<Guid>());

            // Assert
            actual.IsSuccess.Should().BeTrue();
        }

        [TestMethod]
        public void CanDeleteExperiments_ExperimentNotFound_ReturnsSuccess()
        {
            // Arrange
            var experimentGroup = new Mock<IExperimentGroup>();
            experimentGroup.Setup(g => g.IsCompleted).Returns(false);
            var experiments = new List<Experiment>()
            {
                new Experiment()
                {
                    Id = Guid.NewGuid(),
                    Stages = new List<Stage>()
                    {
                        new Stage()
                        {
                            StageType = StageType.Ppv,
                            StageData = new PPVStageData
                            {
                                Status = ProcessStatus.PendingCommit,
                            },
                        },
                    },
                },
            };
            experimentGroup.Setup(g => g.Experiments).Returns(experiments);
            var validator = new ExperimentGroupModificationValidator(
                Mock.Of<ILogger<ExperimentGroupModificationValidator>>());

            // Act
            var actual = validator.CanDeleteExperiments(
                experimentGroup.Object,
                new List<Guid>() { Guid.NewGuid() });

            // Assert
            actual.IsSuccess.Should().BeTrue();
        }

        [TestMethod]
        public void CanDeleteExperiments_1StageStarted1NotStarted_ReturnsFailure()
        {
            // Arrange
            var experimentGroup = new Mock<IExperimentGroup>();
            experimentGroup.Setup(g => g.IsCompleted).Returns(false);
            var idToDelete = Guid.NewGuid();
            var experiments = new List<Experiment>()
            {
                new Experiment()
                {
                    Id = idToDelete,
                    Stages = new List<Stage>()
                    {
                        new Stage()
                        {
                            StageType = StageType.Ppv,
                            StageData = new PPVStageData
                            {
                                Status = ProcessStatus.Running,
                            },
                        },
                        new Stage()
                        {
                            StageType = StageType.Ppv,
                            StageData = new PPVStageData
                            {
                                Status = ProcessStatus.PendingCommit,
                            },
                        },
                    },
                },
            };
            experimentGroup.Setup(g => g.Experiments).Returns(experiments);
            var validator = new ExperimentGroupModificationValidator(
                Mock.Of<ILogger<ExperimentGroupModificationValidator>>());

            // Act
            var actual = validator.CanDeleteExperiments(
                experimentGroup.Object,
                new List<Guid>() { idToDelete });

            // Assert
            actual.IsFailure.Should().BeTrue();
        }

        [TestMethod]
        public void CanDeleteExperiments_1ConditionStarted1NotStarted_ReturnsFailure()
        {
            // Arrange
            var experimentGroup = new Mock<IExperimentGroup>();
            experimentGroup.Setup(g => g.IsCompleted).Returns(false);
            var idToDelete = Guid.NewGuid();
            var experiments = new List<Experiment>()
            {
                new Experiment()
                {
                    Id = idToDelete,
                    Stages = new List<Stage>()
                    {
                        new Stage()
                        {
                            StageData = new ClassStageData
                            {
                                Conditions = new List<Condition>
                                {
                                    new Condition
                                    {
                                        Status = ProcessStatus.Running,
                                    },
                                    new Condition
                                    {
                                        Status = ProcessStatus.PendingCommit,
                                    },
                                },
                            },
                        },
                    },
                },
            };
            experimentGroup.Setup(g => g.Experiments).Returns(experiments);
            var validator = new ExperimentGroupModificationValidator(
                Mock.Of<ILogger<ExperimentGroupModificationValidator>>());

            // Act
            var actual = validator.CanDeleteExperiments(
                experimentGroup.Object,
                new List<Guid>() { idToDelete });

            // Assert
            actual.IsFailure.Should().BeTrue();
        }

        [TestMethod]
        public void CanDeleteExperiments_1ExperimentStarted1NotStarted_ReturnsFailure()
        {
            // Arrange
            var experimentGroup = new Mock<IExperimentGroup>();
            experimentGroup.Setup(g => g.IsCompleted).Returns(false);
            var idToDeleteStarted = Guid.NewGuid();
            var idToDeleteNotStarted = Guid.NewGuid();
            var experiments = new List<Experiment>()
            {
                new Experiment()
                {
                    Id = idToDeleteStarted,
                    Stages = new List<Stage>()
                    {
                        new Stage()
                        {
                            StageType = StageType.Ppv,
                            StageData = new PPVStageData
                            {
                                Status = ProcessStatus.Running,
                            },
                        },
                    },
                },
                new Experiment()
                {
                    Id = idToDeleteNotStarted,
                    Stages = new List<Stage>()
                    {
                        new Stage()
                        {
                            StageType = StageType.Ppv,
                            StageData = new PPVStageData
                            {
                                Status = ProcessStatus.PendingCommit,
                            },
                        },
                    },
                },
            };
            experimentGroup.Setup(g => g.Experiments).Returns(experiments);
            var validator = new ExperimentGroupModificationValidator(
                Mock.Of<ILogger<ExperimentGroupModificationValidator>>());

            // Act
            var actual = validator.CanDeleteExperiments(
                experimentGroup.Object,
                new List<Guid>()
                {
                    idToDeleteStarted,
                    idToDeleteNotStarted,
                });

            // Assert
            actual.IsFailure.Should().BeTrue();
        }

        [TestMethod]
        public void CanDeleteExperiments_1ExperimentNotFound1NotStarted_ReturnsSuccess()
        {
            // Arrange
            var experimentGroup = new Mock<IExperimentGroup>();
            experimentGroup.Setup(g => g.IsCompleted).Returns(false);
            var idToDelete = Guid.NewGuid();
            var experiments = new List<Experiment>()
            {
                new Experiment()
                {
                    Id = idToDelete,
                    Stages = new List<Stage>()
                    {
                        new Stage()
                        {
                            StageType = StageType.Ppv,
                            StageData = new PPVStageData
                            {
                                Status = ProcessStatus.PendingCommit,
                            },
                        },
                    },
                },
            };
            experimentGroup.Setup(g => g.Experiments).Returns(experiments);
            var validator = new ExperimentGroupModificationValidator(
                Mock.Of<ILogger<ExperimentGroupModificationValidator>>());

            // Act
            var actual = validator.CanDeleteExperiments(
                experimentGroup.Object,
                new List<Guid>()
                {
                    idToDelete,
                    Guid.NewGuid(),
                });

            // Assert
            actual.IsSuccess.Should().BeTrue();
        }

        [TestMethod]
        public void CanDeleteExperiments_1ExperimentNotFound1Started_ReturnsFailure()
        {
            // Arrange
            var experimentGroup = new Mock<IExperimentGroup>();
            experimentGroup.Setup(g => g.IsCompleted).Returns(false);
            var idToDelete = Guid.NewGuid();
            var experiments = new List<Experiment>()
            {
                new Experiment()
                {
                    Id = idToDelete,
                    Stages = new List<Stage>()
                    {
                        new Stage()
                        {
                            StageType = StageType.Ppv,
                            StageData = new PPVStageData
                            {
                                Status = ProcessStatus.Running,
                            },
                        },
                    },
                },
            };
            experimentGroup.Setup(g => g.Experiments).Returns(experiments);
            var validator = new ExperimentGroupModificationValidator(
                Mock.Of<ILogger<ExperimentGroupModificationValidator>>());

            // Act
            var actual = validator.CanDeleteExperiments(
                experimentGroup.Object,
                new List<Guid>()
                {
                    idToDelete,
                    Guid.NewGuid(),
                });

            // Assert
            actual.IsFailure.Should().BeTrue();
        }

        [TestMethod]
        public void CanUpdateTestProgram_HappyFlow()
        {
            // Arrange
            var expId = default(Guid);
            var conditions = new Condition[] { new Condition() { Status = ProcessStatus.PendingCommit } };
            var stageData = new ClassStageData { Conditions = conditions };
            var stages = new Stage[] { new Stage() { Id = expId, SequenceId = 1, StageType = StageType.Class, StageData = stageData } };
            var experiments = new Experiment[] { new Experiment() { Stages = stages, IsArchived = false, Id = expId, Vpo = "11235813" } };
            var experimentGroup = new ExperimentGroup() { Experiments = experiments };
            var validator = new ExperimentGroupModificationValidator(
                Mock.Of<ILogger<ExperimentGroupModificationValidator>>());

            // Act
            var actual = validator.CanUpdateExperimentGroup(
                experimentGroup,
                new List<Guid>() { expId });

            // Assert
            actual.IsSuccess.Should().BeTrue();
        }

        [TestMethod]
        public void CanUpdateTestProgram_NumberOfExperimentsMismatch_ErrorReturned()
        {
            // Arrange
            var expId = default(Guid);
            var conditions = new Condition[] { new Condition() { Status = ProcessStatus.PendingCommit } };
            var stageData = new ClassStageData { Conditions = conditions };
            var stages = new Stage[] { new Stage() { Id = expId, SequenceId = 1, StageType = StageType.Class, StageData = stageData } };
            var experiments = new Experiment[] { new Experiment() { Stages = stages, IsArchived = false, Id = expId, Vpo = "11235813" } };
            var experimentGroup = new ExperimentGroup() { Experiments = experiments };
            var validator = new ExperimentGroupModificationValidator(
                Mock.Of<ILogger<ExperimentGroupModificationValidator>>());

            // Act
            var actual = validator.CanUpdateExperimentGroup(
                experimentGroup,
                new List<Guid>() { expId, expId });

            // Assert
            actual.IsSuccess.Should().BeFalse();
        }

        [TestMethod]
        public void CanUpdateTestProgram_ExperimentsProvidedNotUnderGroup_ErrorReturned()
        {
            // Arrange
            var expId = default(Guid);
            var conditions = new Condition[] { new Condition() { Status = ProcessStatus.PendingCommit } };
            var stageData = new ClassStageData { Conditions = conditions };
            var stages = new Stage[] { new Stage() { Id = expId, SequenceId = 1, StageType = StageType.Class, StageData = stageData } };
            var experiments = new Experiment[] { new Experiment() { Stages = stages, IsArchived = false, Id = expId, Vpo = "11235813" } };
            var experimentGroup = new ExperimentGroup() { Experiments = experiments };
            var validator = new ExperimentGroupModificationValidator(
                Mock.Of<ILogger<ExperimentGroupModificationValidator>>());

            // Act
            var actual = validator.CanUpdateExperimentGroup(
                experimentGroup,
                new List<Guid>() { Guid.NewGuid() });

            // Assert
            actual.IsSuccess.Should().BeFalse();
        }

        [TestMethod]
        public void CanUpdateTestProgram_MissingExperimentsInList_ErrorReturned()
        {
            // Arrange
            var expId = default(Guid);
            var conditions = new Condition[] { new Condition() { Status = ProcessStatus.PendingCommit } };
            var stageData = new ClassStageData { Conditions = conditions };
            var stages = new Stage[] { new Stage() { Id = expId, SequenceId = 1, StageType = StageType.Class, StageData = stageData } };
            var experiments = new Experiment[]
            {
                new Experiment() { Stages = stages, IsArchived = false, Id = expId, Vpo = "11235813" },
                new Experiment() { Stages = stages, IsArchived = false, Id = Guid.NewGuid(), Vpo = "ABCD" },
            };
            var experimentGroup = new ExperimentGroup() { Experiments = experiments };
            var validator = new ExperimentGroupModificationValidator(
                Mock.Of<ILogger<ExperimentGroupModificationValidator>>());

            // Act
            var actual = validator.CanUpdateExperimentGroup(
                experimentGroup,
                new List<Guid>() { expId });

            // Assert
            actual.IsSuccess.Should().BeFalse();
        }

        [TestMethod]
        [DataRow(ProcessStatus.Completed)]
        [DataRow(ProcessStatus.Paused)]
        [DataRow(ProcessStatus.Running)]
        public void CanUpdateTestProgram_NotEditableConditions_ReturnsFail(ProcessStatus notEditableCondition)
        {
            // Arrange
            var expId = default(Guid);
            var conditions = new Condition[] { new Condition() { Status = notEditableCondition } };
            var stageData = new ClassStageData { Conditions = conditions };
            var stages = new Stage[] { new Stage() { Id = expId, SequenceId = 1, StageType = StageType.Class, StageData = stageData } };
            var experiments = new Experiment[] { new Experiment() { Stages = stages, IsArchived = false, Id = expId, Vpo = "11235813" } };
            var experimentGroup = new ExperimentGroup() { Experiments = experiments };
            var validator = new ExperimentGroupModificationValidator(
                Mock.Of<ILogger<ExperimentGroupModificationValidator>>());

            // Act
            var actual = validator.CanUpdateExperimentGroup(
                experimentGroup,
                new List<Guid>() { expId });

            // Assert
            actual.IsSuccess.Should().BeFalse();
        }

        [TestMethod]
        public void CanEditTestProgram_ArchivedExperiment_ReturnsFail()
        {
            // Arrange
            var expId = default(Guid);
            var conditions = new Condition[] { new Condition() { Status = ProcessStatus.PendingCommit } };
            var stageData = new ClassStageData { Conditions = conditions };
            var stages = new Stage[] { new Stage() { Id = expId, SequenceId = 1, StageType = StageType.Class, StageData = stageData } };
            var experiments = new Experiment[] { new Experiment() { Stages = stages, IsArchived = true, Id = expId, Vpo = "11235813" } };
            var experimentGroup = new ExperimentGroup() { Experiments = experiments };
            var validator = new ExperimentGroupModificationValidator(
                Mock.Of<ILogger<ExperimentGroupModificationValidator>>());

            // Act
            var actual = validator.CanUpdateExperimentGroup(
                experimentGroup,
                new List<Guid>() { expId });

            // Assert
            actual.IsSuccess.Should().BeFalse();
        }

        [TestMethod]
        public void CanUpdateTestProgram_NullVpo_ReturnsFail()
        {
            // Arrange
            var expId = default(Guid);
            var conditions = new Condition[] { new Condition() { Status = ProcessStatus.PendingCommit } };
            var stageData = new ClassStageData { Conditions = conditions };
            var stages = new Stage[] { new Stage() { Id = expId, SequenceId = 1, StageType = StageType.Class, StageData = stageData } };
            var experiments = new Experiment[] { new Experiment() { Stages = stages, IsArchived = false, Id = expId, Vpo = null } };
            var experimentGroup = new ExperimentGroup() { Experiments = experiments };
            var validator = new ExperimentGroupModificationValidator(
                Mock.Of<ILogger<ExperimentGroupModificationValidator>>());

            // Act
            var actual = validator.CanUpdateExperimentGroup(
                experimentGroup,
                new List<Guid>() { expId });

            // Assert
            actual.IsSuccess.Should().BeFalse();
        }

        [TestMethod]
        public void CanUpdateTestProgram_ExperimentIdsMismatch_ReturnsFail()
        {
            // Arrange
            var expId = new Guid("11223344-5566-7788-99AA-BBCCDDEEFF00");
            var conditions = new Condition[] { new Condition() { Status = ProcessStatus.PendingCommit } };
            var stageData = new ClassStageData { Conditions = conditions };
            var stages = new Stage[] { new Stage() { Id = expId, SequenceId = 1, StageType = StageType.Class, StageData = stageData } };
            var experiments = new Experiment[] { new Experiment() { Stages = stages, IsArchived = false, Id = expId, Vpo = "11235813" } };
            var experimentGroup = new ExperimentGroup() { Experiments = experiments };
            var validator = new ExperimentGroupModificationValidator(
                Mock.Of<ILogger<ExperimentGroupModificationValidator>>());

            // Act
            var actual = validator.CanUpdateExperimentGroup(
                experimentGroup,
                new List<Guid>() { new Guid("11223344-5566-7788-99AA-111111111111") });

            // Assert
            actual.IsSuccess.Should().BeFalse();
        }
    }
}