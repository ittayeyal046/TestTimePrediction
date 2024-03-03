using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Validators;

namespace TTPService.Tests.Validators.Properties
{
    [TestClass]
    public class EngineeringIdValidatorFixtureL0
    {
        [DataRow("--", true)]
        [DataRow("a9", true)]
        [DataRow("Z0", true)]
        [DataRow("9A", true)]
        [DataRow("0z", true)]
        [DataRow("-0", false)]
        [DataRow("9-", false)]
        [DataRow("Zzz", false)]
        [DataRow("111", false)]
        [DataRow("", false)]
        [DataRow(null, false)]
        [DataRow(3, false)]
        [DataTestMethod]
        public void IsValid(object value, bool expected)
        {
            // Arrange
            var sut = new TestValidator<object>(v => v.RuleFor(x => x.Value).EngineeringId());

            // Act
            var actual = sut.Validate(new TestValidatorData<object> { Value = value });

            // Assert
            actual.IsValid.Should().Be(expected);
        }
    }
}