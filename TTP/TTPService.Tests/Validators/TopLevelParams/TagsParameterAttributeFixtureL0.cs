using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Validators.TopLevelParams;

namespace TTPService.Tests.Validators.TopLevelParams
{
    [TestClass]
    public class TagsParameterAttributeFixtureL0
    {
        private const string DisplayName = "value";

        [DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
        [DataTestMethod]
        public void GetValidationResult(object value, ValidationResult expected)
        {
            // Arrange
            var context = new ValidationContext(new object()) { DisplayName = DisplayName };

            var sut = new TagsParameterAttribute();

            // Act
            var actual = sut.GetValidationResult(value, context);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        private static IEnumerable<object[]> GetData()
        {
            var notStringCollection = new ValidationResult($"{DisplayName} is not of the type 'string' collection.");
            var badChars = new ValidationResult($@"{DisplayName} must be alphanumeric or contains these special characters: . _ - + @ # $ % & * ( ) \ /");

            yield return new object[] { new string[] { }, ValidationResult.Success };
            yield return new object[] { new[] { "0T.1h_2i-3s+", "4I@5s#", "6T$7h%8e&", "9E*10n(11d)", @"12G\ 13A/" }, ValidationResult.Success };
            yield return new object[] { null!, notStringCollection };
            yield return new object[] { new int[] { }, notStringCollection };
            yield return new object[] { "sparky", notStringCollection };
            yield return new object[] { new[] { string.Empty }, badChars };
            yield return new object[] { new[] { "   " }, badChars };
            yield return new object[] { new[] { "!" }, badChars };
        }
    }
}
