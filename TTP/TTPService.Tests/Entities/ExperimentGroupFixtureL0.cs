using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Entities;
using TTPService.Enums;

namespace TTPService.Tests.Entities
{
    [TestClass]
    public class ExperimentGroupFixtureL0
    {
        private TestDataGenerator _testDataGenerator;

        [TestInitialize]
        public void Init()
        {
            _testDataGenerator = new TestDataGenerator();
        }

        [TestMethod]
        public void Construction_SubmissionTimeIsSetOnCreation()
        {
            // Arrange

            // Act
            var experimentGroup = new ExperimentGroup();

            // Assert
            experimentGroup.SubmissionTime.Should().NotBe(default(DateTime));
        }

        [TestMethod]
        public void IsCompleted_NoExperiments_ReturnsFalse()
        {
            // Arrange
            var experimentGroup = new ExperimentGroup() { Experiments = new List<Experiment>() };

            // Act
            var actual = experimentGroup.IsCompleted;

            // Assert
            actual.Should().BeFalse();
        }

        [TestMethod]
        public void IsCompleted_NullExperiments_ReturnsFalse()
        {
            // Arrange
            var experimentGroup = new ExperimentGroup();

            // Act
            var actual = experimentGroup.IsCompleted;

            // Assert
            actual.Should().BeFalse();
        }

        [TestMethod]
        public void IsCompleted_NoConditions_ReturnsFalse()
        {
            // Arrange
            var experimentGroup = new ExperimentGroup()
            {
                Experiments = new List<Experiment>()
                {
                    new Experiment()
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>(),
                                },
                            },
                        },
                    },
                    new Experiment()
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>(),
                                },
                            },
                        },
                    },
                },
            };

            // Act
            var actual = experimentGroup.IsCompleted;

            // Assert
            actual.Should().BeFalse();
        }

        [TestMethod]
        public void IsCompleted_NullStages_ReturnsFalse()
        {
            // Arrange
            var experimentGroup = new ExperimentGroup()
            {
                Experiments = new List<Experiment>()
                {
                    new Experiment(),
                    new Experiment
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>(),
                                },
                            },
                        },
                    },
                },
            };

            // Act
            var actual = experimentGroup.IsCompleted;

            // Assert
            actual.Should().BeFalse();
        }

        [TestMethod]
        public void IsCompleted_All_Stages_Completed_Returns_True()
        {
            // Arrange
            var experimentGroup = new ExperimentGroup()
            {
                Experiments = new List<Experiment>()
                {
                    new Experiment()
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>()
                                    {
                                        new Condition()
                                        {
                                            Status = ProcessStatus.Completed,
                                        },
                                    },
                                },
                            },
                        },
                    },
                    new Experiment()
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>()
                                    {
                                        new Condition()
                                        {
                                            Status = ProcessStatus.Completed,
                                        },
                                    },
                                },
                            },
                            new Stage
                            {
                                StageType = StageType.Olb,
                                StageData = new OLBStageData
                                {
                                    Status = ProcessStatus.Completed,
                                },
                            },
                            new Stage
                            {
                                StageType = StageType.Ppv,
                                StageData = new PPVStageData
                                {
                                    Status = ProcessStatus.Completed,
                                },
                            },
                        },
                    },
                },
            };

            // Act
            var actual = experimentGroup.IsCompleted;

            // Assert
            actual.Should().BeTrue();
        }

        [TestMethod]
        public void IsCompleted_All_Conditions_Completed_Returns_True()
        {
            // Arrange
            var experimentGroup = new ExperimentGroup()
            {
                Experiments = new List<Experiment>()
                {
                    new Experiment()
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>()
                                    {
                                        new Condition()
                                        {
                                            Status = ProcessStatus.Completed,
                                        },
                                    },
                                },
                            },
                        },
                    },
                    new Experiment()
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>()
                                    {
                                        new Condition()
                                        {
                                            Status = ProcessStatus.Completed,
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            };

            // Act
            var actual = experimentGroup.IsCompleted;

            // Assert
            actual.Should().BeTrue();
        }

        [TestMethod]
        public void IsCompleted_AllConditionsAreCompletedOrArchived_ReturnsTrue()
        {
            // Arrange
            var experimentGroup = new ExperimentGroup()
            {
                Experiments = new List<Experiment>()
                {
                    new Experiment()
                    {
                        IsArchived = false,
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>()
                                    {
                                        new Condition()
                                        {
                                            Status = ProcessStatus.Completed,
                                        },
                                    },
                                },
                            },
                        },
                    },
                    new Experiment()
                    {
                        IsArchived = true,
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>()
                                    {
                                        new Condition()
                                        {
                                            Status = ProcessStatus.Canceling,
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            };

            // Act
            var actual = experimentGroup.IsCompleted;

            // Assert
            actual.Should().BeTrue();
        }

        [TestMethod]
        public void IsCompleted_AllArchived_ReturnsTrue()
        {
            // Arrange
            var experimentGroup = new ExperimentGroup()
            {
                Experiments = new List<Experiment>()
                {
                    new Experiment()
                    {
                        IsArchived = true,
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>()
                                    {
                                        new Condition()
                                        {
                                            Status = ProcessStatus.Completed,
                                        },
                                    },
                                },
                            },
                        },
                    },
                    new Experiment()
                    {
                        IsArchived = true,
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>()
                                    {
                                        new Condition()
                                        {
                                            Status = ProcessStatus.Canceling,
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            };

            // Act
            var actual = experimentGroup.IsCompleted;

            // Assert
            actual.Should().BeTrue();
        }

        [TestMethod]
        public void IsCompleted_2ExperimentsOnly1Completed_ReturnsFalse()
        {
            // Arrange
            var experimentGroup = new ExperimentGroup()
            {
                Experiments = new List<Experiment>()
                {
                    new Experiment()
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>()
                                    {
                                        new Condition()
                                        {
                                            Status = ProcessStatus.Completed,
                                        },
                                    },
                                },
                            },
                        },
                    },
                    new Experiment()
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>()
                                    {
                                        new Condition()
                                        {
                                            Status = ProcessStatus.Running,
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            };

            // Act
            var actual = experimentGroup.IsCompleted;

            // Assert
            actual.Should().BeFalse();
        }

        [TestMethod]
        public void IsCompleted_ConditionIsPaused_ReturnsFalse()
        {
            // Arrange
            var experimentGroup = new ExperimentGroup()
            {
                Experiments = new List<Experiment>()
                {
                    new Experiment()
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>()
                                    {
                                        new Condition()
                                        {
                                            Status = ProcessStatus.Paused,
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            };

            // Act
            var actual = experimentGroup.IsCompleted;

            // Assert
            actual.Should().BeFalse();
        }

        [TestMethod]
        public void IsCompleted_1Experiment2ConditionsOnly1Completed_ReturnsFalse()
        {
            // Arrange
            var experimentGroup = new ExperimentGroup()
            {
                Experiments = new List<Experiment>()
                {
                    new Experiment()
                    {
                        IsArchived = false,
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>()
                                    {
                                        new Condition()
                                        {
                                            Status = ProcessStatus.Completed,
                                        },
                                        new Condition()
                                        {
                                            Status = ProcessStatus.Committed,
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            };

            // Act
            var actual = experimentGroup.IsCompleted;

            // Assert
            actual.Should().BeFalse();
        }
    }
}
