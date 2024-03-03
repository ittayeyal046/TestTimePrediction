using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Validators;
using TTPService.Validators.Properties;

namespace TTPService.Tests.Validators.Properties
{
    [TestClass]
    public class PathValidatorFixtureL0
    {
        [DataRow(PathType.Default)]
        [DataRow(PathType.Drive)]
        [DataRow(PathType.Unc)]
        [DataTestMethod]
        public void IsValid_InvalidCharacters(PathType pathType)
        {
            // Arrange
            var invalidChars = Path.GetInvalidPathChars();
            var invalidStr = string.Join(string.Empty, invalidChars);
            var path = $@"\\ger.corp.intel.com\{invalidStr}";
            var sut = new TestValidator<object>(v => v.RuleFor(x => x.Value).Path(pathType));

            // Act
            var result = sut.Validate(new TestValidatorData<object> { Value = path });

            // Assert
            result.IsValid.Should().BeFalse();
        }

        [DataRow(@"\\127.0.0.1\My\Folder", PathType.Default, true, "")]
        [DataRow(@"\\127.0.0.1\My\Folder\", PathType.Default, true, "")]
        [DataRow(@"\\127.0.0.1\My Folder", PathType.Default, true, "")]
        [DataRow(@"\\amr.corp.intel.com\This.Is\My-Folder", PathType.Default, true, "")]
        [DataRow(@"\\server-name.iil.intel.com\C$\", PathType.Default, true, "")]
        [DataRow(@"G:\Folder", PathType.Default, true, "")]
        [DataRow(@"G:\Folder\", PathType.Default, true, "")]
        [DataRow(@"G:\My Folder", PathType.Default, true, "")]
        [DataRow(@"G:\This.Is\My-Folder", PathType.Default, true, "")]
        [DataRow(@"G:\", PathType.Default, true, "")]
        [DataRow(@"\\127.0.0.1", PathType.Default, false, "UNC path with no folder except to basic folder, should not be valid")]
        [DataRow(@"\\amr corp intel com\", PathType.Default, false, "UNC path with whitespaces in base dir should not be valid")]
        [DataRow(@"\\amr$\", PathType.Default, false, "UNC path with special characters in base dir should not be valid")]
        [DataRow(@"\127.0.0.1\bla", PathType.Default, false, "UNC path with only one leading '\\' should not be valid")]
        [DataRow(@"GF:\Folder", PathType.Default, false, "Path with more than one letter as drive name should not be valid")]
        [DataRow(@"G:Folder", PathType.Default, false, "Path without '\\' after the mapped drive letter, should not be valid")]

        [DataRow(@"\\127.0.0.1\My\Folder", PathType.Unc, true, "")]
        [DataRow(@"\\127.0.0.1\My\Folder\", PathType.Unc, true, "")]
        [DataRow(@"\\127.0.0.1\My Folder", PathType.Unc, true, "")]
        [DataRow(@"\\amr.corp.intel.com\This.Is\My-Folder", PathType.Unc, true, "")]
        [DataRow(@"\\server-name.iil.intel.com\C$\", PathType.Unc, true, "")]
        [DataRow(@"\\127.0.0.1", PathType.Unc, false, "UNC path with no folder except to basic folder, should not be valid")]
        [DataRow(@"\\amr corp intel com\", PathType.Unc, false, "UNC path with whitespaces in base dir should not be valid")]
        [DataRow(@"\\amr$\", PathType.Unc, false, "UNC path with special characters in base dir should not be valid")]
        [DataRow(@"\127.0.0.1\bla", PathType.Unc, false, "UNC path with only one leading '\\' should not be valid")]
        [DataRow(@"G:\bla", PathType.Unc, false, "UNC path should not start with mapped drive letter")]

        [DataRow(@"G:\Folder", PathType.Drive, true, "")]
        [DataRow(@"G:\Folder\", PathType.Drive, true, "")]
        [DataRow(@"G:\My Folder", PathType.Drive, true, "")]
        [DataRow(@"G:\This.Is\My-Folder", PathType.Drive, true, "")]
        [DataRow(@"G:\", PathType.Drive, true, "")]
        [DataRow(@"GF:\Folder", PathType.Drive, false, "Path with more than one letter as drive name should not be valid")]
        [DataRow(@"G:Folder", PathType.Drive, false, "Path without '\\' after the drive letter, should not be valid")]
        [DataRow(@"\\amr.cop.intel.com\", PathType.Drive, false, "Path not starting with drive letter is not valid drive path")]

        [DataTestMethod]
        public void IsValid(string path, PathType pathType, bool expected, string because)
        {
            // Arrange
            var sut = new TestValidator<string>(v => v.RuleFor(x => x.Value).Path(pathType));

            // Act
            var result = sut.Validate(new TestValidatorData<string> { Value = path });

            // Assert
            result.IsValid.Should().Be(expected, because);
        }
    }
}