using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Validators;

namespace TTPService.Tests.Validators.Properties
{
    [TestClass]
    public class PartTypeValidatorFixtureL0
    {
        [DataRow("BZ 4SXNBV  E LX Q2XX", true)]
        [DataRow("H6 4VXLCV F A  ", true)]
        [DataRow("Q2 4SXNLV  A", true)]
        [DataRow("U3 4XBSCV  A ", true)]
        [DataRow("H6 4JXHVV C D", true)]
        [DataRow("", false)]
        [DataRow(null, false)]
        [DataRow(3, false)]
        [DataTestMethod]
        public void IsValid(object value, bool expected)
        {
            // Arrange
            var sut = new TestValidator<object>(v => v.RuleFor(x => x.Value).PartType());

            // Act
            var actual = sut.Validate(new TestValidatorData<object> { Value = value });

            // Assert
            actual.IsValid.Should().Be(expected);
        }
    }
}