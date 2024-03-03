using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentValidation.TestHelper;
using FluentValidation.Validators;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Configuration;
using TTPService.Dtos.Creation;
using TTPService.Dtos.Update;
using TTPService.Enums;
using TTPService.Validators.Properties;
using TTPService.Validators.Update;

namespace TTPService.Tests.Validators.Update
{
    [TestClass]
    public class ExperimentForUpdateDtoValidatorFixtureL0
    {
        private static readonly Dictionary<ExperimentType, IEnumerable<ActivityType>> _experimetToPossibleActivityTypesMapping
            = new ()
            {
                { ExperimentType.Engineering, new List<ActivityType> { ActivityType.ModuleValidation, ActivityType.MassiveValidation, ActivityType.RejectValidation, ActivityType.Engineering } },
                { ExperimentType.Correlation, new List<ActivityType> { ActivityType.Correlation } },
                { ExperimentType.WalkTheLot, new List<ActivityType> { ActivityType.WalkTheLot } },
            };

        private static ExperimentForUpdateDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var experimentIdentifiersOptionsCreate = Options.Create(new ExperimentIdentifiersOptions()
            {
                ExperimentToPossibleActivityTypesMapping = _experimetToPossibleActivityTypesMapping,
            });

            _sut = new ExperimentForUpdateDtoValidator(experimentIdentifiersOptionsCreate);
        }

