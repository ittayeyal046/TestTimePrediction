using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TTPService.Enums;
using TTPService.FunctionalExtensions;
using TTPService.Helpers;
using TTPService.Models;
using TTPService.Validators.Properties;

namespace TTPService.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/predict")]
    [ApiController]

    public class TTPController : ControllerBase
    {
        private readonly ILogger<TTPController> _logger;
        private readonly ITtpModel _ttpModel;

        public TTPController(ILogger<TTPController> logger, ITtpModel ttpModel)
        {
            _logger = logger;
            _ttpModel = ttpModel;
        }

        /// <summary>
        /// Predict test time.
        /// </summary>
        /// <param name="tpPath">Test program Path.</param>
        /// <param name="partType">PartType.</param>
        /// <param name="bomGroup">BomGroup.</param>
        /// <param name="processStep">ProcessStep.</param>
        /// <param name="experimentType">ExperimentType.</param>
        /// <returns>The estimated test time prediction in milliseconds.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<ActionResult<int>> PredictTestTime(
            [FromQuery, Bind, PathValidator] string stplPath = null,
            [FromQuery, Bind, PathValidator] string tplPath = null,
            [FromQuery, Bind, PartTypeValidator] string partType = null,
            [FromQuery, Bind] string processStep = null,
            [FromQuery, Bind, ExperimentTypeValidator] string experimentType = null)
        {
            if (!ModelState.IsValid)
            {
                return ModelState.CreateValidationError();
            }

            var parsedStatus = (ExperimentType)Enum.Parse(typeof(ExperimentType), experimentType, true);
            var prediction = await _ttpModel.Predict(stplPath, tplPath, partType, processStep, parsedStatus);
            return prediction.ToActionResult(this);
        }
    }
}