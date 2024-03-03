using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Entities;
using TTPService.Entities.Extensions;
using TTPService.Enums;

namespace TTPService.Tests.Entities.Extensions
{
    [TestClass]
    public class StageExtensionsFixtureL0
    {
        [DataRow(ProcessStatus.NotStarted, StageType.Class, true)]
        [DataRow(ProcessStatus.PendingCommit, StageType.Class, true)]
        [DataRow(ProcessStatus.Committed, StageType.Class, true)]
        [DataRow(ProcessStatus.Running, StageType.Class, true)]
        [DataRow(ProcessStatus.Completed, StageType.Class, false)]
        [DataRow(ProcessStatus.Paused, StageType.Class, true)]
        [DataRow(ProcessStatus.Canceled, StageType.Class, false)]
        [DataRow(ProcessStatus.Canceling, StageType.Class, false)]
        [DataRow(ProcessStatus.NotStarted, StageType.Olb, true)]
        [DataRow(ProcessStatus.PendingCommit, StageType.Olb, true)]
        [DataRow(ProcessStatus.Committed, StageType.Olb, true)]
        [DataRow(ProcessStatus.Running, StageType.Olb, true)]
        [DataRow(ProcessStatus.Completed, StageType.Olb, false)]
        [DataRow(ProcessStatus.Paused, StageType.Olb, true)]
        [DataRow(ProcessStatus.Canceled, StageType.Olb, false)]
        [DataRow(ProcessStatus.Canceling, StageType.Olb, false)]
        [DataRow(ProcessStatus.NotStarted, StageType.Ppv, true)]
        [DataRow(ProcessStatus.PendingCommit, StageType.Ppv, true)]
        [DataRow(ProcessStatus.Committed, StageType.Ppv, true)]
        [DataRow(ProcessStatus.Running, StageType.Ppv, true)]
        [DataRow(ProcessStatus.Completed, StageType.Ppv, false)]
        [DataRow(ProcessStatus.Paused, StageType.Ppv, true)]
        [DataRow(ProcessStatus.Canceled, StageType.Ppv, false)]
        [DataRow(ProcessStatus.Canceling, StageType.Ppv, false)]
        [DataRow(ProcessStatus.NotStarted, StageType.Maestro, true)]
        [DataRow(ProcessStatus.PendingCommit, StageType.Maestro, true)]
        [DataRow(ProcessStatus.Committed, StageType.Maestro, true)]
        [DataRow(ProcessStatus.Running, StageType.Maestro, true)]
        [DataRow(ProcessStatus.Completed, StageType.Maestro, false)]
        [DataRow(ProcessStatus.Paused, StageType.Maestro, true)]
        [DataRow(ProcessStatus.Canceled, StageType.Maestro, false)]
        [DataRow(ProcessStatus.Canceling, StageType.Maestro, false)]
        [DataTestMethod]
        public void CanBeCanceled_HappyPath(ProcessStatus status, StageType stageType, bool expected)
        {
            // Arrange
            StageData stageData = new ClassStageData();
            switch (stageType)
            {
                case StageType.Class:
                    stageData = new ClassStageData()
                    {
                        Conditions = new[] { new Condition() { Status = status } },
                    };
                    break;
                case StageType.Olb:
                    stageData = new OLBStageData() { Status = status };
                    break;
                case StageType.Ppv:
                    stageData = new PPVStageData() { Status = status };
                    break;
                case StageType.Maestro:
                    stageData = new MaestroStageData() { Status = status };
                    break;
            }

            var stage = new Stage()
            {
                StageType = stageType,
                StageData = stageData,
            };

            // Act
            var actual = stage.CanBeCanceled();

            // Assert
            actual.Should().Be(expected);
        }

        [DataRow(ProcessStatus.Running, StageType.Olb)]
        [DataRow(ProcessStatus.Running, StageType.Ppv)]
        [DataRow(ProcessStatus.Running, StageType.Maestro)]
        [DataTestMethod]
        public void UpdateStatus_HappyPath(ProcessStatus newStatus, StageType stageType)
        {
            // Arrange
            var status = ProcessStatus.NotStarted;
            StageData stageData = new ClassStageData();
            switch (stageType)
            {
                case StageType.Olb:
                    stageData = new OLBStageData() { Status = status };
                    break;
                case StageType.Ppv:
                    stageData = new PPVStageData() { Status = status };
                    break;
                case StageType.Maestro:
                    stageData = new MaestroStageData() { Status = status };
                    break;
            }

            var stage = new Stage()
            {
                StageType = stageType,
                StageData = stageData,
            };

            // Act
            stage.UpdateStatus(newStatus, "changed");

            // Assert
            stage.GetStatus().Should().Be(newStatus);
        }

        [DataRow(StageType.Olb)]
        [DataRow(StageType.Ppv)]
        [DataRow(StageType.Maestro)]
        [DataTestMethod]
        public void UpdateCompletionTime_HappyPath(StageType stageType)
        {
            // Arrange
            var dateTime = DateTime.MinValue;
            StageData stageData = new ClassStageData();
            switch (stageType)
            {
                case StageType.Olb:
                    stageData = new OLBStageData() { CompletionTime = dateTime };
                    break;
                case StageType.Ppv:
                    stageData = new PPVStageData() { CompletionTime = dateTime };
                    break;
                case StageType.Maestro:
                    stageData = new MaestroStageData() { CompletionTime = dateTime };
                    break;
            }

            var stage = new Stage()
            {
                StageType = stageType,
                StageData = stageData,
            };

            var updatedDate = DateTime.Now;

            // Act
            stage.UpdateCompletionTime(updatedDate);

            // Assert
            stage.GetCompletionTime().Should().Be(updatedDate);
        }

        [DataRow(StageType.Olb)]
        [DataRow(StageType.Ppv)]
        [DataRow(StageType.Maestro)]
        [DataTestMethod]
        public void UpdateModificationTime_HappyPath(StageType stageType)
        {
            // Arrange
            var dateTime = DateTime.MinValue;
            StageData stageData = new ClassStageData();
            switch (stageType)
            {
                case StageType.Olb:
                    stageData = new OLBStageData() { ModificationTime = dateTime };
                    break;
                case StageType.Ppv:
                    stageData = new PPVStageData() { ModificationTime = dateTime };
                    break;
                case StageType.Maestro:
                    stageData = new MaestroStageData() { ModificationTime = dateTime };
                    break;
            }

            var stage = new Stage()
            {
                StageType = stageType,
                StageData = stageData,
            };

            var updatedDate = DateTime.Now;

            // Act
            stage.UpdateModificationTime(updatedDate);

            // Assert
            _ = stageType switch
            {
                StageType.Olb => ((OLBStageData)stage.StageData).ModificationTime.Should().Be(updatedDate),
                StageType.Ppv => ((PPVStageData)stage.StageData).ModificationTime.Should().Be(updatedDate),
                StageType.Maestro => ((MaestroStageData)stage.StageData).ModificationTime.Should().Be(updatedDate),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
