using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Validators.TopLevelParams;

namespace TTPService.Tests.Validators.TopLevelParams
{
    [TestClass]
    public class LotNameAttributeFixtureL0
    {
        [DataRow("lotname", true)]
        [DataRow(null, true)]
        [DataRow("", false)]
        [DataRow(1, false)]
        [DataRow("@@@", false)]
        [DataTestMethod]
        public void IsValid(object lotname, bool expected)
        {
            // Arrange
            var validationAttribute = new LotNameValidatorAttribute();

            // Act
            var actual = validationAttribute.IsValid(lotname);

            // Assert
            actual.Should().Be(expected);
        }

        [TestMethod]
        public void MaxNumberOfCharsTest()
        {
            // Arrange
            var validationAttribute = new LotNameValidatorAttribute();

            // Act
            var actual = validationAttribute.IsValid(new string('d', 21));

            // Assert
            actual.Should().Be(false);
        }
    }
}
