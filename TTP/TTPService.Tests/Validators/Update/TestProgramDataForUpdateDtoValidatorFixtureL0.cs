using FluentValidation.Validators;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Update;
using TTPService.Validators.Properties;
using TTPService.Validators.Update;

namespace TTPService.Tests.Validators.Update
{
    [TestClass]
    public class TestProgramDataForUpdateDtoValidatorFixtureL0
    {
        private static TestProgramDataForUpdateDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new TestProgramDataForUpdateDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_BaseTestProgramName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.BaseTestProgramName, BaseVerifiersSetComposer.Build()
                                                                                         .AddPropertyValidatorVerifier<NotEmptyValidator<TestProgramDataForUpdateDto, string>>()
                                                                                         .AddMaximumLengthValidatorVerifier<TestProgramDataForUpdateDto>(100)
                                                                                         .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_TpName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.TpName, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<TestProgramDataForUpdateDto, string>>()
                .AddMaximumLengthValidatorVerifier<TestProgramDataForUpdateDto>(50)
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_ProgramFamily()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.ProgramFamily, BaseVerifiersSetComposer.Build()
                                                                                   .AddPropertyValidatorVerifier<NotEmptyValidator<TestProgramDataForUpdateDto, string>>()
                                                                                   .AddMaximumLengthValidatorVerifier<TestProgramDataForUpdateDto>(100)
                                                                                   .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_ProgramSubFamily()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.ProgramSubFamily, BaseVerifiersSetComposer.Build()
                                                                                      .AddPropertyValidatorVerifier<NotEmptyValidator<TestProgramDataForUpdateDto, string>>()
                                                                                      .AddMaximumLengthValidatorVerifier<TestProgramDataForUpdateDto>(100)
                                                                                      .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_TestProgramUncPath()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.StplDirectory, BaseVerifiersSetComposer.Build()
                                                                                        .AddPropertyValidatorVerifier<NotEmptyValidator<TestProgramDataForUpdateDto, string>>()
                                                                                        .AddMaximumLengthValidatorVerifier<TestProgramDataForUpdateDto>(3000)
                                                                                        .AddPropertyValidatorVerifier<PathValidator<TestProgramDataForUpdateDto, string>>()
                                                                                        .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_TestProgramRootDirectory()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.TestProgramRootDirectory, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<TestProgramDataForUpdateDto, string>>()
                .AddMaximumLengthValidatorVerifier<TestProgramDataForUpdateDto>(3000)
                .AddPropertyValidatorVerifier<PathValidator<TestProgramDataForUpdateDto, string>>()
                .Create());
        }
    }
}