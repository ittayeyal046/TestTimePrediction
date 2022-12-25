using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trace.Api.Common.TP;
using Trace.Api.Common;
using Trace.Api.Configuration;
using Trace.Api.Services.Common;
using Trace.Api.Services.TestProgramParser;
using Trace.Api.Services.TestProgramParser.Interfaces;
using Trace.Api.Services.TestResults.ItuffIndex;
using Trace.Brita.Api.Interfaces;

namespace TestTimePrediction
{
    internal class TraceParser
    {
        public TestProgram Parse()
        {
            // object containing IDC network drives map
            var driveMapping = ConfigurationLoader.GetDriveMapping(SiteEnum.IDC, SiteDataSourceEnum.CLASSHDMT);

            // gets some valid test program path
            string stplPath, tplPath;

            // get path to stpl and tpl file of latest executed TP
            GetTestProgramPath(driveMapping, out stplPath, out tplPath);

            // now we need an instance to a test program parser
            // first we create the factory and ask it for a parser for the relevant test program type            
            var parserFactory = new TestProgramParserFactory(driveMapping);

            // parse the TP & ask for Plists    
            TestProgram testProgram = parserFactory.ParseTestProgram(stplPath, tplPath, EnumTpParserFlag.PLists);

            return testProgram;
        }

        private static void GetTestProgramPath(IDriveMapping driveMapping, out string stplPath, out string tplPath)
        {
            // prepare index manager to retrieve jobs info
            using (var ituffIndexManager = new ItuffIndexManager(driveMapping))
            {
                // get latest job info
                var ituffDefinition = ituffIndexManager
                    .GetAllItuffDefinitions()
                    //.Where(i => i.Errors.IsNullOrEmpty())
                    .MaxBy(i => i.EndDate);

                // depending on EVG version which was used for that job, it may be null
                stplPath = ituffDefinition.StplPath;
                tplPath = ituffDefinition.TplPath;
            }
        }

    }
}
