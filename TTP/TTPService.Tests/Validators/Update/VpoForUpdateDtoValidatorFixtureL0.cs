using System;
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
    public class VpoForUpdateDtoValidatorFixtureL0
    {
        private static VpoForUpdateDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new VpoForUpdateDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_ConditionId()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.CorrelationId, BaseVerifiersSetComposer.Build()
                                                                                 .AddPropertyValidatorVerifier<NotEmptyValidator<VpoForUpdateDto, Guid>>()
                                                                                 .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Vpo()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Vpo, BaseVerifiersSetComposer.Build()
                                                                         .AddPropertyValidatorVerifier<NotEmptyValidator<VpoForUpdateDto, string>>()
                                                                         .AddMaximumLengthValidatorVerifier<VpoForUpdateDto>(50)
                                                                         .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_ErrorMessage()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.ErrorMessage, BaseVerifiersSetComposer.Build()
                                                                         .AddPropertyValidatorVerifier<NullValidator<VpoForUpdateDto, string>>()
                                                                         .AddPropertyValidatorVerifier<NotEmptyValidator<VpoForUpdateDto, string>>()
                                                                         .Create());
        }

        [TestMethod]
        public void When_IsFinishedSuccessfully_ThenErrorMessageShouldBeNullAndVpoValid()
        {
            // Arrange
            var input = new VpoForUpdateDto()
            {
                CorrelationId = Guid.NewGuid(),
                Vpo = "vpo",
                IsFinishedSuccessfully = true,
                ErrorMessage = null,
            };

            // Act
            var validationResult = _sut.TestValidate(input);

            // Assert
            validationResult.ShouldNotHaveAnyValidationErrors();
        }

        [TestMethod]
        public void When_IsFinishedSuccessfullyWithInvalidVpo_ShouldFail()
        {
            // Arrange
            var input = new VpoForUpdateDto()
            {
                CorrelationId = Guid.NewGuid(),
                Vpo = null,
                IsFinishedSuccessfully = true,
                ErrorMessage = null,
            };

            // Act
            var validationResult = _sut.TestValidate(input);

            // Assert
            validationResult.Errors.Should().Contain(failure => failure.ErrorMessage.Equals("'Vpo' must not be empty."));
            validationResult.Errors.Should().HaveCount(1);
        }

        [TestMethod]
        public void When_IsNotFinishedSuccessfullyWithInvalidErrorMessage_ShouldFail()
        {
            // Arrange
            var input = new VpoForUpdateDto()
            {
                CorrelationId = Guid.NewGuid(),
                Vpo = null,
                IsFinishedSuccessfully = false,
                ErrorMessage = null,
            };

            // Act
            var validationResult = _sut.TestValidate(input);

            // Assert
            validationResult.Errors.Should().Contain(failure => failure.ErrorMessage.Equals("'Error Message' must not be empty."));
            validationResult.Errors.Should().HaveCount(1);
        }
    }
}