using FluentValidation.Validators;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Update;
using TTPService.Validators.Update;

namespace TTPService.Tests.Validators.Update
{
    [TestClass]
    public class ExperimentProgressNotifyDtoValidatorFixtureL0
    {
        private static ExperimentStatusUpdateDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new ExperimentStatusUpdateDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_CorrelationId()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.CorrelationId, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentStatusUpdateDto, System.Guid>>()
                .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_ExperimentStatus()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.ExperimentStatus, BaseVerifiersSetComposer.Build()
                .AddPropertyValidatorVerifier<EnumValidator<ExperimentStatusUpdateDto, TTPService.Dtos.Notification.Enums.ExperimentStatus>>()
                .Create());
        }
    }
}
