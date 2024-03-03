using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Validators.TopLevelParams;

namespace TTPService.Tests.Validators.TopLevelParams
{
    [TestClass]
    public class VisualIdAttributeFixtureL0
    {
        [DataRow("1234", true)]
        [DataRow(null, true)]
        [DataRow("", false)]
        [DataRow(1, false)]
        [DataRow("@@@", false)]
        [DataTestMethod]
        public void IsValid(object visualId, bool expected)
        {
            // Arrange
            var validationAttribute = new VisualIdValidatorAttribute();

            // Act
            var actual = validationAttribute.IsValid(visualId);

            // Assert
            actual.Should().Be(expected);
        }

        [TestMethod]
        public void MaxNumberOfCharsTest()
        {
            // Arrange
            var validationAttribute = new VisualIdValidatorAttribute();

            // Act
            var actual = validationAttribute.IsValid(new string('d', 21));

            // Assert
            actual.Should().Be(false);
        }
    }
}
