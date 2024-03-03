using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TTPService.Controllers;
using TTPService.Dtos;
using TTPService.Dtos.Creation;
using TTPService.Dtos.Update;
using TTPService.FunctionalExtensions;
using TTPService.Helpers;
using TTPService.Models;

namespace TTPService.Tests.Controllers
{
    [TestClass]
    public class ExperimentsControllerFixtureL0
    {
        private ExperimentsController _controller;
        private Mock<IExperimentsModel> _modelMock;
        private TestDataGenerator _testDataGenerator;

        [TestInitialize]
        public void Init()
        {
            _modelMock = new Mock<IExperimentsModel>();
            _controller = new ExperimentsController(Mock.Of<ILogger<ExperimentsController>>(), _modelMock.Object);
            _testDataGenerator = new TestDataGenerator();
        }

        [TestMethod]
        public async Task GetExperimentsForExperimentGroup_HappyPath()
        {
            // Arrange
            var experimentGroup = _testDataGenerator.GenerateExperimentGroupDto(2);
            var experimentIdsToGet = experimentGroup.Experiments.Select(e => e.Id);

            _modelMock.Setup(m => m.GetExperiments(experimentGroup.Id, experimentIdsToGet))
                      .Returns(() => Task.FromResult(Result.Ok<IEnumerable<ExperimentDto>, ErrorResult>(
                                   experimentGroup.Experiments)));

            // Act
            var actual = await _controller.GetExperimentsForExperimentGroup(experimentGroup.Id, experimentIdsToGet);

            // Assert
            actual.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(experimentGroup.Experiments);
        }

        [TestMethod]
        public async Task GetExperimentsForExperimentGroup_NullExperimentIds_ReturnsBadRequest()
        {
            // Arrange
            _modelMock.Setup(m => m.GetExperiments(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()))
                      .Returns(() => Task.FromResult(ResultGenerator.BadRequestError<IEnumerable<ExperimentDto>>("I'm bad!")));

            // Act
            var actual = await _controller.GetExperimentsForExperimentGroup(Guid.NewGuid(), null);

            // Assert
            actual.Result.Should().BeOfType<BadRequestResult>();
        }

        [TestMethod]
        public async Task GetExperimentsForExperimentGroup_BadRequest_ReturnsBadRequest()
        {
            // Arrange
            var experimentGroup = _testDataGenerator.GenerateExperimentGroupDto();
            var experimentIdsToGet = experimentGroup.Experiments.Select(e => e.Id);

            _modelMock.Setup(m => m.GetExperiments(experimentGroup.Id, experimentIdsToGet))
                      .Returns(() => Task.FromResult(ResultGenerator.BadRequestError<IEnumerable<ExperimentDto>>(
                                       "I'm bad!")));

            // Act
            var actual = await _controller.GetExperimentsForExperimentGroup(experimentGroup.Id, experimentIdsToGet);

            // Assert
            actual.Result.Should().BeOfType<BadRequestResult>();
        }

        [TestMethod]
        public async Task GetExperimentsForExperimentGroup_ModelStateInvalid_ReturnsValidationError()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "invalid");

            // Act
            var actual = await _controller.GetExperimentsForExperimentGroup(Guid.NewGuid(), new List<Guid>());

            // Assert
            actual.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task GetExperimentsForExperimentGroup_ModelValidationError_ReturnsValidationError()
        {
            // Arrange
            var experimentGroup = _testDataGenerator.GenerateExperimentGroupDto();
            var experimentIdsToGet = experimentGroup.Experiments.Select(e => e.Id);

            _modelMock.Setup(m => m.GetExperiments(experimentGroup.Id, experimentIdsToGet))
                      .Returns(() => Task.FromResult(ResultGenerator.ValidationError<IEnumerable<ExperimentDto>>(
                           "model validation error")));

            // Act
            var actual = await _controller.GetExperimentsForExperimentGroup(experimentGroup.Id, experimentIdsToGet);

            // Assert
            actual.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(422);
        }

