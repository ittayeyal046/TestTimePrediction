using FluentValidation.Validators;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Creation;
using TTPService.Validators.Creation;

namespace TTPService.Tests.Validators.Creation
{
    [TestClass]
    public class LrfForCreationDtoValidatorFixtureL0
    {
        private LrfForCreationDtoValidator _sut;

        [TestInitialize]
        public void Init()
        {
            _sut = new LrfForCreationDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_FuseReleaseName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.FuseReleaseName, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<LrfForCreationDto, string>>()
                .AddPropertyValidatorVerifier<NotEmptyValidator<LrfForCreationDto, string>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_FuseReleaseRevision()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.FuseReleaseRevision, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<LrfForCreationDto, int>>()
                .AddPropertyValidatorVerifier<GreaterThanOrEqualValidator<LrfForCreationDto, int>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_BinData()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.BinQdfs, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<LrfForCreationDto, System.Collections.Generic.IEnumerable<BinQdfForCreationDto>>>()
                .AddPropertyValidatorVerifier<NotEmptyValidator<LrfForCreationDto, System.Collections.Generic.IEnumerable<BinQdfForCreationDto>>>()
                .AddChildValidatorVerifier<BinQdfForCreationDtoValidator, LrfForCreationDto, BinQdfForCreationDto>()
                .Create());
        }
    }
}
