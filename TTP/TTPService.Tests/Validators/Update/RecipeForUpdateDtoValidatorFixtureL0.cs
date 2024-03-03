using System.Collections.Generic;
using FluentAssertions;
using FluentValidation.TestHelper;
using FluentValidation.Validators;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Update;
using TTPService.Enums;
using TTPService.Validators.Properties;
using TTPService.Validators.Update;

namespace TTPService.Tests.Validators.Update
{
    [TestClass]
    public class RecipeForUpdateDtoValidatorFixtureL0
    {
        private const string RecipePathMustNotBeEmptyWhenByoGeneratedError = "RecipePath must not be empty when RecipeSource is ByoGenerated";
        private const string RecipePathMustNotBeEmptyWhenByoUseAsIsError = "RecipePath must not be empty when RecipeSource is ByoUseAsIs";
        private const string RecipePathMustBeEmptyWhenTpGeneratedError = "RecipePath must be empty when RecipeSource is TpGenerated";

        private static RecipeForUpdateDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new RecipeForUpdateDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_RecipeSource()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.RecipeSource, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<RecipeForUpdateDto, RecipeSource>>()
                .AddPropertyValidatorVerifier<EnumValidator<RecipeForUpdateDto, RecipeSource>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_RecipePath()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.RecipePath, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<PathValidator<RecipeForUpdateDto, string>>()
                .Create());
        }

        [DynamicData(nameof(GetDtoData), DynamicDataSourceType.Method)]
        [TestMethod]
        public void TestValidate_RecipePath_RecipeSource(RecipeForUpdateDto dto, bool isValid, string expected)
        {
            // Arrange
            // Act
            var actual = _sut.TestValidate(dto);

            // Assert
            if (isValid)
            {
                actual.Errors.Should().BeEmpty();
            }
            else
            {
                actual.Errors.Should().Contain(failure => failure.ErrorMessage == expected);
            }
        }

        private static IEnumerable<object[]> GetDtoData()
        {
            yield return new object[] { new RecipeForUpdateDto() { RecipePath = null, RecipeSource = RecipeSource.TpGenerated }, true, null };
            yield return new object[] { new RecipeForUpdateDto() { RecipePath = null, RecipeSource = RecipeSource.ByoGenerated }, false, RecipePathMustNotBeEmptyWhenByoGeneratedError };
            yield return new object[] { new RecipeForUpdateDto() { RecipePath = null, RecipeSource = RecipeSource.ByoUseAsIs }, false, RecipePathMustNotBeEmptyWhenByoUseAsIsError };
            yield return new object[] { new RecipeForUpdateDto() { RecipePath = string.Empty, RecipeSource = RecipeSource.TpGenerated }, true, null };
            yield return new object[] { new RecipeForUpdateDto() { RecipePath = string.Empty, RecipeSource = RecipeSource.ByoGenerated }, false, RecipePathMustNotBeEmptyWhenByoGeneratedError };
            yield return new object[] { new RecipeForUpdateDto() { RecipePath = string.Empty, RecipeSource = RecipeSource.ByoUseAsIs }, false, RecipePathMustNotBeEmptyWhenByoUseAsIsError };
            yield return new object[] { new RecipeForUpdateDto() { RecipePath = @"\\abc\", RecipeSource = RecipeSource.TpGenerated }, false, RecipePathMustBeEmptyWhenTpGeneratedError };
            yield return new object[] { new RecipeForUpdateDto() { RecipePath = @"\\abc\", RecipeSource = RecipeSource.ByoGenerated }, true, null };
            yield return new object[] { new RecipeForUpdateDto() { RecipePath = @"\\abc\", RecipeSource = RecipeSource.ByoUseAsIs }, true, null };
        }
    }
}