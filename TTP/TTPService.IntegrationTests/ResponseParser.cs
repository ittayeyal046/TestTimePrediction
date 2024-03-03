using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace TTPService.IntegrationTests
{
    public class ResponseParser
    {
        private Dictionary<string, string[]> _modelStateErrors;

        public ResponseParser(HttpResponseMessage response)
        {
            Response = response;
        }

        public HttpResponseMessage Response { get; }

        public Dictionary<string, string[]> ModelStateErrors
        {
            get
            {
                if (_modelStateErrors == null)
                {
                    var httpErrorObject = Response.Content.ReadAsStringAsync().Result;

                    _modelStateErrors = JsonConvert.DeserializeAnonymousType(httpErrorObject, new Dictionary<string, string[]>());
                }

                return _modelStateErrors;
            }
        }
    }
}