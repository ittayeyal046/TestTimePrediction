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
using TTPService.Enums;
using TTPService.Validators.Creation;
using TTPService.Validators.Properties;

namespace TTPService.Tests.Validators.Creation
{
    [TestClass]
    public class ExperimentForCreationDtoValidatorFixtureL0
    {
        private static readonly Dictionary<ExperimentType, IEnumerable<ActivityType>> _experimetToPossibleActivityTypesMapping
            = new ()
            {
                { ExperimentType.Engineering, new List<ActivityType> { ActivityType.ModuleValidation, ActivityType.MassiveValidation, ActivityType.RejectValidation, ActivityType.Engineering } },
                { ExperimentType.Correlation, new List<ActivityType> { ActivityType.Correlation } },
                { ExperimentType.WalkTheLot, new List<ActivityType> { ActivityType.WalkTheLot } },
            };

        private static ExperimentForCreationDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var experimentIdentifiersOptionsCreate = Options.Create(new ExperimentIdentifiersOptions()
            {
                ExperimentToPossibleActivityTypesMapping = _experimetToPossibleActivityTypesMapping,
            });

            _sut = new ExperimentForCreationDtoValidator(experimentIdentifiersOptionsCreate);
        }

        [TestMethod]
        public void ShouldHaveRules_BomGroupName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.BomGroupName, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentForCreationDto, string>>()
                .AddMaximumLengthValidatorVerifier<ExperimentForCreationDto>(1000)
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Step()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Step, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentForCreationDto, string>>()
                .AddMaximumLengthValidatorVerifier<ExperimentForCreationDto>(3)
                .AddPropertyValidatorVerifier<StepValidator<ExperimentForCreationDto, string>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_DisplayName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.DisplayName, BaseVerifiersSetComposer.Build()
                .AddMaximumLengthValidatorVerifier<ExperimentForCreationDto>(100)
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_TplFileName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.TplFileName, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentForCreationDto, string>>()
                .AddMaximumLengthValidatorVerifier<ExperimentForCreationDto>(3000)
                .AddPropertyValidatorVerifier<FileNameValidator<ExperimentForCreationDto, string>>()
                .AddPropertyValidatorVerifier<EmptyValidator<ExperimentForCreationDto, string>>() // this relates to the 2nd rule when RecipeSource is ByoUseAsIs
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_StplFileName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.StplFileName, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentForCreationDto, string>>()
                .AddMaximumLengthValidatorVerifier<ExperimentForCreationDto>(3000)
                .AddPropertyValidatorVerifier<FileNameValidator<ExperimentForCreationDto, string>>()
                .AddPropertyValidatorVerifier<EmptyValidator<ExperimentForCreationDto, string>>() // this relates to the 2nd rule when RecipeSource is ByoUseAsIs
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Tags()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Tags, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<TagDuplicationValidator<ExperimentForCreationDto, IEnumerable<string>>>()
                .AddMaximumLengthValidatorVerifier<ExperimentForCreationDto>(50)
                .AddPropertyValidatorVerifier<TagCharactersValidator<ExperimentForCreationDto, string>>()
                .AddPropertyValidatorVerifier<NotNullValidator<ExperimentForCreationDto, string>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Description()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Description, BaseVerifiersSetComposer.Build()
                .AddMaximumLengthValidatorVerifier<ExperimentForCreationDto>(1000)
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_EnvironmentFileName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.EnvironmentFileName, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentForCreationDto, string>>()
                .AddMaximumLengthValidatorVerifier<ExperimentForCreationDto>(3000)
                .AddPropertyValidatorVerifier<FileNameValidator<ExperimentForCreationDto, string>>()
                .AddPropertyValidatorVerifier<EmptyValidator<ExperimentForCreationDto, string>>() // this relates to the 2nd rule when RecipeSource is ByoUseAsIs and no fuse
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_PlistAllFileName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.PlistAllFileName, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentForCreationDto, string>>()
                .AddMaximumLengthValidatorVerifier<ExperimentForCreationDto>(3000)
                .AddPropertyValidatorVerifier<FileNameValidator<ExperimentForCreationDto, string>>()
                .AddPropertyValidatorVerifier<EmptyValidator<ExperimentForCreationDto, string>>() // this relates to the 2nd rule when RecipeSource is ByoUseAsIs
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Material()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Material, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<ExperimentForCreationDto, MaterialForCreationDto>>()
                .AddChildValidatorVerifier<MaterialForCreationDtoValidator, ExperimentForCreationDto, MaterialForCreationDto>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Stages()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Stages, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<ExperimentForCreationDto, IEnumerable<StageForCreationDto>>>()
                .AddChildValidatorVerifier<StagesForCreationDtoValidator, ExperimentForCreationDto, IEnumerable<StageForCreationDto>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_RetestRate()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.RetestRate, BaseVerifiersSetComposer.Build()
                .AddComparisonValidatorVerifier<LessThanOrEqualValidator<ExperimentForCreationDto, uint>, ExperimentForCreationDto, uint>(100U)
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_FlexBomMrv()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.FlexbomMrv, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentForCreationDto, string>>()
                .AddPropertyValidatorVerifier<AlphaNumericValidator<ExperimentForCreationDto, string>>()
                .AddPropertyValidatorVerifier<MaximumLengthValidator<ExperimentForCreationDto>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_FlexBomHri()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.FlexbomHri, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentForCreationDto, string>>()
                .AddPropertyValidatorVerifier<MaximumLengthValidator<ExperimentForCreationDto>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_ExperimentType()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.ExperimentType, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<ExperimentForCreationDto, ExperimentType?>>()
                .AddPropertyValidatorVerifier<EnumValidator<ExperimentForCreationDto, ExperimentType?>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_ActivityType()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.ActivityType, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<ExperimentForCreationDto, ActivityType?>>()
                .AddPropertyValidatorVerifier<EnumValidator<ExperimentForCreationDto, ActivityType?>>()
                .Create());
        }

        [DynamicData(nameof(GetDtoDataForActivityAndExperiment), DynamicDataSourceType.Method)]
        [TestMethod]
        public void TestValidate_ActivityTypeDoneMatchExperimentType(ExperimentForCreationDto dto, bool isValid)
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
                .AddPropertyValidatorVerifier<PredicateValidator<ExperimentForCreationDto, Guid?>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_ExperimentState()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.ExperimentState, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<EnumValidator<ExperimentForCreationDto, ExperimentState?>>()
                .AddPropertyValidatorVerifier<PredicateValidator<ExperimentForCreationDto, ExperimentState?>>()
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
                .AddPropertyValidatorVerifier<NotNullValidator<ExperimentForCreationDto, RecipeForCreationDto>>()
                .AddChildValidatorVerifier<RecipeForCreationDtoValidator, ExperimentForCreationDto, RecipeForCreationDto>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_ContactEmails()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.ContactEmails, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<AspNetCoreCompatibleEmailValidator<ExperimentForCreationDto>>()
                .Create());
        }

        [DynamicData(nameof(GetDtoDataForFactConditionAndFusedMaterial), DynamicDataSourceType.Method)]
        [TestMethod]
        public void TestValidate_FusedMaterialAndFactCondition(ExperimentForCreationDto dto, bool isValid)
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

        [DynamicData(nameof(GetDtoDataForRecipeAndExperimentFiles), DynamicDataSourceType.Method)]
        [TestMethod]
        public void TestValidate_FileNames_RecipeSource(ExperimentForCreationDto dto, string expected)
        {
            // Arrange
            // Act
            var actual = _sut.TestValidate(dto);

            // Assert
            actual.Errors.Should().Contain(failure => failure.ErrorMessage == expected);
        }

        private static IEnumerable<object[]> GetDtoDataForRecipeAndExperimentFiles()
        {
            string errorMsgTplFileNameMustBeEmpty = "'Tpl File Name' must be empty when RecipeSource is ByoUseAsIs.";
            string errorMsgStplFileNameMustBeEmpty = "'Stpl File Name' must be empty when RecipeSource is ByoUseAsIs.";
            string errorMsgEnvironmentFileNameMustBeEmpty = "'Environment File Name' must be empty when RecipeSource is ByoUseAsIs and has no fuse material or fuse condition.";
            string errorMsgPlistAllNameMustBeEmpty = "'Plist All File Name' must be empty when RecipeSource is ByoUseAsIs.";
            string errorMsgTplFileNameMustNotBeEmpty = "'Tpl File Name' must not be empty.";
            string errorMsgStplFileNameMustNotBeEmpty = "'Stpl File Name' must not be empty.";
            string errorMsgEnvironmentFileNameMustNotBeEmpty = "'Environment File Name' must not be empty.";
            string errorMsgPlistAllNameMustNotBeEmpty = "'Plist All File Name' must not be empty.";

            yield return new object[] { new ExperimentForCreationDto() { TplFileName = "Tpl", Recipe = new RecipeForCreationDto() { RecipeSource = RecipeSource.ByoUseAsIs, RecipePath = @"\\abc\", }, }, errorMsgTplFileNameMustBeEmpty, };
            yield return new object[] { new ExperimentForCreationDto() { StplFileName = "Stpl", Recipe = new RecipeForCreationDto() { RecipeSource = RecipeSource.ByoUseAsIs, RecipePath = @"\\abc\", }, }, errorMsgStplFileNameMustBeEmpty, };
            yield return new object[] { new ExperimentForCreationDto() { PlistAllFileName = "Plist", Recipe = new RecipeForCreationDto() { RecipeSource = RecipeSource.ByoUseAsIs, RecipePath = @"\\abc\", }, }, errorMsgPlistAllNameMustBeEmpty, };
            yield return new object[] { new ExperimentForCreationDto() { TplFileName = string.Empty, Recipe = new RecipeForCreationDto() { RecipeSource = RecipeSource.TpGenerated, RecipePath = @"\\abc\", }, }, errorMsgTplFileNameMustNotBeEmpty, };
            yield return new object[] { new ExperimentForCreationDto() { StplFileName = string.Empty, Recipe = new RecipeForCreationDto() { RecipeSource = RecipeSource.TpGenerated, RecipePath = @"\\abc\", }, }, errorMsgStplFileNameMustNotBeEmpty, };
            yield return new object[] { new ExperimentForCreationDto() { EnvironmentFileName = string.Empty, Recipe = new RecipeForCreationDto() { RecipeSource = RecipeSource.TpGenerated, RecipePath = @"\\abc\", }, }, errorMsgEnvironmentFileNameMustNotBeEmpty, };
            yield return new object[] { new ExperimentForCreationDto() { PlistAllFileName = string.Empty, Recipe = new RecipeForCreationDto() { RecipeSource = RecipeSource.TpGenerated, RecipePath = @"\\abc\", }, }, errorMsgPlistAllNameMustNotBeEmpty, };
            yield return new object[] { new ExperimentForCreationDto() { TplFileName = string.Empty, Recipe = new RecipeForCreationDto() { RecipeSource = RecipeSource.ByoGenerated, RecipePath = @"\\abc\", }, }, errorMsgTplFileNameMustNotBeEmpty, };
            yield return new object[] { new ExperimentForCreationDto() { StplFileName = string.Empty, Recipe = new RecipeForCreationDto() { RecipeSource = RecipeSource.ByoGenerated, RecipePath = @"\\abc\", }, }, errorMsgStplFileNameMustNotBeEmpty, };
            yield return new object[] { new ExperimentForCreationDto() { EnvironmentFileName = "env", Recipe = new RecipeForCreationDto() { RecipeSource = RecipeSource.ByoUseAsIs, RecipePath = @"\\abc\", }, }, errorMsgEnvironmentFileNameMustBeEmpty, };
            yield return new object[] { new ExperimentForCreationDto() { EnvironmentFileName = string.Empty, Recipe = new RecipeForCreationDto() { RecipeSource = RecipeSource.ByoGenerated, RecipePath = @"\\abc\", }, }, errorMsgEnvironmentFileNameMustNotBeEmpty, };
            yield return new object[] { new ExperimentForCreationDto() { EnvironmentFileName = string.Empty, Recipe = new RecipeForCreationDto() { RecipeSource = RecipeSource.TpGenerated, RecipePath = @"\\abc\", }, }, errorMsgEnvironmentFileNameMustNotBeEmpty, };
            yield return new object[]
            {
                new ExperimentForCreationDto()
                {
                    EnvironmentFileName = string.Empty,
                    Stages = new StageForCreationDto[]
                    {
                        new StageForCreationDto()
                        {
                            StageType = StageType.Class,
                            StageData = new ClassStageDataForCreationDto()
                            {
                                Conditions = new ConditionForCreationDto[]
                                {
                                    new ConditionForCreationDto()
                                    {
                                        Fuse = new FuseForCreationDto(),
                                    },
                                },
                            },
                        },
                    },
                    Recipe = new RecipeForCreationDto()
                    {
                        RecipeSource = RecipeSource.ByoUseAsIs, RecipePath = @"\\abc\",
                    },
                }, errorMsgEnvironmentFileNameMustNotBeEmpty,
            };

            yield return new object[]
            {
                new ExperimentForCreationDto()
                {
                    EnvironmentFileName = string.Empty,
                    Material = new MaterialForCreationDto()
                    {
                        Sspec = "absd",
                    },
                    Recipe = new RecipeForCreationDto()
                    {
                        RecipeSource = RecipeSource.ByoUseAsIs, RecipePath = @"\\abc\",
                    },
                }, errorMsgEnvironmentFileNameMustNotBeEmpty,
            };
            yield return new object[] { new ExperimentForCreationDto() { PlistAllFileName = string.Empty, Recipe = new RecipeForCreationDto() { RecipeSource = RecipeSource.ByoGenerated, RecipePath = @"\\abc\", }, }, errorMsgPlistAllNameMustNotBeEmpty, };
        }

        private static ExperimentForCreationDto GetValidDtoData()
        {
            return new ExperimentForCreationDto
            {
                ExperimentType = ExperimentType.Correlation,
                ActivityType = ActivityType.Correlation,
                FlexbomHri = FlexbomDefaults.Hri,
                FlexbomMrv = FlexbomDefaults.Mrv,
                Recipe = new RecipeForCreationDto() { RecipeSource = RecipeSource.ByoGenerated },
            };
        }

        private static IEnumerable<object[]> GetDtoDataForActivityAndExperiment()
        {
            yield return new object[] { new ExperimentForCreationDto { ExperimentType = ExperimentType.Engineering, ActivityType = ActivityType.Correlation, Recipe = new RecipeForCreationDto() { RecipeSource = RecipeSource.ByoGenerated} }, false };
            yield return new object[] { new ExperimentForCreationDto { ExperimentType = ExperimentType.Engineering, ActivityType = ActivityType.WalkTheLot, Recipe = new RecipeForCreationDto() { RecipeSource = RecipeSource.ByoGenerated } }, false };
            yield return new object[] { new ExperimentForCreationDto { ExperimentType = ExperimentType.Correlation, ActivityType = ActivityType.ModuleValidation, Recipe = new RecipeForCreationDto() { RecipeSource = RecipeSource.ByoGenerated } }, false };
            yield return new object[] { new ExperimentForCreationDto { ExperimentType = ExperimentType.WalkTheLot, ActivityType = ActivityType.ModuleValidation, Recipe = new RecipeForCreationDto() { RecipeSource = RecipeSource.ByoGenerated } }, false };
            yield return new object[] { new ExperimentForCreationDto { ExperimentType = ExperimentType.Engineering, ActivityType = ActivityType.ModuleValidation, Recipe = new RecipeForCreationDto() { RecipeSource = RecipeSource.ByoGenerated } }, true };
            yield return new object[] { new ExperimentForCreationDto { ExperimentType = ExperimentType.Engineering, ActivityType = ActivityType.RejectValidation, Recipe = new RecipeForCreationDto() { RecipeSource = RecipeSource.ByoGenerated } }, true };
            yield return new object[] { new ExperimentForCreationDto { ExperimentType = ExperimentType.Engineering, ActivityType = ActivityType.MassiveValidation, Recipe = new RecipeForCreationDto() { RecipeSource = RecipeSource.ByoGenerated } }, true };
            yield return new object[] { new ExperimentForCreationDto { ExperimentType = ExperimentType.Engineering, ActivityType = ActivityType.Engineering, Recipe = new RecipeForCreationDto() { RecipeSource = RecipeSource.ByoGenerated } }, true };
            yield return new object[] { new ExperimentForCreationDto { ExperimentType = ExperimentType.Correlation, ActivityType = ActivityType.Correlation, Recipe = new RecipeForCreationDto() { RecipeSource = RecipeSource.ByoGenerated } }, true };
            yield return new object[] { new ExperimentForCreationDto { ExperimentType = ExperimentType.WalkTheLot, ActivityType = ActivityType.WalkTheLot, Recipe = new RecipeForCreationDto() { RecipeSource = RecipeSource.ByoGenerated } }, true };
        }

        private static IEnumerable<object[]> GetDtoDataForFactConditionAndFusedMaterial()
        {
            yield return new object[]
            {
                new ExperimentForCreationDto
                {
                    Stages = new List<StageForCreationDto>
                    {
                        new StageForCreationDto
                        {
                            StageType = StageType.Class,
                            StageData = new ClassStageDataForCreationDto
                            {
                                Conditions = null,
                            },
                        },
                    },
                    Material = new MaterialForCreationDto
                    {
                        Sspec = "QDF1",
                    },
                },
                true,
            };

            yield return new object[]
            {
                new ExperimentForCreationDto
                {
                    Stages = new List<StageForCreationDto>
                    {
                        new StageForCreationDto
                        {
                            StageType = StageType.Class,
                            StageData = new ClassStageDataForCreationDto
                            {
                                Conditions = new List<ConditionForCreationDto>
                                {
                                    new ConditionForCreationDto
                                    {
                                        Fuse = new FuseForCreationDto(),
                                    },
                                },
                            },
                        },
                    },
                    Material = null,
                },
                true,
            };

            yield return new object[]
            {
                new ExperimentForCreationDto
                {
                    Stages = new List<StageForCreationDto>
                    {
                        new StageForCreationDto
                        {
                            StageType = StageType.Class,
                            StageData = new ClassStageDataForCreationDto
                            {
                                Conditions = new List<ConditionForCreationDto>
                                {
                                    new ConditionForCreationDto
                                    {
                                        Fuse = new FuseForCreationDto(),
                                    },
                                },
                            },
                        },
                    },
                    Material = new MaterialForCreationDto
                    {
                        Sspec = "QDF1",
                    },
                },
                false,
            };

            yield return new object[]
            {
                new ExperimentForCreationDto
                {
                    Stages = new List<StageForCreationDto>
                    {
                        new StageForCreationDto
                        {
                            StageType = StageType.Class,
                            StageData = new ClassStageDataForCreationDto
                            {
                                Conditions = new List<ConditionForCreationDto>
                                {
                                    new ConditionForCreationDto
                                    {
                                        Fuse = null,
                                    },
                                },
                            },
                        },
                    },
                    Material = new MaterialForCreationDto
                    {
                        Sspec = "QDF1",
                    },
                },
                true,
            };
            yield return new object[]
            {
                new ExperimentForCreationDto
                {
                    Stages = new List<StageForCreationDto>
                    {
                        new StageForCreationDto
                        {
                            StageType = StageType.Class,
                            StageData = new ClassStageDataForCreationDto
                            {
                                Conditions = new List<ConditionForCreationDto>
                                {
                                    new ConditionForCreationDto
                                    {
                                        Fuse = new FuseForCreationDto(),
                                    },
                                },
                            },
                        },
                    },
                    Material = new MaterialForCreationDto
                    {
                        Sspec = null,
                    },
                },
                true,
            };
        }
    }
}