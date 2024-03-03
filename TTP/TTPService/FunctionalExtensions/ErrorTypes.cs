namespace TTPService.FunctionalExtensions
{
    public enum ErrorTypes
    {
        /// <summary>
        /// Default: None
        /// </summary>
        None,

        /// <summary>
        /// Not Found
        /// </summary>
        NotFound,

        /// <summary>
        /// Bad Request
        /// </summary>
        BadRequest,

        /// <summary>
        /// Validation Failed
        /// </summary>
        ValidationFailed,

        /// <summary>
        /// Repository Error
        /// </summary>
        RepositoryError,

        /// <summary>
        /// Queue Error
        /// </summary>
        QueueError,

        /// <summary>
        /// External Server Error
        /// </summary>
        ExternalServerError,
    }
}
