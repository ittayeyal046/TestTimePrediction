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
    public class BinQdfForCreationDtoValidatorFixtureL0
    {
        private BinQdfForCreationDtoValidator _sut;

        [TestInitialize]
        public void Init()
        {
            _sut = new BinQdfForCreationDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_PlanningBin()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.PlanningBin, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<BinQdfForCreationDto, string>>()
                .AddPropertyValidatorVerifier<NotEmptyValidator<BinQdfForCreationDto, string>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_PassBin()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.PassBin, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<GreaterThanValidator<BinQdfForCreationDto, int>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Qdf()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Qdf, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<BinQdfForCreationDto, string>>()
                .AddPropertyValidatorVerifier<NotEmptyValidator<BinQdfForCreationDto, string>>()
                .AddPropertyValidatorVerifier<AlphaNumericValidator<BinQdfForCreationDto, string>>()
                .AddExactLengthValidatorVerifier<BinQdfForCreationDto>(4)
                .Create());
        }
    }
}