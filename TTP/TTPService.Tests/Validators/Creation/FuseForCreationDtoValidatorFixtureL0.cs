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
    public class FuseForCreationDtoValidatorFixtureL0
    {
        private FuseForCreationDtoValidator _sut;

        [TestInitialize]
        public void Init()
        {
            _sut = new FuseForCreationDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_FuseType()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.FuseType, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<FuseForCreationDto, Enums.FuseType>>()
                .AddPropertyValidatorVerifier<EnumValidator<FuseForCreationDto, Enums.FuseType>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Lrf()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Lrf, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<FuseForCreationDto, LrfForCreationDto>>()
                .AddChildValidatorVerifier<LrfForCreationDtoValidator, FuseForCreationDto, LrfForCreationDto>()
                .AddPropertyValidatorVerifier<NullValidator<FuseForCreationDto, LrfForCreationDto>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Qdf()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Qdf, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<FuseForCreationDto, string>>()
                .AddPropertyValidatorVerifier<AlphaNumericValidator<FuseForCreationDto, string>>()
                .AddPropertyValidatorVerifier<ExactLengthValidator<FuseForCreationDto>>()
                .AddPropertyValidatorVerifier<NullValidator<FuseForCreationDto, string>>()
                .Create());
        }
    }
}
