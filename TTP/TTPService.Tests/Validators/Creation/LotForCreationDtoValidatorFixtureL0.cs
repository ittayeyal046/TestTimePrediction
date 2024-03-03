using System.Collections;
using FluentAssertions;
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
    public class LotForCreationDtoValidatorFixtureL0
    {
        private static LotForCreationDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new LotForCreationDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_Name()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Name, BaseVerifiersSetComposer.Build()
                                                                          .AddPropertyValidatorVerifier<NotEmptyValidator<LotForCreationDto, string>>()
                                                                          .AddMaximumLengthValidatorVerifier<LotForCreationDto>(20)
                                                                          .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_NumberOfUnitsToRun()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.NumberOfUnitsToRun, BaseVerifiersSetComposer.Build()
                                                                                        .AddPropertyValidatorVerifier<NotEmptyValidator<LotForCreationDto, uint>>()
                                                                                        .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_PartType()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.PartType, BaseVerifiersSetComposer.Build()
                                                                         .AddPropertyValidatorVerifier<NotEmptyValidator<LotForCreationDto, string>>()
                                                                         .AddMaximumLengthValidatorVerifier<LotForCreationDto>(21)
                                                                         .AddPropertyValidatorVerifier<PartTypeValidator<LotForCreationDto, string>>()
                                                                         .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_OriginalPartType()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.OriginalPartType, BaseVerifiersSetComposer.Build()
                                                                         .AddMaximumLengthValidatorVerifier<LotForCreationDto>(21)
                                                                         .AddPropertyValidatorVerifier<PartTypeValidator<LotForCreationDto, string>>()
                                                                         .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Lab()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Lab, BaseVerifiersSetComposer.Build()
                                                                         .AddPropertyValidatorVerifier<NotEmptyValidator<LotForCreationDto, string>>()
                                                                         .AddMaximumLengthValidatorVerifier<LotForCreationDto>(40)
                                                                         .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Locations()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Locations, BaseVerifiersSetComposer.Build()
                                                                               .AddPropertyValidatorVerifier<NotEmptyValidator<LotForCreationDto, System.Collections.Generic.IEnumerable<LocationWithQuantityForCreationDto>>>()
                                                                               .AddPropertyValidatorVerifier<MaximumCollectionLengthValidator<LotForCreationDto, IEnumerable>>()
                                                                               .AddChildValidatorVerifier<LocationWithQuantityForCreationDtoValidator, LotForCreationDto, LocationWithQuantityForCreationDto>()
                                                                               .Create());
        }

        [TestMethod]
        public void TestValidate_NumberOfUnitsToRun_NotValid()
        {
            // Arrange
            var expected = "Number Of Units To Run must be less or equal to total number of units in lot locations";
            var dto = new LotForCreationDto()
            { NumberOfUnitsToRun = 3, Locations = new[] { new LocationWithQuantityForCreationDto() { Quantity = 2 } } };

            // Act
            var actual = _sut.TestValidate(dto);

            // Assert
            actual.Errors.Should().Contain(failure => failure.ErrorMessage == expected);
        }

        [TestMethod]
        public void TestValidate_NumberOfUnitsToRunEqualsQuantitySum_Valid()
        {
            // Arrange
            var expected = "Number Of Units To Run must be less or equal to total number of units in lot locations";
            var dto = new LotForCreationDto()
            { NumberOfUnitsToRun = 3, Locations = new[] { new LocationWithQuantityForCreationDto() { Quantity = 1 }, new LocationWithQuantityForCreationDto() { Quantity = 2 } } };

            // Act
            var actual = _sut.TestValidate(dto);

            // Assert
            actual.Errors.Should().NotContain(failure => failure.ErrorMessage == expected);
        }

        [TestMethod]
        public void TestValidate_NumberOfUnitsToRunMoreThanQuantitySum_Valid()
        {
            // Arrange
            var expected = "Number Of Units To Run must be less or equal to total number of units in lot locations";
            var dto = new LotForCreationDto()
            { NumberOfUnitsToRun = 2, Locations = new[] { new LocationWithQuantityForCreationDto() { Quantity = 3 }, new LocationWithQuantityForCreationDto() { Quantity = 2 } } };

            // Act
            var actual = _sut.TestValidate(dto);

            // Assert
            actual.Errors.Should().NotContain(failure => failure.ErrorMessage == expected);
        }
    }
}