using FluentValidation.Validators;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Creation;
using TTPService.Validators.Creation;

namespace TTPService.Tests.Validators.Creation
{
    [TestClass]
    public class ThermalForCreationDtoValidatorFixtureL0
    {
        private static ThermalForCreationDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new ThermalForCreationDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_ThermalName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Name, BaseVerifiersSetComposer.Build()
                                                                             .AddPropertyValidatorVerifier<NotEmptyValidator<ThermalForCreationDto, string>>()
                                                                             .AddMaximumLengthValidatorVerifier<ThermalForCreationDto>(100)
                                                                             .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_SequenceId()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.SequenceId, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ThermalForCreationDto, uint>>()
                .Create());
        }
    }
}
