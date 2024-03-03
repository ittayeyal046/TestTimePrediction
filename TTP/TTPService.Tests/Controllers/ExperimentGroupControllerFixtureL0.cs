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
using TTPService.Dtos.Update;
using TTPService.Enums;
using TTPService.FunctionalExtensions;
using TTPService.Helpers;
using TTPService.Models;

namespace TTPService.Tests.Controllers
{
    [TestClass]
    public class ExperimentGroupControllerFixtureL0
    {
        private ExperimentGroupsController _controller;
        private Mock<ITTPModel> _modelMock;
        private TestDataGenerator _testDataGenerator;

        [TestInitialize]
        public void Init()
        {
            _modelMock = new Mock<ITTPModel>();
            _controller = new ExperimentGroupsController(Mock.Of<ILogger<ExperimentGroupsController>>(), _modelMock.Object);
            _testDataGenerator = new TestDataGenerator();
        }

        [TestMethod]
        public async Task GetExperimentGroups_HappyPath()
        {
            // Arrange
            var username = TestDataGenerationHelper.GenerateUserName();
            var experimentGroups =
                _testDataGenerator.GenerateListOfExperimentGroupDtos(3);
            _modelMock.Setup(m => m.GetExperimentGroups(
                           It.IsAny<string>(),
                           It.IsAny<ExperimentGroupStatus?>(),
                           It.IsAny<DateTime?>(),
                           It.IsAny<DateTime?>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<IEnumerable<string>>()))
                      .Returns(() => Task.FromResult(Result.Ok<IEnumerable<PredictionRecordDto>, ErrorResult>(
                           experimentGroups)));

            // Act
            var actual = await _controller.GetExperimentGroups(username, ExperimentGroupStatus.Completed.ToString(), DateTime.Today, DateTime.UtcNow);

            // Assert
            actual.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should()
                  .Be(experimentGroups);
        }

        [TestMethod]
        public async Task GetExperimentGroups_AllParametersAreOptional_ReturnsOK()
        {
            // Arrange
            var experimentGroups =
                _testDataGenerator.GenerateListOfExperimentGroupDtos(2);
            _modelMock.Setup(m => m.GetExperimentGroups(
                           It.IsAny<string>(),
                           It.IsAny<ExperimentGroupStatus?>(),
                           It.IsAny<DateTime?>(),
                           It.IsAny<DateTime?>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<IEnumerable<string>>()))
                      .Returns(() => Task.FromResult(Result.Ok<IEnumerable<PredictionRecordDto>, ErrorResult>(
                           experimentGroups)));

            // Act
            var actual = await _controller.GetExperimentGroups();

            // Assert
            actual.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should()
                  .Be(experimentGroups);
        }

        [TestMethod]
        public async Task GetExperimentGroups_UserNameCaseAgnostic()
        {
            // Arrange
            var experimentGroups = _testDataGenerator.GenerateListOfExperimentGroupDtos().ToList();
            var lowerCaseUserName = "caseagnostic";
            experimentGroups.First().Username = lowerCaseUserName;

            _modelMock.Setup(m => m.GetExperimentGroups(
                           lowerCaseUserName,
                           It.IsAny<ExperimentGroupStatus?>(),
                           It.IsAny<DateTime?>(),
                           It.IsAny<DateTime?>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<IEnumerable<string>>()))
                      .Returns(() => Task.FromResult(Result.Ok<IEnumerable<PredictionRecordDto>, ErrorResult>(
                           experimentGroups)));

            // Act
            var actual = await _controller.GetExperimentGroups("CaseAGNOSTIC");

            // Assert
            actual.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should()
                  .Be(experimentGroups);
        }

        [TestMethod]
        public async Task GetExperimentGroups_InvalidExperimentGroupStatus_ReturnsValidationError()
        {
            // Arrange
            var username = TestDataGenerationHelper.GenerateUserName();

            // Act
            _controller.ModelState.AddModelError("status", "user entered wrong status");
            var actual =
                await _controller.GetExperimentGroups(username, "wrong status");

            // Assert
            actual.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task GetExperimentGroups_ModelStateInvalid_ReturnsValidationError()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "invalid");

