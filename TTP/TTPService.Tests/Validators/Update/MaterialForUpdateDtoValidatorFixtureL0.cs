using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using FluentValidation.TestHelper;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Update;
using TTPService.Validators.Properties;
using TTPService.Validators.Update;

namespace TTPService.Tests.Validators.Update
{
    [TestClass]
    public class MaterialForUpdateDtoValidatorFixtureL0
    {
        private static MaterialForUpdateDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new MaterialForUpdateDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_Units()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Units, BaseVerifiersSetComposer.Build()
                                                                                .AddPropertyValidatorVerifier<MaximumCollectionLengthValidator<MaterialForUpdateDto, IEnumerable>>()
                                                                                .AddPropertyValidatorVerifier<UniqueIEnumerableValidator<MaterialForUpdateDto, UnitForUpdateDto, string>>()
                                                                                .AddPropertyValidatorVerifier<DistinctIEnumerableValidator<MaterialForUpdateDto, UnitForUpdateDto, string>>()
                                                                                .AddChildValidatorVerifier<UnitForUpdateDtoValidator, MaterialForUpdateDto, UnitForUpdateDto>()
                                                                                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Lots()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Lots, BaseVerifiersSetComposer.Build()
                                                                           .AddPropertyValidatorVerifier<MaximumCollectionLengthValidator<MaterialForUpdateDto, IEnumerable>>()
                                                                           .AddPropertyValidatorVerifier<UniqueIEnumerableValidator<MaterialForUpdateDto, LotForUpdateDto, string>>()
                                                                           .AddPropertyValidatorVerifier<DistinctIEnumerableValidator<MaterialForUpdateDto, LotForUpdateDto, string>>()
                                                                           .AddChildValidatorVerifier<LotForUpdateDtoValidator, MaterialForUpdateDto, LotForUpdateDto>()
                                                                           .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Sspec()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Sspec, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<AlphaNumericValidator<MaterialForUpdateDto, string>>()
                .AddExactLengthValidatorVerifier<MaterialForUpdateDto>(4)
                .Create());
        }

        [DynamicData(nameof(GetDtoData), DynamicDataSourceType.Method)]
        [TestMethod]
        public void TestValidate_Units_Lots_AtLeastOne(MaterialForUpdateDto dto, bool isValid)
        {
            // Arrange
            var expected = "Material must provide at least one Lot or one Unit";

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

        private static IEnumerable<object[]> GetDtoData()
        {
            yield return new object[] { new MaterialForUpdateDto(), false };
            yield return new object[] { new MaterialForUpdateDto() { Units = new List<UnitForUpdateDto>(), Lots = new List<LotForUpdateDto>() }, false };
            yield return new object[] { new MaterialForUpdateDto() { Units = new[] { new UnitForUpdateDto() }, Lots = null }, true };
            yield return new object[] { new MaterialForUpdateDto() { Units = null, Lots = new[] { new LotForUpdateDto() } }, true };
        }
    }
}
