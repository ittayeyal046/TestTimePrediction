using FluentAssertions;
using FluentValidation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Validators;

namespace TTPService.Tests.Validators.Properties
{
    [TestClass]
    public class FileExtensionValidatorFixtureL0
    {
        [DataRow(@"\\amr.corp.intel.com\This.Is\My-Folder\earthquake.pl", ".pl")]
        [DataRow(@"\\amr.corp.intel.com\This.Is\My-Folder\earthquake.xml", ".xml")]
        [DataRow(@"\\amr.corp.intel.com\This.Is\My-Folder\earthquake.PL", ".pl")]
        [TestMethod]
        public void IsValid_HappyFlow(string path, string extension)
        {
            // Arrange
            var sut = new TestValidator<string>(v => v.RuleFor(x => x.Value).FileExtension(extension));

            // Act
            var result = sut.Validate(new TestValidatorData<string> { Value = path });

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [DataRow(null, ".pl", "is not of the type 'string'")]
        [DataRow(@"\\amr.corp.intel.com\This.Is\My-Folder\earthquake.jpt", ".pl", "must have .pl extension.")]
        [DataRow(@"\\amr.corp.intel.com\This.Is\My-Folder\earthquake", ".pl", "must have .pl extension.")]
        [DataRow(@"\\amr.corp.intel.com\This.Is\My-Folder\earthquake-pl", ".pl", "must have .pl extension.")]
        [DataRow(@"\\amr.corp.intel.com\This.Is\My-Folder\earthquake.p l", ".pl", "must have .pl extension.")]
        [DataRow(@"\\amr.corp.intel.com\This.Is\My-Folder\earthquake.pl ", ".pl", "must have .pl extension.")]
        [TestMethod]
        public void IsValid_InvalidPathExtension_ValidationFailure(string path, string extension, string error)
        {
            // Arrange
            var sut = new TestValidator<string>(v => v.RuleFor(x => x.Value).FileExtension(extension));

            // Act
            var result = sut.Validate(new TestValidatorData<string> { Value = path });

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorMessage.Should().Be("Value " + error);
        }
    }
}