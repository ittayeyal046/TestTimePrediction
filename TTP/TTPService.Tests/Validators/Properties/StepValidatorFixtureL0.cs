using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Validators;

namespace TTPService.Tests.Validators.Properties
{
    [TestClass]
    public class StepValidatorFixtureL0
    {
        [DataRow("A1", true)]
        [DataRow("A11", true)]
        [DataRow("A11Z", false)]
        [DataRow("AZ", false)]
        [DataRow("1A", false)]
        [DataRow("A", false)]
        [DataRow(null, false)]
        [DataRow(3, false)]
        [DataTestMethod]
        public void IsValid(object value, bool expected)
        {
            // Arrange
            var sut = new TestValidator<object>(v => v.RuleFor(x => x.Value).Step());

            // Act
            var actual = sut.Validate(new TestValidatorData<object> { Value = value });

            // Assert
            actual.IsValid.Should().Be(expected);
        }
    }
}