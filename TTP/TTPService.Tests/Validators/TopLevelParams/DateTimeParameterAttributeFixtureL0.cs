using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Validators.TopLevelParams;

namespace TTPService.Tests.Validators.TopLevelParams
{
    [TestClass]
    public class DateTimeParameterAttributeFixtureL0
    {
        [TestMethod]
        public void IsValid_ValidDateTime_ReturnsTrue()
        {
            // Arrange
            var validationAttribute = new DateTimeParameterAttribute();

            // Act
            var actual = validationAttribute.IsValid(DateTime.Now);

            // Assert
            actual.Should().Be(true);
        }

        [TestMethod]
        public void IsValid_NullDateTime_ReturnsTrue()
        {
            // Arrange
            var validationAttribute = new DateTimeParameterAttribute();

            // Act
            var actual = validationAttribute.IsValid(null);

            // Assert
            actual.Should().Be(true);
        }

        [TestMethod]
        public void IsValid_InvalidDateTime_ReturnsFalse()
        {
            // Arrange
            var validationAttribute = new DateTimeParameterAttribute();

            // Act
            var actual = validationAttribute.IsValid("blabla");

            // Assert
            actual.Should().Be(false);
        }
    }
}
