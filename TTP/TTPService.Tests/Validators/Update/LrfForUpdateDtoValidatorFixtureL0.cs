using FluentValidation.Validators;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Update;
using TTPService.Validators.Update;

namespace TTPService.Tests.Validators.Update
{
    [TestClass]
    public class LrfForUpdateDtoValidatorFixtureL0
    {
        private LrfForUpdateDtoValidator _sut;

        [TestInitialize]
        public void Init()
        {
            _sut = new LrfForUpdateDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_FuseReleaseName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.FuseReleaseName, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<LrfForUpdateDto, string>>()
                .AddPropertyValidatorVerifier<NotEmptyValidator<LrfForUpdateDto, string>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_FuseReleaseRevision()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.FuseReleaseRevision, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<LrfForUpdateDto, int>>()
                .AddPropertyValidatorVerifier<GreaterThanOrEqualValidator<LrfForUpdateDto, int>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_BinData()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.BinQdfs, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<LrfForUpdateDto, System.Collections.Generic.IEnumerable<BinQdfsForUpdateDto>>>()
                .AddPropertyValidatorVerifier<NotEmptyValidator<LrfForUpdateDto, System.Collections.Generic.IEnumerable<BinQdfsForUpdateDto>>>()
                .AddChildValidatorVerifier<BinQdfForUpdateDtoValidator, LrfForUpdateDto, BinQdfsForUpdateDto>()
                .Create());
        }
    }
}
