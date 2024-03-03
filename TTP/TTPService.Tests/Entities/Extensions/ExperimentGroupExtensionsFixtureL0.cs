using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Entities;
using TTPService.Entities.Extensions;
using TTPService.Models;

namespace TTPService.Tests.Entities.Extensions
{
    [TestClass]
    public class ExperimentGroupExtensionsFixtureL0
    {
        [TestMethod]
        public void Initialize_HappyPath()
        {
            // Arrange
            var username = "USER";
            var baseTpName = "BaseTpName";
            var bomGroup = "BomGroup";
            var step = "Step";
            var experimentGroup = new ExperimentGroup()
            {
                Username = username,
                DisplayName = string.Empty,
                TestProgramData = new TestProgramData { BaseTestProgramName = baseTpName },
                Experiments = new List<Experiment>()
                {
                    new Experiment
                    {
                        DisplayName = string.Empty,
                        BomGroupName = bomGroup,
                        Step = step,
                    },
                },
            };

            // Act
            experimentGroup.Initialize();

            // Assert
            experimentGroup.Username.Should().Be(username.ToLower());
            experimentGroup.DisplayName.Should().Be(baseTpName);
            experimentGroup.DisplayNameSource.Should().Be(Enums.DisplayNameSource.SparkProvided);
            experimentGroup.Experiments.First().DisplayName.Should().Be(baseTpName + " - " + bomGroup + " " + step);
        }

        [TestMethod]
        public void Initialize_DisplayNameIsNotEmpty()
        {
            // Arrange
            var username = "USER";
            var baseTpName = "BaseTpName";
            var bomGroup = "BomGroup";
            var step = "Step";
            var experimentGroupDisplayName = "MyExperimentGroupDisplayName";
            var experimentDisplayName = "MyExperimentDisplayName";
            var experimentGroup = new ExperimentGroup()
            {
                Username = username,
                DisplayName = experimentGroupDisplayName,
                DisplayNameSource = Enums.DisplayNameSource.UserProvided,
                TestProgramData = new TestProgramData { BaseTestProgramName = baseTpName },
                Experiments = new List<Experiment>()
                {
                    new Experiment
                    {
                        DisplayName = experimentDisplayName,
                        BomGroupName = bomGroup,
                        Step = step,
                    },
                },
            };

            // Act
            experimentGroup.Initialize();

            // Assert
            experimentGroup.Username.Should().Be(username.ToLower());
            experimentGroup.DisplayName.Should().Be(experimentGroupDisplayName);
            experimentGroup.DisplayNameSource.Should().Be(Enums.DisplayNameSource.UserProvided);
            experimentGroup.Experiments.First().DisplayName.Should().Be(experimentDisplayName);
        }
    }
}