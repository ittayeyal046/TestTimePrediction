using System.Collections;
using System.Collections.Generic;
using FluentValidation.Validators;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Configuration;
using TTPService.Dtos.Update;
using TTPService.Enums;
using TTPService.Validators.Properties;
using TTPService.Validators.Update;

namespace TTPService.Tests.Validators.Update
{
    [TestClass]
    public class ExperimentGroupForUpdateDtoValidatorFixtureL0
    {
        private static readonly Dictionary<ExperimentType, IEnumerable<ActivityType>> _experimetToPossibleActivityTypesMapping
            = new ()
            {
                { ExperimentType.Engineering, new List<ActivityType> { ActivityType.ModuleValidation, ActivityType.MassiveValidation, ActivityType.RejectValidation, ActivityType.Engineering } },
                { ExperimentType.Correlation, new List<ActivityType> { ActivityType.Correlation } },
                { ExperimentType.WalkTheLot, new List<ActivityType> { ActivityType.WalkTheLot } },
            };

        private static ExperimentGroupForUpdateDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var experimentIdentifiersOptionsCreate = Options.Create(new ExperimentIdentifiersOptions()
            {
                ExperimentToPossibleActivityTypesMapping = _experimetToPossibleActivityTypesMapping,
            });

            _sut = new ExperimentGroupForUpdateDtoValidator(experimentIdentifiersOptionsCreate);
        }

        [TestMethod]
        public void ShouldHaveRules_Wwid()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Wwid, BaseVerifiersSetComposer.Build()
                                                                              .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentGroupForUpdateDto, uint>>()
                                                                              .AddPropertyValidatorVerifier<NumberBiggerThanZeroValidator<ExperimentGroupForUpdateDto, uint>>()
                                                                              .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Username()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Username, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentGroupForUpdateDto, string>>()
                .AddPropertyValidatorVerifier<UserNameValidator<ExperimentGroupForUpdateDto, string>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Team()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Team, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentGroupForUpdateDto, string>>()
                .AddMaximumLengthValidatorVerifier<ExperimentGroupForUpdateDto>(50)
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Segment()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Segment, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentGroupForUpdateDto, string>>()
                .AddMaximumLengthValidatorVerifier<ExperimentGroupForUpdateDto>(50)
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_DisplayName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.DisplayName, BaseVerifiersSetComposer.Build()
                .AddMaximumLengthValidatorVerifier<ExperimentGroupForUpdateDto>(100)
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_TestProgramData()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.TestProgramData, BaseVerifiersSetComposer.Build()
                                                                              .AddPropertyValidatorVerifier<NotNullValidator<ExperimentGroupForUpdateDto, TestProgramDataForUpdateDto>>()
                                                                              .AddChildValidatorVerifier<TestProgramDataForUpdateDtoValidator, ExperimentGroupForUpdateDto, TestProgramDataForUpdateDto>()
                                                                              .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Experiments()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Experiments, BaseVerifiersSetComposer.Build()
                                                                              .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentGroupForUpdateDto, IEnumerable<ExperimentForUpdateDto>>>()
                                                                              .AddPropertyValidatorVerifier<MaximumCollectionLengthValidator<ExperimentGroupForUpdateDto, IEnumerable>>()
                                                                              .AddPropertyValidatorVerifier<UniqueIEnumerableValidator<ExperimentGroupForUpdateDto, ExperimentForUpdateDto, uint>>()
                                                                              .AddChildValidatorVerifier<ExperimentForUpdateDtoValidator, ExperimentGroupForUpdateDto, ExperimentForUpdateDto>()
                                                                              .Create());
        }
    }
}
