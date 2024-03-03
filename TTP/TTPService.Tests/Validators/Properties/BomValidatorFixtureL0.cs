using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Validators;

namespace TTPService.Tests.Validators.Properties
{
    [TestClass]
    public class BomValidatorFixtureL0
    {
        [DataRow("A2C4E6G8_0", true)]
        [DataRow("A2C4E6G8 0", true)]
        [DataRow("12345678 0", true)]
        [DataRow("12345678_0", true)]
        [DataRow("ABCDEFGH_J", true)]
        [DataRow("ABCDEFGH J", true)]
        [DataRow("ABCDEFGHIJ", true)]
        [DataRow("1234567890", true)]
        [DataRow("_2345678 0", false)]
        [DataRow("1_345678 0", false)]
        [DataRow("12_45678 0", false)]
        [DataRow("123_5678 0", false)]
        [DataRow("1234_678 0", false)]
        [DataRow("12345_78 0", false)]
        [DataRow("123456_8 0", false)]
        [DataRow("1234567_ 0", false)]
        [DataRow("A2C4E6G8 _", false)]
        [DataRow("A2C4E6G8__", false)]
        [DataRow("A2C4E6G8_ ", false)]
        [DataRow("A2C4E6G_0", false)]
        [DataRow("A2C4E6G8_01", false)]
        [DataRow("", false)]
        [DataRow(null, false)]
        [DataRow(3, false)]
        [DataTestMethod]
        public void IsValid(object value, bool expected)
        {
            // Arrange
            var sut = new TestValidator<object>(v => v.RuleFor(x => x.Value).Bom());

            // Act
            var actual = sut.Validate(new TestValidatorData<object> { Value = value });

            // Assert
            actual.IsValid.Should().Be(expected);
        }
    }
}