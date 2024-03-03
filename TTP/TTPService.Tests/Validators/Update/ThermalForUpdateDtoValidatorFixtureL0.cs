using FluentValidation.Validators;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Update;
using TTPService.Validators.Update;

namespace TTPService.Tests.Validators.Update
{
    [TestClass]
    public class ThermalForUpdateDtoValidatorFixtureL0
    {
        private static ThermalForUpdateDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new ThermalForUpdateDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_ThermalName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Name, BaseVerifiersSetComposer.Build()
                                                                             .AddPropertyValidatorVerifier<NotEmptyValidator<ThermalForUpdateDto, string>>()
                                                                             .AddMaximumLengthValidatorVerifier<ThermalForUpdateDto>(100)
                                                                             .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_SequenceId()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.SequenceId, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ThermalForUpdateDto, uint>>()
                .Create());
        }
    }
}
