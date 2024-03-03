using System.Threading.Tasks;
using AutoMapper;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TTPService.Controllers;
using TTPService.FunctionalExtensions;
using TTPService.Helpers;
using TTPService.Models;

namespace TTPService.Tests.Controllers
{
    [TestClass]
    public class CallbacksControllerFixtureL0
    {
        private CallbacksController _controller;
        private Mock<ICallbacksModel> _modelMock;
        private Mock<IMapper> _mapperMock;
        private TestDataGenerator _testDataGenerator;

        [TestInitialize]
        public void Init()
        {
            _modelMock = new Mock<ICallbacksModel>();
            _mapperMock = new Mock<IMapper>();
            _controller = new CallbacksController(_modelMock.Object);
            _testDataGenerator = new TestDataGenerator();
        }

        [TestMethod]
        public async Task UpdateConditionStatus_HappyPath()
        {
            // Arrange
            var statusForUpdateDto = _testDataGenerator.GenerateStatusForUpdateDto();
            _modelMock.Setup(m => m.UpdateConditionOrStageStatus(statusForUpdateDto))
                      .Returns(() => Task.FromResult(Result.Ok<VoidResult, ErrorResult>(VoidResult.Instance)));

            // Act
            var actual = await _controller.UpdateConditionOrStageStatus(statusForUpdateDto);

            // Assert
            var createdAtRouteResult = actual.Result.Should().BeOfType<NoContentResult>();
        }

