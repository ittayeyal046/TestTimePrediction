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
    public class BllFilesForCreationDtoValidatorFixtureL0
    {
        private BllFilesForCreationDtoValidator _sut;

        [TestInitialize]
        public void Init()
        {
            _sut = new BllFilesForCreationDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_ScriptFile()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.ScriptFile, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<BllFilesForCreationDto, string>>()
                .AddMaximumLengthValidatorVerifier<BllFilesForCreationDto>(1000)
                .AddPropertyValidatorVerifier<PathValidator<BllFilesForCreationDto, string>>()
                .AddPropertyValidatorVerifier<FileExtensionValidator<BllFilesForCreationDto, string>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_ConstFile()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.ConstFile, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<BllFilesForCreationDto, string>>()
                .AddMaximumLengthValidatorVerifier<BllFilesForCreationDto>(1000)
                .AddPropertyValidatorVerifier<PathValidator<BllFilesForCreationDto, string>>()
                .AddPropertyValidatorVerifier<FileExtensionValidator<BllFilesForCreationDto, string>>()
                .Create());
        }
    }
}