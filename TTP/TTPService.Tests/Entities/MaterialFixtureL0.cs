using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Entities;

namespace TTPService.Tests.Entities
{
    [TestClass]
    public class MaterialFixtureL0
    {
        [TestMethod]
        public void MaterialIsDefined_NoUnitsNoLots_ReturnsFalse()
        {
            // Arrange
            var material = new Material() { Lots = new List<Lot>(), Units = new List<Unit>() };

            // Act
            var actual = material.MaterialIsDefined;

            // Assert
            actual.Should().Be(false);
        }

        [TestMethod]
        public void MaterialIsDefined_UnitsAndLotsAreNull_ReturnsFalse()
        {
            // Arrange
            var material = new Material();

            // Act
            var actual = material.MaterialIsDefined;

            // Assert
            actual.Should().Be(false);
        }

        [TestMethod]
        public void MaterialIsDefined_OnlyLots_ReturnsTrue()
        {
            // Arrange
            var material = new Material()
            {
                Lots = new List<Lot>() { new Lot() { Name = "Lot" } },
                Units = new List<Unit>(),
            };

            // Act
            var actual = material.MaterialIsDefined;

            // Assert
            actual.Should().Be(true);
        }

        [TestMethod]
        public void MaterialIsDefined_OnlyUnits_ReturnsTrue()
        {
            // Arrange
            var material = new Material()
            {
                Lots = new List<Lot>(),
                Units = new List<Unit>() { new Unit() { VisualId = "visualId" } },
            };

            // Act
            var actual = material.MaterialIsDefined;

            // Assert
            actual.Should().Be(true);
        }

        [TestMethod]
        public void HasMaterialIssueFailure_NoMaterialIssue_ReturnsFalse()
        {
            // Arrange
            var material = new Material()
            {
                Lots = new List<Lot>() { new Lot() { Name = "Lot" } },
                Units = new List<Unit>(),
            };

            // Act
            var actual = material.HasMaterialIssueFailure;

            // Assert
            actual.Should().BeFalse();
        }

        [TestMethod]
        [DataRow("some comments", true, true, DisplayName = "With material issue failure comments will return true")]
        [DataRow("some comments", false, false, DisplayName = "With material issue failure comments but no issue step will return false")]
        [DataRow("", true, false, DisplayName = "Without material issue failure comments and issue step will return false")]
        public void HasMaterialIssueFailure_WithMaterialIssue_ReturnsFalse(string comments, bool issueMaterial, bool expectedResult)
        {
            // Arrange
            var material = new Material()
            {
                Lots = new List<Lot>() { new Lot() { Name = "Lot" } },
                Units = new List<Unit>(),
                MaterialIssue = new MaterialIssue()
                {
                    MaterialIssueIsRequired = issueMaterial,
                    MaterialIssueErrorComments = comments,
                },
            };

            // Act
            var actual = material.HasMaterialIssueFailure;

            // Assert
            actual.Should().Be(expectedResult);
        }
    }
}