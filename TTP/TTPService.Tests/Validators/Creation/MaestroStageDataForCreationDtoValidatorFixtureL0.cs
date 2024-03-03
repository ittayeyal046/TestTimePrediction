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
    public class MaestroStageDataForCreationDtoValidatorFixtureL0
    {
        private MaestroStageDataForCreationDtoValidator _sut;

        [TestInitialize]
        public void Init()
        {
            _sut = new MaestroStageDataForCreationDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_Operation()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Operation, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<MaestroStageDataForCreationDto, string>>()
                .AddPropertyValidatorVerifier<ExactLengthValidator<MaestroStageDataForCreationDto>>()
                .AddPropertyValidatorVerifier<NumericValidator<MaestroStageDataForCreationDto, string>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_EngId()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.EngId, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<MaestroStageDataForCreationDto, string>>()
                .AddPropertyValidatorVerifier<AlphaNumericValidator<MaestroStageDataForCreationDto, string>>()
                .AddPropertyValidatorVerifier<ExactLengthValidator<MaestroStageDataForCreationDto>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_TpEnvironment()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.TpEnvironment, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<MaestroStageDataForCreationDto, Enums.TpEnvironment>>()
                .AddPropertyValidatorVerifier<EnumValidator<MaestroStageDataForCreationDto, Enums.TpEnvironment>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Comment()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Comment, BaseVerifiersSetComposer.Build()
                .AddMaximumLengthValidatorVerifier<MaestroStageDataForCreationDto>(1000)
                .Create());
        }
    }
}