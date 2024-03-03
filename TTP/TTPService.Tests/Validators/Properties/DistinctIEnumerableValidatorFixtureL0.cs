using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Creation;
using TTPService.Validators;

namespace TTPService.Tests.Validators.Properties
{
    [TestClass]
    public class DistinctIEnumerableValidatorFixtureL0
    {
        [DynamicData(nameof(UnitForCreationData), DynamicDataSourceType.Method)]
        [DataTestMethod]
        public void UniqueUnitTest(IEnumerable<UnitForCreationDto> units, bool isValid)
        {
            // Arrange
            var sut = new TestValidator<IEnumerable<UnitForCreationDto>>(v =>
                v.RuleFor(x => x.Value).UniqueUnitsParttype());

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
                v.RuleFor(x => x.Value).UniqueLotsParttype());

            // Act
            var actual =
                sut.Validate(new TestValidatorData<IEnumerable<LotForCreationDto>> { Value = lots });

            // Assert
            actual.IsValid.Should().Be(isValid);
        }

        private static IEnumerable<object[]> UnitForCreationData()
        {
            yield return new object[]
            {
                new[]
                {
                    new UnitForCreationDto {VisualId = "1234", PartType = "AAAAA"},
                    new UnitForCreationDto {VisualId = "2345", PartType = "AAAAA"},
                },
                true,
            };

            yield return new object[]
            {
                new[]
                {
                    new UnitForCreationDto {VisualId = "1234", PartType = "AAAAA"},
                    new UnitForCreationDto {VisualId = "2345", PartType = "BBBBB"},
                },
                false,
            };
        }

        private static IEnumerable<object[]> LotForCreationData()
        {
            yield return new object[]
            {
                new[]
                {
                    new LotForCreationDto {Name = "lot1", PartType = "AAAAA"},
                    new LotForCreationDto {Name = "lot2", PartType = "AAAAA"},
                },
                true,
            };

            yield return new object[]
            {
                new[]
                {
                    new LotForCreationDto {Name = "lot1", PartType = "AAAAA"},
                    new LotForCreationDto {Name = "lot2", PartType = "BBBBB"},
                },
                false,
            };
        }
    }
}
