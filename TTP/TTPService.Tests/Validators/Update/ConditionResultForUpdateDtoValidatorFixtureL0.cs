using System.Collections.Generic;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Update;
using TTPService.Validators.Update;

namespace TTPService.Tests.Validators.Update
{
    [TestClass]
    public class ConditionResultForUpdateDtoValidatorFixtureL0
    {
        private static ConditionResultForUpdateDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new ConditionResultForUpdateDtoValidator();
        }

        [DynamicData(nameof(GetDtoData), DynamicDataSourceType.Method)]
        [TestMethod]
        public void TestValidate_NumberOfBadBins_NumberOfGoodBins_AtLeastOne(ConditionResultForUpdateDto dto, bool isValid)
        {
            // Arrange
            var expected = "new result must contain at least 1 good bins or at least 1 bad bins.";

            // Act
            var actual = _sut.TestValidate(dto);

            // Assert
            if (isValid)
            {
                actual.ShouldNotHaveValidationErrorFor(x => x.NumberOfBadBins);
                actual.ShouldNotHaveValidationErrorFor(x => x.NumberOfGoodBins);
            }
            else
            {
                actual.Errors.Should().Contain(failure => failure.ErrorMessage == expected);
            }
        }

        private static IEnumerable<object[]> GetDtoData()
        {
            yield return new object[] { new ConditionResultForUpdateDto() { }, false };
            yield return new object[] { new ConditionResultForUpdateDto() { NumberOfBadBins = 0, NumberOfGoodBins = 0 }, false };
            yield return new object[] { new ConditionResultForUpdateDto() { NumberOfGoodBins = 0 }, false };
            yield return new object[] { new ConditionResultForUpdateDto() { NumberOfBadBins = 0 }, false };
            yield return new object[] { new ConditionResultForUpdateDto() { NumberOfBadBins = 1 }, true };
            yield return new object[] { new ConditionResultForUpdateDto() { NumberOfGoodBins = 3 }, true };
            yield return new object[] { new ConditionResultForUpdateDto() { NumberOfBadBins = 1, NumberOfGoodBins = 0 }, true };
            yield return new object[] { new ConditionResultForUpdateDto() { NumberOfBadBins = 1, NumberOfGoodBins = 0 }, true };
            yield return new object[] { new ConditionResultForUpdateDto() { NumberOfBadBins = 0, NumberOfGoodBins = 2 }, true };
        }
    }
}