            // Act
            var actual = await _controller.GetExperimentGroups();

            // Assert
            actual.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task GetExperimentGroups_ModelValidationError_ReturnsValidationError()
        {
            // Arrange
            _modelMock.Setup(m => m.GetExperimentGroups(
                           It.IsAny<string>(),
                           It.IsAny<ExperimentGroupStatus?>(),
                           It.IsAny<DateTime?>(),
                           It.IsAny<DateTime?>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<IEnumerable<string>>()))
                      .Returns(() => Task.FromResult(ResultGenerator.ValidationError<IEnumerable<PredictionRecordDto>>(
                                       "model validation error")));

            // Act
            var actual = await _controller.GetExperimentGroups();

            // Assert
            actual.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(422);
        }

        [TestMethod]
        public async Task GetExperimentGroups_RepositoryError_ReturnsServerErrorStatusCode()
        {
            // Arrange
            _modelMock.Setup(m => m.GetExperimentGroups(
                           It.IsAny<string>(),
                           It.IsAny<ExperimentGroupStatus?>(),
                           It.IsAny<DateTime?>(),
                           It.IsAny<DateTime?>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<IEnumerable<string>>()))
                      .Returns(() =>
                           Task.FromResult(ResultGenerator.RepositoryError<IEnumerable<PredictionRecordDto>>()));

            // Act
            var actual = await _controller.GetExperimentGroups();

            // Assert
            actual.Result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(500);
        }

        [TestMethod]
        public async Task GetExperimentGroups_BadRequest_ReturnsBadRequestStatusCode()
        {
            // Arrange
            var username = TestDataGenerationHelper.GenerateUserName();
            _modelMock.Setup(m => m.GetExperimentGroups(
                           It.IsAny<string>(),
                           It.IsAny<ExperimentGroupStatus?>(),
                           It.IsAny<DateTime?>(),
                           It.IsAny<DateTime?>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<string>(),
                           It.IsAny<IEnumerable<string>>()))
                      .Returns(() =>
                           Task.FromResult(ResultGenerator.BadRequestError<IEnumerable<PredictionRecordDto>>("I'm bad!")));

            // Act
            var actual = await _controller.GetExperimentGroups(username);

            // Assert
            actual.Result.Should().BeOfType<BadRequestResult>();
        }

        [TestMethod]
        public async Task GetExperimentGroup_HappyPath()
        {
            // Arrange
            var experimentGroup = _testDataGenerator.GenerateExperimentGroupDto();
            var id = Guid.NewGuid();
            _modelMock.Setup(m => m.GetExperimentGroupById(id))
                      .Returns(() => Task.FromResult(Result.Ok<PredictionRecordDto, ErrorResult>(
                           experimentGroup)));

            // Act
            var actual = await _controller.GetExperimentGroup(id);

            // Assert
            actual.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should()
                  .Be(experimentGroup);
        }

        [TestMethod]
        public async Task GetExperimentGroup_ModelValidationError_ReturnsValidationError()
        {
            // Arrange
            _modelMock.Setup(m => m.GetExperimentGroupById(It.IsAny<Guid>()))
                      .Returns(() => Task.FromResult(ResultGenerator.ValidationError<PredictionRecordDto>(
                                       "model validation error")));

            // Act
            var actual = await _controller.GetExperimentGroup(Guid.NewGuid());

            // Assert
            actual.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(422);
        }

        [TestMethod]
        public async Task GetExperimentGroup_ModelStateInvalid_ReturnsValidationError()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "invalid");

            // Act
            var actual = await _controller.GetExperimentGroup(Guid.NewGuid());

