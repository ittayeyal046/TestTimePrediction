using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Entities;
using TTPService.Entities.Extensions;
using TTPService.Enums;

namespace TTPService.Tests.Entities.Extensions
{
    [TestClass]
    public class ConditionExtensionsFixtureL0
    {
        [DataRow(ProcessStatus.NotStarted, true)]
        [DataRow(ProcessStatus.PendingCommit, true)]
        [DataRow(ProcessStatus.Committed, true)]
        [DataRow(ProcessStatus.Running, true)]
        [DataRow(ProcessStatus.Completed, false)]
        [DataRow(ProcessStatus.Paused, true)]
        [DataRow(ProcessStatus.Canceled, false)]
        [DataRow(ProcessStatus.Canceling, false)]
        [DataTestMethod]
        public void CanBeCanceled_HappyPath(ProcessStatus status, bool expected)
        {
            // Arrange
            var condition = new Condition() { Status = status };

            // Act
            var actual = condition.CanBeCanceled();

            // Assert
            actual.Should().Be(expected);
        }
    }
}