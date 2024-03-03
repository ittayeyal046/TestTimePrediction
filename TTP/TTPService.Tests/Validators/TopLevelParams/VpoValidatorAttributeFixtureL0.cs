using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Validators.TopLevelParams;

namespace TTPService.Tests.Validators.TopLevelParams
{
    [TestClass]
    public class VpoValidatorAttributeFixtureL0
    {
        [DataRow("YA930008FM", true)]
        [DataRow(null, true)]
        [DataRow("", false)]
        [DataRow(1, false)]

        [DataTestMethod]
        public void IsValid(object vpo, bool expected)
        {
            // Arrange
            var validationAttribute = new VpoValidatorAttribute();

            // Act
            var actual = validationAttribute.IsValid(vpo);

            // Assert
            actual.Should().Be(expected);
        }

        [TestMethod]
        public void MaxNumberOfCharsTest()
        {
            // Arrange
            var validationAttribute = new VpoValidatorAttribute();

            // Act
            var actual = validationAttribute.IsValid(new string('d', 51));

            // Assert
            actual.Should().Be(false);
        }
    }
}
