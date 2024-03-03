using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Entities;
using TTPService.FunctionalExtensions;

namespace TTPService.Tests.FunctionalExtensions
{
    [TestClass]
    public class ExperimentGroupExtensionsFixtureL0
    {
        private TestDataGenerator _testDataGenerator;

        [TestInitialize]
        public void Init()
        {
            _testDataGenerator = new TestDataGenerator();
        }

        [TestMethod]
        public void OverridePropertiesForUpdate_HappyPath()
        {
            // Arrange
            var experimentGroup = _testDataGenerator.GenerateExperimentGroup();
            var newExperimentGroup = _testDataGenerator.GenerateExperimentGroup();
            newExperimentGroup.TestProgramData = new TestProgramData()
            {
                BaseTestProgramName = "NewBaseTpName",
                DirectoryChecksum = "/NewCheckSum",
                ProgramFamily = "NewProgFam",
                StplDirectory = "//NewDirectory",
                ProgramSubFamily = "ProgramSubFam",
                TestProgramRootDirectory = "//RootDirectoy",
            };

            // Act
            experimentGroup.OverridePropertiesForUpdate(newExperimentGroup);

            // Assert
            experimentGroup.TestProgramData.Should().BeEquivalentTo(newExperimentGroup.TestProgramData, opt =>
            {
                opt.IncludingAllRuntimeProperties();
                opt.IncludingNestedObjects();
                opt.AllowingInfiniteRecursion();
                return opt;
            });

            experimentGroup.Experiments.All(x => x.ExperimentState == Enums.ExperimentState.DraftUpdateInProgress).Should().BeTrue();
        }
    }
}