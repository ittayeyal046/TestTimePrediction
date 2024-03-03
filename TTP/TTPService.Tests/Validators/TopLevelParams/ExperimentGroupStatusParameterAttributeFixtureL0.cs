using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Validators.TopLevelParams;

namespace TTPService.Tests.Validators.TopLevelParams
{
    [TestClass]
    public class ExperimentGroupStatusParameterAttributeFixtureL0
    {
        [DataRow("Rolling", true)]
        [DataRow("Completed", true)]
        [DataRow("NotAStatus", false)]
        [DataRow(null, true)]
        [DataRow(3, false)]
        [DataTestMethod]
        public void IsValid(object experimentGroupStatus, bool expected)
        {
            // Arrange
            var validationAttribute = new ExperimentGroupStatusParameterAttribute();

            // Act
            var actual = validationAttribute.IsValid(experimentGroupStatus);

            // Assert
            actual.Should().Be(expected);
        }
    }
}
