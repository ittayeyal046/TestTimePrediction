using FluentValidation.Validators;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Creation;
using TTPService.Validators.Creation;
using TTPService.Validators.Properties;

namespace TTPService.Tests.Validators.Creation
{
    [TestClass]
    public class TestProgramDataForCreationDtoValidatorFixtureL0
    {
        private static TestProgramDataForCreationDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new TestProgramDataForCreationDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_BaseTestProgramName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.BaseTestProgramName, BaseVerifiersSetComposer.Build()
                                                                                         .AddPropertyValidatorVerifier<NotEmptyValidator<TestProgramDataForCreationDto, string>>()
                                                                                         .AddMaximumLengthValidatorVerifier<TestProgramDataForCreationDto>(100)
                                                                                         .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_TpName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.TpName, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<TestProgramDataForCreationDto, string>>()
                .AddMaximumLengthValidatorVerifier<TestProgramDataForCreationDto>(50)
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_ProgramFamily()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.ProgramFamily, BaseVerifiersSetComposer.Build()
                                                                                   .AddPropertyValidatorVerifier<NotEmptyValidator<TestProgramDataForCreationDto, string>>()
                                                                                   .AddMaximumLengthValidatorVerifier<TestProgramDataForCreationDto>(100)
                                                                                   .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_ProgramSubFamily()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.ProgramSubFamily, BaseVerifiersSetComposer.Build()
                                                                                      .AddPropertyValidatorVerifier<NotEmptyValidator<TestProgramDataForCreationDto, string>>()
                                                                                      .AddMaximumLengthValidatorVerifier<TestProgramDataForCreationDto>(100)
                                                                                      .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_TestProgramUncPath()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.StplDirectory, BaseVerifiersSetComposer.Build()
                                                                                        .AddPropertyValidatorVerifier<NotEmptyValidator<TestProgramDataForCreationDto, string>>()
                                                                                        .AddMaximumLengthValidatorVerifier<TestProgramDataForCreationDto>(3000)
                                                                                        .AddPropertyValidatorVerifier<PathValidator<TestProgramDataForCreationDto, string>>()
                                                                                        .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_TestProgramRootDirectory()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.TestProgramRootDirectory, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<TestProgramDataForCreationDto, string>>()
                .AddMaximumLengthValidatorVerifier<TestProgramDataForCreationDto>(3000)
                .AddPropertyValidatorVerifier<PathValidator<TestProgramDataForCreationDto, string>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_DirectoryChecksum()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.DirectoryChecksum, BaseVerifiersSetComposer.Build()
                                                                                       .AddMaximumLengthValidatorVerifier<TestProgramDataForCreationDto>(1000)
                                                                                       .Create());
        }
    }
}