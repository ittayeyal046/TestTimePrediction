using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Validators.TopLevelParams;

namespace TTPService.Tests.Validators.TopLevelParams
{
    [TestClass]
    public class UsernameParameterAttributeFixtureL0
    {
        [DataRow("validUser", true)]
        [DataRow("inValid!", false)]
        [DataRow("in Valid", false)]
        [DataRow("tooooooooooooooooooooolong", false)]
        [DataRow(null, true)]
        [DataRow(3, false)]
        [DataTestMethod]
        public void IsValid(object username, bool expected)
        {
            // Arrange
            var validationAttribute = new UsernameValidationAttribute();

            // Act
            var actual = validationAttribute.IsValid(username);

            // Assert
            actual.Should().Be(expected);
        }
    }
}