        [TestMethod]
        public async Task UpdateConditionStatus_DtoIsNull_ReturnsBadRequestStatusCode()
        {
            // Arrange

            // Act
            var actual = await _controller.UpdateConditionOrStageStatus(null);

            // Assert
            actual.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [TestMethod]
        public async Task UpdateConditionStatus_BadRequest_ReturnsBadRequestStatusCode()
        {
            // Arrange
            var statusForUpdateDto = _testDataGenerator.GenerateStatusForUpdateDto();
            _modelMock.Setup(m => m.UpdateConditionOrStageStatus(statusForUpdateDto))
                      .Returns(() => Task.FromResult(ResultGenerator.BadRequestError<VoidResult>("I'm bad!")));

            // Act
            var actual = await _controller.UpdateConditionOrStageStatus(statusForUpdateDto);

            // Assert
            actual.Result.Should().BeOfType<BadRequestResult>();
        }

        [TestMethod]
        public async Task UpdateConditionStatus_ModelStateInvalid_ReturnsValidationError()
        {
            // Arrange
            var statusForUpdateDto = _testDataGenerator.GenerateStatusForUpdateDto();
            _controller.ModelState.AddModelError("Error", "invalid");

            // Act
            var actual = await _controller.UpdateConditionOrStageStatus(statusForUpdateDto);

            // Assert
            actual.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task UpdateConditionStatus_ModelValidationError_ReturnsValidationError()
        {
            // Arrange
            var statusForUpdateDto = _testDataGenerator.GenerateStatusForUpdateDto();
            _modelMock.Setup(m => m.UpdateConditionOrStageStatus(statusForUpdateDto))
                      .Returns(() => Task.FromResult(ResultGenerator.ValidationError<VoidResult>(
                           "model validation error")));

            // Act
            var actual = await _controller.UpdateConditionOrStageStatus(statusForUpdateDto);

            // Assert
            actual.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(422);
        }

        [TestMethod]
        public async Task UpdateConditionStatus_RepositoryError_ReturnsServerErrorStatusCode()
        {
            // Arrange
            var statusForUpdateDto = _testDataGenerator.GenerateStatusForUpdateDto();
            _modelMock.Setup(m => m.UpdateConditionOrStageStatus(statusForUpdateDto))
                      .Returns(() => Task.FromResult(ResultGenerator.RepositoryError<VoidResult>()));

            // Act
            var actual = await _controller.UpdateConditionOrStageStatus(statusForUpdateDto);

            // Assert
            actual.Result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(500);
        }

        [TestMethod]
        public async Task UpdateConditionVpo_HappyPath()
        {
            // Arrange
            var vpoForUpdateDto = _testDataGenerator.GenerateVpoForUpdateDto();
            _modelMock.Setup(m => m.UpdateExperimentVpo(vpoForUpdateDto))
                      .Returns(() => Task.FromResult(Result.Ok<VoidResult, ErrorResult>(VoidResult.Instance)));

            // Act
            var actual = await _controller.UpdateExperimentVpo(vpoForUpdateDto);

            // Assert
            var createdAtRouteResult = actual.Result.Should().BeOfType<NoContentResult>();
        }

        [TestMethod]
        public async Task UpdateConditionVpo_DtoIsNull_ReturnsBadRequestStatusCode()
        {
            // Arrange

            // Act
            var actual = await _controller.UpdateExperimentVpo(null);

            // Assert
            actual.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [TestMethod]
        public async Task UpdateConditionVpo_BadRequest_ReturnsBadRequestStatusCode()
        {
            // Arrange
            var vpoForUpdateDto = _testDataGenerator.GenerateVpoForUpdateDto();
            _modelMock.Setup(m => m.UpdateExperimentVpo(vpoForUpdateDto))
                      .Returns(() => Task.FromResult(ResultGenerator.BadRequestError<VoidResult>("I'm bad!")));

            // Act
            var actual = await _controller.UpdateExperimentVpo(vpoForUpdateDto);

            // Assert
            actual.Result.Should().BeOfType<BadRequestResult>();
        }

        [TestMethod]
        public async Task UpdateConditionVpo_ModelStateInvalid_ReturnsValidationError()
        {
            // Arrange
            var vpoForUpdateDto = _testDataGenerator.GenerateVpoForUpdateDto();
            _controller.ModelState.AddModelError("Error", "invalid");

            // Act
            var actual = await _controller.UpdateExperimentVpo(vpoForUpdateDto);

            // Assert
            actual.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task UpdateConditionVpo_ModelValidationError_ReturnsValidationError()
        {
            // Arrange
            var vpoForUpdateDto = _testDataGenerator.GenerateVpoForUpdateDto();
            _modelMock.Setup(m => m.UpdateExperimentVpo(vpoForUpdateDto))
                      .Returns(() => Task.FromResult(ResultGenerator.ValidationError<VoidResult>(
                           "model validation error")));

            // Act
            var actual = await _controller.UpdateExperimentVpo(vpoForUpdateDto);

            // Assert
            actual.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(422);
        }

        [TestMethod]
        public async Task UpdateConditionVpo_RepositoryError_ReturnsServerErrorStatusCode()
        {
            // Arrange
            var vpoForUpdateDto = _testDataGenerator.GenerateVpoForUpdateDto();
            _modelMock.Setup(m => m.UpdateExperimentVpo(vpoForUpdateDto))
                      .Returns(() => Task.FromResult(ResultGenerator.RepositoryError<VoidResult>()));

            // Act
            var actual = await _controller.UpdateExperimentVpo(vpoForUpdateDto);

            // Assert
            actual.Result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(500);
        }

        [TestMethod]
        public async Task UpdateConditionResult_HappyPath()
        {
            // Arrange
            var resultForUpdateDto = _testDataGenerator.GenerateResultForUpdateDto();
            _modelMock.Setup(m => m.UpdateConditionResult(resultForUpdateDto))
                      .Returns(() => Task.FromResult(Result.Ok<VoidResult, ErrorResult>(VoidResult.Instance)));

            // Act
            var actual = await _controller.UpdateConditionResult(resultForUpdateDto);

            // Assert
            var createdAtRouteResult = actual.Result.Should().BeOfType<NoContentResult>();
        }

        [TestMethod]
        public async Task UpdateConditionResult_DtoIsNull_ReturnsBadRequestStatusCode()
        {
            // Arrange

            // Act
            var actual = await _controller.UpdateConditionResult(null);

            // Assert
            actual.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [TestMethod]
        public async Task UpdateConditionResult_BadRequest_ReturnsBadRequestStatusCode()
        {
            // Arrange
            var resultForUpdateDto = _testDataGenerator.GenerateResultForUpdateDto();
            _modelMock.Setup(m => m.UpdateConditionResult(resultForUpdateDto))
                      .Returns(() => Task.FromResult(ResultGenerator.BadRequestError<VoidResult>("I'm bad!")));

            // Act
            var actual = await _controller.UpdateConditionResult(resultForUpdateDto);

            // Assert
            actual.Result.Should().BeOfType<BadRequestResult>();
        }

        [TestMethod]
        public async Task UpdateConditionResult_ModelStateInvalid_ReturnsValidationError()
        {
            // Arrange
            var resultForUpdateDto = _testDataGenerator.GenerateResultForUpdateDto();
            _controller.ModelState.AddModelError("Error", "invalid");

            // Act
            var actual = await _controller.UpdateConditionResult(resultForUpdateDto);

            // Assert
            actual.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task UpdateConditionResult_ModelValidationError_ReturnsValidationError()
        {
            // Arrange
            var resultForUpdateDto = _testDataGenerator.GenerateResultForUpdateDto();
            _modelMock.Setup(m => m.UpdateConditionResult(resultForUpdateDto))
                      .Returns(() => Task.FromResult(ResultGenerator.ValidationError<VoidResult>(
                           "model validation error")));

            // Act
            var actual = await _controller.UpdateConditionResult(resultForUpdateDto);

            // Assert
            actual.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(422);
        }

        [TestMethod]
        public async Task UpdateConditionResult_RepositoryError_ReturnsServerErrorStatusCode()
        {
            // Arrange
            var resultForUpdateDto = _testDataGenerator.GenerateResultForUpdateDto();
            _modelMock.Setup(m => m.UpdateConditionResult(resultForUpdateDto))
                      .Returns(() => Task.FromResult(ResultGenerator.RepositoryError<VoidResult>()));

            // Act
            var actual = await _controller.UpdateConditionResult(resultForUpdateDto);

            // Assert
            actual.Result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(500);
        }

        [TestMethod]
        public async Task NotifyExperimentProgress_HappyPath()
        {
            // Arrange
            var experimentProgressNotifyDto = _testDataGenerator.GenerateExperimentProgressNotifyDto();
            _modelMock.Setup(m => m.NotifyExperimentProgress(experimentProgressNotifyDto))
              .Returns(() => Task.FromResult(Result.Ok<VoidResult, ErrorResult>(VoidResult.Instance)));

            // Act
            var actual = await _controller.UpdateExperimentStatus(experimentProgressNotifyDto);

            // Assert
            var createdAtRouteResult = actual.Result.Should().BeOfType<NoContentResult>();
        }

        [TestMethod]
        public async Task NotifyExperimentProgress_DtoIsNull_ReturnsBadRequestStatusCode()
        {
            // Arrange

            // Act
            var actual = await _controller.UpdateExperimentStatus(null);

            // Assert
            actual.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [TestMethod]
        public async Task NotifyExperimentProgress_ExternalServerError_ReturnsExternalServerErrorCode()
        {
            // Arrange
            var experimentProgressNotifyDto = _testDataGenerator.GenerateExperimentProgressNotifyDto();
            _modelMock.Setup(m => m.NotifyExperimentProgress(experimentProgressNotifyDto))
                      .Returns(() => Task.FromResult(ResultGenerator.ExternalServerError<VoidResult>()));

            // Act
            var actual = await _controller.UpdateExperimentStatus(experimentProgressNotifyDto);

            // Assert
            actual.Result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(500);
        }

        [TestMethod]
        public async Task NotifyExperimentProgress_ModelStateInvalid_ReturnsValidationError()
        {
            // Arrange
            var experimentProgressNotifyDto = _testDataGenerator.GenerateExperimentProgressNotifyDto();
            _controller.ModelState.AddModelError("Error", "invalid");

            // Act
            var actual = await _controller.UpdateExperimentStatus(experimentProgressNotifyDto);

            // Assert
            actual.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task NotifyExperimentProgress_ModelValidationError_ReturnsValidationError()
        {
            // Arrange
            var experimentProgressNotifyDto = _testDataGenerator.GenerateExperimentProgressNotifyDto();
            _modelMock.Setup(m => m.NotifyExperimentProgress(experimentProgressNotifyDto))
                      .Returns(() => Task.FromResult(ResultGenerator.ValidationError<VoidResult>(
                           "model validation error")));

            // Act
            var actual = await _controller.UpdateExperimentStatus(experimentProgressNotifyDto);

            // Assert
            actual.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(422);
        }

        [TestMethod]
        public async Task NotifyExperimentProgress_RepositoryError_ReturnsServerErrorStatusCode()
        {
            // Arrange
            var experimentProgressNotifyDto = _testDataGenerator.GenerateExperimentProgressNotifyDto();
            _modelMock.Setup(m => m.NotifyExperimentProgress(experimentProgressNotifyDto))
                      .Returns(() => Task.FromResult(ResultGenerator.RepositoryError<VoidResult>()));

            // Act
            var actual = await _controller.UpdateExperimentStatus(experimentProgressNotifyDto);

            // Assert
            actual.Result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(500);
        }
    }
}
