using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using Moq;
using TTPService.Configuration;
using TTPService.Entities;
using TTPService.Enums;
using TTPService.Repositories;

namespace TTPService.IntegrationTests
{
    [DoNotParallelize]
    [TestClass]
    public class RepositoryIntegrationTests
    {
        private static Mock<IOptions<RepositoryOptions>> _optionsMock;
        private static Mock<ILogger<Repository>> _loggerMock;
        private static MongoInMemory _mongoInMemory;
        private static RequestContext _requestContext;
        private static Repository _sut;
        private TestDataGenerator _testDataGenerator;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _loggerMock = new Mock<ILogger<Repository>>();
            _optionsMock = new Mock<IOptions<RepositoryOptions>>();
            _optionsMock.Setup(x => x.Value.ConnectionString).Returns(MongoInMemory.ConnectionString);
            _optionsMock.Setup(x => x.Value.Database).Returns(MongoInMemory.DbName);
            _optionsMock.Setup(x => x.Value.ShouldSeedEmptyRepository).Returns(false);
            _optionsMock.Setup(x => x.Value.Collection).Returns("ExperimentGroups");

            _requestContext = new RequestContext(_optionsMock.Object);

            _mongoInMemory = new MongoInMemory();

            _sut = new Repository(_loggerMock.Object, _requestContext);

