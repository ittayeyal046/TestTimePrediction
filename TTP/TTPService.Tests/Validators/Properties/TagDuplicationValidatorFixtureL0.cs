using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Validators;

namespace TTPService.Tests.Validators.Properties
{
    [TestClass]
    public class TagDuplicationValidatorFixtureL0
    {
        [DataRow(new[] { "tag", "Tag" }, false)]
        [DataRow(new[] { "tag", "" }, false)]
        [DataRow(new[] { "tag", null }, false)]
        [DataRow(new[] { "tag1", "Tag2" }, true)]
        [DataRow(new[] { "tag1", "tag 1" }, true)]
        [DataTestMethod]
        public void IsValid(IEnumerable<string> values, bool expected)
        {
            // Arrange
            var sut = new TestValidator<IEnumerable<string>>(v => v.RuleFor(x => x.Value).TagsDuplication());

            // Act
            var actual = sut.Validate(new TestValidatorData<IEnumerable<string>> { Value = values });

            // Assert
            actual.IsValid.Should().Be(expected);
        }
    }
}
