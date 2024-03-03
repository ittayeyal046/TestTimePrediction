using System.Collections;
using FluentValidation.Validators;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Update;
using TTPService.Validators.Properties;
using TTPService.Validators.Update;

namespace TTPService.Tests.Validators.Update
{
    [TestClass]
    public class ConditionForUpdateDtoValidatorFixtureL0
    {
        private static ConditionForUpdateDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new ConditionForUpdateDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_LocationCode()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.LocationCode, BaseVerifiersSetComposer.Build()
                                                                                  .AddPropertyValidatorVerifier<NotEmptyValidator<ConditionForUpdateDto, string>>()
                                                                                  .AddMaximumLengthValidatorVerifier<ConditionForUpdateDto>(50)
                                                                                  .AddPropertyValidatorVerifier<AlphaNumericValidator<ConditionForUpdateDto, string>>()
                                                                                  .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Thermals()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Thermals, BaseVerifiersSetComposer.Build()
                                                                                .AddPropertyValidatorVerifier<NotEmptyValidator<ConditionForUpdateDto, System.Collections.Generic.IEnumerable<ThermalForUpdateDto>>>()
                                                                                .AddPropertyValidatorVerifier<MaximumCollectionLengthValidator<ConditionForUpdateDto, IEnumerable>>()
                                                                                .AddPropertyValidatorVerifier<UniqueIEnumerableValidator<ConditionForUpdateDto, ThermalForUpdateDto, uint>>()
                                                                                .AddChildValidatorVerifier<ThermalForUpdateDtoValidator, ConditionForUpdateDto, ThermalForUpdateDto>()
                                                                                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_EngineeringId()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.EngineeringId, BaseVerifiersSetComposer.Build()
                                                                                   .AddPropertyValidatorVerifier<NotEmptyValidator<ConditionForUpdateDto, string>>()
                                                                                   .AddMaximumLengthValidatorVerifier<ConditionForUpdateDto>(2)
                                                                                   .AddPropertyValidatorVerifier<EngineeringIdValidator<ConditionForUpdateDto, string>>()
                                                                                   .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Comment()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Comment, BaseVerifiersSetComposer.Build()
                .AddMaximumLengthValidatorVerifier<ConditionForUpdateDto>(1000)
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Fuse()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Fuse, BaseVerifiersSetComposer.Build()
                .AddChildValidatorVerifier<FuseForUpdateDtoValidator, ConditionForUpdateDto, FuseForUpdateDto>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_MoveUnits()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.MoveUnits, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<EnumValidator<ConditionForUpdateDto, Enums.MoveUnits?>>()
                .Create());
        }
    }
}