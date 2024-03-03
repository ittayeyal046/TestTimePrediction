using FluentValidation.Validators;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Validators.Update;

namespace TTPService.Tests.Validators.Update
{
    [TestClass]
    public class LocationWithQuantityForUpdateDtoValidatorFixtureL0
    {
        private static LocationWithQuantityForUpdateDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new LocationWithQuantityForUpdateDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_Quantity()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Quantity, BaseVerifiersSetComposer.Build()
                                                                              .AddPropertyValidatorVerifier<NotEmptyValidator<Dtos.Update.LocationWithQuantityForUpdateDto, int>>()
                                                                              .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_LocationType()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.LocationType, BaseVerifiersSetComposer.Build()
                                                                                  .AddPropertyValidatorVerifier<NotEmptyValidator<Dtos.Update.LocationWithQuantityForUpdateDto, string>>()
                                                                                  .AddMaximumLengthValidatorVerifier<Dtos.Update.LocationWithQuantityForUpdateDto>(100)
                                                                                  .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_LocationName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.LocationName, BaseVerifiersSetComposer.Build()
                                                                                  .AddPropertyValidatorVerifier<NotEmptyValidator<Dtos.Update.LocationWithQuantityForUpdateDto, string>>()
                                                                                  .AddMaximumLengthValidatorVerifier<Dtos.Update.LocationWithQuantityForUpdateDto>(100)
                                                                                  .Create());
        }
    }
}