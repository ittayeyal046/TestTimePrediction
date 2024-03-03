using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using FluentValidation.TestHelper;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Creation;
using TTPService.Validators.Creation;
using TTPService.Validators.Properties;

namespace TTPService.Tests.Validators.Creation
{
    [TestClass]
    public class MaterialForCreationDtoValidatorFixtureL0
    {
        private static MaterialForCreationDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new MaterialForCreationDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_Units()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Units, BaseVerifiersSetComposer.Build()
                                                                                .AddPropertyValidatorVerifier<MaximumCollectionLengthValidator<MaterialForCreationDto, IEnumerable>>()
                                                                                .AddPropertyValidatorVerifier<UniqueIEnumerableValidator<MaterialForCreationDto, UnitForCreationDto, string>>()
                                                                                .AddPropertyValidatorVerifier<DistinctIEnumerableValidator<MaterialForCreationDto, UnitForCreationDto, string>>()
                                                                                .AddChildValidatorVerifier<UnitForCreationDtoValidator, MaterialForCreationDto, UnitForCreationDto>()
                                                                                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Lots()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Lots, BaseVerifiersSetComposer.Build()
                                                                           .AddPropertyValidatorVerifier<MaximumCollectionLengthValidator<MaterialForCreationDto, IEnumerable>>()
                                                                           .AddPropertyValidatorVerifier<UniqueIEnumerableValidator<MaterialForCreationDto, LotForCreationDto, string>>()
                                                                           .AddPropertyValidatorVerifier<DistinctIEnumerableValidator<MaterialForCreationDto, LotForCreationDto, string>>()
                                                                           .AddChildValidatorVerifier<LotForCreationDtoValidator, MaterialForCreationDto, LotForCreationDto>()
                                                                           .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Sspec()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Sspec, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<AlphaNumericValidator<MaterialForCreationDto, string>>()
                .AddExactLengthValidatorVerifier<MaterialForCreationDto>(4)
                .Create());
        }

        [DynamicData(nameof(GetDtoData), DynamicDataSourceType.Method)]
        [TestMethod]
        public void TestValidate_Units_Lots_AtLeastOne(MaterialForCreationDto dto, bool isValid)
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
            yield return new object[] { new MaterialForCreationDto(), false };
            yield return new object[] { new MaterialForCreationDto() { Units = new List<UnitForCreationDto>(), Lots = new List<LotForCreationDto>() }, false };
            yield return new object[] { new MaterialForCreationDto() { Units = new[] { new UnitForCreationDto() }, Lots = null }, true };
            yield return new object[] { new MaterialForCreationDto() { Units = null, Lots = new[] { new LotForCreationDto() } }, true };
        }
    }
}
