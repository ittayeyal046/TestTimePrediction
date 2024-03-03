using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TTPService.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ExperimentType
    {
        /// <summary>
        /// Represents engineering experiment
        /// </summary>
        Engineering,

        /// <summary>
        /// Represents correlation experiment
        /// </summary>
        Correlation,

        /// <summary>
        /// Represents Walk The Lot experiment
        /// </summary>
        WalkTheLot,
    }
}