            // Assert
            actual.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task GetExperimentGroup_RepositoryError_ReturnsServerErrorStatusCode()
        {
            // Arrange
            _modelMock.Setup(m => m.GetExperimentGroupById(It.IsAny<Guid>()))
                      .Returns(() =>
                           Task.FromResult(ResultGenerator.RepositoryError<PredictionRecordDto>()));

            // Act
            var actual = await _controller.GetExperimentGroup(Guid.NewGuid());

            // Assert
            actual.Result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(500);
        }

        [TestMethod]
        public async Task GetExperimentGroup_BadRequest_ReturnsBadRequestStatusCode()
        {
            // Arrange
            _modelMock.Setup(m => m.GetExperimentGroupById(It.IsAny<Guid>()))
                      .Returns(() =>
                           Task.FromResult(ResultGenerator.BadRequestError<PredictionRecordDto>("I'm bad!")));

            // Act
            var actual = await _controller.GetExperimentGroup(Guid.NewGuid());

            // Assert
            actual.Result.Should().BeOfType<BadRequestResult>();
        }

        [TestMethod]
        public async Task GetExperimentGroup_NotFound_ReturnsNotFoundStatusCode()
        {
            // Arrange
            _modelMock.Setup(m => m.GetExperimentGroupById(It.IsAny<Guid>()))
                      .Returns(() =>
                           Task.FromResult(ResultGenerator.NotFoundError<PredictionRecordDto>()));

            // Act
            var actual = await _controller.GetExperimentGroup(Guid.NewGuid());

            // Assert
            actual.Result.Should().BeOfType<NotFoundResult>();
        }

        [TestMethod]
        public async Task GetTopCommonLocationCodes_HappyFlow()
        {
            // Arrange
            var result = new List<uint>();

            _modelMock.Setup(m => m.GetTopCommonLocationCodes(It.IsAny<string>(), It.IsAny<uint>()))
                .Returns(() => Task.FromResult(Result.Ok<IEnumerable<uint>, ErrorResult>(result)));

            // Act
            var actual = await _controller.GetTopCommonLocationCodes("programFamily");

            // Assert
            actual.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should()
                .Be(result);
        }

        [TestMethod]
        public async Task GetTopCommonLocationCodes_RepositoryError_ReturnsServerErrorStatusCode()
        {
            // Arrange
            _modelMock.Setup(m => m.GetTopCommonLocationCodes(It.IsAny<string>(), It.IsAny<uint>()))
                .Returns(() => Task.FromResult(ResultGenerator.RepositoryError<IEnumerable<uint>>()));

            // Act
            var actual = await _controller.GetTopCommonLocationCodes("programFamily");

            // Assert
            actual.Result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(500);
        }

        [TestMethod]
        public async Task GetTopCommonLocationCodes_BadRequest_ReturnsBadRequestStatusCode()
        {
            // Arrange
            _modelMock.Setup(m => m.GetTopCommonLocationCodes(It.IsAny<string>(), It.IsAny<uint>()))
                      .Returns(() => Task.FromResult(ResultGenerator.BadRequestError<IEnumerable<uint>>(
                           "bad")));

            // Act
            var actual = await _controller.GetTopCommonLocationCodes("programFamily");

            // Assert
            actual.Result.Should().BeOfType<BadRequestResult>();
        }