            // [Team]<-[Golan] - required for object comparison since mongo will save DateTime w/ different accuracy
            AssertionOptions.AssertEquivalencyUsing(options => options.Using<DateTime>(ctx =>
                    ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(100)))
                .WhenTypeIs<DateTime>());
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _mongoInMemory.Dispose();
        }

        [TestInitialize]
        public void Init()
        {
            _testDataGenerator = new TestDataGenerator();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _requestContext.ExperimentGroups.DeleteMany(FilterDefinition<ExperimentGroup>.Empty);
        }

        [TestMethod]
        public async Task AddExperimentGroup_HappyFlow()
        {
            // Arrange
            var experimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();

            // Act
            var actual = await _sut.AddExperimentGroup(experimentGroup);

            // Assert
            actual.Should().BeOfType<Result<ExperimentGroup>>().Which.Value.Id.Should().NotBeEmpty();
            actual.Value.Experiments.All(experiment => experiment.Id != Guid.Empty);
            actual.Value.Experiments
                .SelectMany(e => e.Stages)
                .Where(s => s.StageType == StageType.Class)
                .SelectMany(s => s.StageData.As<ClassStageData>().Conditions)
                .All(condition => condition.Id != Guid.Empty);
        }

        [TestMethod]
        public async Task AddExperimentGroup_MPS_HappyFlow()
        {
            // Arrange
            var experimentGroup = _testDataGenerator.GenerateExperimentForCreationDtoWithMPSCondition();

            // Act
            var actual = await _sut.AddExperimentGroup(experimentGroup);

            // Assert
            actual.Should().BeOfType<Result<ExperimentGroup>>().Which.Value.Id.Should().NotBeEmpty();
            actual.Value.Experiments.All(experiment => experiment.Id != Guid.Empty);
            var actualConditions = actual.Value.Experiments
                .SelectMany(e => e.Stages)
                .Where(s => s.StageType == StageType.Class)
                .SelectMany(s => s.StageData.As<ClassStageData>().Conditions);
            actualConditions.All(condition => condition.Id != Guid.Empty);
            actualConditions.SelectMany(c => c.Thermals).Any(t => t.SequenceId == 1 && t.Name.Equals("HOT"));
            actualConditions.SelectMany(c => c.Thermals).Any(t => t.SequenceId == 2 && t.Name.Equals("COLD"));
        }

        [TestMethod]
        public async Task AddExperimentGroup_ExperimentWithId_ReturnFailure()
        {
            // Arrange
            var experimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            experimentGroup.Experiments.First().Id = Guid.NewGuid();

            // Act
            var actual = await _sut.AddExperimentGroup(experimentGroup);

            // Assert
            actual.IsFailure.Should().BeTrue();
        }

        [TestMethod]
        public async Task AddExperimentGroup_ConditionWithId_ReturnFailure()
        {
            // Arrange
            var experimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            experimentGroup.Experiments.First().Stages.First().StageData.As<ClassStageData>().Conditions.First().Id = Guid.NewGuid();

            // Act
            var actual = await _sut.AddExperimentGroup(experimentGroup);

            // Assert
            actual.IsFailure.Should().BeTrue();
        }

        [TestMethod]
        public async Task GetExperimentGroups_WithNoParameters_HappyFlow()
        {
            // Arrange
            var rollingExperimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            var completedExperimentGroup = _testDataGenerator.CreateExperimentGroupsWithCompletedAndCanceledExperiment();
            await _sut.AddExperimentGroup(rollingExperimentGroup);
            await _sut.AddExperimentGroup(completedExperimentGroup);

            // Act
            var actual = _sut.GetExperimentGroups().Result;

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(2);
        }

        [TestMethod]
        public async Task GetExperimentCompletedGroups_ConditionsWithFinalState_ReturnsCompletedExperimentGroup()
        {
            // Arrange
            var completedExperimentGroup = _testDataGenerator.CreateExperimentGroupsWithCompletedAndCanceledExperiment();
            var cancelExperimentGroup = _testDataGenerator.CreateExperimentGroupsWithCanceledExperiment();
            await _sut.AddExperimentGroup(completedExperimentGroup);
            await _sut.AddExperimentGroup(cancelExperimentGroup);

            // Act
            var actual = _sut.GetExperimentGroups(status: ExperimentGroupStatus.Completed).Result;

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(2);
            actual.Value.First().Id.Should().Be(completedExperimentGroup.Id);
        }

        [TestMethod]
        public async Task GetRollingExperimentGroups_ConditionWithNonFinalStatus_ReturnsRollingExperimentGroup()
        {
            // Arrange
            var rollingExperimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            await _sut.AddExperimentGroup(rollingExperimentGroup);

            // Act
            var actual = _sut.GetExperimentGroups(status: ExperimentGroupStatus.Rolling).Result;

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(1);
            actual.Value.First().Id.Should().Be(rollingExperimentGroup.Id);
        }

        [DataRow(ProcessStatus.Canceling)]
        [DataRow(ProcessStatus.Resuming)]
        [DataRow(ProcessStatus.NotStarted)]
        [DataRow(ProcessStatus.PendingCommit)]
        [DataRow(ProcessStatus.Committed)]
        [DataRow(ProcessStatus.Running)]
        [DataRow(ProcessStatus.Paused)]
        [DataTestMethod]
        public async Task GetExperimentGroups_ConditionWithNotFinalStatus_ReturnsRollingExperimentGroupAndNoCompleted(ProcessStatus status)
        {
            // Arrange
            var experimentGroup = new ExperimentGroup
            {
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        IsArchived = false,
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new Condition[]
                                    {
                                        new Condition
                                        {
                                            Status = status,
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            };
            await _sut.AddExperimentGroup(experimentGroup);

            // Act
            var actualRolling = _sut.GetExperimentGroups(status: ExperimentGroupStatus.Rolling).Result;
            var actualCompleted = _sut.GetExperimentGroups(status: ExperimentGroupStatus.Completed).Result;

            // Assert
            actualRolling.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(1);
            actualCompleted.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(0);
            actualRolling.Value.First().Id.Should().Be(experimentGroup.Id);
        }

        [DataRow(ProcessStatus.Canceling)]
        [DataRow(ProcessStatus.Resuming)]
        [DataRow(ProcessStatus.NotStarted)]
        [DataRow(ProcessStatus.PendingCommit)]
        [DataRow(ProcessStatus.Committed)]
        [DataRow(ProcessStatus.Running)]
        [DataRow(ProcessStatus.Paused)]
        [DataTestMethod]
        public async Task GetExperimentGroups_WithOneArchivedAndOneNoneArchivedExperiment_ReturnsRollingExperimentGroupAndNoCompleted(ProcessStatus status)
        {
            // Arrange
            var experimentGroup = new ExperimentGroup
            {
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        IsArchived = false,
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new Condition[]
                                    {
                                        new Condition
                                        {
                                            Status = status,
                                        },
                                    },
                                },
                            },
                        },
                    },
                    new Experiment
                    {
                        IsArchived = false,
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new Condition[]
                                    {
                                        new Condition
                                        {
                                            Status = status,
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            };
            experimentGroup.Experiments.First().IsArchived = true;

            await _sut.AddExperimentGroup(experimentGroup);

            // Act
            var actualRolling = _sut.GetExperimentGroups(status: ExperimentGroupStatus.Rolling).Result;
            var actualCompleted = _sut.GetExperimentGroups(status: ExperimentGroupStatus.Completed).Result;

            // Assert
            actualRolling.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(1);
            actualCompleted.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(0);
            actualRolling.Value.First().Id.Should().Be(experimentGroup.Id);
        }

        [DataRow(ProcessStatus.Canceling)]
        [DataRow(ProcessStatus.Resuming)]
        [DataRow(ProcessStatus.NotStarted)]
        [DataRow(ProcessStatus.PendingCommit)]
        [DataRow(ProcessStatus.Committed)]
        [DataRow(ProcessStatus.Running)]
        [DataRow(ProcessStatus.Paused)]
        [DataTestMethod]
        public async Task GetExperimentGroups_OneExperimentArchived_ReturnsCompletedExperimentGroupAndNoRolling(ProcessStatus status)
        {
            // Arrange
            var experimentGroup = new ExperimentGroup
            {
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        IsArchived = true,
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                StageData = new ClassStageData
                                {
                                    Conditions = new Condition[]
                                    {
                                        new Condition
                                        {
                                            Status = status,
                                        },
                                    },
                                },
                            },
                        },
                    },
                    new Experiment
                    {
                        IsArchived = false,
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                StageData = new ClassStageData
                                {
                                    Conditions = new Condition[]
                                    {
                                        new Condition
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
            await _sut.AddExperimentGroup(experimentGroup);

            // Act
            var actualRolling = _sut.GetExperimentGroups(status: ExperimentGroupStatus.Rolling).Result;
            var actualCompleted = _sut.GetExperimentGroups(status: ExperimentGroupStatus.Completed).Result;

            // Assert
            actualRolling.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(0);
            actualCompleted.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(1);
        }

        [DataRow(ProcessStatus.Completed)]
        [DataRow(ProcessStatus.Canceled)]
        [DataTestMethod]
        public async Task GetExperimentGroups_ConditionWithNonRollingStatus_ReturnsCompletedExperimentGroupAndNoRolling(ProcessStatus status)
        {
            // Arrange
            var experimentGroup = new ExperimentGroup
            {
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                StageData = new ClassStageData
                                {
                                    Conditions = new Condition[]
                                    {
                                        new Condition
                                        {
                                            Status = status,
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            };
            await _sut.AddExperimentGroup(experimentGroup);

            // Act
            var actualRolling = _sut.GetExperimentGroups(status: ExperimentGroupStatus.Rolling).Result;
            var actualCompleted = _sut.GetExperimentGroups(status: ExperimentGroupStatus.Completed).Result;

            // Assert
            actualRolling.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(0);
            actualCompleted.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(1);
            actualCompleted.Value.First().Id.Should().Be(experimentGroup.Id);
        }

        [TestMethod]
        public async Task GetExperimentGroups_ByUserName_HappyFlow()
        {
            // Arrange
            const string userName = "user1";
            var rollingExperimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            var completedExperimentGroup = _testDataGenerator.CreateCompletedExperimentGroup();
            await _sut.AddExperimentGroup(rollingExperimentGroup);
            await _sut.AddExperimentGroup(completedExperimentGroup);

            // Act
            var actual = _sut.GetExperimentGroups(userName).Result;

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(1);
            actual.Value.First().Id.Should().Be(rollingExperimentGroup.Id);
        }

        [DataRow("cift")]
        [DataRow("CIFT")]

        [DataTestMethod]
        public async Task GetExperimentGroups_ByTeam_HappyFlow(string team)
        {
            // Arrange
            await _sut.AddExperimentGroup(new ExperimentGroup { Team = "cift", Id = new Guid("11223344-5566-7788-99AA-BBCCDDEEFF01"), Experiments = new Experiment[] { } });
            await _sut.AddExperimentGroup(new ExperimentGroup { Team = "spark", Id = new Guid("11223344-5566-7788-99AA-BBCCDDEEFF02"), Experiments = new Experiment[] { } });
            await _sut.AddExperimentGroup(new ExperimentGroup { Team = "spark", Id = new Guid("11223344-5566-7788-99AA-BBCCDDEEFF03"), Experiments = new Experiment[] { } });

            // Act
            var actual = _sut.GetExperimentGroups(team: team).Result;

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(1);
            actual.Value.First().Id.Should().Be(new Guid("11223344-5566-7788-99AA-BBCCDDEEFF01"));
        }

        [TestMethod]

        public async Task GetExperimentGroups_ByTeam_NotFound()
        {
            // Arrange
            await _sut.AddExperimentGroup(new ExperimentGroup { Team = "cift", Experiments = new Experiment[] { } });
            await _sut.AddExperimentGroup(new ExperimentGroup { Team = "cift", Experiments = new Experiment[] { } });

            // Act
            var actual = _sut.GetExperimentGroups(team: "spark").Result;

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(0);
        }

        [DataRow("segment1")]
        [DataRow("Segment1")]

        [DataTestMethod]
        public async Task GetExperimentGroups_BySegment_HappyFlow(string segment)
        {
            // Arrange
            await _sut.AddExperimentGroup(new ExperimentGroup { Segment = "segment1", Id = new Guid("11223344-5566-7788-99AA-BBCCDDEEFF01"), Experiments = new Experiment[] { } });
            await _sut.AddExperimentGroup(new ExperimentGroup { Segment = "segment2", Id = new Guid("11223344-5566-7788-99AA-BBCCDDEEFF02"), Experiments = new Experiment[] { } });
            await _sut.AddExperimentGroup(new ExperimentGroup { Segment = "segment3", Id = new Guid("11223344-5566-7788-99AA-BBCCDDEEFF03"), Experiments = new Experiment[] { } });

            // Act
            var actual = _sut.GetExperimentGroups(segment: "segment1").Result;

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(1);
            actual.Value.First().Id.Should().Be(new Guid("11223344-5566-7788-99AA-BBCCDDEEFF01"));
        }

        [TestMethod]
        public async Task GetExperimentGroups_BySegment_NotFound()
        {
            // Arrange
            await _sut.AddExperimentGroup(new ExperimentGroup { Segment = "segment2", Experiments = new Experiment[] { } });
            await _sut.AddExperimentGroup(new ExperimentGroup { Segment = "segment3", Experiments = new Experiment[] { } });

            // Act
            var actual = _sut.GetExperimentGroups(segment: "segment1").Result;

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(0);
        }

        [DataRow("vpo1")]
        [DataRow("Vpo1")]

        [DataTestMethod]
        public async Task GetExperimentGroups_ByVpo_HappyFlow(string vpo)
        {
            // Arrange
            await _sut.AddExperimentGroup(new ExperimentGroup
            {
                Id = new Guid("11223344-5566-7788-99AA-BBCCDDEEFF01"),
                Experiments = new Experiment[]
                {
                    new Experiment()
                    {
                        Vpo = "vpo1",
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new Condition[] {},
                                },
                            },
                        },
                    },
                    new Experiment()
                    {
                        Vpo = "vpo2",
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new Condition[] {},
                                },
                            },
                        },
                    },
                },
            });
            await _sut.AddExperimentGroup(new ExperimentGroup
            {
                Id = new Guid("11223344-5566-7788-99AA-BBCCDDEEFF02"),
                Experiments = new Experiment[]
                {
                    new Experiment()
                    {
                        Vpo = "vpo3",
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new Condition[] {},
                                },
                            },
                        },
                    },
                    new Experiment()
                    {
                        Vpo = "vpo4",
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new Condition[] {},
                                },
                            },
                        },
                    },
                },
            });

            // Act
            var actual = _sut.GetExperimentGroups(vpo: vpo).Result;

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(1);
            actual.Value.First().Id.Should().Be(new Guid("11223344-5566-7788-99AA-BBCCDDEEFF01"));
        }

        [TestMethod]
        public async Task GetExperimentGroups_ByVpo_NotFound()
        {
            // Arrange
            await _sut.AddExperimentGroup(new ExperimentGroup
            {
                Experiments = new Experiment[]
                {
                    new Experiment()
                    {
                        Vpo = "vpo1",
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new Condition[] {},
                                },
                            },
                        },
                    },
                    new Experiment()
                    {
                        Vpo = "vpo2",
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new Condition[] {},
                                },
                            },
                        },
                    },
                },
            });

            // Act
            var actual = _sut.GetExperimentGroups(vpo: "vpo3").Result;

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(0);
        }

        [DataRow("lot1")]
        [DataRow("Lot1")]

        [DataTestMethod]
        public async Task GetExperimentGroups_ByLotName_HappyFlow(string lotName)
        {
            // Arrange
            await _sut.AddExperimentGroup(new ExperimentGroup
            {
                Id = new Guid("11223344-5566-7788-99AA-BBCCDDEEFF01"),
                Experiments = new Experiment[]
                {
                    new Experiment()
                    {
                        Material = new Material()
                        {
                           Lots = new List<Lot>(){new Lot(){Name = "lot1"}},
                        },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new Condition[] {},
                                },
                            },
                        },
                    },
                    new Experiment()
                    {
                        Material = new Material()
                        {
                            Lots = new List<Lot>(){new Lot(){Name = "lot2"}},
                        },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new Condition[] {},
                                },
                            },
                        },
                    },
                },
            });
            await _sut.AddExperimentGroup(new ExperimentGroup
            {
                Id = new Guid("11223344-5566-7788-99AA-BBCCDDEEFF02"),
                Experiments = new Experiment[]
                {
                    new Experiment()
                    {
                        Material = new Material()
                        {
                            Lots = new List<Lot>(){new Lot(){Name = "lot3"}},
                        },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new Condition[] {},
                                },
                            },
                        },
                    },
                },
            });

            // Act
            var actual = _sut.GetExperimentGroups(lot: lotName).Result;

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(1);
            actual.Value.First().Id.Should().Be(new Guid("11223344-5566-7788-99AA-BBCCDDEEFF01"));
        }

        [TestMethod]
        public async Task GetExperimentGroups_ByLotName_NotFound()
        {
            // Arrange
            await _sut.AddExperimentGroup(new ExperimentGroup
            {
                Experiments = new Experiment[]
                {
                    new Experiment()
                    {
                        Material = new Material()
                        {
                            Lots = new List<Lot>(){new Lot(){Name = "lot1"}},
                        },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new Condition[] {},
                                },
                            },
                        },
                    },
                    new Experiment()
                    {
                        Material = new Material()
                        {
                            Lots = new List<Lot>(){new Lot(){Name = "lot2"}},
                        },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new Condition[] {},
                                },
                            },
                        },
                    },
                },
            });

            // Act
            var actual = _sut.GetExperimentGroups(lot: "lot3").Result;

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(0);
        }

        [DataRow("VisualId1")]
        [DataRow("visualId1")]

        [DataTestMethod]
        public async Task GetExperimentGroups_ByVisualId_HappyFlow(string visualId)
        {
            // Arrange
            await _sut.AddExperimentGroup(new ExperimentGroup
            {
                Id = new Guid("11223344-5566-7788-99AA-BBCCDDEEFF01"),
                Experiments = new Experiment[]
                {
                    new Experiment()
                    {
                        Material = new Material()
                        {
                            Units = new List<Unit>(){new Unit(){VisualId = "VisualId1" } },
                        },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new Condition[] {},
                                },
                            },
                        },
                    },
                    new Experiment()
                    {
                        Material = new Material()
                        {
                            Units = new List<Unit>(){new Unit(){VisualId = "VisualId2" } },
                        },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new Condition[] {},
                                },
                            },
                        },
                    },
                },
            });
            await _sut.AddExperimentGroup(new ExperimentGroup
            {
                Id = new Guid("11223344-5566-7788-99AA-BBCCDDEEFF02"),
                Experiments = new Experiment[]
                {
                    new Experiment()
                    {
                        Material = new Material()
                        {
                            Units = new List<Unit>(){new Unit(){VisualId = "VisualId3" } },
                        },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new Condition[] {},
                                },
                            },
                        },
                    },
                },
            });

            // Act
            var actual = _sut.GetExperimentGroups(visualId: visualId).Result;

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(1);
            actual.Value.First().Id.Should().Be(new Guid("11223344-5566-7788-99AA-BBCCDDEEFF01"));
        }

        [TestMethod]
        public async Task GetExperimentGroups_ByVisualId_NotFound()
        {
            // Arrange
            await _sut.AddExperimentGroup(new ExperimentGroup
            {
                Experiments = new Experiment[]
                {
                    new Experiment()
                    {
                        Material = new Material()
                        {
                            Units = new List<Unit>(){new Unit(){VisualId = "VisualId1" } },
                        },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new Condition[] {},
                                },
                            },
                        },
                    },
                    new Experiment()
                    {
                        Material = new Material()
                        {
                            Units = new List<Unit>(){new Unit(){VisualId = "VisualId2" } },
                        },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageData = new ClassStageData
                                {
                                    Conditions = new Condition[] {},
                                },
                            },
                        },
                    },
                },
            });

            // Act
            var actual = _sut.GetExperimentGroups(visualId: "VisualId3").Result;

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(0);
        }

        [DataRow("notExistingUser", null)]
        [DataRow("notExistingUser", ExperimentGroupStatus.Rolling)]
        [DataRow("notExistingUser", ExperimentGroupStatus.Completed)]
        [DataTestMethod]
        public async Task GetExperimentGroups_ByUserName_NotFound(string userName, ExperimentGroupStatus status)
        {
            // Arrange
            var rollingExperimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            var completedExperimentGroup = _testDataGenerator.CreateCompletedExperimentGroup();
            await _sut.AddExperimentGroup(rollingExperimentGroup);
            await _sut.AddExperimentGroup(completedExperimentGroup);

            // Act
            var actual = _sut.GetExperimentGroups(userName).Result;

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(0);
        }

        [DataRow(null, ExperimentGroupStatus.Rolling)]
        [DataRow("user1", ExperimentGroupStatus.Rolling)]
        [DataTestMethod]
        public async Task GetExperimentGroups_ByUserNameAndRollingStatus_HappyFlow(string userName, ExperimentGroupStatus status)
        {
            // Arrange
            var rollingExperimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            var completedExperimentGroup = _testDataGenerator.CreateCompletedExperimentGroup();
            await _sut.AddExperimentGroup(rollingExperimentGroup);
            await _sut.AddExperimentGroup(completedExperimentGroup);

            // Act
            var actual = _sut.GetExperimentGroups(userName, status).Result;

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(1);
            actual.Value.First().Id.Should().Be(rollingExperimentGroup.Id);
        }

        [DataRow(null, ExperimentGroupStatus.Completed)]
        [DataRow("user2", ExperimentGroupStatus.Completed)]
        [DataTestMethod]
        public async Task GetExperimentGroups_ByCompletedStatusAndStartDateOnly_HappyFlow(string userName, ExperimentGroupStatus status)
        {
            // Arrange
            var completedDateStart = new DateTime(2019, 3, 31);
            var rollingExperimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            var completedExperimentGroup = _testDataGenerator.CreateCompletedExperimentGroup();
            await _sut.AddExperimentGroup(rollingExperimentGroup);
            await _sut.AddExperimentGroup(completedExperimentGroup);

            // Act
            var actual = _sut.GetExperimentGroups(userName, status, completedDateStart).Result;

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(1);
            actual.Value.First().Id.Should().Be(completedExperimentGroup.Id);
        }

        [DataRow(null, ExperimentGroupStatus.Completed)]
        [DataRow("user2", ExperimentGroupStatus.Completed)]
        [DataTestMethod]
        public async Task GetExperimentGroups_ByCompletedStatusAndWithValidTimeRange_HappyFlow(string userName, ExperimentGroupStatus status)
        {
            // Arrange
            var completedDateStart = new DateTime(2019, 3, 31);
            var completedDateEnd = new DateTime(2019, 4, 10);
            var rollingExperimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            var completedExperimentGroup = _testDataGenerator.CreateCompletedExperimentGroup();
            await _sut.AddExperimentGroup(rollingExperimentGroup);
            await _sut.AddExperimentGroup(completedExperimentGroup);

            // Act
            var actual = _sut.GetExperimentGroups(userName, status, completedDateStart, completedDateEnd).Result;

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(1);
            actual.Value.First().Id.Should().Be(completedExperimentGroup.Id);
        }

        [DataRow(null, ExperimentGroupStatus.Completed)]
        [DataRow("user2", ExperimentGroupStatus.Completed)]
        [DataRow("notExistingUser", ExperimentGroupStatus.Completed)]
        [DataTestMethod]
        public async Task GetExperimentGroups_ByUserNameAndCompletedStatusAndNotValidTimeRange_ReturnEmptyExperimentGroupList(string userName, ExperimentGroupStatus status)
        {
            // Arrange
            var completedDateStart = new DateTime(2019, 2, 28);
            var completedDateEnd = new DateTime(2019, 3, 10);
            var rollingExperimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            var completedExperimentGroup = _testDataGenerator.CreateCompletedExperimentGroup();
            await _sut.AddExperimentGroup(rollingExperimentGroup);
            await _sut.AddExperimentGroup(completedExperimentGroup);

            // Act
            var actual = _sut.GetExperimentGroups(userName, status, completedDateStart, completedDateEnd).Result;

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(0);
        }

        [DataRow(null, ExperimentGroupStatus.Completed)]
        [DataRow("user2", ExperimentGroupStatus.Completed)]
        [DataRow("notExistingUser", ExperimentGroupStatus.Completed)]
        [DataTestMethod]
        public async Task GetExperimentGroups_ByUserNameAndCompletedStatusAndLateStartDate_ReturnEmptyExperimentGroupList(string userName, ExperimentGroupStatus status)
        {
            // Arrange
            var completedDateStart = new DateTime(2019, 4, 6);
            var completedDateEnd = new DateTime(2019, 4, 10);
            var rollingExperimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            var completedExperimentGroup = _testDataGenerator.CreateCompletedExperimentGroup();
            await _sut.AddExperimentGroup(rollingExperimentGroup);
            await _sut.AddExperimentGroup(completedExperimentGroup);

            // Act
            var actual = _sut.GetExperimentGroups(userName, status, completedDateStart, completedDateEnd).Result;

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(0);
        }

        [DataRow(null, ExperimentGroupStatus.Completed)]
        [DataRow("user2", ExperimentGroupStatus.Completed)]
        [DataRow("notExistingUser", ExperimentGroupStatus.Completed)]
        [DataTestMethod]
        public async Task GetExperimentGroups_ByUserNameAndCompletedStatusAndEarlyEndDate_ReturnEmptyExperimentGroupList(string userName, ExperimentGroupStatus status)
        {
            // Arrange
            var completedDateStart = new DateTime(2019, 3, 31);
            var completedDateEnd = new DateTime(2019, 4, 2);
            var rollingExperimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            var completedExperimentGroup = _testDataGenerator.CreateCompletedExperimentGroup();
            await _sut.AddExperimentGroup(rollingExperimentGroup);
            await _sut.AddExperimentGroup(completedExperimentGroup);

            // Act
            var actual = _sut.GetExperimentGroups(userName, status, completedDateStart, completedDateEnd).Result;

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(0);
        }

        [DataRow(null, ExperimentGroupStatus.Completed)]
        [DataRow("user2", ExperimentGroupStatus.Completed)]
        [DataTestMethod]
        public async Task GetExperimentGroups_ByUserNameAndCompletedStatusAndOneConditionInValidTimeRange_ReturnEmptyExperimentGroupList(string userName, ExperimentGroupStatus status)
        {
            // Arrange
            var completedDateStart = new DateTime(2019, 3, 31);
            var completedDateEnd = new DateTime(2019, 4, 4);
            var rollingExperimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            var completedExperimentGroup = _testDataGenerator.CreateCompletedExperimentGroup();
            await _sut.AddExperimentGroup(rollingExperimentGroup);
            await _sut.AddExperimentGroup(completedExperimentGroup);

            // Act
            var actual = _sut.GetExperimentGroups(userName, status, completedDateStart, completedDateEnd).Result;

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(0);
        }

        [DataRow(null, ExperimentGroupStatus.Rolling)]
        [DataRow("user1", ExperimentGroupStatus.Rolling)]
        [DataTestMethod]
        public async Task GetExperimentGroups_ByRollingStatusAndStartDateOnly_ReturnEmptyExperimentGroupList(string userName, ExperimentGroupStatus status)
        {
            // Arrange
            var completedDateStart = new DateTime(2019, 3, 31);
            var rollingExperimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            var completedExperimentGroup = _testDataGenerator.CreateCompletedExperimentGroup();
            await _sut.AddExperimentGroup(rollingExperimentGroup);
            await _sut.AddExperimentGroup(completedExperimentGroup);

            // Act
            var actual = _sut.GetExperimentGroups(userName, status, completedDateStart, null, null, null, null, null).Result;

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(0);
        }

        [DataRow(null, ExperimentGroupStatus.Rolling)]
        [DataRow("user1", ExperimentGroupStatus.Rolling)]
        [DataTestMethod]
        public async Task GetExperimentGroups_ByRollingStatusAndWithValidTimeRange_ReturnEmptyExperimentGroupList(string userName, ExperimentGroupStatus status)
        {
            // Arrange
            var completedDateStart = new DateTime(2019, 3, 31);
            var completedDateEnd = new DateTime(2019, 4, 10);
            var rollingExperimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            var completedExperimentGroup = _testDataGenerator.CreateCompletedExperimentGroup();
            await _sut.AddExperimentGroup(rollingExperimentGroup);
            await _sut.AddExperimentGroup(completedExperimentGroup);

            // Act
            var actual = _sut.GetExperimentGroups(userName, status, completedDateStart, completedDateEnd).Result;

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(0);
        }

        [DataRow(null)]
        [DataRow("user2")]
        [DataTestMethod]
        public async Task GetExperimentGroups_ByValidTimeRange_ReturnCompletedOnlyCollection(string userName)
        {
            // Arrange
            var completedDateStart = new DateTime(2019, 3, 31);
            var completedDateEnd = new DateTime(2019, 4, 10);
            var runningExperimentGroup = _testDataGenerator.CreateExperimentGroupWithRunningExperiment();
            var runningAndCompletedExperimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            var completedExperimentGroup = _testDataGenerator.CreateCompletedExperimentGroup();
            await _sut.AddExperimentGroup(runningExperimentGroup);
            await _sut.AddExperimentGroup(runningAndCompletedExperimentGroup);
            await _sut.AddExperimentGroup(completedExperimentGroup);

            // Act
            var actual = _sut.GetExperimentGroups(userName, null, completedDateStart, completedDateEnd).Result;
            actual.Value.First().Id.Should().Be(completedExperimentGroup.Id);

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(1);
        }

        [DataRow(new string[] {"hello"}, 3)]
        [DataRow(new string[] { "  HeLlo  " }, 3)]
        [DataRow(new string[] { "hello", "world", null }, 2)]
        [DataRow(new string[] { "  hello  ", "WORLD  ", null }, 2)]
        [DataTestMethod]
        public async Task GetExperimentGroups_ByExistentTags_ReturnCollection(IEnumerable<string> tags, int numOfMatches)
        {
            // Arrange
            var completedExperimentGroupWithManyTags = _testDataGenerator.CreateCompletedExperimentGroup();
            completedExperimentGroupWithManyTags.Experiments.First().Tags = new string[] {"hello", "world"};
            var completedExperimentGroupWithMoreTags = _testDataGenerator.CreateCompletedExperimentGroup();
            completedExperimentGroupWithMoreTags.Experiments.First().Tags = new string[] { "hello", "world", "again" };
            var rollingExperimentGroupWithOneTag = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            rollingExperimentGroupWithOneTag.Experiments.First().Tags = new string[] { "hello" };
            await _sut.AddExperimentGroup(completedExperimentGroupWithManyTags);
            await _sut.AddExperimentGroup(completedExperimentGroupWithMoreTags);
            await _sut.AddExperimentGroup(rollingExperimentGroupWithOneTag);

            // Act
            var actual = _sut.GetExperimentGroups(null, null, null, null, null, null, null, null, null, tags).Result;

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(numOfMatches);
            actual.Value.ElementAt(0).Id.Should().Be(completedExperimentGroupWithManyTags.Id);
            actual.Value.ElementAt(1).Id.Should().Be(completedExperimentGroupWithMoreTags.Id);
            if (numOfMatches == 3)
            {
                actual.Value.ElementAt(2).Id.Should().Be(rollingExperimentGroupWithOneTag.Id);
            }
        }

        [DataRow(new string[] { "hello" })]
        [DataRow(new string[] { "  HeLlo  " })]
        [DataRow(new string[] { "hello", "world", null })]
        [DataRow(new string[] { "  hello  ", "WORLD  ", null })]
        [DataTestMethod]
        public async Task GetExperimentGroups_ByNonExistentTag_ReturnEmptyCollection(IEnumerable<string> tags)
        {
            // Arrange
            var completedExperimentGroupWithoutTags = _testDataGenerator.CreateCompletedExperimentGroup();
            var completedExperimentGroupWithOtherTags = _testDataGenerator.CreateCompletedExperimentGroup();
            completedExperimentGroupWithOtherTags.Experiments.First().Tags = new string[] { "hi" };
            var rollingExperimentGroupWithSomeTags = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            rollingExperimentGroupWithSomeTags.Experiments.First().Tags = new string[] { "hi", "world" };
            await _sut.AddExperimentGroup(completedExperimentGroupWithoutTags);
            await _sut.AddExperimentGroup(completedExperimentGroupWithOtherTags);
            await _sut.AddExperimentGroup(rollingExperimentGroupWithSomeTags);

            // Act
            var actual = _sut.GetExperimentGroups(null, null, null, null, null, null, null, null, null, tags).Result;

            // Assert
            actual.Should().BeOfType<Result<IEnumerable<ExperimentGroup>>>().Which.Value.Count().Should().Be(0);
        }

        [TestMethod]
        public async Task GetExperimentGroup_HappyFlow()
        {
            // Arrange
            var experimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            var entity = await _sut.AddExperimentGroup(experimentGroup);

            // Act
            var actual = await _sut.GetExperimentGroup(entity.Value.Id);

            // Assert
            actual.Should().BeOfType<Result<Maybe<ExperimentGroup>>>().Which.Value.HasValue.Should().BeTrue();
            actual.Value.Value.Id.Should().Be(entity.Value.Id);
        }

        [TestMethod]
        public async Task GetExperimentGroup_NotFound()
        {
            // Arrange
            var experimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            await _sut.AddExperimentGroup(experimentGroup);
            var nonExistingId = Guid.NewGuid();

            // Act
            var actual = await _sut.GetExperimentGroup(nonExistingId);

            // Assert
            actual.Should().BeOfType<Result<Maybe<ExperimentGroup>>>().Which.Value.HasValue.Should().BeFalse();
        }

        [TestMethod]
        public async Task RemoveExperimentGroup_HappyFlow()
        {
            // Arrange
            var experimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            var entity = await _sut.AddExperimentGroup(experimentGroup);

            // Act
            var actual = await _sut.RemoveExperimentGroup(entity.Value.Id);

            // Assert
            actual.IsSuccess.Should().BeTrue();
        }

        [TestMethod]
        public async Task RemoveExperimentGroup_NotFound()
        {
            // Arrange
            var experimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            await _sut.AddExperimentGroup(experimentGroup);
            var nonExistingId = Guid.NewGuid();

            // Act
            var actual = await _sut.RemoveExperimentGroup(nonExistingId);

            // Assert
            actual.IsFailure.Should().BeTrue();
        }

        [TestMethod]
        public async Task UpdateExperimentGroup_AddExperiments_HappyFlow()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().First();
            var entity = await _sut.AddExperimentGroup(group);
            var experiment1 = _testDataGenerator.CreateExperiment();
            var experiment2 = _testDataGenerator.CreateExperiment();

            var currentCount = group.Experiments.ToList().Count;
            var experiments = group.Experiments.ToList();

            experiments.Add(experiment1);
            experiments.Add(experiment2);

            group.Experiments = experiments;

            // Act
            var result = await _sut.UpdateExperimentGroup(group);
            var actual = await _sut.GetExperimentGroup(group.Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            actual.Should().BeOfType<Result<Maybe<ExperimentGroup>>>();
            var actualGroup = actual.Value.Value;
            actualGroup.Experiments.Count().Should().Be(currentCount + 2);
            actualGroup.Id.Should().Be(entity.Value.Id);
            actualGroup.Experiments.All(x => x.Id != Guid.NewGuid()).Should().BeTrue();
            actualGroup.Experiments.Any(x => x.Id == experiment1.Id).Should().BeTrue();
            actualGroup.Experiments.Any(x => x.Id == experiment2.Id).Should().BeTrue();
        }

        [TestMethod]
        public async Task UpdateExperimentGroup_DeleteExperiments_HappyFlow()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().First();
            var entity = await _sut.AddExperimentGroup(group);

            var currentCount = group.Experiments.ToList().Count;
            var experiments = group.Experiments.ToList();
            var experiment1 = experiments.First();

            experiments.Remove(experiment1);

            group.Experiments = experiments;

            // Act
            var result = await _sut.UpdateExperimentGroup(group);
            var actual = await _sut.GetExperimentGroup(group.Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            actual.Should().BeOfType<Result<Maybe<ExperimentGroup>>>();
            var actualGroup = actual.Value.Value;
            actualGroup.Experiments.Count().Should().Be(currentCount - 1);
            actualGroup.Id.Should().Be(entity.Value.Id);
            actualGroup.Experiments.Any(x => x.Id == experiment1.Id).Should().BeFalse();
        }

        [TestMethod]
        public async Task UpdateExperimentGroup_ModifyExperiments_HappyFlow()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().First();
            var entity = await _sut.AddExperimentGroup(group);

            var currentCount = group.Experiments.ToList().Count;
            var experiments = group.Experiments.ToList();
            var experiment1 = experiments.First();
            var modifiedValue = "modifiedValue";
            var experiment2 = experiments.Last();

            experiment1.EnvironmentFileName = modifiedValue;

            group.Experiments = experiments;

            // Act
            var result = await _sut.UpdateExperimentGroup(group);
            var actual = await _sut.GetExperimentGroup(group.Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            actual.Should().BeOfType<Result<Maybe<ExperimentGroup>>>();
            var actualGroup = actual.Value.Value;
            actualGroup.Experiments.Count().Should().Be(currentCount);
            actualGroup.Id.Should().Be(entity.Value.Id);
            actualGroup.Experiments.Any(x => x.Id == experiment1.Id).Should().BeTrue();
            actualGroup.Experiments.Where(x => x.Id == experiment1.Id).Select(s => s.EnvironmentFileName).First().Should().Be(modifiedValue);
            actualGroup.Experiments.Any(x => x.Id == experiment2.Id).Should().BeTrue();
            actualGroup.Experiments.Where(x => x.Id == experiment2.Id).Select(s => s.EnvironmentFileName).First().Should().NotBe(modifiedValue);
        }

        [TestMethod]
        public async Task UpdateExperimentGroup_AddAndRemoveExperiments_HappyFlow()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().First();
            var entity = await _sut.AddExperimentGroup(group);

            var currentCount = group.Experiments.ToList().Count;
            var experiments = group.Experiments.ToList();
            var experiment1 = experiments.First();
            var modifiedValue = "modifiedValue";
            var experiment2 = experiments.Last();
            var experiment3 = _testDataGenerator.CreateExperiment();
            var experiment4 = _testDataGenerator.CreateExperiment();

            experiments.Remove(experiment2);
            experiments.Add(experiment3);
            experiments.Add(experiment4);

            experiment1.EnvironmentFileName = modifiedValue;

            group.Experiments = experiments;

            // Act
            var result = await _sut.UpdateExperimentGroup(group);
            var actual = await _sut.GetExperimentGroup(group.Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            actual.Should().BeOfType<Result<Maybe<ExperimentGroup>>>();
            var actualGroup = actual.Value.Value;
            actualGroup.Experiments.Count().Should().Be(currentCount + 2 - 1);
            actualGroup.Id.Should().Be(entity.Value.Id);
            actualGroup.Experiments.All(x => x.Id != Guid.NewGuid()).Should().BeTrue();
            actualGroup.Experiments.Any(x => x.Id == experiment1.Id).Should().BeTrue();
            actualGroup.Experiments.Where(x => x.Id == experiment1.Id).Select(s => s.EnvironmentFileName).First().Should().Be(modifiedValue);
            actualGroup.Experiments.Any(x => x.Id == experiment2.Id).Should().BeFalse();
            actualGroup.Experiments.Any(x => x.Id == experiment3.Id).Should().BeTrue();
            actualGroup.Experiments.Any(x => x.Id == experiment4.Id).Should().BeTrue();
        }

        [TestMethod]
        public async Task UpdateExperimentGroup_AddAndRemoveExperiments_IdNotFound()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().First();
            await _sut.AddExperimentGroup(group);

            group.Id = Guid.NewGuid(); // sabotage!

            // Act
            var result = await _sut.UpdateExperimentGroup(group);
            var actual = await _sut.GetExperimentGroup(group.Id);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Contain($"Failed to update experiment group {group.Id} in Request database. Experiment group not found");
            result.Should().BeOfType<Result>();
            actual.Should().BeOfType<Result<Maybe<ExperimentGroup>>>().Which
                  .Value.HasNoValue.Should().BeTrue();
        }

        [TestMethod]
        public async Task UpdateExperimentGroupSubmissionToQueueState_HappyFlow()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().First();
            var entity1 = await _sut.AddExperimentGroup(group);
            var wasSubmitted = !entity1.Value.WasSubmittedToQueue;

            // Act
            var actual = await _sut.UpdateExperimentGroupSubmissionToQueueState(entity1.Value.Id, wasSubmitted);
            var entity2 = await _sut.GetExperimentGroup(entity1.Value.Id);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            entity2.Should().BeOfType<Result<Maybe<ExperimentGroup>>>().Which.Value.HasValue.Should().BeTrue();
            entity2.Value.Value.Id.Should().Be(entity1.Value.Id);
            entity2.Value.Value.WasSubmittedToQueue.Should().Be(wasSubmitted);
        }

        [TestMethod]
        public async Task UpdateExperimentGroupSubmissionToQueueState_ExperimentGroupIdNotExists_FailSceario()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().First();
            var entity1 = await _sut.AddExperimentGroup(group);
            var wasSubmitted = !entity1.Value.WasSubmittedToQueue;
            var nonExistingExperimentGroupId = Guid.NewGuid();

            // Act
            var actual = await _sut.UpdateExperimentGroupSubmissionToQueueState(nonExistingExperimentGroupId, wasSubmitted);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.Should().Contain($"Failed to update experiment group {nonExistingExperimentGroupId} with submission to queue value of '{wasSubmitted}' in Request database. Experiment Group not found");
        }

        [TestMethod]
        public async Task UpdateExperiment_ModifyStatus_HappyFlow()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().ToList().First();
            var entity = await _sut.AddExperimentGroup(group);

            var experiment1 = group.Experiments.ToList().First();
            var condition1 = experiment1.Stages.First().StageData.As<ClassStageData>().Conditions.First();
            var condition2 = experiment1.Stages.First().StageData.As<ClassStageData>().Conditions.Last();
            var modifiedStatus = ProcessStatus.PendingCommit;
            condition1.Status = modifiedStatus;

            // Act
            var result = await _sut.UpdateExperiment(group.Id, experiment1);
            var actual = await _sut.GetExperimentGroup(group.Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            actual.IsSuccess.Should().BeTrue();
            actual.Value.HasValue.Should().BeTrue();
            var actualExperiment = actual.Value.Value.Experiments.First(e => e.Id == experiment1.Id);
            actualExperiment.Stages.First().StageData.As<ClassStageData>().Conditions.First(c => c.Id == condition1.Id).Status
                            .Should().Be(modifiedStatus);
            actualExperiment.Stages.First().StageData.As<ClassStageData>().Conditions.First(c => c.Id == condition2.Id).Status
                            .Should().NotBe(modifiedStatus);
        }

        [TestMethod]
        public async Task UpdateExperiment_GroupIdNotFound()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().ToList().First();
            var entity = await _sut.AddExperimentGroup(group);

            var experiment1 = group.Experiments.ToList().First();
            var newExperimentGroupId = Guid.NewGuid();

            // Act
            var result = await _sut.UpdateExperiment(newExperimentGroupId, experiment1);
            var actual = await _sut.GetExperimentGroup(newExperimentGroupId);

            // Assert
            actual.Value.HasValue.Should().BeFalse();
            result.Error.Should().Contain($"Failed to update experiment { experiment1.Id} in Request database");
        }

        [TestMethod]
        public async Task UpdateExperiment_ModifyStatus_ExperimentNotFound()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().ToList().First();
            var entity = await _sut.AddExperimentGroup(group);

            var experiment1 = group.Experiments.ToList().First();
            var condition1 = experiment1.Stages.First().StageData.As<ClassStageData>().Conditions.First();
            var condition2 = experiment1.Stages.First().StageData.As<ClassStageData>().Conditions.Last();
            var modifiedStatus = ProcessStatus.PendingCommit;
            condition1.Status = modifiedStatus;
            var oldExperimentId = experiment1.Id;
            experiment1.Id = Guid.NewGuid();

            // Act
            var result = await _sut.UpdateExperiment(group.Id, experiment1);
            var actual = await _sut.GetExperimentGroup(group.Id);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Value.HasValue.Should().BeTrue();
            var actualGroup = actual.Value.Value;
            actualGroup.Experiments.Count().Should().Be(2);
            actualGroup.Experiments.Any(e => e.Id == experiment1.Id).Should().BeFalse();
            var actualExperiment = actualGroup.Experiments.First(e => e.Id == oldExperimentId);
            actualExperiment.Stages.First().StageData.As<ClassStageData>().Conditions.First(c => c.Id == condition1.Id).Status
                            .Should().NotBe(modifiedStatus);
            actualExperiment.Stages.First().StageData.As<ClassStageData>().Conditions.First(c => c.Id == condition2.Id).Status
                            .Should().NotBe(modifiedStatus);
            result.Error.Should().Contain($"Failed to update experiment { experiment1.Id} in Request database");
        }

        [TestMethod]
        public async Task UpdateExperiment_ModifyResult_HappyFlow()
        {
            // Arrange
            var experimentGroup = _testDataGenerator.CreateExperimentGroupWithCompletedAndRunningExperiment();
            var entity = await _sut.AddExperimentGroup(experimentGroup);

            var experimentGroupId = entity.Value.Id;
            var experiments = experimentGroup.Experiments.ToList();
            var experiment1 = experiments.First();
            var condition1 = experiment1.Stages.First().StageData.As<ClassStageData>().Conditions.First();
            var result1 = condition1.Results; // null
            var newResult = new ConditionResult { NumberOfBadBins = 10, NumberOfGoodBins = 10 };
            condition1.Results = newResult;

            // Act
            var result = await _sut.UpdateExperiment(experimentGroupId, experiment1);
            var actual = await _sut.GetExperimentGroup(experimentGroupId);

            // Assert
            actual.Should().BeOfType<Result<Maybe<ExperimentGroup>>>().Which.Value.Value.Id.Should().Be(experimentGroupId);
            actual.Value.Value.Experiments.Any(e => e.Id == experiment1.Id).Should().BeTrue();
            actual.Value.Value.Experiments.First(e => e.Id == experiment1.Id).Stages.First().StageData.As<ClassStageData>().Conditions.Any(c => c.Id == condition1.Id).Should().BeTrue();
            actual.Value.Value.Experiments.First(e => e.Id == experiment1.Id).Stages.First().StageData.As<ClassStageData>().Conditions.First(c => c.Id == condition1.Id).Results.Should().NotBeNull();
            actual.Value.Value.Experiments.First(e => e.Id == experiment1.Id).Stages.First().StageData.As<ClassStageData>().Conditions.First(c => c.Id == condition1.Id).Results.NumberOfBadBins.Should().Be(newResult.NumberOfBadBins);
            actual.Value.Value.Experiments.First(e => e.Id == experiment1.Id).Stages.First().StageData.As<ClassStageData>().Conditions.First(c => c.Id == condition1.Id).Results.NumberOfGoodBins.Should().Be(newResult.NumberOfGoodBins);
            result1.Should().BeNull();
        }

        [TestMethod]
        public async Task GetTopCommonEngineeringIds_HappyFlow()
        {
            // Arrange
            await _sut.AddExperimentGroup(_testDataGenerator.CreateIclExperimentGroupForAggregationUser1());
            await _sut.AddExperimentGroup(_testDataGenerator.CreateIclExperimentGroupForAggregationUser2());
            await _sut.AddExperimentGroup(_testDataGenerator.CreateIclExperimentGroupForAggregationUser3());
            await _sut.AddExperimentGroup(_testDataGenerator.CreateClxExperimentGroupForAggregationUser4());
            var programFamily = "ICL";
            var amount = 3;

            // Act
            var actual = await _sut.GetTopCommonEngineeringIds(programFamily, (uint)amount);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Value.Should().NotBeNull();
            actual.Value.Count().Should().Be(amount);
            actual.Value.Contains("EngineeringId12").Should().BeTrue();  // EngineeringId12 : 5
            actual.Value.Contains("EngineeringId15").Should().BeTrue();  // EngineeringId15 : 4
            actual.Value.Contains("EngineeringId17").Should().BeTrue();  // EngineeringId17 : 4
            actual.Value.Contains("EngineeringId19").Should().BeFalse(); // EngineeringId19 : 2
        }

        [TestMethod]
        public async Task GetTopCommonEngineeringIds_NonExistingProgramFamily()
        {
            // Arrange
            await _sut.AddExperimentGroup(_testDataGenerator.CreateIclExperimentGroupForAggregationUser1());
            await _sut.AddExperimentGroup(_testDataGenerator.CreateIclExperimentGroupForAggregationUser2());
            await _sut.AddExperimentGroup(_testDataGenerator.CreateIclExperimentGroupForAggregationUser3());
            await _sut.AddExperimentGroup(_testDataGenerator.CreateClxExperimentGroupForAggregationUser4());
            var programFamily = "nonExisting";
            var amount = 3;

            // Act
            var actual = await _sut.GetTopCommonEngineeringIds(programFamily, (uint)amount);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Value.Should().NotBeNull();
            actual.Value.Count().Should().Be(0);
        }

        [TestMethod]
        public async Task GetTopCommonEngineeringIds_AmountFarGreaterThanAvailableResults()
        {
            // Arrange
            await _sut.AddExperimentGroup(_testDataGenerator.CreateIclExperimentGroupForAggregationUser1());
            await _sut.AddExperimentGroup(_testDataGenerator.CreateIclExperimentGroupForAggregationUser2());
            await _sut.AddExperimentGroup(_testDataGenerator.CreateIclExperimentGroupForAggregationUser3());
            await _sut.AddExperimentGroup(_testDataGenerator.CreateClxExperimentGroupForAggregationUser4());
            var programFamily = "ICL";
            var amount = 30;

            // Act
            var actual = await _sut.GetTopCommonEngineeringIds(programFamily, (uint)amount);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Value.Should().NotBeNull();
            actual.Value.Count().Should().BeLessThan(amount);
        }

        [TestMethod]
        public async Task GetTopCommonLocationCodes_HappyFlow()
        {
            // Arrange
            await _sut.AddExperimentGroup(_testDataGenerator.CreateIclExperimentGroupForAggregationUser1());
            await _sut.AddExperimentGroup(_testDataGenerator.CreateIclExperimentGroupForAggregationUser2());
            await _sut.AddExperimentGroup(_testDataGenerator.CreateIclExperimentGroupForAggregationUser3());
            await _sut.AddExperimentGroup(_testDataGenerator.CreateClxExperimentGroupForAggregationUser4());
            var programFamily = "ICL";
            var amount = 3;

            // Act
            var actual = await _sut.GetTopCommonLocationCodes(programFamily, (uint)amount);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Value.Should().NotBeNull();
            actual.Value.Count().Should().Be(amount);
            actual.Value.Should().Contain(6262);   // 6262 : 8
            actual.Value.Should().Contain(7712);   // 7712 : 11
            actual.Value.Should().Contain(6881);    // 6881 : 6
            actual.Value.Should().NotContain(7717); // 7717 : 1
        }

        [TestMethod]
        public async Task GetTopCommonLocationCodes_NonExistingProgramFamily()
        {
            // Arrange
            await _sut.AddExperimentGroup(_testDataGenerator.CreateIclExperimentGroupForAggregationUser1());
            await _sut.AddExperimentGroup(_testDataGenerator.CreateIclExperimentGroupForAggregationUser2());
            await _sut.AddExperimentGroup(_testDataGenerator.CreateIclExperimentGroupForAggregationUser3());
            await _sut.AddExperimentGroup(_testDataGenerator.CreateClxExperimentGroupForAggregationUser4());
            var programFamily = "nonExisting";
            var amount = 3;

            // Act
            var actual = await _sut.GetTopCommonLocationCodes(programFamily, (uint)amount);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Value.Should().NotBeNull();
            actual.Value.Count().Should().Be(0);
        }

        [TestMethod]
        public async Task GetTopCommonLocationCodes_AmountFarGreaterThanAvailableResults()
        {
            // Arrange
            await _sut.AddExperimentGroup(_testDataGenerator.CreateIclExperimentGroupForAggregationUser1());
            await _sut.AddExperimentGroup(_testDataGenerator.CreateIclExperimentGroupForAggregationUser2());
            await _sut.AddExperimentGroup(_testDataGenerator.CreateIclExperimentGroupForAggregationUser3());
            await _sut.AddExperimentGroup(_testDataGenerator.CreateClxExperimentGroupForAggregationUser4());
            var programFamily = "ICL";
            var amount = 30;

            // Act
            var actual = await _sut.GetTopCommonLocationCodes(programFamily, (uint)amount);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            actual.Value.Should().NotBeNull();
            actual.Value.Count().Should().BeLessThan(amount);
        }

        [TestMethod]
        public async Task GetExperimentGroupByConditionId_HappyFLow()
        {
            // Arrange
            var group = _testDataGenerator.CreateExperimentGroups().First();
            var result = await _sut.AddExperimentGroup(group);
            var groupId = result.Value.Id;
            var conditionId = result.Value.Experiments.First().Stages.First().StageData.As<ClassStageData>().Conditions.First().Id;

            // Act
            var actual = await _sut.GetExperimentGroupByConditionId(conditionId);

            // Assert
            actual.Should().BeOfType<Result<Maybe<ExperimentGroup>>>()
                  .Which.Value.HasValue.Should().BeTrue();
            actual.Value.Value.Id.Should().Be(groupId);
        }

        [TestMethod]
        public async Task GetExperimentGroupByConditionId_NotFound()
        {
            // Act
            var actual = await _sut.GetExperimentGroupByConditionId(Guid.NewGuid());

            // Assert
            actual.Should().BeOfType<Result<Maybe<ExperimentGroup>>>().
                   Which.Value.HasValue.Should().BeFalse();
        }

        [TestMethod]
        public async Task UpdateExperimentCondition_Happy()
        {
            // Arrange
            var experimentGroupToAdd = _testDataGenerator.CreateExperimentGroups().First();
            var addResult = await _sut.AddExperimentGroup(experimentGroupToAdd);
            var experimentGroup = addResult.Value;
            var experiment = experimentGroup.Experiments.First();
            var stage = experiment.Stages.First();
            stage.StageData.As<ClassStageData>().Conditions.Should()
                .HaveCountGreaterOrEqualTo(2, "experiment must have 2 or more conditions to run this test");

            var updateCondition1 = stage.StageData.As<ClassStageData>().Conditions.First();
            updateCondition1.Comment = "fake comment 1";
            updateCondition1.CompletionTime = DateTime.UtcNow;
            updateCondition1.Status = ProcessStatus.Paused;
            updateCondition1.StatusChangeComment = "on hold comment";
            updateCondition1.Results = new ConditionResult { NumberOfBadBins = 3, NumberOfGoodBins = 6 };

            var updateCondition2 = stage.StageData.As<ClassStageData>().Conditions.Skip(1).First();
            updateCondition2.Comment = "fake comment 2";
            updateCondition2.CompletionTime = DateTime.UtcNow;
            updateCondition2.Status = ProcessStatus.Canceled;
            updateCondition2.StatusChangeComment = "cancel comment";

            // Act
            var update1 = _sut.UpdateExperimentCondition(
                experimentGroup.Id,
                experiment.Id,
                stage.Id,
                updateCondition1);

            var update2 = _sut.UpdateExperimentCondition(
                experimentGroup.Id,
                experiment.Id,
                stage.Id,
                updateCondition2);

            var results = await Task.WhenAll(update1, update2);

            // Assert
            Result.Combine(results).IsSuccess.Should().BeTrue();
            var experimentGroupResult = await _sut.GetExperimentGroup(experimentGroup.Id);
            var actual = experimentGroupResult.Value.Value;
            actual.Should().BeEquivalentTo(experimentGroup);
        }

        [TestMethod]
        public async Task UpdateExperimentCondition_Idempotent_ReturnSuccess()
        {
            // Arrange
            var experimentGroupToAdd = _testDataGenerator.CreateExperimentGroups().First();
            var addResult = await _sut.AddExperimentGroup(experimentGroupToAdd);
            var experimentGroup = addResult.Value;
            var experiment = experimentGroup.Experiments.First();
            var stage = experiment.Stages.First();
            var updateCondition1 = stage.StageData.As<ClassStageData>().Conditions.First();

            // Act
            var result = await _sut.UpdateExperimentCondition(
                experimentGroup.Id,
                experiment.Id,
                stage.Id,
                updateCondition1);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [TestMethod]
        public async Task UpdateExperimentCondition_ExperimentGroupIdNotExist_ReturnFail()
        {
            // Arrange
            var experimentGroupToAdd = _testDataGenerator.CreateExperimentGroups().First();
            var addResult = await _sut.AddExperimentGroup(experimentGroupToAdd);
            var experimentGroup = addResult.Value;
            var experiment = experimentGroup.Experiments.First();
            var stage = experiment.Stages.First();
            var updateCondition1 = stage.StageData.As<ClassStageData>().Conditions.First();

            // Act
            var result = await _sut.UpdateExperimentCondition(
                Guid.Empty,
                experiment.Id,
                stage.Id,
                updateCondition1);

            // Assert
            result.IsFailure.Should().BeTrue();
        }

        [TestMethod]
        public async Task UpdateExperimentCondition_ExperimentIdNotExist_ReturnFail()
        {
            // Arrange
            var experimentGroupToAdd = _testDataGenerator.CreateExperimentGroups().First();
            var addResult = await _sut.AddExperimentGroup(experimentGroupToAdd);
            var experimentGroup = addResult.Value;
            var experiment = experimentGroup.Experiments.First();
            var stage = experiment.Stages.First();
            var updateCondition1 = stage.StageData.As<ClassStageData>().Conditions.First();

            // Act
            var result = await _sut.UpdateExperimentCondition(
                experimentGroup.Id,
                Guid.Empty,
                stage.Id,
                updateCondition1);

            // Assert
            result.IsFailure.Should().BeTrue();
        }

        [TestMethod]
        public async Task UpdateExperimentCondition_ConditionIdNotExist_ReturnFail()
        {
            // Arrange
            var experimentGroupToAdd = _testDataGenerator.CreateExperimentGroups().First();
            var addResult = await _sut.AddExperimentGroup(experimentGroupToAdd);
            var experimentGroup = addResult.Value;
            var experiment = experimentGroup.Experiments.First();
            var stage = experiment.Stages.First();
            var updateCondition1 = stage.StageData.As<ClassStageData>().Conditions.First();
            updateCondition1.Id = Guid.Empty;

            // Act
            var result = await _sut.UpdateExperimentCondition(
                experimentGroup.Id,
                experiment.Id,
                stage.Id,
                updateCondition1);

            // Assert
            result.IsFailure.Should().BeTrue();
        }

        [TestMethod]
        public async Task UpdateExperimentCondition_Exception_ReturnFail()
        {
            // Arrange
            var requestContextMock = new Mock<IRequestContext>();
            requestContextMock.Setup(s => s.ExperimentGroups).Throws(new Exception("fake exception"));
            var sut = new Repository(_loggerMock.Object, requestContextMock.Object);

            var experimentGroupToAdd = _testDataGenerator.CreateExperimentGroups().First();
            var addResult = await _sut.AddExperimentGroup(experimentGroupToAdd);
            var experimentGroup = addResult.Value;
            var experiment = experimentGroup.Experiments.First();
            var stage = experiment.Stages.First();
            var updateCondition1 = stage.StageData.As<ClassStageData>().Conditions.First();

            // Act
            var result = await sut.UpdateExperimentCondition(
                experimentGroup.Id,
                experiment.Id,
                stage.Id,
                updateCondition1);

            // Assert
            result.IsFailure.Should().BeTrue();
        }
    }
}