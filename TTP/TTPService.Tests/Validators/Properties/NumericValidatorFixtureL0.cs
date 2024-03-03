using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Validators;

namespace TTPService.Tests.Validators.Properties
{
    [TestClass]
    public class NumericValidatorFixtureL0
    {
        [DataRow("123456", true)]
        [DataRow("0", true)]
        [DataRow("", true)]
        [DataRow(null, false)]
        [DataRow("AbC4eF", false)]
        [DataRow("1+23", false)]
        [DataRow("1 23", false)]
        [DataRow("1_23", false)]
        [DataRow("1-23", false)]
        [DataRow(3, false)]
        [DataTestMethod]
        public void IsValid(object value, bool expected)
        {
            // Arrange
            var sut = new TestValidator<object>(v => v.RuleFor(x => x.Value).Numeric());

            // Act
            var actual = sut.Validate(new TestValidatorData<object> { Value = value });

            // Assert
            actual.IsValid.Should().Be(expected);
        }
    }
}
