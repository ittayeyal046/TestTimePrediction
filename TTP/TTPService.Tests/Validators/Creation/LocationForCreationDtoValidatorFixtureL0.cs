using FluentValidation.Validators;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Creation;
using TTPService.Validators.Creation;

namespace TTPService.Tests.Validators.Creation
{
    [TestClass]
    public class LocationForCreationDtoValidatorFixtureL0
    {
        private static LocationForCreationDtoValidator<LocationForCreationDto> _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new LocationForCreationDtoValidator<LocationForCreationDto>();
        }

        [TestMethod]
        public void ShouldHaveRules_LocationType()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.LocationType, BaseVerifiersSetComposer.Build()
                                                                                  .AddPropertyValidatorVerifier<NotEmptyValidator<LocationForCreationDto, string>>()
                                                                                  .AddMaximumLengthValidatorVerifier<LocationForCreationDto>(100)
                                                                                  .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_LocationName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.LocationName, BaseVerifiersSetComposer.Build()
                                                                                  .AddPropertyValidatorVerifier<NotEmptyValidator<LocationForCreationDto, string>>()
                                                                                  .AddMaximumLengthValidatorVerifier<LocationForCreationDto>(100)
                                                                                  .Create());
        }
    }
}