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
        private readonly ILogger<TTPController> logger;
        private readonly ITtpModel ttpModel;

        public TTPController(ILogger<TTPController> logger, ITtpModel ttpModel)
        {
            this.logger = logger;
            this.ttpModel = ttpModel;
        }

        /// <summary>
        /// Predict test time.
        /// </summary>
        /// <param name="stplPath">stplPath.</param>
        /// <param name="tplPath">tplPath.</param>
        /// <param name="partType">PartType.</param>
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
        public async Task<ActionResult<double>> PredictTestTimeAsync(
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
            var prediction = await this.ttpModel.PredictAsync(stplPath, tplPath, partType, processStep, parsedStatus);
            return prediction.ToActionResult(this);
        }
    }
}