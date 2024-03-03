using FluentAssertions;
using FluentValidation.TestHelper;
using FluentValidation.Validators;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Update;
using TTPService.Validators.Update;

namespace TTPService.Tests.Validators.Update
{
    [TestClass]
    public class CustomAttributeForUpdateDtoValidatorFixtureL0
    {
        private static CustomAttributeForUpdateDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new CustomAttributeForUpdateDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_AttributeName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.AttributeName, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<CustomAttributeForUpdateDto, string>>()
                .AddPropertyValidatorVerifier<NotEmptyValidator<CustomAttributeForUpdateDto, string>>()
                .AddPropertyValidatorVerifier<PredicateValidator<CustomAttributeForUpdateDto, string>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_AttributeValue()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.AttributeValue, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<CustomAttributeForUpdateDto, string>>()
                .AddPropertyValidatorVerifier<NotEmptyValidator<CustomAttributeForUpdateDto, string>>()
                .AddPropertyValidatorVerifier<PredicateValidator<CustomAttributeForUpdateDto, string>>()
                .Create());
        }

        [TestMethod]
        [DataRow("Attr", "AttrVal", false)]
        [DataRow("Attr", "", true)]
        [DataRow("", "AttrVal", true)]
        [DataRow("", "", true)]
        [DataRow("", " ", true)]
        [DataRow(" ", "", true)]
        [DataRow(" ", "test", true)]
        public void TestValidate_AttributeNameAndValue_Variations(string attrName, string attrValue, bool errorExpected)
        {
            // Arrange
            var dto = new CustomAttributeForUpdateDto() { AttributeName = attrName, AttributeValue = attrValue };

            // Act
            var actual = _sut.TestValidate(dto);

            // Assert
            if (errorExpected)
            {
                actual.Errors.Should().NotBeEmpty();
            }
            else
            {
                actual.Errors.Should().BeNullOrEmpty();
            }
        }
    }
}
