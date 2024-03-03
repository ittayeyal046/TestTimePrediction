using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentValidation.TestHelper;
using FluentValidation.Validators;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Update;
using TTPService.Validators.Properties;
using TTPService.Validators.Update;

namespace TTPService.Tests.Validators.Update
{
    [TestClass]
    public class ConditionsForUpdateDtoValidatorFixtureL0
    {
        private ConditionsForUpdateDtoValidator _sut;

        [TestInitialize]
        public void Init()
        {
            _sut = new ConditionsForUpdateDtoValidator();
        }

        [TestMethod]
        public void ConditionsForUpdateDtoValidator_Conditions()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<MaximumCollectionLengthValidator<IEnumerable<ConditionForUpdateDto>, IEnumerable>>()
                .AddPropertyValidatorVerifier<NotEmptyValidator<IEnumerable<ConditionForUpdateDto>, IEnumerable>>()
                .AddPropertyValidatorVerifier<UniqueIEnumerableValidator<IEnumerable<ConditionForUpdateDto>, ConditionForUpdateDto, uint>>()
                .AddChildValidatorVerifier<ConditionForUpdateDtoValidator, IEnumerable<ConditionForUpdateDto>, ConditionForUpdateDto>()
                .AddPropertyValidatorVerifier<PredicateValidator<IEnumerable<ConditionForUpdateDto>, IEnumerable<ConditionForUpdateDto>>>()
                .AddPropertyValidatorVerifier<PredicateValidator<IEnumerable<ConditionForUpdateDto>, IEnumerable<ConditionForUpdateDto>>>()
                .AddPropertyValidatorVerifier<PredicateValidator<IEnumerable<ConditionForUpdateDto>, IEnumerable<ConditionForUpdateDto>>>()
                .AddPropertyValidatorVerifier<PredicateValidator<IEnumerable<ConditionForUpdateDto>, IEnumerable<ConditionForUpdateDto>>>()
                .Create());
        }

        [TestMethod]
        public void ConditionsForUpdateDto_NumberOfConditionsOverpass_Error()
        {
            // Arrange
            var expectedError1 = " can have max 100 elements";
            var expectedError2 = "Conditions must have unique sequence id.";
            var expectedError3 = "Validation failed for duplicated conditions. Make sure the distribution is: 1 condition with an empty CustomVpoSuffix and the other ones are unique.";
            var expectedError4 = "Duplicated conditions vpo suffix should be 1 or 2 alphanumeric characters and cannot be repeated across same location codes.";
            var input = Enumerable.Repeat(GenerateBaseConditionDto("6881", 1, "RA"), 1000);

            // Act
            var actual = _sut.TestValidate(input);

            // Assert
            actual.Errors.Count.Should().Be(4);
            actual.Errors.Should().Contain(failure => failure.ErrorMessage == expectedError1);
            actual.Errors.Should().Contain(failure => failure.ErrorMessage == expectedError2);
            actual.Errors.Should().Contain(failure => failure.ErrorMessage == expectedError3);
            actual.Errors.Should().Contain(failure => failure.ErrorMessage == expectedError4);
        }

        [TestMethod]
        public void ConditionsWithoutDuplicatedConditionsAndEmptySuffix_NoError()
        {
            // Arrange
            var input = new[]
            {
                GenerateBaseConditionDto("6881", 1, null),
                GenerateBaseConditionDto("1234", 2, null),
                GenerateBaseConditionDto("5678", 3, null),
                GenerateBaseConditionDto("5465", 4, null),
            };

            // Act
            var actual = _sut.TestValidate(input);

            // Assert
            actual.ShouldNotHaveAnyValidationErrors();
        }

        [TestMethod]
        public void ConditionsWithoutDuplicatedConditionsAndNonEmptySuffix_Error()
        {
            // Arrange
            var expected = "VPO suffix should be provided in case of multiple conditions with same operation. It must have exactly one empty value, and the rest unique values in correct format.";
            var input = new[]
            {
                GenerateBaseConditionDto("6881", 1, "RA"),
                GenerateBaseConditionDto("1234", 2, null),
                GenerateBaseConditionDto("5678", 3, string.Empty),
                GenerateBaseConditionDto("5465", 4, null),
            };

            // Act
            var actual = _sut.TestValidate(input);

            // Assert
            actual.Errors.Count.Should().Be(1);
            actual.Errors.Should().Contain(failure => failure.ErrorMessage == expected);
        }

        [TestMethod]
        public void ConditionsWithDuplicatedConditionsAndNonEmptySuffix_NoError()
        {
            // Arrange
            var input = new[]
            {
                GenerateBaseConditionDto("6881", 1, string.Empty),
                GenerateBaseConditionDto("5678", 2, string.Empty),
                GenerateBaseConditionDto("5465", 3, null),
                GenerateBaseConditionDto("6881", 4, "R3"),
            };

            // Act
            var actual = _sut.TestValidate(input);

            // Assert
            actual.ShouldNotHaveAnyValidationErrors();
        }

        [TestMethod]
        public void ConditionsWithDuplicatedConditionsAndNonEmptySuffixLowercase_Error()
        {
            // Arrange
            var expectedError1 = "Duplicated conditions vpo suffix should be 1 or 2 alphanumeric characters and cannot be repeated across same location codes.";
            var input = new[]
            {
                GenerateBaseConditionDto("6881", 1, string.Empty),
                GenerateBaseConditionDto("5678", 2, string.Empty),
                GenerateBaseConditionDto("5465", 3, null),
                GenerateBaseConditionDto("6881", 4, "r3"),
            };

            // Act
            var actual = _sut.TestValidate(input);

            // Assert
            actual.Errors.Count.Should().Be(1);
            actual.Errors.Should().Contain(failure => failure.ErrorMessage == expectedError1);
        }

        [TestMethod]
        public void ConditionsWithDuplicatedConditionsAndMoreThan1EmptySuffix_Error()
        {
            // Arrange
            var expectedError1 = "Validation failed for duplicated conditions. Make sure the distribution is: 1 condition with an empty CustomVpoSuffix and the other ones are unique.";
            var expectedError2 = "Duplicated conditions vpo suffix should be 1 or 2 alphanumeric characters and cannot be repeated across same location codes.";
            var input = new[]
            {
                GenerateBaseConditionDto("6881", 1, string.Empty),
                GenerateBaseConditionDto("6881", 2, "R3"),
                GenerateBaseConditionDto("5678", 3, string.Empty),
                GenerateBaseConditionDto("5465", 4, null),
                GenerateBaseConditionDto("6881", 5, string.Empty),
            };

            // Act
            var actual = _sut.TestValidate(input);

            // Assert
            actual.Errors.Count.Should().Be(2);
            actual.Errors.Should().Contain(failure => failure.ErrorMessage == expectedError1);
            actual.Errors.Should().Contain(failure => failure.ErrorMessage == expectedError2);
        }

        [TestMethod]
        public void ConditionsWithDuplicatedConditionsAndRepeatedNonEmptySuffix_Error()
        {
            // Arrange
            var expectedError1 = "Validation failed for duplicated conditions. Make sure the distribution is: 1 condition with an empty CustomVpoSuffix and the other ones are unique.";
            var expectedError2 = "Duplicated conditions vpo suffix should be 1 or 2 alphanumeric characters and cannot be repeated across same location codes.";
            var input = new[]
            {
                GenerateBaseConditionDto("6881", 1, "R3"),
                GenerateBaseConditionDto("6881", 2, "R3"),
                GenerateBaseConditionDto("5678", 3, string.Empty),
                GenerateBaseConditionDto("5465", 4, null),
            };

            // Act
            var actual = _sut.TestValidate(input);

            // Assert
            actual.Errors.Count.Should().Be(2);
            actual.Errors.Should().Contain(failure => failure.ErrorMessage == expectedError1);
            actual.Errors.Should().Contain(failure => failure.ErrorMessage == expectedError2);
        }

        [TestMethod]
        [DataRow("R$")]
        [DataRow("R3 ")]
        [DataRow("%%")]
        [DataRow("r3")]
        public void ConditionsWithDuplicatedConditionsAndBadFormatSuffix_Error(string suffix)
        {
            // Arrange
            var expected = "Duplicated conditions vpo suffix should be 1 or 2 alphanumeric characters and cannot be repeated across same location codes.";
            var input = new[]
            {
                GenerateBaseConditionDto("6881", 1, suffix),
                GenerateBaseConditionDto("6881", 2, string.Empty),
                GenerateBaseConditionDto("5678", 3, string.Empty),
                GenerateBaseConditionDto("5465", 4, null),
            };

            // Act
            var actual = _sut.TestValidate(input);

            // Assert
            actual.Errors.Count.Should().Be(1);
            actual.Errors.Should().Contain(failure => failure.ErrorMessage == expected);
        }

        [TestMethod]
        public void ConditionsWithDuplicatedConditionsAndNonEmptySuffix_IfNoEmptySuffix_Error()
        {
            // Arrange
            var expected = "Validation failed for duplicated conditions. Make sure the distribution is: 1 condition with an empty CustomVpoSuffix and the other ones are unique.";
            var input = new[]
            {
                GenerateBaseConditionDto("6881", 1, "DA"),
                GenerateBaseConditionDto("6881", 2, "R3"),
                GenerateBaseConditionDto("5678", 3, string.Empty),
                GenerateBaseConditionDto("5465", 4, null),
            };

            // Act
            var actual = _sut.TestValidate(input);

            // Assert
            actual.Errors.Count.Should().Be(1);
            actual.Errors.Should().Contain(failure => failure.ErrorMessage == expected);
        }

        [TestMethod]
        public void ConditionsWithoutDuplicatedConditionsAndEmptySuffix_Error()
        {
            // Arrange
            var expectedError1 = "Validation failed for duplicated conditions. Make sure the distribution is: 1 condition with an empty CustomVpoSuffix and the other ones are unique.";
            var expectedError2 = "Duplicated conditions vpo suffix should be 1 or 2 alphanumeric characters and cannot be repeated across same location codes.";
            var input = new[]
            {
                GenerateBaseConditionDto("6881", 1, string.Empty),
                GenerateBaseConditionDto("6881", 2, string.Empty),
                GenerateBaseConditionDto("5678", 3, string.Empty),
                GenerateBaseConditionDto("5465", 4, null),
            };

            // Act
            var actual = _sut.TestValidate(input);

            // Assert
            actual.Errors.Count.Should().Be(2);
            actual.Errors.Should().Contain(failure => failure.ErrorMessage == expectedError1);
            actual.Errors.Should().Contain(failure => failure.ErrorMessage == expectedError2);
        }

        private ConditionForUpdateDto GenerateBaseConditionDto(string operation, uint sequence, string customVpoSuffix)
        {
            return new ConditionForUpdateDto()
            {
                LocationCode = operation,
                SequenceId = sequence,
                VpoCustomSuffix = customVpoSuffix,
                EngineeringId = "--",
                Thermals = new[]
                {
                    new ThermalForUpdateDto() {Name = "someName", SequenceId = 1, },
                },
            };
        }
    }
}
