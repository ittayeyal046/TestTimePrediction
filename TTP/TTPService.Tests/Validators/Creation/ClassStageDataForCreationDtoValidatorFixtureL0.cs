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
    public class ClassStageDataForCreationDtoValidatorFixtureL0
    {
        private ClassStageDataForCreationDtoValidator _sut;

        [TestInitialize]
        public void Init()
        {
            _sut = new ClassStageDataForCreationDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_Conditions()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Conditions, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<MaximumCollectionLengthValidator<ClassStageDataForCreationDto, IEnumerable>>()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ClassStageDataForCreationDto, IEnumerable>>()
                .AddPropertyValidatorVerifier<UniqueIEnumerableValidator<ClassStageDataForCreationDto, ConditionForCreationDto, uint>>()
                .AddChildValidatorVerifier<ConditionForCreationDtoValidator, ClassStageDataForCreationDto, ConditionForCreationDto>()
                .Create());
        }
    }
}