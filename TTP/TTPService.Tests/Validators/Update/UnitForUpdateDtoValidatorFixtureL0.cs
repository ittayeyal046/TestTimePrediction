using System.Linq;
using FluentValidation.TestHelper;
using FluentValidation.Validators;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RequestService.Dtos.Update;
using RequestService.Validators.Properties;
using RequestService.Validators.Update;

namespace RequestService.Tests.Validators.Update
{
    [TestClass]
    public class UnitForUpdateDtoValidatorFixtureL0
    {
        private static UnitForUpdateDtoValidator _sut;
        private static string _veryLongString;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new UnitForUpdateDtoValidator();
            _veryLongString = string.Concat(Enumerable.Repeat("a", 21));
        }

        [TestMethod]
        public void ShouldHaveRules_VisualId()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.VisualId, BaseVerifiersSetComposer.Build()
                                                                              .AddPropertyValidatorVerifier<NotEmptyValidator<UnitForUpdateDto, string>>()
                                                                              .AddMaximumLengthValidatorVerifier<UnitForUpdateDto>(20)
                                                                              .AddPropertyValidatorVerifier<AlphaNumericValidator<UnitForUpdateDto, string>>()
                                                                              .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Lot()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Lot, BaseVerifiersSetComposer.Build()
                                                                         .AddPropertyValidatorVerifier<NotEmptyValidator<UnitForUpdateDto, string>>()
                                                                         .AddMaximumLengthValidatorVerifier<UnitForUpdateDto>(20)
                                                                         .Create());
        }

        [TestMethod]
        public void ShouldHaveValidationErrorFor_Long_Lot()
        {
            // Arrange
            // Act
            var actual = _sut.TestValidate(new UnitForUpdateDto()
            {
                Lot = _veryLongString,
            });

            // Assert
            actual.ShouldHaveValidationErrorFor(dto => dto.Lot);
        }

        [TestMethod]
        public void ShouldHaveRules_PartType()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.PartType, BaseVerifiersSetComposer.Build()
                                                                         .AddPropertyValidatorVerifier<NotEmptyValidator<UnitForUpdateDto, string>>()
                                                                         .AddMaximumLengthValidatorVerifier<UnitForUpdateDto>(21)
                                                                         .AddPropertyValidatorVerifier<PartTypeValidator<UnitForUpdateDto, string>>()
                                                                         .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_OriginalPartType()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.OriginalPartType, BaseVerifiersSetComposer.Build()
                                                                         .AddMaximumLengthValidatorVerifier<UnitForUpdateDto>(21)
                                                                         .AddPropertyValidatorVerifier<PartTypeValidator<UnitForUpdateDto, string>>()
                                                                         .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Lab()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Lab, BaseVerifiersSetComposer.Build()
                                                                         .AddPropertyValidatorVerifier<NotEmptyValidator<UnitForUpdateDto, string>>()
                                                                         .AddMaximumLengthValidatorVerifier<UnitForUpdateDto>(20)
                                                                         .Create());
        }

        [TestMethod]
        public void ShouldHaveValidationErrorFor_Long_VisualId()
        {
            // Arrange
            // Act
            var actual = _sut.TestValidate(new UnitForUpdateDto()
            {
                VisualId = _veryLongString,
            });

            // Assert
            actual.ShouldHaveValidationErrorFor(dto => dto.VisualId);
        }

        [TestMethod]
        public void ShouldHaveRules_Location()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Location, BaseVerifiersSetComposer.Build()
                                                                              .AddPropertyValidatorVerifier<NotNullValidator<UnitForUpdateDto, LocationForUpdateDto>>()
                                                                              .AddChildValidatorVerifier<LocationForUpdateDtoValidator<LocationForUpdateDto>, UnitForUpdateDto, LocationForUpdateDto>()
                                                                              .Create());
        }
    }
}