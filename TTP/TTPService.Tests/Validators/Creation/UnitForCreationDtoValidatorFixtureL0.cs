using System.Linq;
using FluentValidation.TestHelper;
using FluentValidation.Validators;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RequestService.Dtos.Creation;
using RequestService.Validators.Creation;
using RequestService.Validators.Properties;

namespace RequestService.Tests.Validators.Creation
{
    [TestClass]
    public class UnitForCreationDtoValidatorFixtureL0
    {
        private static UnitForCreationDtoValidator _sut;
        private static string _veryLongString;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new UnitForCreationDtoValidator();
            _veryLongString = string.Concat(Enumerable.Repeat("a", 21));
        }

        [TestMethod]
        public void ShouldHaveRules_VisualId()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.VisualId, BaseVerifiersSetComposer.Build()
                                                                              .AddPropertyValidatorVerifier<NotEmptyValidator<UnitForCreationDto, string>>()
                                                                              .AddMaximumLengthValidatorVerifier<UnitForCreationDto>(20)
                                                                              .AddPropertyValidatorVerifier<AlphaNumericValidator<UnitForCreationDto, string>>()
                                                                              .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Lot()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Lot, BaseVerifiersSetComposer.Build()
                                                                         .AddPropertyValidatorVerifier<NotEmptyValidator<UnitForCreationDto, string>>()
                                                                         .AddMaximumLengthValidatorVerifier<UnitForCreationDto>(20)
                                                                         .Create());
        }

        [TestMethod]
        public void ShouldHaveValidationErrorFor_Long_Lot()
        {
            // Arrange
            var actual = _sut.TestValidate(new UnitForCreationDto()
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
                                                                         .AddPropertyValidatorVerifier<NotEmptyValidator<UnitForCreationDto, string>>()
                                                                         .AddMaximumLengthValidatorVerifier<UnitForCreationDto>(21)
                                                                         .AddPropertyValidatorVerifier<PartTypeValidator<UnitForCreationDto, string>>()
                                                                         .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_OriginalPartType()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.OriginalPartType, BaseVerifiersSetComposer.Build()
                                                                         .AddMaximumLengthValidatorVerifier<UnitForCreationDto>(21)
                                                                         .AddPropertyValidatorVerifier<PartTypeValidator<UnitForCreationDto, string>>()
                                                                         .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Lab()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Lab, BaseVerifiersSetComposer.Build()
                                                                         .AddPropertyValidatorVerifier<NotEmptyValidator<UnitForCreationDto, string>>()
                                                                         .AddMaximumLengthValidatorVerifier<UnitForCreationDto>(20)
                                                                         .Create());
        }

        [TestMethod]
        public void ShouldHaveValidationErrorFor_Long_VisualId()
        {
            // Arrange
            // Act
            var actual = _sut.TestValidate(new UnitForCreationDto()
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
                                                                              .AddPropertyValidatorVerifier<NotNullValidator<UnitForCreationDto, LocationForCreationDto>>()
                                                                              .AddChildValidatorVerifier<LocationForCreationDtoValidator<LocationForCreationDto>, UnitForCreationDto, LocationForCreationDto>()
                                                                              .Create());
        }
    }
}