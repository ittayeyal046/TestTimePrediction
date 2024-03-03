using FluentValidation.TestHelper;
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
    public class OlbStageDataForCreationDtoValidatorFixtureL0
    {
        private OlbStageDataForCreationDtoValidator _sut;

        [TestInitialize]
        public void Init()
        {
            _sut = new OlbStageDataForCreationDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_MoveUnits()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.MoveUnits, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<OlbStageDataForCreationDto, Enums.MoveUnits?>>()
                .AddPropertyValidatorVerifier<EnumValidator<OlbStageDataForCreationDto, Enums.MoveUnits?>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Operation()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Operation, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<OlbStageDataForCreationDto, string>>()
                .AddMaximumLengthValidatorVerifier<OlbStageDataForCreationDto>(50)
                .AddPropertyValidatorVerifier<AlphaNumericValidator<OlbStageDataForCreationDto, string>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Qdf()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Qdf, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<AlphaNumericValidator<OlbStageDataForCreationDto, string>>()
                .AddExactLengthValidatorVerifier<OlbStageDataForCreationDto>(4)
                .Create());
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void Qdf_ShouldNotHaveErrors_WhenNullOrEmpty(string qdf)
        {
            // Act
            var olbStageDataForCreationDto = new OlbStageDataForCreationDto()
            {
                Qdf = qdf,
            };

            var actual = _sut.TestValidate(olbStageDataForCreationDto);

            // Assert
            actual.ShouldNotHaveValidationErrorFor(olbStageDataForCreationDto => olbStageDataForCreationDto.Qdf);
        }

        [TestMethod]
        public void Qdf_ShouldHaveErrors_WhenNotNullNotAlphanumeric()
        {
            // Act
            var olbStageDataForCreationDto = new OlbStageDataForCreationDto()
            {
                Qdf = "qdf12@",
            };

            var actual = _sut.TestValidate(olbStageDataForCreationDto);

            // Assert
            actual.ShouldHaveValidationErrorFor(olbStageDataForCreationDto => olbStageDataForCreationDto.Qdf);
        }

        [TestMethod]
        public void ShouldHaveRules_Recipe()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Recipe, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<OlbStageDataForCreationDto, string>>()
                .AddMaximumLengthValidatorVerifier<OlbStageDataForCreationDto>(500)
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Comment()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Comment, BaseVerifiersSetComposer.Build()
                .AddMaximumLengthValidatorVerifier<OlbStageDataForCreationDto>(1000)
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_BLLFiles()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.BllFiles, BaseVerifiersSetComposer.Build()
                .AddChildValidatorVerifier<BllFilesForCreationDtoValidator, OlbStageDataForCreationDto, BllFilesForCreationDto>()
                .Create());
        }
    }
}