        [TestMethod]
        public async Task GetExperimentsForExperimentGroup_RepositoryError_ReturnsServerErrorStatusCode()
        {
            // Arrange
            var experimentGroup = _testDataGenerator.GenerateExperimentGroupDto();
            var experimentIdsToGet = experimentGroup.Experiments.Select(e => e.Id);

            _modelMock.Setup(m => m.GetExperiments(experimentGroup.Id, experimentIdsToGet))
                      .Returns(() =>
                           Task.FromResult(ResultGenerator.RepositoryError<IEnumerable<ExperimentDto>>()));

            // Act
            var actual = await _controller.GetExperimentsForExperimentGroup(experimentGroup.Id, experimentIdsToGet);

            // Assert
            actual.Result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(500);
        }

        [TestMethod]
        public async Task GetExperimentsForExperimentGroup_NotFound_ReturnsNotFoundStatusCode()
        {
            // Arrange
            var experimentGroup = _testDataGenerator.GenerateExperimentGroupDto();
            var experimentIdsToGet = experimentGroup.Experiments.Select(e => e.Id);

            _modelMock.Setup(m => m.GetExperiments(experimentGroup.Id, experimentIdsToGet))
                      .Returns(() =>
                           Task.FromResult(ResultGenerator.NotFoundError<IEnumerable<ExperimentDto>>()));

            // Act
            var actual = await _controller.GetExperimentsForExperimentGroup(experimentGroup.Id, experimentIdsToGet);

            // Assert
            actual.Result.Should().BeOfType<NotFoundResult>();
        }

        [TestMethod]
        public async Task AddNewExperimentsToExperimentGroup_HappyPath()
        {
            // Arrange
            var experimentsForCreationDtos = _testDataGenerator.GenerateListOfExperimentsForCreationDtos(3);
            var createdExperimentDtos = _testDataGenerator.GenerateListOfExperimentDtos(3);
            var experimentGroupId = Guid.NewGuid();

            _modelMock.Setup(m => m.AddNewExperiments(experimentGroupId, experimentsForCreationDtos))
                      .Returns(() => Task.FromResult(Result.Ok<IEnumerable<ExperimentDto>, ErrorResult>(
                           createdExperimentDtos)));

            // Act
            var actual = await _controller.AddNewExperimentsToExperimentGroup(experimentGroupId, experimentsForCreationDtos);

            // Assert
            var createdAtRouteResult = actual.Result.Should().BeOfType<CreatedAtRouteResult>();
            createdAtRouteResult.Which.Value.Should().Be(createdExperimentDtos);
            createdAtRouteResult.Which.RouteName.Should().Be(nameof(ExperimentsController.GetExperimentsForExperimentGroup));
            createdAtRouteResult.Which.RouteValues.GetValueOrDefault("experimentGroupId").Should().Be(experimentGroupId);
            createdAtRouteResult.Which.RouteValues.GetValueOrDefault("experimentIds").Should().BeEquivalentTo(createdExperimentDtos.Select(e => e.Id));
        }

        [TestMethod]
        public async Task AddNewExperimentsToExperimentGroup_DtosAreEmpty_ReturnsBadRequestStatusCode()
        {
            // Arrange

            // Act
            var actual =
                await _controller.AddNewExperimentsToExperimentGroup(
                    Guid.NewGuid(),
                    new List<ExperimentForCreationDto>());

            // Assert
            actual.Result.Should().BeOfType<BadRequestResult>();
        }

        [TestMethod]
        public async Task AddNewExperimentsToExperimentGroup_DtoIsNull_ReturnsBadRequestStatusCode()
        {
            // Arrange

            // Act
            var actual = await _controller.AddNewExperimentsToExperimentGroup(Guid.NewGuid(), null);

            // Assert
            actual.Result.Should().BeOfType<BadRequestResult>();
        }

        [TestMethod]
        public async Task AddNewExperimentsToExperimentGroup_BadRequest_ReturnsBadRequestStatusCode()
        {
            // Arrange
            var experimentsForCreationDtos = _testDataGenerator.GenerateListOfExperimentsForCreationDtos(3);
            var createdExperimentDtos = _testDataGenerator.GenerateListOfExperimentDtos(3);
            var experimentGroupId = Guid.NewGuid();

            _modelMock.Setup(m => m.AddNewExperiments(experimentGroupId, experimentsForCreationDtos))
                      .Returns(() => Task.FromResult(ResultGenerator.BadRequestError<IEnumerable<ExperimentDto>>(
                                       "I'm bad!")));

            // Act
            var actual = await _controller.AddNewExperimentsToExperimentGroup(experimentGroupId, experimentsForCreationDtos);

            // Assert
            actual.Result.Should().BeOfType<BadRequestResult>();
        }

