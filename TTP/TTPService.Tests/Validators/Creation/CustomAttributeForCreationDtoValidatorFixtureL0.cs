using FluentAssertions;
using FluentValidation.TestHelper;
using FluentValidation.Validators;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Creation;
using TTPService.Validators.Creation;

namespace TTPService.Tests.Validators.Creation
{
    [TestClass]
    public class CustomAttributeForCreationDtoValidatorFixtureL0
    {
        private static CustomAttributeForCreationDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new CustomAttributeForCreationDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_AttributeName()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.AttributeName, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<CustomAttributeForCreationDto, string>>()
                .AddPropertyValidatorVerifier<NotEmptyValidator<CustomAttributeForCreationDto, string>>()
                .AddPropertyValidatorVerifier<PredicateValidator<CustomAttributeForCreationDto, string>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_AttributeValue()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.AttributeValue, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<CustomAttributeForCreationDto, string>>()
                .AddPropertyValidatorVerifier<NotEmptyValidator<CustomAttributeForCreationDto, string>>()
                .AddPropertyValidatorVerifier<PredicateValidator<CustomAttributeForCreationDto, string>>()
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
            var dto = new CustomAttributeForCreationDto() { AttributeName = attrName, AttributeValue = attrValue };

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
