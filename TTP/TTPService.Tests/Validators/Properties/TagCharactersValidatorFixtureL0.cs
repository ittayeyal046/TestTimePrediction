using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Validators;

namespace TTPService.Tests.Validators.Properties
{
    [TestClass]
    public class TagCharactersValidatorFixtureL0
    {
        [DataRow("aab1@", true)]
        [DataRow("aab#1", true)]
        [DataRow("aa(b)1", true)]
        [DataRow("123456", true)]
        [DataRow("abCdef", true)]
        [DataRow("ab-Cd_ef", true)]
        [DataRow("a#b.c d$+ef/h", true)]
        [DataRow("aa\\c v", true)]
        [DataRow("#aa\\c v", true)]
        [DataRow("A\"C4eF", false)]
        [DataRow("A'C4eF", false)]
        [DataRow("", false)]
        [DataRow(3, false)]
        [DataTestMethod]
        public void IsValid(object value, bool expected)
        {
            // Arrange
            var sut = new TestValidator<object>(v => v.RuleFor(x => x.Value).TagCharacters());

            // Act
            var actual = sut.Validate(new TestValidatorData<object> { Value = value });

            // Assert
            actual.IsValid.Should().Be(expected);
        }
    }
}
