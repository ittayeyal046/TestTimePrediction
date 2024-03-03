using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Creation;
using TTPService.Validators;

namespace TTPService.Tests.Validators.Properties
{
    [TestClass]
    public class UniqueStageSequenceIdValidatorFixtureL0
    {
        [DataTestMethod]
        [DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
        public void IsValid(object value, bool expected)
        {
            // Arrange
            var sut = new TestValidator<object>(v => v.RuleFor(x => x.Value).UniqueStageSequenceId());

            // Act
            var actual = sut.Validate(new TestValidatorData<object> { Value = value });

            // Assert
            actual.IsValid.Should().Be(expected);
        }

        private static IEnumerable<object[]> GetData()
        {
            yield return new object[]
            {
                new[] { new StageForCreationDto { SequenceId = 1 }, new StageForCreationDto { SequenceId = 2 } },
                true,
            };
            yield return new object[]
            {
                new[] { new StageForCreationDto { SequenceId = 3 }, new StageForCreationDto { SequenceId = 1 } },
                true,
            };
            yield return new object[]
            {
                new[] { new StageForCreationDto { SequenceId = 1 }, new StageForCreationDto { SequenceId = 1 } },
                false,
            };
            yield return new object[]
            {
                "this is not of type IEnumerable<StageForCreationDto>",
                false,
            };
        }
    }
}