        [TestMethod]
        public async Task AddNewExperimentsToExperimentGroup_ModelStateInvalid_ReturnsValidationError()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "invalid");

            // Act
            var actual = await _controller.AddNewExperimentsToExperimentGroup(Guid.NewGuid(), new List<ExperimentForCreationDto>());

            // Assert
            actual.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task AddNewExperimentsToExperimentGroup_ModelValidationError_ReturnsValidationError()
        {
            // Arrange
            var experimentsForCreationDtos = _testDataGenerator.GenerateListOfExperimentsForCreationDtos(3);
            var createdExperimentDtos = _testDataGenerator.GenerateListOfExperimentDtos(3);
            var experimentGroupId = Guid.NewGuid();

            _modelMock.Setup(m => m.AddNewExperiments(experimentGroupId, experimentsForCreationDtos))
                      .Returns(() => Task.FromResult(ResultGenerator.ValidationError<IEnumerable<ExperimentDto>>(
                           "model validation error")));

            // Act
            var actual = await _controller.AddNewExperimentsToExperimentGroup(experimentGroupId, experimentsForCreationDtos);

            // Assert
            actual.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(422);
        }

        [TestMethod]
        public async Task AddNewExperimentsToExperimentGroup_RepositoryError_ReturnsServerErrorStatusCode()
        {
            // Arrange
            var experimentsForCreationDtos = _testDataGenerator.GenerateListOfExperimentsForCreationDtos(3);
            var createdExperimentDtos = _testDataGenerator.GenerateListOfExperimentDtos(3);
            var experimentGroupId = Guid.NewGuid();

            _modelMock.Setup(m => m.AddNewExperiments(experimentGroupId, experimentsForCreationDtos))
                      .Returns(() => Task.FromResult(ResultGenerator.RepositoryError<IEnumerable<ExperimentDto>>()));

            // Act
            var actual = await _controller.AddNewExperimentsToExperimentGroup(experimentGroupId, experimentsForCreationDtos);

            // Assert
            actual.Result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(500);
        }

        [TestMethod]
        public async Task AddNewExperimentsToExperimentGroup_NotFound_ReturnsNotFoundStatusCode()
        {
            // Arrange
            var experimentsForCreationDtos = _testDataGenerator.GenerateListOfExperimentsForCreationDtos(3);
            var createdExperimentDtos = _testDataGenerator.GenerateListOfExperimentDtos(3);
            var experimentGroupId = Guid.NewGuid();

            _modelMock.Setup(m => m.AddNewExperiments(experimentGroupId, experimentsForCreationDtos))
                      .Returns(() => Task.FromResult(ResultGenerator.NotFoundError<IEnumerable<ExperimentDto>>()));

            // Act
            var actual = await _controller.AddNewExperimentsToExperimentGroup(experimentGroupId, experimentsForCreationDtos);

            // Assert
            actual.Result.Should().BeOfType<NotFoundResult>();
        }

        [TestMethod]
        public async Task Cancel_HappyPath()
        {
            // Arrange
            _modelMock.Setup(m => m.Cancel(It.IsAny<Guid>(), It.IsAny<ExperimentsForUpdateOperationsDto>()))
                      .Returns(() => Task.FromResult(Result.Ok<VoidResult, ErrorResult>(VoidResult.Instance)));

            // Act
            var actual = await _controller.Cancel(Guid.NewGuid(), new ExperimentsForUpdateOperationsDto() { ExperimentIds = new[] { Guid.NewGuid() }, Comment = "cancel comment" });

            // Assert
            actual.Result.Should().BeOfType<NoContentResult>();
        }

        [TestMethod]
        public async Task Cancel_DtoIsNull_ReturnsBadRequest()
        {
            // Arrange

            // Act
            var actual = await _controller.Cancel(Guid.NewGuid(), null);

            // Assert
            actual.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [TestMethod]
        public async Task Cancel_ModelStateInvalid_ReturnsValidationError()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "invalid");

            // Act
            var actual = await _controller.Cancel(Guid.NewGuid(), new ExperimentsForUpdateOperationsDto() { ExperimentIds = new[] { Guid.Empty }, Comment = "cancel comment" });

            // Assert
            actual.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task Cancel_ModelValidationError_ReturnsValidationError()
        {
            // Arrange
            _modelMock.Setup(m => m.Cancel(It.IsAny<Guid>(), It.IsAny<ExperimentsForUpdateOperationsDto>()))
                      .Returns(() => Task.FromResult(ResultGenerator.ValidationError<VoidResult>(
                           "model validation error")));

            // Act
            var actual = await _controller.Cancel(Guid.NewGuid(), new ExperimentsForUpdateOperationsDto() { ExperimentIds = new[] { Guid.Empty }, Comment = "some comment" });

            // Assert
            actual.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(422);
        }

        [TestMethod]
        public async Task Cancel_ExperimentNotFound_ReturnsNotFoundError()
        {
            // Arrange
            _modelMock.Setup(m => m.Cancel(It.IsAny<Guid>(), It.IsAny<ExperimentsForUpdateOperationsDto>()))
                      .Returns(() => Task.FromResult(ResultGenerator.NotFoundError<VoidResult>()));

            // Act
            var actual = await _controller.Cancel(Guid.NewGuid(), new ExperimentsForUpdateOperationsDto() { ExperimentIds = new[] { Guid.NewGuid() }, Comment = "cancel comment" });

            // Assert
            actual.Result.Should().BeOfType<NotFoundResult>();
        }

        [TestMethod]
        public async Task Cancel_RepositoryError_ReturnsServerErrorStatusCode()
        {
            // Arrange
            _modelMock.Setup(m => m.Cancel(It.IsAny<Guid>(), It.IsAny<ExperimentsForUpdateOperationsDto>()))
                      .Returns(() => Task.FromResult(ResultGenerator.RepositoryError<VoidResult>()));

            // Act
            var actual = await _controller.Cancel(
                Guid.NewGuid(),
                new ExperimentsForUpdateOperationsDto() { ExperimentIds = new[] { Guid.NewGuid() }, Comment = "cancel comment" });

            // Assert
            actual.Result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(500);
        }

        [TestMethod]
        public async Task Delete_HappyPath()
        {
            // Arrange
            _modelMock.Setup(m => m.Delete(It.IsAny<Guid>(), It.IsAny<ExperimentsForUpdateOperationsDto>()))
                      .Returns(() => Task.FromResult(Result.Ok<VoidResult, ErrorResult>(VoidResult.Instance)));

            // Act
            var actual = await _controller.Delete(Guid.NewGuid(), new ExperimentsForUpdateOperationsDto() { ExperimentIds = new[] { Guid.NewGuid() }, Comment = "delete comment" });

            // Assert
            actual.Result.Should().BeOfType<NoContentResult>();
        }

        [TestMethod]
        public async Task Delete_DtoIsNull_ReturnsBadRequest()
        {
            // Arrange

            // Act
            var actual = await _controller.Delete(Guid.NewGuid(), null);

            // Assert
            actual.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [TestMethod]
        public async Task Delete_ModelStateInvalid_ReturnsValidationError()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "invalid");

            // Act
            var actual = await _controller.Delete(Guid.NewGuid(), new ExperimentsForUpdateOperationsDto() { ExperimentIds = new[] { Guid.Empty }, Comment = "delete comment" });

            // Assert
            actual.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task Delete_ModelValidationError_ReturnsValidationError()
        {
            // Arrange
            _modelMock.Setup(m => m.Delete(It.IsAny<Guid>(), It.IsAny<ExperimentsForUpdateOperationsDto>()))
                      .Returns(() => Task.FromResult(ResultGenerator.ValidationError<VoidResult>(
                           "model validation error")));

            // Act
            var actual = await _controller.Delete(Guid.NewGuid(), new ExperimentsForUpdateOperationsDto() { ExperimentIds = new[] { Guid.Empty }, Comment = "some comment" });

            // Assert
            actual.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(422);
        }

        [TestMethod]
        public async Task Delete_ExperimentNotFound_ReturnsNotFoundError()
        {
            // Arrange
            _modelMock.Setup(m => m.Delete(It.IsAny<Guid>(), It.IsAny<ExperimentsForUpdateOperationsDto>()))
                      .Returns(() => Task.FromResult(ResultGenerator.NotFoundError<VoidResult>()));

            // Act
            var actual = await _controller.Delete(Guid.NewGuid(), new ExperimentsForUpdateOperationsDto() { ExperimentIds = new[] { Guid.NewGuid() }, Comment = "delete comment" });

            // Assert
            actual.Result.Should().BeOfType<NotFoundResult>();
        }

        [TestMethod]
        public async Task Delete_RepositoryError_ReturnsServerErrorStatusCode()
        {
            // Arrange
            _modelMock.Setup(m => m.Delete(It.IsAny<Guid>(), It.IsAny<ExperimentsForUpdateOperationsDto>()))
                      .Returns(() => Task.FromResult(ResultGenerator.RepositoryError<VoidResult>()));

            // Act
            var actual = await _controller.Delete(
                Guid.NewGuid(),
                new ExperimentsForUpdateOperationsDto() { ExperimentIds = new[] { Guid.NewGuid() }, Comment = "delete comment" });

            // Assert
            actual.Result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(500);
        }

        [TestMethod]
        public async Task Resume_HappyPath()
        {
            // Arrange
            _modelMock.Setup(m => m.Resume(It.IsAny<Guid>(), It.IsAny<ExperimentForUpdateOperationsDto>()))
                      .Returns(() => Task.FromResult(Result.Ok<VoidResult, ErrorResult>(VoidResult.Instance)));

            // Act
            var actual = await _controller.Resume(
                Guid.NewGuid(),
                new ExperimentForUpdateOperationsDto()
                {
                    ExperimentId = Guid.NewGuid(),
                    Comment = "resume comment",
                });

            // Assert
            actual.Result.Should().BeOfType<NoContentResult>();
        }

        [TestMethod]
        public async Task Resume_DtoIsNull_ReturnsBadRequest()
        {
            // Arrange

            // Act
            var actual = await _controller.Resume(Guid.NewGuid(), null);

            // Assert
            actual.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [TestMethod]
        public async Task Resume_ModelStateInvalid_ReturnsValidationError()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "invalid");

            // Act
            var actual = await _controller.Resume(
                Guid.NewGuid(),
                new ExperimentForUpdateOperationsDto()
                {
                    ExperimentId = Guid.Empty,
                    Comment = "Resume comment",
                });

            // Assert
            actual.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task Resume_ModelValidationError_ReturnsValidationError()
        {
            // Arrange
            _modelMock.Setup(m => m.Resume(It.IsAny<Guid>(), It.IsAny<ExperimentForUpdateOperationsDto>()))
                      .Returns(() => Task.FromResult(ResultGenerator.ValidationError<VoidResult>(
                           "model validation error")));

            // Act
            var actual = await _controller.Resume(
                Guid.NewGuid(),
                new ExperimentForUpdateOperationsDto()
                {
                    ExperimentId = Guid.Empty,
                    Comment = "some comment",
                });

            // Assert
            actual.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(422);
        }

        [TestMethod]
        public async Task Resume_ExperimentNotFound_ReturnsNotFoundError()
        {
            // Arrange
            _modelMock.Setup(m => m.Resume(It.IsAny<Guid>(), It.IsAny<ExperimentForUpdateOperationsDto>()))
                      .Returns(() => Task.FromResult(ResultGenerator.NotFoundError<VoidResult>()));

            // Act
            var actual = await _controller.Resume(
                Guid.NewGuid(),
                new ExperimentForUpdateOperationsDto()
                {
                    ExperimentId = Guid.NewGuid(),
                    Comment = "Resume comment",
                });

            // Assert
            actual.Result.Should().BeOfType<NotFoundResult>();
        }

        [TestMethod]
        public async Task Resume_RepositoryError_ReturnsServerErrorStatusCode()
        {
            // Arrange
            _modelMock.Setup(m => m.Resume(It.IsAny<Guid>(), It.IsAny<ExperimentForUpdateOperationsDto>()))
                      .Returns(() => Task.FromResult(ResultGenerator.RepositoryError<VoidResult>()));

            // Act
            var actual = await _controller.Resume(
                Guid.NewGuid(),
                new ExperimentForUpdateOperationsDto() { ExperimentId = Guid.NewGuid(), Comment = "Resume comment" });

            // Assert
            actual.Result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(500);
        }

        [TestMethod]
        public async Task UpdateExperimentsState_HappyPath()
        {
            // Arrange
            _modelMock.Setup(m => m.UpdateExperimentsState(It.IsAny<Guid>(), It.IsAny<ExperimentsStateForUpdateDto>()))
                      .Returns(() => Task.FromResult(Result.Ok<VoidResult, ErrorResult>(VoidResult.Instance)));

            // Act
            var actual = await _controller.UpdateExperimentsState(Guid.NewGuid(), new ExperimentsStateForUpdateDto() { ExperimentIds = new[] { Guid.NewGuid() }, ExperimentState = Enums.ExperimentState.Draft });

            // Assert
            actual.Result.Should().BeOfType<NoContentResult>();
            _modelMock.Verify(m => m.UpdateExperimentsState(It.IsAny<Guid>(), It.IsAny<ExperimentsStateForUpdateDto>()), Times.Once);
        }

        [TestMethod]
        public async Task UpdateExperimentsState_RepositoryError_ReturnsServerErrorStatusCode()
        {
            // Arrange
            _modelMock.Setup(m => m.UpdateExperimentsState(It.IsAny<Guid>(), It.IsAny<ExperimentsStateForUpdateDto>()))
                      .Returns(() => Task.FromResult(ResultGenerator.RepositoryError<VoidResult>()));

            // Act
            var actual = await _controller.UpdateExperimentsState(Guid.NewGuid(), new ExperimentsStateForUpdateDto() { ExperimentIds = new[] { Guid.NewGuid() }, ExperimentState = Enums.ExperimentState.Draft });

            // Assert
            actual.Result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(500);
            _modelMock.Verify(m => m.UpdateExperimentsState(It.IsAny<Guid>(), It.IsAny<ExperimentsStateForUpdateDto>()), Times.Once);
        }

        [TestMethod]
        public async Task UpdateExperimentsState_ExperimentNotFound_ReturnsNotFoundError()
        {
            // Arrange
            _modelMock.Setup(m => m.UpdateExperimentsState(It.IsAny<Guid>(), It.IsAny<ExperimentsStateForUpdateDto>()))
                      .Returns(() => Task.FromResult(ResultGenerator.NotFoundError<VoidResult>()));

            // Act
            var actual = await _controller.UpdateExperimentsState(Guid.NewGuid(), new ExperimentsStateForUpdateDto() { ExperimentIds = new[] { Guid.NewGuid() }, ExperimentState = Enums.ExperimentState.Draft });

            // Assert
            actual.Result.Should().BeOfType<NotFoundResult>();
            _modelMock.Verify(m => m.UpdateExperimentsState(It.IsAny<Guid>(), It.IsAny<ExperimentsStateForUpdateDto>()), Times.Once);
        }

        [TestMethod]
        public async Task UpdateExperimentsState_ModelValidationError_ReturnsValidationError()
        {
            // Arrange
            _modelMock.Setup(m => m.UpdateExperimentsState(It.IsAny<Guid>(), It.IsAny<ExperimentsStateForUpdateDto>()))
                      .Returns(() => Task.FromResult(ResultGenerator.ValidationError<VoidResult>(
                           "model validation error")));

            // Act
            var actual = await _controller.UpdateExperimentsState(Guid.NewGuid(), new ExperimentsStateForUpdateDto() { ExperimentIds = new[] { Guid.NewGuid() }, ExperimentState = Enums.ExperimentState.Draft });

            // Assert
            actual.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(422);
            _modelMock.Verify(m => m.UpdateExperimentsState(It.IsAny<Guid>(), It.IsAny<ExperimentsStateForUpdateDto>()), Times.Once);
        }

        [TestMethod]
        public async Task UpdateExperimentsState_ModelStateInvalid_ReturnsValidationError()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "invalid");

            // Act
            var actual = await _controller.UpdateExperimentsState(Guid.NewGuid(), new ExperimentsStateForUpdateDto() { ExperimentIds = new[] { Guid.NewGuid() }, ExperimentState = Enums.ExperimentState.Draft });

            // Assert
            actual.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
            _modelMock.Verify(m => m.UpdateExperimentsState(It.IsAny<Guid>(), It.IsAny<ExperimentsStateForUpdateDto>()), Times.Never);
        }
    }
}