        [TestMethod]
        public async Task GetTopCommonLocationCodes_ModelStateInvalid_ReturnsValidationError()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "invalid");

            // Act
            var actual = await _controller.GetTopCommonLocationCodes("programFamily");

            // Assert
            actual.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task GetTopCommonLocationCodes_ModelValidationError_ReturnsValidationError()
        {
            // Arrange
            _modelMock.Setup(m => m.GetTopCommonLocationCodes(It.IsAny<string>(), It.IsAny<uint>()))
                .Returns(() => Task.FromResult(ResultGenerator.ValidationError<IEnumerable<uint>>(
                    "model validation error")));

            // Act
            var actual = await _controller.GetTopCommonLocationCodes("programFamily");

            // Assert
            actual.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(422);
        }

        [TestMethod]
        public async Task GetTopCommonEngineeringIds_HappyFlow()
        {
            // Arrange
            var result = new List<string>();

            _modelMock.Setup(m => m.GetTopCommonEngineeringIds(It.IsAny<string>(), It.IsAny<uint>()))
                .Returns(() => Task.FromResult(Result.Ok<IEnumerable<string>, ErrorResult>(result)));

            // Act
            var actual = await _controller.GetTopCommonEngineeringIds("programFamily");

            // Assert
            actual.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should()
                .Be(result);
        }

        [TestMethod]
        public async Task GetTopCommonEngineeringIds_RepositoryError_ReturnsServerErrorStatusCode()
        {
            // Arrange
            _modelMock.Setup(m => m.GetTopCommonEngineeringIds(It.IsAny<string>(), It.IsAny<uint>()))
                .Returns(() => Task.FromResult(ResultGenerator.RepositoryError<IEnumerable<string>>()));

            // Act
            var actual = await _controller.GetTopCommonEngineeringIds("programFamily");

            // Assert
            actual.Result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(500);
        }

        [TestMethod]
        public async Task GetTopCommonEngineeringIds_BadRequest_ReturnsBadRequestStatusCode()
        {
            // Arrange
            _modelMock.Setup(m => m.GetTopCommonEngineeringIds(It.IsAny<string>(), It.IsAny<uint>()))
                      .Returns(() => Task.FromResult(ResultGenerator.BadRequestError<IEnumerable<string>>(
                           "bad")));

            // Act
            var actual = await _controller.GetTopCommonEngineeringIds("programFamily");

            // Assert
            actual.Result.Should().BeOfType<BadRequestResult>();
        }

        [TestMethod]
        public async Task GetTopCommonEngineeringIds_ModelValidationError_ReturnsValidationError()
        {
            // Arrange
            _modelMock.Setup(m => m.GetTopCommonEngineeringIds(It.IsAny<string>(), It.IsAny<uint>()))
                .Returns(() => Task.FromResult(ResultGenerator.ValidationError<IEnumerable<string>>(
                    "model validation error")));

            // Act
            var actual = await _controller.GetTopCommonEngineeringIds("programFamily");

            // Assert
            actual.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(422);
        }

        [TestMethod]
        public async Task GetTopCommonEngineeringIds_ModelStateInvalid_ReturnsValidationError()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "invalid");

            // Act
            var actual = await _controller.GetTopCommonEngineeringIds("programFamily");

            // Assert
            actual.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_HappyPath()
        {
            // Arrange
            var experimentGroupToCreate = _testDataGenerator.GenerateExperimentGroupForCreationDto();
            var createdExperimentGroup = _testDataGenerator.GenerateExperimentGroupDto();
            _modelMock.Setup(m => m.CreateExperimentGroup(experimentGroupToCreate))
                      .Returns(() => Task.FromResult(Result.Ok<PredictionRecordDto, ErrorResult>(createdExperimentGroup)));

            // Act
            var actual = await _controller.CreateNewExperimentGroup(experimentGroupToCreate);

            // Assert
            var createdAtRouteResult = actual.Result.Should().BeOfType<CreatedAtRouteResult>();
            createdAtRouteResult.Which.Value.Should().Be(createdExperimentGroup);
            createdAtRouteResult.Which.RouteName.Should().Be(nameof(ExperimentGroupsController.GetExperimentGroup));
            createdAtRouteResult.Which.RouteValues.GetValueOrDefault("id").Should().Be(createdExperimentGroup.Id);
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_DtoIsNull_ReturnsBadRequestStatusCode()
        {
            // Arrange

            // Act
            var actual = await _controller.CreateNewExperimentGroup(null);

            // Assert
            actual.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_BadRequest_ReturnsBadRequestStatusCode()
        {
            // Arrange
            var experimentGroupToCreate = _testDataGenerator.GenerateExperimentGroupForCreationDto();
            _modelMock.Setup(m => m.CreateExperimentGroup(experimentGroupToCreate))
                      .Returns(() => Task.FromResult(ResultGenerator.BadRequestError<PredictionRecordDto>("I'm bad!")));

            // Act
            var actual = await _controller.CreateNewExperimentGroup(experimentGroupToCreate);

            // Assert
            actual.Result.Should().BeOfType<BadRequestResult>();
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_ModelValidationError_ReturnsValidationError()
        {
            // Arrange
            var experimentGroupToCreate = _testDataGenerator.GenerateExperimentGroupForCreationDto();
            _modelMock.Setup(m => m.CreateExperimentGroup(experimentGroupToCreate))
                      .Returns(() => Task.FromResult(ResultGenerator.ValidationError<PredictionRecordDto>(
                           "model validation error")));

            // Act
            var actual = await _controller.CreateNewExperimentGroup(experimentGroupToCreate);

            // Assert
            actual.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(422);
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_ModelStateInvalid_ReturnsValidationError()
        {
            // Arrange
            var experimentGroupToCreate = _testDataGenerator.GenerateExperimentGroupForCreationDto();
            _controller.ModelState.AddModelError("Error", "invalid");

            // Act
            var actual = await _controller.CreateNewExperimentGroup(experimentGroupToCreate);

            // Assert
            actual.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task CreateNewExperimentGroup_RepositoryError_ReturnsServerErrorStatusCode()
        {
            // Arrange
            var experimentGroupToCreate = _testDataGenerator.GenerateExperimentGroupForCreationDto();
            _modelMock.Setup(m => m.CreateExperimentGroup(experimentGroupToCreate))
                      .Returns(() => Task.FromResult(ResultGenerator.RepositoryError<PredictionRecordDto>()));

            // Act
            var actual = await _controller.CreateNewExperimentGroup(experimentGroupToCreate);

            // Assert
            actual.Result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(500);
        }

        [TestMethod]
        public async Task UpdateExperimentGroup_HappyPath()
        {
            // Arrange
            var experimentGroupForEdit = _testDataGenerator.GenerateExperimentGroupForEdit();
            var updatedExperimentGroupDto = _testDataGenerator.GenerateExperimentGroupDto();
            _modelMock.Setup(m => m.UpdateExperimentGroup(It.IsAny<Guid>(), experimentGroupForEdit))
                      .Returns(() => Task.FromResult(Result.Ok<PredictionRecordDto, ErrorResult>(updatedExperimentGroupDto)));

            // Act
            var actual = await _controller.UpdateExperimentGroup(default(Guid), experimentGroupForEdit);

            // Assert
            actual.Result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
        }

        [TestMethod]
        public async Task UpdateExperimentGroup_ModelValidationFails_ThrowsValidationFailed()
        {
            // Arrange
            var experimentGroupForEdit = _testDataGenerator.GenerateExperimentGroupForEdit();
            var updatedExperimentGroupDto = _testDataGenerator.GenerateExperimentGroupDto();
            _modelMock.Setup(m => m.UpdateExperimentGroup(It.IsAny<Guid>(), It.IsAny<ExperimentGroupForUpdateDto>()))
                .Returns(() => Task.FromResult(ResultGenerator.ValidationError<PredictionRecordDto>("Error message")));

            // Act
            var actual = await _controller.UpdateExperimentGroup(default(Guid), experimentGroupForEdit);

            // Assert
            actual.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(422);
            actual.Result.Should().BeOfType<ObjectResult>().Which.Value.Should().Be("Error message");
        }

        [TestMethod]
        public async Task UpdateExperimentGroup_RepositoryError_ReturnsServerErrorStatusCode()
        {
            // Arrange
            var experimentGroupToUpdate = _testDataGenerator.GenerateExperimentGroupForEdit();
            _modelMock.Setup(m => m.UpdateExperimentGroup(It.IsAny<Guid>(), experimentGroupToUpdate))
                      .Returns(() => Task.FromResult(ResultGenerator.RepositoryError<PredictionRecordDto>()));

            // Act
            var actual = await _controller.UpdateExperimentGroup(default(Guid), experimentGroupToUpdate);

            // Assert
            actual.Result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(500);
        }
    }
}