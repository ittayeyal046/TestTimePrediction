using FluentValidation.Validators;
using FluentValidation.Validators.UnitTestExtension.Composer;
using FluentValidation.Validators.UnitTestExtension.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Update;
using TTPService.Validators.Update;

namespace TTPService.Tests.Validators.Update
{
    [TestClass]
    public class ResultsForUpdateDtoValidatorFixtureL0
    {
        private static ResultsForUpdateDtoValidator _sut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _sut = new ResultsForUpdateDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveRules_ConditionId()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.ConditionId, BaseVerifiersSetComposer.Build()
                                                                                 .AddPropertyValidatorVerifier<NotEmptyValidator<ResultsForUpdateDto, System.Guid>>()
                                                                                 .Create());
        }

        [TestMethod]
        public void ShouldHaveRules_ConditionResultForUpdate()
        {
            // Arrange
            // Act
            // Assert
            _sut.ShouldHaveRules(dto => dto.ConditionResultForUpdate, BaseVerifiersSetComposer.Build()
                                                                                              .AddPropertyValidatorVerifier<NotNullValidator<ResultsForUpdateDto, ConditionResultForUpdateDto>>()
                                                                                              .AddChildValidatorVerifier<ConditionResultForUpdateDtoValidator, ResultsForUpdateDto, ConditionResultForUpdateDto>()
                                                                                              .Create());
        }
    }
}