using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Validators;

namespace TTPService.Tests.Validators.Properties
{
    [TestClass]
    public class FileNameValidatorFixtureL0
    {
        [DataRow("fileName", true)]
        [DataRow("fil3Name", true)]
        [DataRow("41135433", true)]
        [DataRow("fil.Name", true)]
        [DataRow("fil?Name", false)]
        [DataRow("fil*Name", false)]
        [DataRow("fil/Name", false)]
        [DataRow("", false)]
        [DataRow(null, false)]
        [DataRow(3, false)]
        [DataTestMethod]
        public void IsValid(object value, bool expected)
        {
            // Arrange
            var sut = new TestValidator<object>(v => v.RuleFor(x => x.Value).FileName());

            // Act
            var actual = sut.Validate(new TestValidatorData<object> { Value = value });

            // Assert
            actual.IsValid.Should().Be(expected);
        }
    }
}