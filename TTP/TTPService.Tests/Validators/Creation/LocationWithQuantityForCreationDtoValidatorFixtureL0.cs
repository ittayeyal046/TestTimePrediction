using FluentValidation.Validators;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Creation;
using TTPService.Validators.Creation;

namespace TTPService.Tests.Validators.Creation
{
    [TestClass]
    public class LocationWithQuantityForCreationDtoValidatorFixtureL0
    {
        private static LocationWithQuantityForCreationDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new LocationWithQuantityForCreationDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_Quantity()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Quantity, BaseVerifiersSetComposer.Build()
                                                                              .AddPropertyValidatorVerifier<NotEmptyValidator<LocationWithQuantityForCreationDto, uint>>()
                                                                              .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_LocationType()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.LocationType, BaseVerifiersSetComposer.Build()
                                                                                  .AddPropertyValidatorVerifier<NotEmptyValidator<LocationWithQuantityForCreationDto, string>>()
                                                                                  .AddMaximumLengthValidatorVerifier<LocationWithQuantityForCreationDto>(100)
                                                                                  .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_LocationName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.LocationName, BaseVerifiersSetComposer.Build()
                                                                                  .AddPropertyValidatorVerifier<NotEmptyValidator<LocationWithQuantityForCreationDto, string>>()
                                                                                  .AddMaximumLengthValidatorVerifier<LocationWithQuantityForCreationDto>(100)
                                                                                  .Create());
        }
    }
}