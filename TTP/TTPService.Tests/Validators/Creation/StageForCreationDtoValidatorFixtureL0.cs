using FluentValidation.Validators;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Creation;
using TTPService.Validators.Creation;

namespace TTPService.Tests.Validators.Creation
{
    [TestClass]
    public class StageForCreationDtoValidatorFixtureL0
    {
        private StageForCreationDtoValidator _sut;

        [TestInitialize]
        public void Init()
        {
            _sut = new StageForCreationDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_SequenceId()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.SequenceId, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<StageForCreationDto, uint>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_StageType()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.StageType, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<StageForCreationDto, Enums.StageType>>()
                .AddPropertyValidatorVerifier<EnumValidator<StageForCreationDto, Enums.StageType>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_StageData()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.StageData, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<PolymorphicValidator<StageForCreationDto, StageDataForCreationDto>>()
                .Create());
        }
    }
}