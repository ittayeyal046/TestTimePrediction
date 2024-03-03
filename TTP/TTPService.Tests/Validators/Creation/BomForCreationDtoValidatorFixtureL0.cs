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
    public class BomForCreationDtoValidatorFixtureL0
    {
        private static BomForCreationDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new BomForCreationDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_Name()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Name, BaseVerifiersSetComposer.Build()
                                                                          .AddPropertyValidatorVerifier<NotEmptyValidator<BomForCreationDto, string>>()
                                                                          .AddMaximumLengthValidatorVerifier<BomForCreationDto>(10)
                                                                          .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Packages()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Packages, BaseVerifiersSetComposer.Build()
                                                                              .AddPropertyValidatorVerifier<NotEmptyValidator<BomForCreationDto, string>>()
                                                                              .AddMaximumLengthValidatorVerifier<BomForCreationDto>(10)
                                                                              .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Device()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Device, BaseVerifiersSetComposer.Build()
                                                                            .AddPropertyValidatorVerifier<NotEmptyValidator<BomForCreationDto, string>>()
                                                                            .AddMaximumLengthValidatorVerifier<BomForCreationDto>(10)
                                                                            .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Step()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Step, BaseVerifiersSetComposer.Build()
                                                                          .AddPropertyValidatorVerifier<NotEmptyValidator<BomForCreationDto, string>>()
                                                                          .AddMaximumLengthValidatorVerifier<BomForCreationDto>(3)
                                                                          .AddPropertyValidatorVerifier<StepValidator<BomForCreationDto, string>>()
                                                                          .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Rev()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Rev, BaseVerifiersSetComposer.Build()
                                                                         .AddPropertyValidatorVerifier<NotEmptyValidator<BomForCreationDto, string>>()
                                                                         .AddMaximumLengthValidatorVerifier<BomForCreationDto>(10)
                                                                         .Create());
        }
    }
}