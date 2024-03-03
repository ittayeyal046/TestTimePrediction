using FluentValidation.Validators;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Update;
using TTPService.Validators.Update;

namespace TTPService.Tests.Validators.Update
{
    [TestClass]
    public class StatusForUpdateDtoValidatorFixtureL0
    {
        private static StatusForUpdateDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new StatusForUpdateDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_ConditionId()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.CorrelationId, BaseVerifiersSetComposer.Build()
                                                                                 .AddPropertyValidatorVerifier<NotEmptyValidator<StatusForUpdateDto, System.Guid>>()
                                                                                 .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Status()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Status, BaseVerifiersSetComposer.Build()
                                                                            .AddPropertyValidatorVerifier<NotNullValidator<StatusForUpdateDto, Enums.ProcessStatus?>>()
                                                                            .AddPropertyValidatorVerifier<EnumValidator<StatusForUpdateDto, Enums.ProcessStatus?>>()
                                                                            .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Comment()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Comment, BaseVerifiersSetComposer.Build()
                                                                            .AddPropertyValidatorVerifier<MaximumLengthValidator<StatusForUpdateDto>>()
                                                                             .Create());
        }
    }
}