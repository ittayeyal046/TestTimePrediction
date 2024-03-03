using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Entities;
using TTPService.Enums;

namespace TTPService.Tests.Entities
{
    [TestClass]
    public class ExperimentFixtureL0
    {
        [TestMethod]
        public void HasStarted_NullStages_ReturnsFalse()
        {
            // Arrange
            var experiment = new Experiment();

            // Act
            var actual = experiment.HasStarted;

            // Assert
            actual.Should().BeFalse();
        }

        [TestMethod]
        public void HasStarted_NoStages_ReturnsFalse()
        {
            // Arrange
            var experiment = new Experiment() { Stages = new List<Stage>() };

            // Act
            var actual = experiment.HasStarted;

            // Assert
            actual.Should().BeFalse();
        }

        [DataRow(ProcessStatus.NotStarted)]
        [DataRow(ProcessStatus.PendingCommit)]
        [DataTestMethod]
        public void HasStarted_NoStageStarted_ReturnsFalse(ProcessStatus status)
        {
            // Arrange
            var experiment = new Experiment()
            {
                Stages = new List<Stage>()
                {
                    new Stage()
                    {
                        StageType = StageType.Class,
                        StageData = new ClassStageData()
                        {
                            Conditions = new Condition[]
                            {
                                new Condition(){Status = ProcessStatus.PendingCommit},
                            },
                        },
                    },
                    new Stage()
                    {
                        StageType = StageType.Olb,
                        StageData = new OLBStageData
                        {
                            Status = status,
                        },
                    },
                },
            };

            // Act
            var actual = experiment.HasStarted;

            // Assert
            actual.Should().BeFalse();
        }

        [DataRow(ProcessStatus.Committed)]
        [DataRow(ProcessStatus.Completed)]
        [DataRow(ProcessStatus.Running)]
        [DataRow(ProcessStatus.Paused)]
        [DataTestMethod]
        public void HasStarted_1StartedStage_ReturnsTrue(ProcessStatus startedStatus)
        {
            // Arrange
            var experiment = new Experiment()
            {
                Stages = new List<Stage>()
                {
                    new Stage()
                    {
                        StageType = StageType.Olb,
                        StageData = new OLBStageData
                        {
                            Status = startedStatus,
                        },
                    },
                },
            };

            // Act
            var actual = experiment.HasStarted;

            // Assert
            actual.Should().BeTrue();
        }

        [DataRow(ProcessStatus.Committed)]
        [DataRow(ProcessStatus.Completed)]
        [DataRow(ProcessStatus.Running)]
        [DataRow(ProcessStatus.Paused)]
        [DataTestMethod]
        public void HasStarted_OnlyClassStageStarted_ReturnsTrue(ProcessStatus startedStatus)
        {
            // Arrange
            var experiment = new Experiment()
            {
                Stages = new List<Stage>()
                {
                    new Stage()
                    {
                        StageType = StageType.Class,
                        StageData = new ClassStageData()
                        {
                            Conditions = new Condition[]
                            {
                                new Condition(){Status = ProcessStatus.PendingCommit},
                                new Condition(){Status = startedStatus},
                            },
                        },
                    },
                    new Stage()
                    {
                        StageType = StageType.Olb,
                        StageData = new OLBStageData
                        {
                            Status = ProcessStatus.NotStarted,
                        },
                    },
                },
            };

            // Act
            var actual = experiment.HasStarted;

            // Assert
            actual.Should().BeTrue();
        }

        [TestMethod]
        public void HasStarted_StartedAndNotStartedStages_ReturnsTrue()
        {
            // Arrange
            var experiment = new Experiment()
            {
                Stages = new List<Stage>()
                {
                    new Stage()
                    {
                        StageType = StageType.Olb,
                        StageData = new OLBStageData
                        {
                            Status = ProcessStatus.Running,
                        },
                    },
                    new Stage()
                    {
                        StageType = StageType.Ppv,
                        StageData = new PPVStageData()
                        {
                            Status = ProcessStatus.PendingCommit,
                        },
                    },
                },
            };

            // Act
            var actual = experiment.HasStarted;

            // Assert
            actual.Should().BeTrue();
        }

        [TestMethod]
        public void HasMaterialIssue_NoMaterialDetails_ReturnsFalse()
        {
            // Arrange
            var experiment = new Experiment()
            {
            };

            // Act
            var actual = experiment.HasMaterialIssue;

            // Assert
            actual.Should().BeFalse();
        }

        [TestMethod]
        public void HasMaterialIssue_OldObjectContents_ReturnsFalse()
        {
            // Arrange
            var experiment = new Experiment()
            {
                Material = new Material()
                {
                    Lots = new[]
                    {
                        new Lot()
                        {
                            Name = "dummy",
                        },
                    },
                },
            };

            // Act
            var actual = experiment.HasMaterialIssue;

            // Assert
            actual.Should().BeFalse();
        }

        [TestMethod]
        public void HasMaterialIssue_MaterialIsRequired_ReturnsTrue()
        {
            // Arrange
            var experiment = ExperimentWithFailedIssueStep();

            // Act
            var actual = experiment.HasMaterialIssue;

            // Assert
            actual.Should().BeTrue();
        }

        [TestMethod]
        public void HasMaterialIssueFailure_MaterialIsRequiredAndNonEmptyComments_ReturnsTrue()
        {
            // Arrange
            var experiment = ExperimentWithFailedIssueStep();

            // Act
            var actual = experiment.HasMaterialIssueFailure;

            // Assert
            actual.Should().BeTrue();
        }

        [TestMethod]
        public void HasMaterialIssueFailure_MaterialIsRequiredAndEmptyComments_ReturnsFalse()
        {
            // Arrange
            var experiment = ExperimentWithFailedIssueStep();
            experiment.Material.MaterialIssue.MaterialIssueErrorComments = string.Empty;

            // Act
            var actual = experiment.HasMaterialIssueFailure;

            // Assert
            actual.Should().BeFalse();
        }

        [TestMethod]
        public void HasMaterialIssueFailure_OldObjectContents_ReturnsFalse()
        {
            // Arrange
            var experiment = new Experiment()
            {
                Material = new Material()
                {
                    Lots = new[]
                    {
                        new Lot()
                        {
                            Name = "dummy",
                        },
                    },
                },
            };

            // Act
            var actual = experiment.HasMaterialIssueFailure;

            // Assert
            actual.Should().BeFalse();
        }

        private static Experiment ExperimentWithFailedIssueStep()
        {
            return new Experiment()
            {
                Material = new Material()
                {
                    Lots = new[]
                    {
                        new Lot()
                        {
                            Name = "dummy",
                        },
                    },
                    MaterialIssue = new MaterialIssue()
                    {
                        MaterialIssueIsRequired = true,
                        MaterialIssueErrorComments = "some comments",
                    },
                },
            };
        }
    }
}