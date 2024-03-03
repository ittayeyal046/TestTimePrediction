using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Validators;

namespace TTPService.Tests.Validators.Properties
{
    [TestClass]
    public class AlphaNumericValidatorFixtureL0
    {
        [DataRow("aBcDeF", true)]
        [DataRow("123456", true)]
        [DataRow("AbC4eF", true)]
        [DataRow("A+C4eF", false)]
        [DataRow("A C4eF", false)]
        [DataRow("A_C4eF", false)]
        [DataRow("A-C4eF", false)]
        [DataRow("", true)]
        [DataRow(null, true)]
        [DataRow(3, false)]
        [DataTestMethod]
        public void IsValid(object value, bool expected)
        {
            // Arrange
            var sut = new TestValidator<object>(v => v.RuleFor(x => x.Value).AlphaNumeric());

            // Act
            var actual = sut.Validate(new TestValidatorData<object> { Value = value });

            // Assert
            actual.IsValid.Should().Be(expected);
        }
    }
}