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
using TTPService.Dtos.Notification;
using TTPService.Dtos.Notification.Enums;
using TTPService.Entities;
using TTPService.Enums;
using TTPService.Services;

namespace TTPService.Tests.Services
{
    [TestClass]
    public class NotifierFixtureL0
    {
        private static IMapper _mapper;
        private Notifier _sut;
        private Mock<INotificationApi> _apiMock;

        [TestInitialize]
        public void Init()
        {
            _apiMock = new Mock<INotificationApi>();
            _mapper = new MapperConfiguration(config =>
                         config.AddProfile(new MapProfile())).CreateMapper();
            _sut = new Notifier(_mapper, Mock.Of<ILogger<Notifier>>(), _apiMock.Object);
        }

        [DataRow(ExperimentStatus.PendingCommit)]
        [DataRow(ExperimentStatus.Committed)]
        [DataRow(ExperimentStatus.Completed)]
        [DataRow(ExperimentStatus.OnHold)]
        [TestMethod]
        public async Task NotifyExperimentUpdated_HappyPath(ExperimentStatus originalStatus)
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
                                Stages = new List<Stage>()
                                {
                                    new Stage()
                                    {
                                        Id = Guid.NewGuid(),
                                        StageType = StageType.Class,
                                        StageData = new ClassStageData()
                                        {
                                            Conditions = new List<Condition>()
                                            {
                                                new Condition()
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

            _apiMock.Setup(m =>
                    m.NotifyExperimentUpdated(It.IsAny<ExperimentProgressNotificationDto>()))
                .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _sut.NotifyExperimentUpdated(experimentGroup.Value, firstExperiment, originalStatus);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            _apiMock.Verify(
                m => m.NotifyExperimentUpdated(It.IsAny<ExperimentProgressNotificationDto>()), Times.Once);
        }

        [DataRow(ExperimentStatus.Canceled)]
        [TestMethod]
        public async Task NotifyExperimentUpdated_InvalidStatus(ExperimentStatus originalStatus)
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
                                    Stages = new List<Stage>()
                                    {
                                        new Stage()
                                        {
                                            Id = Guid.NewGuid(),
                                            StageType = StageType.Class,
                                            StageData = new ClassStageData()
                                            {
                                                Conditions = new List<Condition>()
                                                {
                                                    new Condition()
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

            _apiMock.Setup(m =>
                    m.NotifyExperimentUpdated(It.IsAny<ExperimentProgressNotificationDto>()))
                .Returns(Task.FromResult(Result.Ok()));

            // Act
            var actual = await _sut.NotifyExperimentUpdated(experimentGroup.Value, firstExperiment, originalStatus);

            // Assert
            actual.IsSuccess.Should().BeTrue();
            _apiMock.Verify(
                m => m.NotifyExperimentUpdated(It.IsAny<ExperimentProgressNotificationDto>()), Times.Never);
        }

        [DataRow(ExperimentStatus.PendingCommit)]
        [DataRow(ExperimentStatus.Committed)]
        [DataRow(ExperimentStatus.Completed)]
        [DataRow(ExperimentStatus.OnHold)]
        [TestMethod]
        public async Task NotifyExperimentUpdated_ReturnsFail(ExperimentStatus originalStatus)
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
                                    Stages = new List<Stage>()
                                    {
                                        new Stage()
                                        {
                                            Id = Guid.NewGuid(),
                                            StageType = StageType.Class,
                                            StageData = new ClassStageData()
                                            {
                                                Conditions = new List<Condition>()
                                                {
                                                    new Condition()
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

            _apiMock.Setup(m =>
                    m.NotifyExperimentUpdated(It.IsAny<ExperimentProgressNotificationDto>()))
              .Returns(Task.FromResult(Result.Fail("Failed to notify experiment status.")));

            // Act
            var actual = await _sut.NotifyExperimentUpdated(experimentGroup.Value, firstExperiment, originalStatus);

            // Assert
            actual.IsFailure.Should().BeTrue();
            _apiMock.Verify(
                m => m.NotifyExperimentUpdated(It.IsAny<ExperimentProgressNotificationDto>()), Times.Once);
        }
    }
}
