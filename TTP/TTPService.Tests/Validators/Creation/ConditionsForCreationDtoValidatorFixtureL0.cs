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
    public class ConditionsForCreationDtoValidatorFixtureL0
    {
        private ConditionsForCreationDtoValidator _sut;

        [TestInitialize]
        public void Init()
        {
            _sut = new ConditionsForCreationDtoValidator();
        }

        [TestMethod]
        public void ConditionsForCreationDtoValidator_Conditions()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<MaximumCollectionLengthValidator<IEnumerable<ConditionForCreationDto>, IEnumerable>>()
                .AddPropertyValidatorVerifier<NotEmptyValidator<IEnumerable<ConditionForCreationDto>, IEnumerable>>()
                .AddPropertyValidatorVerifier<UniqueIEnumerableValidator<IEnumerable<ConditionForCreationDto>, ConditionForCreationDto, uint>>()
                .AddChildValidatorVerifier<ConditionForCreationDtoValidator, IEnumerable<ConditionForCreationDto>, ConditionForCreationDto>()
                .Create());
        }
    }
}
