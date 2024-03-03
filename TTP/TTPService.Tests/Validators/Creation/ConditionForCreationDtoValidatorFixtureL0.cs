using System.Collections;
using FluentValidation.Validators;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Creation;
using TTPService.Enums;
using TTPService.Validators.Creation;
using TTPService.Validators.Properties;

namespace TTPService.Tests.Validators.Creation
{
    [TestClass]
    public class ConditionForCreationDtoValidatorFixtureL0
    {
        private static ConditionForCreationDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new ConditionForCreationDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_LocationCode()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.LocationCode, BaseVerifiersSetComposer.Build()
                                                                                  .AddPropertyValidatorVerifier<NotEmptyValidator<ConditionForCreationDto, string>>()
                                                                                  .AddMaximumLengthValidatorVerifier<ConditionForCreationDto>(50)
                                                                                  .AddPropertyValidatorVerifier<AlphaNumericValidator<ConditionForCreationDto, string>>()
                                                                                  .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Thermals()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Thermals, BaseVerifiersSetComposer.Build()
                                                                                .AddPropertyValidatorVerifier<NotEmptyValidator<ConditionForCreationDto, System.Collections.Generic.IEnumerable<ThermalForCreationDto>>>()
                                                                                .AddPropertyValidatorVerifier<MaximumCollectionLengthValidator<ConditionForCreationDto, IEnumerable>>()
                                                                                .AddPropertyValidatorVerifier<UniqueIEnumerableValidator<ConditionForCreationDto, ThermalForCreationDto, uint>>()
                                                                                .AddChildValidatorVerifier<ThermalForCreationDtoValidator, ConditionForCreationDto, ThermalForCreationDto>()
                                                                                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_EngineeringId()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.EngineeringId, BaseVerifiersSetComposer.Build()
                                                                                   .AddPropertyValidatorVerifier<NotEmptyValidator<ConditionForCreationDto, string>>()
                                                                                   .AddMaximumLengthValidatorVerifier<ConditionForCreationDto>(2)
                                                                                   .AddPropertyValidatorVerifier<EngineeringIdValidator<ConditionForCreationDto, string>>()
                                                                                   .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Comment()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Comment, BaseVerifiersSetComposer.Build()
                .AddMaximumLengthValidatorVerifier<ConditionForCreationDto>(1000)
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Fuse()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Fuse, BaseVerifiersSetComposer.Build()
                .AddChildValidatorVerifier<FuseForCreationDtoValidator, ConditionForCreationDto, FuseForCreationDto>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_MoveUnits()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.MoveUnits, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<EnumValidator<ConditionForCreationDto, MoveUnits?>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_CustomAttributes()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.CustomAttributes, BaseVerifiersSetComposer.Build()
                .AddChildValidatorVerifier<CustomAttributeForCreationDtoValidator, ConditionForCreationDto, CustomAttributeForCreationDto>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_VpoCustomSuffix()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.VpoCustomSuffix, BaseVerifiersSetComposer.Build()
                .AddMaximumLengthValidatorVerifier<ConditionForCreationDto>(2)
                .AddPropertyValidatorVerifier<AlphaNumericValidator<ConditionForCreationDto, string>>()
                .Create());
        }
    }
}