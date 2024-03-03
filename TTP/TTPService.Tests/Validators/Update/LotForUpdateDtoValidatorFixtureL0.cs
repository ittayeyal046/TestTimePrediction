using System.Collections;
using FluentAssertions;
using FluentValidation.TestHelper;
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
    public class LotForUpdateDtoValidatorFixtureL0
    {
        private static LotForUpdateDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new LotForUpdateDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_Name()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Name, BaseVerifiersSetComposer.Build()
                                                                          .AddPropertyValidatorVerifier<NotEmptyValidator<LotForUpdateDto, string>>()
                                                                          .AddMaximumLengthValidatorVerifier<LotForUpdateDto>(20)
                                                                          .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_NumberOfUnitsToRun()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.NumberOfUnitsToRun, BaseVerifiersSetComposer.Build()
                                                                                        .AddPropertyValidatorVerifier<NotEmptyValidator<LotForUpdateDto, uint>>()
                                                                                        .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_PartType()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.PartType, BaseVerifiersSetComposer.Build()
                                                                         .AddPropertyValidatorVerifier<NotEmptyValidator<LotForUpdateDto, string>>()
                                                                         .AddMaximumLengthValidatorVerifier<LotForUpdateDto>(21)
                                                                         .AddPropertyValidatorVerifier<PartTypeValidator<LotForUpdateDto, string>>()
                                                                         .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_OriginalPartType()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.OriginalPartType, BaseVerifiersSetComposer.Build()
                                                                         .AddMaximumLengthValidatorVerifier<LotForUpdateDto>(21)
                                                                         .AddPropertyValidatorVerifier<PartTypeValidator<LotForUpdateDto, string>>()
                                                                         .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Lab()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Lab, BaseVerifiersSetComposer.Build()
                                                                         .AddPropertyValidatorVerifier<NotEmptyValidator<LotForUpdateDto, string>>()
                                                                         .AddMaximumLengthValidatorVerifier<LotForUpdateDto>(40)
                                                                         .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Locations()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Locations, BaseVerifiersSetComposer.Build()
                                                                               .AddPropertyValidatorVerifier<NotEmptyValidator<LotForUpdateDto, System.Collections.Generic.IEnumerable<LocationWithQuantityForUpdateDto>>>()
                                                                               .AddPropertyValidatorVerifier<MaximumCollectionLengthValidator<LotForUpdateDto, IEnumerable>>()
                                                                               .AddChildValidatorVerifier<LocationWithQuantityForUpdateDtoValidator, LotForUpdateDto, LocationWithQuantityForUpdateDto>()
                                                                               .Create());
        }

        [TestMethod]
        public void TestValidate_NumberOfUnitsToRun_NotValid()
        {
            // Arrange
            var expected = "Number Of Units To Run must be less or equal to total number of units in lot locations";
            var dto = new LotForUpdateDto()
            { NumberOfUnitsToRun = 3, Locations = new[] { new LocationWithQuantityForUpdateDto() { Quantity = 2 } } };

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
            var dto = new LotForUpdateDto()
            { NumberOfUnitsToRun = 3, Locations = new[] { new LocationWithQuantityForUpdateDto() { Quantity = 1 }, new LocationWithQuantityForUpdateDto() { Quantity = 2 } } };

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
            var dto = new LotForUpdateDto()
            { NumberOfUnitsToRun = 2, Locations = new[] { new LocationWithQuantityForUpdateDto() { Quantity = 3 }, new LocationWithQuantityForUpdateDto() { Quantity = 2 } } };

            // Act
            var actual = _sut.TestValidate(dto);

            // Assert
            actual.Errors.Should().NotContain(failure => failure.ErrorMessage == expected);
        }
    }
}