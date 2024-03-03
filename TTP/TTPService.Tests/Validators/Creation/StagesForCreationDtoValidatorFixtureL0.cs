using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using FluentValidation.TestHelper;
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
    public class StagesForCreationDtoValidatorFixtureL0
    {
        private StagesForCreationDtoValidator _sut;

        [TestInitialize]
        public void Init()
        {
            _sut = new StagesForCreationDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_Stages()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<IEnumerable<StageForCreationDto>, IEnumerable<StageForCreationDto>>>()
                .AddPropertyValidatorVerifier<MaximumCollectionLengthValidator<IEnumerable<StageForCreationDto>, IEnumerable>>()
                .AddPropertyValidatorVerifier<UniqueStageSequenceIdValidator<IEnumerable<StageForCreationDto>, IEnumerable>>()
                .AddChildValidatorVerifier<StageForCreationDtoValidator, IEnumerable<StageForCreationDto>, StageForCreationDto>()
                .AddPropertyValidatorVerifier<PredicateValidator<IEnumerable<StageForCreationDto>, IEnumerable<StageForCreationDto>>>()
                .AddPropertyValidatorVerifier<PredicateValidator<IEnumerable<StageForCreationDto>, IEnumerable<StageForCreationDto>>>()
                .AddPropertyValidatorVerifier<PredicateValidator<IEnumerable<StageForCreationDto>, IEnumerable<StageForCreationDto>>>()
                .AddPropertyValidatorVerifier<PredicateValidator<IEnumerable<StageForCreationDto>, IEnumerable<StageForCreationDto>>>()
                .Create());
        }

        [DynamicData(nameof(GetDtoDataForMaestroStage), DynamicDataSourceType.Method)]
        [TestMethod]
        public void TestValidate_MaestroStage(IEnumerable<StageForCreationDto> stages, bool expectedIsValid, string expectedErrorMessage)
        {
            // Arrange
            // Act
            var actual = _sut.TestValidate(stages);

            // Assert
            if (expectedIsValid)
            {
                actual.Errors.Should().BeEmpty();
            }
            else
            {
                actual.Errors.Should().Contain(failure => failure.ErrorMessage == expectedErrorMessage);
            }
        }

        [TestMethod]
        [DynamicData(nameof(GetStagesForDuplicatedConditionsVpoSuffix), DynamicDataSourceType.Method)]
        public void TestValidate_DuplicateConditionsVpoSuffix(IEnumerable<StageForCreationDto> stages)
        {
            // Arrange
            var expected = "VPO suffix should be provided in case of multiple conditions with same operation. It must have exactly one empty value, and the rest unique values in correct format.";

            // Act
            var actual = _sut.TestValidate(stages);

            // Assert
            actual.Errors.Count.Should().Be(1);
            actual.Errors.Should().Contain(failure => failure.ErrorMessage == expected);
        }

        private static IEnumerable<object[]> GetDtoDataForMaestroStage()
        {
            yield return new object[]
            {
                new List<StageForCreationDto>
                {
                    new StageForCreationDto
                    {
                        SequenceId = 1,
                        StageType = StageType.Maestro,
                        StageData = new MaestroStageDataForCreationDto
                        {
                            Operation = "1234",
                            EngId = "AB",
                            TpEnvironment = TpEnvironment.Production,
                        },
                    },
                    new StageForCreationDto
                    {
                        SequenceId = 2,
                        StageType = StageType.Ppv,
                        StageData = new PpvStageDataForCreationDto
                        {
                            TestType = TestType.Ppv,
                            Operation = "1234",
                            Qdf = "qdf1",
                            Recipe = "recipe",
                        },
                    },
                },
                true,
                string.Empty,
            };

            yield return new object[]
            {
                new List<StageForCreationDto>
                {
                    new StageForCreationDto
                    {
                        SequenceId = 1,
                        StageType = StageType.Maestro,
                        StageData = new MaestroStageDataForCreationDto
                        {
                            Operation = "1234",
                            EngId = "AB",
                            TpEnvironment = TpEnvironment.Production,
                        },
                    },
                },
                false,
                "Experiment must have more than a Maestro stage.",
            };

            yield return new object[]
            {
                new List<StageForCreationDto>
                {
                    new StageForCreationDto
                    {
                        SequenceId = 1,
                        StageType = StageType.Class,
                        StageData = new ClassStageDataForCreationDto
                        {
                            Conditions = new List<ConditionForCreationDto>
                            {
                                new ConditionForCreationDto(),
                            },
                        },
                    },
                    new StageForCreationDto
                    {
                        SequenceId = 2,
                        StageType = StageType.Maestro,
                        StageData = new MaestroStageDataForCreationDto
                        {
                            Operation = "1234",
                            EngId = "AB",
                            TpEnvironment = TpEnvironment.Production,
                        },
                    },
                },
                false,
                "Maestro stage must be the first in the experiment.",
            };

            yield return new object[]
            {
                new List<StageForCreationDto>
                {
                    new StageForCreationDto
                    {
                        SequenceId = 1,
                        StageType = StageType.Maestro,
                        StageData = new MaestroStageDataForCreationDto
                        {
                            Operation = "1235",
                            EngId = "AC",
                            TpEnvironment = TpEnvironment.Production,
                        },
                    },
                    new StageForCreationDto
                    {
                        SequenceId = 2,
                        StageType = StageType.Class,
                        StageData = new ClassStageDataForCreationDto
                        {
                            Conditions = new List<ConditionForCreationDto>
                            {
                                new ConditionForCreationDto(),
                            },
                        },
                    },
                    new StageForCreationDto
                    {
                        SequenceId = 3,
                        StageType = StageType.Maestro,
                        StageData = new MaestroStageDataForCreationDto
                        {
                            Operation = "1234",
                            EngId = "AB",
                            TpEnvironment = TpEnvironment.Production,
                        },
                    },
                },
                false,
                "There can only be 1 Maestro stage in an experiment.",
            };
        }

        private static IEnumerable<object[]> GetStagesForDuplicatedConditionsVpoSuffix()
        {
            // ConditionsWithoutDuplicatedConditionsAndNonEmptySuffix_Error
            yield return new object[]
            {
                GenerateSingleStageList(new[] { GenerateBaseConditionDto("6881", 1, "RA"), }),
            };

            // ConditionsWithDuplicatedConditionsAndMoreThan1EmptySuffix_Error
            yield return new object[]
            {
                GenerateSingleStageList(new[]
                {
                    GenerateBaseConditionDto("6881", 1, string.Empty),
                    GenerateBaseConditionDto("6881", 2, "R3"),
                    GenerateBaseConditionDto("6881", 3, string.Empty),
                }),
            };

            // ConditionsWithoutDuplicatedConditionsAndNonEmptySuffixAcrossTwoStages_Error
            yield return new object[]
            {
                GenerateMultipleStagesList(
                    new[] { GenerateBaseConditionDto("1234", 1, null), },
                    new[] { GenerateBaseConditionDto("6881", 2, "RA"), }),
            };

            // ConditionsWithDuplicatedConditionsAndMoreThan1EmptySuffixAcrossTwoStages_Error
            yield return new object[]
            {
                GenerateMultipleStagesList(
                    new[]
                    {
                        GenerateBaseConditionDto("6881", 1, string.Empty),
                        GenerateBaseConditionDto("6881", 2, "R3"),
                    },
                    new[] { GenerateBaseConditionDto("6881", 3, string.Empty), }),
            };

            // ConditionsWithDuplicatedConditionsWithSameSuffix_Error
            yield return new object[]
            {
                GenerateSingleStageList(new[]
                {
                    GenerateBaseConditionDto("6881", 1, string.Empty),
                    GenerateBaseConditionDto("6881", 2, "R3"),
                    GenerateBaseConditionDto("6881", 3, "R3"),
                }),
            };

            // ConditionsWithDuplicatedConditionsWithSameSuffixAcrossTwoStages_Error
            yield return new object[]
            {
                GenerateMultipleStagesList(
                    new[]
                    {
                        GenerateBaseConditionDto("6881", 1, string.Empty),
                        GenerateBaseConditionDto("6881", 2, "R3"),
                    },
                    new[] { GenerateBaseConditionDto("6881", 3, "R3"), }),
            };

            // ConditionsWithDuplicatedConditionsAndNonEmptySuffix_Error
            yield return new object[]
            {
                GenerateSingleStageList(new[]
                {
                    GenerateBaseConditionDto("6881", 1, "DA"),
                    GenerateBaseConditionDto("6881", 2, "DA"),
                }),
            };

            // ConditionsWithDuplicatedConditionsAndNonEmptySuffixAcrossTwoStages_Error
            yield return new object[]
            {
                GenerateMultipleStagesList(
                    new[] { GenerateBaseConditionDto("6881", 1, "DA"), },
                    new[] { GenerateBaseConditionDto("6881", 2, "R3"), }),
            };

            // ConditionsWithMultipleDuplicatedConditions_Error
            yield return new object[]
            {
                GenerateSingleStageList(new[]
                {
                    GenerateBaseConditionDto("6881", 1, string.Empty),
                    GenerateBaseConditionDto("6881", 2, "R3"),
                    GenerateBaseConditionDto("6881", 3, "R3"),
                    GenerateBaseConditionDto("6882", 4, string.Empty),
                    GenerateBaseConditionDto("6882", 5, "R3"),
                    GenerateBaseConditionDto("6882", 6, "R3"),
                }),
            };

            // ConditionsWithMultipleDuplicatedConditionsAcrossTwoStages_Error
            yield return new object[]
            {
                GenerateMultipleStagesList(
                    new[]
                    {
                        GenerateBaseConditionDto("6881", 1, string.Empty),
                        GenerateBaseConditionDto("6881", 2, "R3"),
                        GenerateBaseConditionDto("6882", 3, string.Empty),
                        GenerateBaseConditionDto("6882", 4, "R3"),
                    },
                    new[] { GenerateBaseConditionDto("6881", 5, "R3"), GenerateBaseConditionDto("6882", 6, "R3"), }),
            };
        }

        private static ConditionForCreationDto GenerateBaseConditionDto(string operation, uint sequence, string customVpoSuffix)
        {
            return new ConditionForCreationDto()
            {
                LocationCode = operation,
                SequenceId = sequence,
                VpoCustomSuffix = customVpoSuffix,
                EngineeringId = "--",
                Thermals = new[]
                {
                    new ThermalForCreationDto() {Name = "someName", SequenceId = 1, },
                },
            };
        }

        private static StageForCreationDto GenerateStage(uint sequenceId, IEnumerable<ConditionForCreationDto> conditions)
        {
            return new StageForCreationDto
            {
                StageType = StageType.Class,
                SequenceId = sequenceId,
                StageData = new ClassStageDataForCreationDto
                {
                    Conditions = conditions,
                },
            };
        }

        private static IEnumerable<StageForCreationDto> GenerateSingleStageList(IEnumerable<ConditionForCreationDto> conditions)
        {
            return new List<StageForCreationDto>
            {
                GenerateStage(1, conditions),
            };
        }

        private static IEnumerable<StageForCreationDto> GenerateMultipleStagesList(IEnumerable<ConditionForCreationDto> conditionsFirstStage, IEnumerable<ConditionForCreationDto> conditionsSecondStage)
        {
            return new List<StageForCreationDto>
            {
                GenerateStage(1, conditionsFirstStage),
                GenerateStage(2, conditionsSecondStage),
            };
        }
    }
}
