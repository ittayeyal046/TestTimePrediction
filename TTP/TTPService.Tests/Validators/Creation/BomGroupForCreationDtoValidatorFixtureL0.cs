using System.Collections;
using System.Collections.Generic;
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
    public class BomGroupForCreationDtoValidatorFixtureL0
    {
        private static BomGroupForCreationDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new BomGroupForCreationDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_Name()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Name, BaseVerifiersSetComposer.Build()
                                                                          .AddPropertyValidatorVerifier<NotEmptyValidator<BomGroupForCreationDto, string>>()
                                                                          .AddMaximumLengthValidatorVerifier<BomGroupForCreationDto>(100)
                                                                          .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Boms()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Boms, BaseVerifiersSetComposer.Build()
                                                                          .AddPropertyValidatorVerifier<NotEmptyValidator<BomGroupForCreationDto, IEnumerable<BomForCreationDto>>>()
                                                                          .AddPropertyValidatorVerifier<MaximumCollectionLengthValidator<BomGroupForCreationDto, IEnumerable>>()
                                                                          .AddChildValidatorVerifier<BomForCreationDtoValidator, BomGroupForCreationDto, BomForCreationDto>()
                                                                          .Create());
        }
    }
}