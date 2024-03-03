using System;
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
    public class CallbacksValidatorFixtureL0
    {
        private CallbacksValidator _validator;
        private TestDataGenerator _testDataGenerator;

        [TestInitialize]
        public void Init()
        {
            _validator = new CallbacksValidator(Mock.Of<ILogger<CallbacksValidator>>());
            _testDataGenerator = new TestDataGenerator();
        }

        [TestMethod]
        public void ValidateUpdateResultIsAllowed_HappyPath_ReturnsSuccess()
        {
            // Arrange
            var condition = new Condition() { Status = ProcessStatus.Completed };

            // Act
            var actual = _validator.ValidateUpdateResultIsAllowed(condition);

            // Assert
            actual.IsSuccess.Should().BeTrue();
        }

        [DataRow(ProcessStatus.NotStarted)]
        [DataRow(ProcessStatus.Committed)]
        [DataRow(ProcessStatus.Paused)]
        [DataRow(ProcessStatus.PendingCommit)]
        [DataRow(ProcessStatus.Running)]
        [DataTestMethod]
        public void ValidateUpdateResultIsAllowed_ProcessStatusIsNotCompleted_ReturnsFailure(ProcessStatus status)
        {
            // Arrange
            var condition = new Condition() { Status = status };

            // Act
            var actual = _validator.ValidateUpdateResultIsAllowed(condition);

            // Assert
            actual.IsFailure.Should().BeTrue();
        }

        [TestMethod]
        public void ValidateUpdateResultIsAllowed_ResultsAlreadyExist_ReturnsFailure()
        {
            // Arrange
            var condition = new Condition()
            {
                Status = ProcessStatus.Completed,
                Results = new ConditionResult() { NumberOfGoodBins = 2 },
            };

            // Act
            var actual = _validator.ValidateUpdateResultIsAllowed(condition);

            // Assert
            actual.IsFailure.Should().BeTrue();
        }

        [TestMethod]
        public void ValidateUpdateResultIsAllowed_AllValidationsFailed_ReturnsFailure()
        {
            // Arrange
            var condition = new Condition()
            {
                Status = ProcessStatus.PendingCommit,
                Results = new ConditionResult() { NumberOfBadBins = 5 },
            };

            // Act
            var actual = _validator.ValidateUpdateResultIsAllowed(condition);

            // Assert
            actual.IsFailure.Should().BeTrue();
            actual.Error.Should().Contain("not completed");
            actual.Error.Should().Contain("already set");
        }

        [TestMethod]
        public void ValidateUpdateResultIsAllowed_ConditionIsNull_ThrowsException()
        {
            // Arrange

            // Act
            Action action = () => _validator.ValidateUpdateResultIsAllowed(null);

            // Assert
            action.Should().Throw<Exception>();
        }

        [DataRow(ProcessStatus.NotStarted, ProcessStatus.NotStarted)]
        [DataRow(ProcessStatus.NotStarted, ProcessStatus.PendingCommit)]
        [DataRow(ProcessStatus.NotStarted, ProcessStatus.Committed)]
        [DataRow(ProcessStatus.NotStarted, ProcessStatus.Running)]
        [DataRow(ProcessStatus.NotStarted, ProcessStatus.Completed)]
        [DataRow(ProcessStatus.NotStarted, ProcessStatus.Paused)]
        [DataRow(ProcessStatus.NotStarted, ProcessStatus.Canceling)]
        [DataRow(ProcessStatus.NotStarted, ProcessStatus.Canceled)]
        [DataRow(ProcessStatus.PendingCommit, ProcessStatus.PendingCommit)]
        [DataRow(ProcessStatus.PendingCommit, ProcessStatus.Committed)]
        [DataRow(ProcessStatus.PendingCommit, ProcessStatus.Running)]
        [DataRow(ProcessStatus.PendingCommit, ProcessStatus.Completed)]
        [DataRow(ProcessStatus.PendingCommit, ProcessStatus.Paused)]
        [DataRow(ProcessStatus.PendingCommit, ProcessStatus.Canceling)]
        [DataRow(ProcessStatus.PendingCommit, ProcessStatus.Canceled)]
        [DataRow(ProcessStatus.Committed, ProcessStatus.Committed)]
        [DataRow(ProcessStatus.Committed, ProcessStatus.Running)]
        [DataRow(ProcessStatus.Committed, ProcessStatus.Completed)]
        [DataRow(ProcessStatus.Committed, ProcessStatus.Paused)]
        [DataRow(ProcessStatus.Committed, ProcessStatus.PendingCommit)]
        [DataRow(ProcessStatus.Committed, ProcessStatus.Canceling)]
        [DataRow(ProcessStatus.Committed, ProcessStatus.Canceled)]
        [DataRow(ProcessStatus.Running, ProcessStatus.Running)]
        [DataRow(ProcessStatus.Running, ProcessStatus.Completed)]
        [DataRow(ProcessStatus.Running, ProcessStatus.Paused)]
        [DataRow(ProcessStatus.Running, ProcessStatus.Canceling)]
        [DataRow(ProcessStatus.Running, ProcessStatus.Canceled)]
        [DataRow(ProcessStatus.Completed, ProcessStatus.Completed)]
        [DataRow(ProcessStatus.Canceled, ProcessStatus.Canceled)]
        [DataRow(ProcessStatus.Paused, ProcessStatus.Paused)]
        [DataRow(ProcessStatus.Paused, ProcessStatus.PendingCommit)]
        [DataRow(ProcessStatus.Paused, ProcessStatus.Committed)]
        [DataRow(ProcessStatus.Paused, ProcessStatus.Running)]
        [DataRow(ProcessStatus.Paused, ProcessStatus.Completed)]
        [DataRow(ProcessStatus.Paused, ProcessStatus.Canceling)]
        [DataRow(ProcessStatus.Paused, ProcessStatus.Canceled)]
        [DataRow(ProcessStatus.Paused, ProcessStatus.Resuming)]
        [DataRow(ProcessStatus.Canceling, ProcessStatus.Canceled)]
        [DataRow(ProcessStatus.Canceling, ProcessStatus.Canceling)]

        [DataTestMethod]
        public void ValidateUpdateStatusIsAllowed_AllowedStatusChanges_ReturnsSuccess(ProcessStatus oldStatus, ProcessStatus newStatus)
        {
            // Arrange

            // Act
            var actual = _validator.ValidateUpdateStatusIsAllowed(oldStatus, newStatus);

            // Assert
            actual.IsSuccess.Should().BeTrue();
        }

        [DataRow(ProcessStatus.NotStarted, ProcessStatus.Resuming)]
        [DataRow(ProcessStatus.PendingCommit, ProcessStatus.NotStarted)]
        [DataRow(ProcessStatus.PendingCommit, ProcessStatus.Resuming)]
        [DataRow(ProcessStatus.Committed, ProcessStatus.NotStarted)]
        [DataRow(ProcessStatus.Committed, ProcessStatus.Resuming)]
        [DataRow(ProcessStatus.Running, ProcessStatus.NotStarted)]
        [DataRow(ProcessStatus.Running, ProcessStatus.PendingCommit)]
        [DataRow(ProcessStatus.Running, ProcessStatus.Committed)]
        [DataRow(ProcessStatus.Running, ProcessStatus.Resuming)]
        [DataRow(ProcessStatus.Completed, ProcessStatus.NotStarted)]
        [DataRow(ProcessStatus.Completed, ProcessStatus.PendingCommit)]
        [DataRow(ProcessStatus.Completed, ProcessStatus.Committed)]
        [DataRow(ProcessStatus.Completed, ProcessStatus.Running)]
        [DataRow(ProcessStatus.Completed, ProcessStatus.Paused)]
        [DataRow(ProcessStatus.Completed, ProcessStatus.Canceled)]
        [DataRow(ProcessStatus.Completed, ProcessStatus.Resuming)]
        [DataRow(ProcessStatus.Canceled, ProcessStatus.NotStarted)]
        [DataRow(ProcessStatus.Canceled, ProcessStatus.PendingCommit)]
        [DataRow(ProcessStatus.Canceled, ProcessStatus.Committed)]
        [DataRow(ProcessStatus.Canceled, ProcessStatus.Running)]
        [DataRow(ProcessStatus.Canceled, ProcessStatus.Paused)]
        [DataRow(ProcessStatus.Canceled, ProcessStatus.Completed)]
        [DataRow(ProcessStatus.Canceled, ProcessStatus.Resuming)]
        [DataRow(ProcessStatus.Paused, ProcessStatus.NotStarted)]
        [DataRow(ProcessStatus.Canceling, ProcessStatus.NotStarted)]
        [DataRow(ProcessStatus.Canceling, ProcessStatus.PendingCommit)]
        [DataRow(ProcessStatus.Canceling, ProcessStatus.Committed)]
        [DataRow(ProcessStatus.Canceling, ProcessStatus.Running)]
        [DataRow(ProcessStatus.Canceling, ProcessStatus.Completed)]
        [DataRow(ProcessStatus.Canceling, ProcessStatus.Paused)]
        [DataRow(ProcessStatus.Canceling, ProcessStatus.Resuming)]
        [DataTestMethod]
        public void ValidateUpdateStatusIsAllowed_NotAllowedStatusChanges_ReturnsFailure(ProcessStatus oldStatus, ProcessStatus newStatus)
        {
            // Arrange

            // Act
            var actual = _validator.ValidateUpdateStatusIsAllowed(oldStatus, newStatus);

            // Assert
            actual.IsFailure.Should().BeTrue();
        }
    }
}