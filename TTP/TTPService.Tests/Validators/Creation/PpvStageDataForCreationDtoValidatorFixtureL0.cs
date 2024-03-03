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
    public class PpvStageDataForCreationDtoValidatorFixtureL0
    {
        private PpvStageDataForCreationDtoValidator _sut;

        [TestInitialize]
        public void Init()
        {
            _sut = new PpvStageDataForCreationDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_TestType()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.TestType, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<PpvStageDataForCreationDto, Enums.TestType?>>()
                .AddPropertyValidatorVerifier<EnumValidator<PpvStageDataForCreationDto, Enums.TestType?>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Operation()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Operation, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<PpvStageDataForCreationDto, string>>()
                .AddMaximumLengthValidatorVerifier<PpvStageDataForCreationDto>(50)
                .AddPropertyValidatorVerifier<AlphaNumericValidator<PpvStageDataForCreationDto, string>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Qdf()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Qdf, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<PpvStageDataForCreationDto, string>>()
                .AddPropertyValidatorVerifier<AlphaNumericValidator<PpvStageDataForCreationDto, string>>()
                .AddExactLengthValidatorVerifier<PpvStageDataForCreationDto>(4)
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Recipe()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Recipe, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<PpvStageDataForCreationDto, string>>()
                .AddMaximumLengthValidatorVerifier<PpvStageDataForCreationDto>(500)
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Comment()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Comment, BaseVerifiersSetComposer.Build()
                .AddMaximumLengthValidatorVerifier<PpvStageDataForCreationDto>(1000)
                .Create());
        }
    }
}