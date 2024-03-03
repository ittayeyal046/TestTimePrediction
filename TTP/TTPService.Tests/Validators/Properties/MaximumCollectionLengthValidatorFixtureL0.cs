using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Validators;

namespace TTPService.Tests.Validators.Properties
{
    [TestClass]
    public class MaximumCollectionLengthValidatorFixtureL0
    {
        [DataRow(null, true)]
        [DataRow(new int[0], true)]
        [DataRow(new[] { 1 }, true)]
        [DataRow(new[] { 1, 2 }, true)]
        [DataRow(new[] { 1, 2, 3 }, false)]
        [DataTestMethod]
        public void IsValid(IEnumerable<int> values, bool expected)
        {
            // Arrange
            var sut = new TestValidator<IEnumerable<int>>(v => v.RuleFor(x => x.Value).MaximumCollectionLength(2));

            // Act
            var actual = sut.Validate(new TestValidatorData<IEnumerable<int>> { Value = values });

            // Assert
            actual.IsValid.Should().Be(expected);
        }
    }
}