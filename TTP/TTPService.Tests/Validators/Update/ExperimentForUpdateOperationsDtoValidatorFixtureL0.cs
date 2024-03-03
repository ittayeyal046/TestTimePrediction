using FluentValidation.Validators;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Update;
using TTPService.Validators.Update;

namespace TTPService.Tests.Validators.Update
{
    [TestClass]
    public class ExperimentForUpdateOperationsDtoValidatorFixtureL0
    {
        private static ExperimentForUpdateOperationsDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new ExperimentForUpdateOperationsDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_ExperimentId()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.ExperimentId, BaseVerifiersSetComposer.Build()
                                                                                  .AddPropertyValidatorVerifier<NotEmptyValidator<ExperimentForUpdateOperationsDto, System.Guid>>()
                                                                                  .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_Comment()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.Comment, BaseVerifiersSetComposer.Build()
                                                                             .AddMaximumLengthValidatorVerifier<ExperimentForUpdateOperationsDto>(1000)
                                                                          .Create());
        }
    }
}