using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Creation;
using TTPService.Validators;

namespace TTPService.Tests.Validators.Properties
{
    [TestClass]
    public class UniqueIEnumerableValidatorFixtureL0
    {
        [DynamicData(nameof(ConditionForCreationData), DynamicDataSourceType.Method)]
        [DataTestMethod]
        public void UniqueConditionSequenceIdTest(IEnumerable<ConditionForCreationDto> conditions, bool isValid)
        {
            // Arrange
            var sut = new TestValidator<IEnumerable<ConditionForCreationDto>>(v =>
                v.RuleFor(x => x.Value).UniqueConditionSequenceId());

            // Act
            var actual = sut.Validate(new TestValidatorData<IEnumerable<ConditionForCreationDto>> { Value = conditions });

            // Assert
            actual.IsValid.Should().Be(isValid);
        }

        [DynamicData(nameof(ExperimentForCreationData), DynamicDataSourceType.Method)]
        [DataTestMethod]
        public void UniqueExperimentSequenceIdTest(IEnumerable<ExperimentForCreationDto> experiments, bool isValid)
        {
            // Arrange
            var sut = new TestValidator<IEnumerable<ExperimentForCreationDto>>(v =>
                v.RuleFor(x => x.Value).UniqueExperimentSequenceId());

            // Act
            var actual =
                sut.Validate(new TestValidatorData<IEnumerable<ExperimentForCreationDto>> { Value = experiments });

            // Assert
            actual.IsValid.Should().Be(isValid);
        }

        [DynamicData(nameof(ThermalForCreationData), DynamicDataSourceType.Method)]

        [DataTestMethod]
        public void UniqueThermalSequenceIdTest(IEnumerable<ThermalForCreationDto> thermals, bool isValid)
        {
            // Arrange
            var sut = new TestValidator<IEnumerable<ThermalForCreationDto>>(v =>
                v.RuleFor(x => x.Value).UniqueThermalSequenceId());

            // Act
            var actual =
                sut.Validate(new TestValidatorData<IEnumerable<ThermalForCreationDto>> { Value = thermals });

            // Assert
            actual.IsValid.Should().Be(isValid);
        }

        [DynamicData(nameof(UnitForCreationData), DynamicDataSourceType.Method)]
        [DataTestMethod]
        public void UniqueUnitTest(IEnumerable<UnitForCreationDto> units, bool isValid)
        {
            // Arrange
            var sut = new TestValidator<IEnumerable<UnitForCreationDto>>(v =>
                v.RuleFor(x => x.Value).UniqueUnits());

            // Act
            var actual =
                sut.Validate(new TestValidatorData<IEnumerable<UnitForCreationDto>> { Value = units });

            // Assert
            actual.IsValid.Should().Be(isValid);
        }

        [DynamicData(nameof(LotForCreationData), DynamicDataSourceType.Method)]
        [DataTestMethod]
        public void UniqueLotTest(IEnumerable<LotForCreationDto> lots, bool isValid)
        {
            // Arrange
            var sut = new TestValidator<IEnumerable<LotForCreationDto>>(v =>
                v.RuleFor(x => x.Value).UniqueLots());

            // Act
            var actual =
                sut.Validate(new TestValidatorData<IEnumerable<LotForCreationDto>> { Value = lots });

            // Assert
            actual.IsValid.Should().Be(isValid);
        }

        private static IEnumerable<object[]> ExperimentForCreationData()
        {
            yield return new object[]
            {
                new[]
                {
                    new ExperimentForCreationDto {SequenceId = 1234},
                    new ExperimentForCreationDto {SequenceId = 2345},
                    new ExperimentForCreationDto {SequenceId = 1234},
                },
                false,
            };

            yield return new object[]
            {
                new[]
                {
                    new ExperimentForCreationDto {SequenceId = 1234},
                    new ExperimentForCreationDto {SequenceId = 2345},
                },
                true,
            };
        }

        private static IEnumerable<object[]> ConditionForCreationData()
        {
            yield return new object[]
            {
                new[]
                {
                    new ConditionForCreationDto {SequenceId = 1234},
                    new ConditionForCreationDto {SequenceId = 2345},
                    new ConditionForCreationDto {SequenceId = 1234},
                },
                false,
            };

            yield return new object[]
            {
                new[]
                {
                    new ConditionForCreationDto {SequenceId = 1234},
                    new ConditionForCreationDto {SequenceId = 2345},
                },
                true,
            };
        }

        private static IEnumerable<object[]> ThermalForCreationData()
        {
            yield return new object[]
            {
                new[]
                {
                    new ThermalForCreationDto {SequenceId = 1234},
                    new ThermalForCreationDto {SequenceId = 2345},
                    new ThermalForCreationDto {SequenceId = 1234},
                },
                false,
            };

            yield return new object[]
            {
                new[]
                {
                    new ThermalForCreationDto {SequenceId = 1234},
                    new ThermalForCreationDto {SequenceId = 2345},
                },
                true,
            };
        }

        private static IEnumerable<object[]> UnitForCreationData()
        {
            yield return new object[]
            {
                new[]
                {
                    new UnitForCreationDto {VisualId = "1234"},
                    new UnitForCreationDto {VisualId = "2345"},
                    new UnitForCreationDto {VisualId = "1234"},
                },
                false,
            };

            yield return new object[]
            {
                new[]
                {
                    new UnitForCreationDto {VisualId = "1234"},
                    new UnitForCreationDto {VisualId = "2345"},
                },
                true,
            };
        }

        private static IEnumerable<object[]> LotForCreationData()
        {
            yield return new object[]
            {
                new[]
                {
                    new LotForCreationDto {Name = "lot1"},
                    new LotForCreationDto {Name = "lot2"},
                    new LotForCreationDto {Name = "lot1"},
                },
                false,
            };

            yield return new object[]
            {
                new[]
                {
                    new LotForCreationDto {Name = "lot1"}, new LotForCreationDto {Name = "lot2"},
                },
                true,
            };
        }
    }
}