        [TestMethod]
        public void ShouldHaveRules_BomGroupName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.BomGroupName, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentForUpdateDto, string>>()
                .AddMaximumLengthValidatorVerifier<ExperimentForUpdateDto>(1000)
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Step()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Step, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentForUpdateDto, string>>()
                .AddMaximumLengthValidatorVerifier<ExperimentForUpdateDto>(3)
                .AddPropertyValidatorVerifier<StepValidator<ExperimentForUpdateDto, string>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_DisplayName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.DisplayName, BaseVerifiersSetComposer.Build()
                .AddMaximumLengthValidatorVerifier<ExperimentForUpdateDto>(100)
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_TplFileName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.TplFileName, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentForUpdateDto, string>>()
                .AddMaximumLengthValidatorVerifier<ExperimentForUpdateDto>(3000)
                .AddPropertyValidatorVerifier<FileNameValidator<ExperimentForUpdateDto, string>>()
                .AddPropertyValidatorVerifier<EmptyValidator<ExperimentForUpdateDto, string>>() // this relates to the 2nd rule which RecipeSource is ByoUseAsIs
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_StplFileName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.StplFileName, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentForUpdateDto, string>>()
                .AddMaximumLengthValidatorVerifier<ExperimentForUpdateDto>(3000)
                .AddPropertyValidatorVerifier<FileNameValidator<ExperimentForUpdateDto, string>>()
                .AddPropertyValidatorVerifier<EmptyValidator<ExperimentForUpdateDto, string>>() // this relates to the 2nd rule which RecipeSource is ByoUseAsIs
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Tags()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Tags, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<TagDuplicationValidator<ExperimentForUpdateDto, IEnumerable<string>>>()
                .AddMaximumLengthValidatorVerifier<ExperimentForUpdateDto>(50)
                .AddPropertyValidatorVerifier<TagCharactersValidator<ExperimentForUpdateDto, string>>()
                .AddPropertyValidatorVerifier<NotNullValidator<ExperimentForUpdateDto, string>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Description()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Description, BaseVerifiersSetComposer.Build()
                .AddMaximumLengthValidatorVerifier<ExperimentForUpdateDto>(1000)
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_EnvironmentFileName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.EnvironmentFileName, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentForUpdateDto, string>>()
                .AddMaximumLengthValidatorVerifier<ExperimentForUpdateDto>(3000)
                .AddPropertyValidatorVerifier<FileNameValidator<ExperimentForUpdateDto, string>>()
                .AddPropertyValidatorVerifier<EmptyValidator<ExperimentForUpdateDto, string>>() // this relates to the 2nd rule when RecipeSource is ByoUseAsIs and no fuse
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_PlistAllFileName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.PlistAllFileName, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentForUpdateDto, string>>()
                .AddMaximumLengthValidatorVerifier<ExperimentForUpdateDto>(3000)
                .AddPropertyValidatorVerifier<FileNameValidator<ExperimentForUpdateDto, string>>()
                .AddPropertyValidatorVerifier<EmptyValidator<ExperimentForUpdateDto, string>>() // this relates to the 2nd rule which RecipeSource is ByoUseAsIs
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Material()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Material, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<ExperimentForUpdateDto, MaterialForUpdateDto>>()
                .AddChildValidatorVerifier<MaterialForUpdateDtoValidator, ExperimentForUpdateDto, MaterialForUpdateDto>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Conditions()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Conditions, BaseVerifiersSetComposer.Build()
                .AddChildValidatorVerifier<ConditionsForUpdateDtoValidator, ExperimentForUpdateDto, IEnumerable<ConditionForUpdateDto>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_RetestRate()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.RetestRate, BaseVerifiersSetComposer.Build()
                .AddComparisonValidatorVerifier<LessThanOrEqualValidator<ExperimentForUpdateDto, uint>, ExperimentForUpdateDto, uint>(100U)
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_FlexBomMrv()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.FlexbomMrv, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentForUpdateDto, string>>()
                .AddPropertyValidatorVerifier<AlphaNumericValidator<ExperimentForUpdateDto, string>>()
                .AddPropertyValidatorVerifier<MaximumLengthValidator<ExperimentForUpdateDto>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_FlexBomHri()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.FlexbomHri, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentForUpdateDto, string>>()
                .AddPropertyValidatorVerifier<MaximumLengthValidator<ExperimentForUpdateDto>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_ExperimentType()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.ExperimentType, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<ExperimentForUpdateDto, ExperimentType>>()
                .AddPropertyValidatorVerifier<EnumValidator<ExperimentForUpdateDto, ExperimentType>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_ActivityType()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.ActivityType, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<ExperimentForUpdateDto, ActivityType>>()
                .AddPropertyValidatorVerifier<EnumValidator<ExperimentForUpdateDto, ActivityType>>()
                .Create());
        }

        [DynamicData(nameof(GetDtoDataForActivityAndExperiment), DynamicDataSourceType.Method)]
        [TestMethod]
        public void TestValidate_ActivityTypeDoneMatchExperimentType(ExperimentForUpdateDto dto, bool isValid)
        {
            // Arrange
            var expected = $"The combination of ExperimentType {dto.ExperimentType} and ActivityType {dto.ActivityType} is not supported";

            // Act
            var actual = _sut.TestValidate(dto);

            // Assert
            if (isValid)
            {
                actual.Errors.Should().NotContain(failure => failure.ErrorMessage == expected);
            }
            else
            {
                actual.Errors.Should().Contain(failure => failure.ErrorMessage == expected);
            }
        }

        [TestMethod]
        public void ShouldHaveRules_ExperimentSplitId()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.ExperimentSplitId, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<PredicateValidator<ExperimentForUpdateDto, Guid?>>()
                .Create());
        }

        [TestMethod]
        [DataRow(false, DisplayName = "When no experiment split id")]
        [DataRow(true, DisplayName = "When experiment split id")]
        public void TestValidate_ExperimentSplitId_ValidCases(bool includeExperimentSplitGuid)
        {
            // Arrange
            var dto = GetValidDtoData();
            dto.ExperimentSplitId = includeExperimentSplitGuid ? new Guid?(Guid.Parse("baff463d-28f8-4a79-a354-c583dc61f3c6")) : (Guid?)null;

            // Act
            var actual = _sut.TestValidate(dto);

            // Assert
            actual.Errors.Should().NotContain(e => e.ErrorMessage == "Experiment split id should not be empty when provided.");
        }

        [TestMethod]
        public void TestValidate_ExperimentSplitId_InvalidCases()
        {
            // Arrange
            var dto = GetValidDtoData();
            dto.ExperimentSplitId = Guid.Empty;

            // Act
            var actual = _sut.TestValidate(dto);

            // Assert
            actual.Errors.Should().Contain(e => e.PropertyName == "ExperimentSplitId" && e.ErrorMessage == "Experiment split id should not be empty when provided.");
        }

        [TestMethod]
        public void ShouldHaveRules_Recipe()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Recipe, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<ExperimentForUpdateDto, RecipeForUpdateDto>>()
                .AddChildValidatorVerifier<RecipeForUpdateDtoValidator, ExperimentForUpdateDto, RecipeForUpdateDto>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_ContactEmails()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.ContactEmails, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<AspNetCoreCompatibleEmailValidator<ExperimentForUpdateDto>>()
                .Create());
        }

        [DynamicData(nameof(GetDtoDataForFactConditionAndFusedMaterial), DynamicDataSourceType.Method)]
        [TestMethod]
        public void TestValidate_FusedMaterialAndFactCondition(ExperimentForUpdateDto dto, bool isValid)
        {
            // Arrange
            var expected = "Experiment with FaCT conditions running on fused units is not allowed";

            // Act
            var actual = _sut.TestValidate(dto);

            // Assert
            if (isValid)
            {
                actual.Errors.Should().NotContain(failure => failure.ErrorMessage == expected);
            }
            else
            {
                actual.Errors.Should().Contain(failure => failure.ErrorMessage == expected);
            }
        }

        [TestMethod]
        public void ShouldHaveRules_ExperimentState()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.ExperimentState, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<EnumValidator<ExperimentForUpdateDto, ExperimentState>>()
                .AddPropertyValidatorVerifier<PredicateValidator<ExperimentForUpdateDto, ExperimentState>>()
                .Create());
        }

        [DynamicData(nameof(GetDtoData), DynamicDataSourceType.Method)]
        [TestMethod]
        public void TestValidate_FileNames_RecipeSource(ExperimentForUpdateDto dto, string expected)
        {
            // Arrange
            // Act
            var actual = _sut.TestValidate(dto);

            // Assert
            actual.Errors.Should().Contain(failure => failure.ErrorMessage == expected);
        }

        private static IEnumerable<object[]> GetDtoData()
        {
            string errorMsgTplFileNameMustBeEmpty = "'Tpl File Name' must be empty when RecipeSource is ByoUseAsIs.";
            string errorMsgStplFileNameMustBeEmpty = "'Stpl File Name' must be empty when RecipeSource is ByoUseAsIs.";
            string errorMsgEnvironmentFileNameMustBeEmpty = "'Environment File Name' must be empty when RecipeSource is ByoUseAsIs and has no fuse material or fuse condition.";
            string errorMsgPlistAllNameMustBeEmpty = "'Plist All File Name' must be empty when RecipeSource is ByoUseAsIs.";
            string errorMsgTplFileNameMustNotBeEmpty = "'Tpl File Name' must not be empty.";
            string errorMsgStplFileNameMustNotBeEmpty = "'Stpl File Name' must not be empty.";
            string errorMsgEnvironmentFileNameMustNotBeEmpty = "'Environment File Name' must not be empty.";
            string errorMsgPlistAllNameMustNotBeEmpty = "'Plist All File Name' must not be empty.";

            yield return new object[] { new ExperimentForUpdateDto() { TplFileName = "Tpl", Recipe = new RecipeForUpdateDto() { RecipeSource = RecipeSource.ByoUseAsIs, RecipePath = @"\\abc\", }, }, errorMsgTplFileNameMustBeEmpty, };
            yield return new object[] { new ExperimentForUpdateDto() { StplFileName = "Stpl", Recipe = new RecipeForUpdateDto() { RecipeSource = RecipeSource.ByoUseAsIs, RecipePath = @"\\abc\", }, }, errorMsgStplFileNameMustBeEmpty, };
            yield return new object[] { new ExperimentForUpdateDto() { PlistAllFileName = "Plist", Recipe = new RecipeForUpdateDto() { RecipeSource = RecipeSource.ByoUseAsIs, RecipePath = @"\\abc\", }, }, errorMsgPlistAllNameMustBeEmpty, };
            yield return new object[] { new ExperimentForUpdateDto() { TplFileName = string.Empty, Recipe = new RecipeForUpdateDto() { RecipeSource = RecipeSource.TpGenerated, RecipePath = @"\\abc\", }, }, errorMsgTplFileNameMustNotBeEmpty, };
            yield return new object[] { new ExperimentForUpdateDto() { StplFileName = string.Empty, Recipe = new RecipeForUpdateDto() { RecipeSource = RecipeSource.TpGenerated, RecipePath = @"\\abc\", }, }, errorMsgStplFileNameMustNotBeEmpty, };
            yield return new object[] { new ExperimentForUpdateDto() { EnvironmentFileName = "env", Recipe = new RecipeForUpdateDto() { RecipeSource = RecipeSource.ByoUseAsIs, RecipePath = @"\\abc\", }, }, errorMsgEnvironmentFileNameMustBeEmpty, };
            yield return new object[] { new ExperimentForUpdateDto() { EnvironmentFileName = string.Empty, Recipe = new RecipeForUpdateDto() { RecipeSource = RecipeSource.TpGenerated, RecipePath = @"\\abc\", }, }, errorMsgEnvironmentFileNameMustNotBeEmpty, };
            yield return new object[] { new ExperimentForUpdateDto() { EnvironmentFileName = string.Empty, Recipe = new RecipeForUpdateDto() { RecipeSource = RecipeSource.ByoGenerated, RecipePath = @"\\abc\", }, }, errorMsgEnvironmentFileNameMustNotBeEmpty, };
            yield return new object[]
            {
                new ExperimentForUpdateDto()
                {
                EnvironmentFileName = string.Empty,
                Conditions = new ConditionForUpdateDto[]
                {
                    new ConditionForUpdateDto()
                    {
                        Fuse = new FuseForUpdateDto(),
                    },
                },
                Recipe = new RecipeForUpdateDto()
                {
                    RecipeSource = RecipeSource.ByoUseAsIs, RecipePath = @"\\abc\",
                },
                }, errorMsgEnvironmentFileNameMustNotBeEmpty,
            };

            yield return new object[]
            {
                new ExperimentForUpdateDto()
                {
                    EnvironmentFileName = string.Empty,
                    Material = new MaterialForUpdateDto()
                   {
                       Sspec = "absd",
                   },
                    Recipe = new RecipeForUpdateDto()
                    {
                        RecipeSource = RecipeSource.ByoUseAsIs, RecipePath = @"\\abc\",
                    },
                }, errorMsgEnvironmentFileNameMustNotBeEmpty,
            };

            yield return new object[] { new ExperimentForUpdateDto() { PlistAllFileName = string.Empty, Recipe = new RecipeForUpdateDto() { RecipeSource = RecipeSource.TpGenerated, RecipePath = @"\\abc\", }, }, errorMsgPlistAllNameMustNotBeEmpty, };
        }

        private static ExperimentForUpdateDto GetValidDtoData()
        {
            return new ExperimentForUpdateDto
            {
                ExperimentType = ExperimentType.Correlation,
                ActivityType = ActivityType.Correlation,
                FlexbomHri = FlexbomDefaults.Hri,
                FlexbomMrv = FlexbomDefaults.Mrv,
                Recipe = new RecipeForUpdateDto() { RecipeSource = RecipeSource.ByoGenerated },
            };
        }

        private static IEnumerable<object[]> GetDtoDataForActivityAndExperiment()
        {
            yield return new object[] { new ExperimentForUpdateDto { ExperimentType = ExperimentType.Engineering, ActivityType = ActivityType.Correlation, Recipe = new RecipeForUpdateDto() { RecipeSource = RecipeSource.ByoGenerated } }, false };
            yield return new object[] { new ExperimentForUpdateDto { ExperimentType = ExperimentType.Engineering, ActivityType = ActivityType.WalkTheLot, Recipe = new RecipeForUpdateDto() { RecipeSource = RecipeSource.ByoGenerated } }, false };
            yield return new object[] { new ExperimentForUpdateDto { ExperimentType = ExperimentType.Correlation, ActivityType = ActivityType.ModuleValidation, Recipe = new RecipeForUpdateDto() { RecipeSource = RecipeSource.ByoGenerated } }, false };
            yield return new object[] { new ExperimentForUpdateDto { ExperimentType = ExperimentType.WalkTheLot, ActivityType = ActivityType.ModuleValidation, Recipe = new RecipeForUpdateDto() { RecipeSource = RecipeSource.ByoGenerated } }, false };
            yield return new object[] { new ExperimentForUpdateDto { ExperimentType = ExperimentType.Engineering, ActivityType = ActivityType.ModuleValidation, Recipe = new RecipeForUpdateDto() { RecipeSource = RecipeSource.ByoGenerated } }, true };
            yield return new object[] { new ExperimentForUpdateDto { ExperimentType = ExperimentType.Engineering, ActivityType = ActivityType.RejectValidation, Recipe = new RecipeForUpdateDto() { RecipeSource = RecipeSource.ByoGenerated } }, true };
            yield return new object[] { new ExperimentForUpdateDto { ExperimentType = ExperimentType.Engineering, ActivityType = ActivityType.MassiveValidation, Recipe = new RecipeForUpdateDto() { RecipeSource = RecipeSource.ByoGenerated } }, true };
            yield return new object[] { new ExperimentForUpdateDto { ExperimentType = ExperimentType.Engineering, ActivityType = ActivityType.Engineering, Recipe = new RecipeForUpdateDto() { RecipeSource = RecipeSource.ByoGenerated } }, true };
            yield return new object[] { new ExperimentForUpdateDto { ExperimentType = ExperimentType.Correlation, ActivityType = ActivityType.Correlation, Recipe = new RecipeForUpdateDto() { RecipeSource = RecipeSource.ByoGenerated } }, true };
            yield return new object[] { new ExperimentForUpdateDto { ExperimentType = ExperimentType.WalkTheLot, ActivityType = ActivityType.WalkTheLot, Recipe = new RecipeForUpdateDto() { RecipeSource = RecipeSource.ByoGenerated } }, true };
        }

        private static IEnumerable<object[]> GetDtoDataForFactConditionAndFusedMaterial()
        {
            yield return new object[]
            {
                new ExperimentForUpdateDto
                {
                    Conditions = null,
                    Material = new MaterialForUpdateDto
                    {
                        Sspec = "QDF1",
                    },
                    Recipe = new RecipeForUpdateDto() { RecipeSource = RecipeSource.ByoGenerated },
                },
                true,
            };

            yield return new object[]
            {
                new ExperimentForUpdateDto
                {
                    Conditions = new List<ConditionForUpdateDto>
                    {
                        new ConditionForUpdateDto
                        {
                            Fuse = new FuseForUpdateDto(),
                        },
                    },
                    Material = null,
                    Recipe = new RecipeForUpdateDto() { RecipeSource = RecipeSource.ByoGenerated },
                },
                true,
            };

            yield return new object[]
            {
                new ExperimentForUpdateDto
                {
                    Conditions = new List<ConditionForUpdateDto>
                    {
                        new ConditionForUpdateDto
                        {
                            Fuse = new FuseForUpdateDto(),
                        },
                    },
                    Material = new MaterialForUpdateDto
                    {
                        Sspec = "QDF1",
                    },
                    Recipe = new RecipeForUpdateDto() { RecipeSource = RecipeSource.ByoGenerated },
                },
                false,
            };

            yield return new object[]
            {
                new ExperimentForUpdateDto
                {
                    Conditions = new List<ConditionForUpdateDto>
                    {
                        new ConditionForUpdateDto
                        {
                            Fuse = null,
                        },
                    },
                    Material = new MaterialForUpdateDto
                    {
                        Sspec = "QDF1",
                    },
                    Recipe = new RecipeForUpdateDto() { RecipeSource = RecipeSource.ByoGenerated },
                },
                true,
            };
            yield return new object[]
            {
                new ExperimentForUpdateDto
                {
                    Conditions = new List<ConditionForUpdateDto>
                    {
                        new ConditionForUpdateDto
                        {
                            Fuse = new FuseForUpdateDto(),
                        },
                    },
                    Material = new MaterialForUpdateDto
                    {
                        Sspec = null,
                    },
                    Recipe = new RecipeForUpdateDto() { RecipeSource = RecipeSource.ByoGenerated },
                },
                true,
            };
        }
    }
}