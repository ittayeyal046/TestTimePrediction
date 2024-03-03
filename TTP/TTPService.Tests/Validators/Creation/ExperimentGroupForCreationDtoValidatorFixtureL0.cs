using System.Collections;
using System.Collections.Generic;
using FluentValidation.Validators;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Configuration;
using TTPService.Dtos.Creation;
using TTPService.Enums;
using TTPService.Validators.Creation;
using TTPService.Validators.Properties;

namespace TTPService.Tests.Validators.Creation
{
    [TestClass]
    public class ExperimentGroupForCreationDtoValidatorFixtureL0
    {
        private static readonly Dictionary<ExperimentType, IEnumerable<ActivityType>> _experimetToPossibleActivityTypesMapping
            = new ()
            {
                { ExperimentType.Engineering, new List<ActivityType> { ActivityType.ModuleValidation, ActivityType.MassiveValidation, ActivityType.RejectValidation, ActivityType.Engineering } },
                { ExperimentType.Correlation, new List<ActivityType> { ActivityType.Correlation } },
                { ExperimentType.WalkTheLot, new List<ActivityType> { ActivityType.WalkTheLot } },
            };

        private static ExperimentGroupForCreationDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var experimentIdentifiersOptionsCreate = Options.Create(new ExperimentIdentifiersOptions()
            {
                ExperimentToPossibleActivityTypesMapping = _experimetToPossibleActivityTypesMapping,
            });

            _sut = new ExperimentGroupForCreationDtoValidator(experimentIdentifiersOptionsCreate);
        }

        [TestMethod]
        public void ShouldHaveRules_Wwid()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Wwid, BaseVerifiersSetComposer.Build()
                                                                              .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentGroupForCreationDto, uint>>()
                                                                              .AddPropertyValidatorVerifier<NumberBiggerThanZeroValidator<ExperimentGroupForCreationDto, uint>>()
                                                                              .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Username()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Username, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentGroupForCreationDto, string>>()
                .AddPropertyValidatorVerifier<UserNameValidator<ExperimentGroupForCreationDto, string>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Team()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Team, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentGroupForCreationDto, string>>()
                .AddMaximumLengthValidatorVerifier<ExperimentGroupForCreationDto>(50)
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Segment()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Segment, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentGroupForCreationDto, string>>()
                .AddMaximumLengthValidatorVerifier<ExperimentGroupForCreationDto>(50)
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_DisplayName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.DisplayName, BaseVerifiersSetComposer.Build()
                .AddMaximumLengthValidatorVerifier<ExperimentGroupForCreationDto>(100)
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_TestProgramData()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.TestProgramData, BaseVerifiersSetComposer.Build()
                                                                              .AddPropertyValidatorVerifier<NotNullValidator<ExperimentGroupForCreationDto, TestProgramDataForCreationDto>>()
                                                                              .AddChildValidatorVerifier<TestProgramDataForCreationDtoValidator, ExperimentGroupForCreationDto, TestProgramDataForCreationDto>()
                                                                              .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Experiments()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Experiments, BaseVerifiersSetComposer.Build()
                                                                              .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentGroupForCreationDto, IEnumerable<ExperimentForCreationDto>>>()
                                                                              .AddPropertyValidatorVerifier<MaximumCollectionLengthValidator<ExperimentGroupForCreationDto, IEnumerable>>()
                                                                              .AddPropertyValidatorVerifier<UniqueIEnumerableValidator<ExperimentGroupForCreationDto, ExperimentForCreationDto, uint>>()
                                                                              .AddChildValidatorVerifier<ExperimentForCreationDtoValidator, ExperimentGroupForCreationDto, ExperimentForCreationDto>()
                                                                              .Create());
        }
    }
}
