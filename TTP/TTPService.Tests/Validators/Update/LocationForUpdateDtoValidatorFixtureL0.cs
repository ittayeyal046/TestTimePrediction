using FluentValidation.Validators;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Update;
using TTPService.Validators.Update;

namespace TTPService.Tests.Validators.Update
{
    [TestClass]
    public class LocationForUpdateDtoValidatorFixtureL0
    {
        private static LocationForUpdateDtoValidator<LocationForUpdateDto> _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new LocationForUpdateDtoValidator<LocationForUpdateDto>();
        }

        [TestMethod]
        public void ShouldHaveRules_LocationType()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.LocationType, BaseVerifiersSetComposer.Build()
                                                                                  .AddPropertyValidatorVerifier<NotEmptyValidator<LocationForUpdateDto, string>>()
                                                                                  .AddMaximumLengthValidatorVerifier<LocationForUpdateDto>(100)
                                                                                  .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_LocationName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.LocationName, BaseVerifiersSetComposer.Build()
                                                                                  .AddPropertyValidatorVerifier<NotEmptyValidator<LocationForUpdateDto, string>>()
                                                                                  .AddMaximumLengthValidatorVerifier<LocationForUpdateDto>(100)
                                                                                  .Create());
        }
    }
}