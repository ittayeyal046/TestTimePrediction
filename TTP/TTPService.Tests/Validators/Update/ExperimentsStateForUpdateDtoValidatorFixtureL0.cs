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
    public class ExperimentsStateForUpdateDtoValidatorFixtureL0
    {
        private static ExperimentsStateForUpdateDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new ExperimentsStateForUpdateDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_ExperimentIds()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.ExperimentIds, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotNullValidator<ExperimentsStateForUpdateDto, System.Collections.Generic.IEnumerable<Guid>>>()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentsStateForUpdateDto, System.Collections.Generic.IEnumerable<Guid>>>()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentsStateForUpdateDto, Guid>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_ExperimentState()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.ExperimentState, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<EnumValidator<ExperimentsStateForUpdateDto, Enums.ExperimentState>>()
                .Create());
        }

        [TestMethod]
        public void TestValidate_HappyPath()
        {
            // Arrange
            var dto = new ExperimentsStateForUpdateDto()
            {
                ExperimentIds = new[] { Guid.NewGuid() },
                ExperimentState = Enums.ExperimentState.Draft,
            };

            // Act
            var result = _sut.TestValidate(dto);

            // Assert
            result.IsValid.Should().BeTrue();
            result.ShouldNotHaveAnyValidationErrors();
        }

        [TestMethod]
        public void TestValidate_NoExperiments_ValidationTriggered()
        {
            // Arrange
            var dto = new ExperimentsStateForUpdateDto()
            {
                ExperimentIds = new Guid[] { },
                ExperimentState = Enums.ExperimentState.Draft,
            };

            // Act
            var result = _sut.TestValidate(dto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorMessage.Should().Be("'Experiment Ids' must not be empty.");
        }
    }
}
