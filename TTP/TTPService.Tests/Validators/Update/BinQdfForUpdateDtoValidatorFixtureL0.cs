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
    public class BinQdfForUpdateDtoValidatorFixtureL0
    {
        private BinQdfForUpdateDtoValidator _sut;

        [TestInitialize]
        public void Init()
        {
            _sut = new BinQdfForUpdateDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_PlanningBin()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.PlanningBin, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<BinQdfsForUpdateDto, string>>()
                .AddPropertyValidatorVerifier<NotEmptyValidator<BinQdfsForUpdateDto, string>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_PassBin()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.PassBin, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<GreaterThanValidator<BinQdfsForUpdateDto, int>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Qdf()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Qdf, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<BinQdfsForUpdateDto, string>>()
                .AddPropertyValidatorVerifier<NotEmptyValidator<BinQdfsForUpdateDto, string>>()
                .AddPropertyValidatorVerifier<AlphaNumericValidator<BinQdfsForUpdateDto, string>>()
                .AddExactLengthValidatorVerifier<BinQdfsForUpdateDto>(4)
                .Create());
        }
    }
}