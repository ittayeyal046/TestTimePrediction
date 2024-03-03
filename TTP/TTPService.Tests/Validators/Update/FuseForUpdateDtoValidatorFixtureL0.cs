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
    public class FuseForUpdateDtoValidatorFixtureL0
    {
        private FuseForUpdateDtoValidator _sut;

        [TestInitialize]
        public void Init()
        {
            _sut = new FuseForUpdateDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_FuseType()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.FuseType, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<FuseForUpdateDto, Enums.FuseType>>()
                .AddPropertyValidatorVerifier<EnumValidator<FuseForUpdateDto, Enums.FuseType>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Lrf()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Lrf, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<FuseForUpdateDto, LrfForUpdateDto>>()
                .AddChildValidatorVerifier<LrfForUpdateDtoValidator, FuseForUpdateDto, LrfForUpdateDto>()
                .AddPropertyValidatorVerifier<NullValidator<FuseForUpdateDto, LrfForUpdateDto>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Qdf()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Qdf, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<FuseForUpdateDto, string>>()
                .AddPropertyValidatorVerifier<AlphaNumericValidator<FuseForUpdateDto, string>>()
                .AddPropertyValidatorVerifier<ExactLengthValidator<FuseForUpdateDto>>()
                .AddPropertyValidatorVerifier<NullValidator<FuseForUpdateDto, string>>()
                .Create());
        }
    }
}
