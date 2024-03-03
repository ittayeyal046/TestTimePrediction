using System;
using System.Collections.Generic;
using System.Linq;
using TTPService.Dtos;
using TTPService.Dtos.Creation;
using TTPService.Dtos.Update;
using TTPService.Entities;
using TTPService.Enums;
using TTPService.Tests;

namespace TTPService.IntegrationTests
{
    public class TestDataGenerator
    {
        private static readonly Random Random = new Random();

        public static string ExperimentsIdsFromQueryBuilder(IEnumerable<Guid> experimentIds)
        {
            return string.Join("&", experimentIds.Select(id => $"experimentIds={id}").ToArray());
        }

        public static char RandomChar()
        {
            return (char)Random.Next(char.MinValue, char.MaxValue);
        }

        public Experiment CreateExperiment()
        {
            return new Experiment
            {
                BomGroupName = "BomGroup11",
                EnvironmentFileName = "EnvironmentFileName11",
                TestTimeInSecPerUnit = 100,
                PlistAllFileName = "PlistAllFileName11",
                Step = "Step11",
                TplFileName = "TplFileName11",
                StplFileName = "StplFileName11",
            };
        }

        public ExperimentGroupForCreationDto GenerateExperimentGroupForCreationDto()
        {
            return new ExperimentGroupForCreationDto
            {
                Wwid = 1234,
                Username = "userName1",
                TestProgramData = GenerateTestProgramDataForCreationDto(),
                Experiments = new List<ExperimentForCreationDto> { GenerateExperimentsForCreationDto() },
            };
        }

        public ExperimentGroupForCreationDto GenerateWalkTheLotExperimentGroupForCreationDto()
        {
            return new ExperimentGroupForCreationDto
            {
                Wwid = 1234,
                Username = "username1",
                DisplayName = "Name",
                TestProgramData = GenerateTestProgramDataForCreationDto(),
                Experiments = new List<ExperimentForCreationDto> { GenerateWalkTheLotExperimentsForCreationDto() },
            };
        }

        public ExperimentGroupForCreationDto GenerateWTLMultipleMaestroExperimentGroupForCreationDto()
        {
            return new ExperimentGroupForCreationDto
            {
                Wwid = 1234,
                Username = "userName1",
                TestProgramData = GenerateTestProgramDataForCreationDto(),
                Experiments = new List<ExperimentForCreationDto> { GenerateWTLMultipleMaestroStagesExperimentForCreationDto() },
            };
        }

        public FuseForCreationDto GenerateOneStepFuseForCreationDto()
        {
            return new FuseForCreationDto()
            {
                FuseType = FuseType.OneStep,
                Lrf = new LrfForCreationDto()
                {
                    FuseReleaseName = "DummyRelease",
                    FuseReleaseRevision = 5,
                    BinQdfs = new[]
                    {
                        new BinQdfForCreationDto()
                        {
                            PassBin = 1002,
                            PlanningBin = "AB",
                            Qdf = "DADD",
                        },
                    },
                },
                Qdf = null,
            };
        }

        public FuseForCreationDto GenerateTwoStepLrfFuseForCreationDto()
        {
            return new FuseForCreationDto()
            {
                FuseType = FuseType.TwoStepLrf,
                Lrf = new LrfForCreationDto()
                {
                    FuseReleaseName = "DummyRelease",
                    FuseReleaseRevision = 5,
                    BinQdfs = new[]
                    {
                        new BinQdfForCreationDto()
                        {
                            PassBin = 1002,
                            PlanningBin = "AB",
                            Qdf = "DADD",
                        },
                    },
                },
                Qdf = null,
            };
        }

        public FuseForCreationDto GenerateTwoStepQdfFuseForCreationDto()
        {
            return new FuseForCreationDto()
            {
                FuseType = FuseType.TwoStepQdf,
                Lrf = null,
                Qdf = "DADD",
            };
        }

        public ExperimentGroupForCreationDto GenerateExperimentGroupForCreationDto_EmptyOriginalPartType()
        {
            return new ExperimentGroupForCreationDto
            {
                Wwid = 1234,
                Username = "userName1",
                TestProgramData = GenerateTestProgramDataForCreationDto(),
                Experiments = new List<ExperimentForCreationDto> { GenerateExperimentsForCreationDto_EmptyOriginalPartType() },
            };
        }

        public ExperimentGroupForCreationDto GenerateExperimentGroupForCreationDto_ValidOriginalPartType()
        {
            return new ExperimentGroupForCreationDto
            {
                Wwid = 1234,
                Username = "userName1",
                TestProgramData = GenerateTestProgramDataForCreationDto(),
                Experiments = new List<ExperimentForCreationDto> { GenerateExperimentsForCreationDto_ValidOriginalPartType() },
            };
        }

        public ExperimentForCreationDto GenerateExperimentsForCreationDto()
        {
            return new ExperimentForCreationDto()
            {
                BomGroupName = $"BomGroup{new Random().Next(1, 100)}",
                ExperimentType = ExperimentType.Engineering,
                ActivityType = ActivityType.ModuleValidation,
                Step = $"A{new Random().Next(1, 10)}",
                TplFileName = "TplFileName",
                StplFileName = "StplFileName",
                EnvironmentFileName = "EnvironmentFileName",
                PlistAllFileName = "PlistAllFileName",
                Material = new MaterialForCreationDto
                {
                    Units = new List<UnitForCreationDto>
                    {
                        new UnitForCreationDto
                        {
                            VisualId = "VisualId11",
                            PartType = "HH 6HY789 A J",
                            Lot = "LotName",
                            Lab = "LabName",
                            Location = new LocationForCreationDto
                            {
                                LocationType = "LocationType",
                                LocationName = "LocationName",
                            },
                        },
                    },
                    Lots = new List<LotForCreationDto>(),
                },
                Stages = new List<StageForCreationDto>
                {
                    new StageForCreationDto
                    {
                        StageType = StageType.Class,
                        SequenceId = 1,
                        StageData = new ClassStageDataForCreationDto
                        {
                            Conditions = new List<ConditionForCreationDto>
                            {
                                new ConditionForCreationDto
                                {
                                    Thermals = new List<ThermalForCreationDto> { new ThermalForCreationDto { Name = "HOT", SequenceId = 1} },
                                    EngineeringId = "9A",
                                    LocationCode = "19",
                                    SequenceId = 1,
                                    FuseEnabled = false,
                                    DieSelection = "DummyDieSelection",
                                    MoveUnits = MoveUnits.All,
                                    CustomAttributes = new List<CustomAttributeForCreationDto>
                                    {
                                        new CustomAttributeForCreationDto
                                        {
                                            AttributeName = "Attribute1",
                                            AttributeValue = "Value for Attribute 1",
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
                SequenceId = 1,
                Recipe = new RecipeForCreationDto { RecipeSource = RecipeSource.TpGenerated, RecipePath = string.Empty },
                Tags = new List<string>() { "tag1", "tag2" },
            };
        }

        public ExperimentForCreationDto GenerateWalkTheLotExperimentsForCreationDto()
        {
            return new ExperimentForCreationDto()
            {
                BomGroupName = $"BomGroup{new Random().Next(1, 100)}",
                ExperimentType = ExperimentType.Engineering,
                ActivityType = ActivityType.ModuleValidation,
                DisplayName = "BaseTestProgramName - BomGroup17 A9",
                ContactEmails = new List<string>(),
                Step = $"A{new Random().Next(1, 10)}",
                TplFileName = "TplFileName",
                StplFileName = "StplFileName",
                EnvironmentFileName = "EnvironmentFileName",
                PlistAllFileName = "PlistAllFileName",
                Material = new MaterialForCreationDto
                {
                    Units = new List<UnitForCreationDto>
                    {
                        new UnitForCreationDto
                        {
                            VisualId = "VisualId11",
                            PartType = "HH 6HY789 A J",
                            Lot = "LotName",
                            Lab = "LabName",
                            Location = new LocationForCreationDto
                            {
                                LocationType = "LocationType",
                                LocationName = "LocationName",
                            },
                        },
                    },
                    Lots = new List<LotForCreationDto>(),
                    Sspec = "qdf1",
                },
                Stages = new List<StageForCreationDto>
                {
                    new StageForCreationDto
                    {
                        StageType = StageType.Maestro,
                        SequenceId = 1,
                        StageData = new MaestroStageDataForCreationDto
                        {
                            Operation = "1234",
                            EngId = "AB",
                            TpEnvironment = TpEnvironment.Production,
                        },
                    },
                    new StageForCreationDto
                    {
                        StageType = StageType.Class,
                        SequenceId = 2,
                        StageData = new ClassStageDataForCreationDto
                        {
                            Conditions = new List<ConditionForCreationDto>
                            {
                                new ConditionForCreationDto
                                {
                                    Thermals = new List<ThermalForCreationDto> { new ThermalForCreationDto { Name = "HOT", SequenceId = 1} },
                                    EngineeringId = "9A",
                                    LocationCode = "19",
                                    SequenceId = 1,
                                    FuseEnabled = false,
                                    DieSelection = "DummyDieSelection",
                                    MoveUnits = MoveUnits.All,
                                    CustomAttributes = new List<CustomAttributeForCreationDto>(),
                                },
                            },
                        },
                    },
                    new StageForCreationDto
                    {
                        StageType = StageType.Ppv,
                        SequenceId = 3,
                        StageData = new PpvStageDataForCreationDto
                        {
                            TestType = TestType.Ppv,
                            Operation = "1234",
                            Comment = "captain America and Iron Man are very popular",
                            Qdf = "qdf1",
                            Recipe = "recipe",
                        },
                    },
                    new StageForCreationDto
                    {
                        StageType = StageType.Olb,
                        SequenceId = 4,
                        StageData = new OlbStageDataForCreationDto
                        {
                            MoveUnits = MoveUnits.All,
                            Operation = "1234",
                            Qdf = "qdf1",
                            Recipe = "recipe",
                            BllFiles = new BllFilesForCreationDto
                            {
                                ScriptFile = "\\\\somePath\\bamba.pl",
                                ConstFile = "\\\\somePath\\bisly.xml",
                            },
                        },
                    },
                },
                SequenceId = 1,
                Recipe = new RecipeForCreationDto { RecipeSource = RecipeSource.TpGenerated, RecipePath = string.Empty },
                Tags = new List<string>() { "tag1", "tag2" },
            };
        }

        public ExperimentForCreationDto GenerateWTLMultipleMaestroStagesExperimentForCreationDto()
        {
            return new ExperimentForCreationDto()
            {
                BomGroupName = $"BomGroup{new Random().Next(1, 100)}",
                ExperimentType = ExperimentType.Engineering,
                ActivityType = ActivityType.ModuleValidation,
                Step = $"A{new Random().Next(1, 10)}",
                TplFileName = "TplFileName",
                StplFileName = "StplFileName",
                EnvironmentFileName = "EnvironmentFileName",
                PlistAllFileName = "PlistAllFileName",
                Material = new MaterialForCreationDto
                {
                    Units = new List<UnitForCreationDto>
                    {
                        new UnitForCreationDto
                        {
                            VisualId = "VisualId11",
                            PartType = "HH 6HY789 A J",
                            Lot = "LotName",
                            Lab = "LabName",
                            Location = new LocationForCreationDto
                            {
                                LocationType = "LocationType",
                                LocationName = "LocationName",
                            },
                        },
                    },
                    Lots = new List<LotForCreationDto>(),
                    Sspec = "qdf1",
                },
                Stages = new List<StageForCreationDto>
                {
                    new StageForCreationDto
                    {
                        StageType = StageType.Maestro,
                        SequenceId = 1,
                        StageData = new MaestroStageDataForCreationDto
                        {
                            Operation = "1234",
                            EngId = "AB",
                            TpEnvironment = TpEnvironment.Production,
                        },
                    },
                    new StageForCreationDto
                    {
                        StageType = StageType.Class,
                        SequenceId = 2,
                        StageData = new ClassStageDataForCreationDto
                        {
                            Conditions = new List<ConditionForCreationDto>
                            {
                                new ConditionForCreationDto
                                {
                                    Thermals = new List<ThermalForCreationDto> { new ThermalForCreationDto { Name = "HOT", SequenceId = 1} },
                                    EngineeringId = "9A",
                                    LocationCode = "19",
                                    SequenceId = 1,
                                    FuseEnabled = false,
                                    DieSelection = "DummyDieSelection",
                                    MoveUnits = MoveUnits.All,
                                    CustomAttributes = new List<CustomAttributeForCreationDto>(),
                                },
                            },
                        },
                    },
                    new StageForCreationDto
                    {
                        StageType = StageType.Ppv,
                        SequenceId = 3,
                        StageData = new PpvStageDataForCreationDto
                        {
                            TestType = TestType.Ppv,
                            Operation = "1234",
                            Comment = "captain America and Iron Man are very popular",
                            Qdf = "qdf1",
                            Recipe = "recipe",
                        },
                    },
                    new StageForCreationDto
                    {
                        StageType = StageType.Olb,
                        SequenceId = 4,
                        StageData = new OlbStageDataForCreationDto
                        {
                            MoveUnits = MoveUnits.All,
                            Operation = "1234",
                            Qdf = "qdf1",
                            Recipe = "recipe",
                            BllFiles = new BllFilesForCreationDto
                            {
                                ScriptFile = "\\\\somePath\\bamba.pl",
                                ConstFile = "\\\\somePath\\bisly.xml",
                            },
                        },
                    },
                    new StageForCreationDto
                    {
                        StageType = StageType.Maestro,
                        SequenceId = 5,
                        StageData = new MaestroStageDataForCreationDto
                        {
                            Operation = "1234",
                            EngId = "AB",
                            TpEnvironment = TpEnvironment.Production,
                        },
                    },
                },
                SequenceId = 1,
                Recipe = new RecipeForCreationDto { RecipeSource = RecipeSource.TpGenerated, RecipePath = string.Empty },
                Tags = new List<string>() { "tag1", "tag2" },
            };
        }

        public ExperimentForCreationDto GenerateExperimentsForCreationDto_EmptyOriginalPartType()
        {
            return new ExperimentForCreationDto()
            {
                BomGroupName = $"BomGroup{new Random().Next(1, 100)}",
                ExperimentType = ExperimentType.Engineering,
                ActivityType = ActivityType.ModuleValidation,
                Step = $"A{new Random().Next(1, 10)}",
                TplFileName = "TplFileName",
                StplFileName = "StplFileName",
                EnvironmentFileName = "EnvironmentFileName",
                PlistAllFileName = "PlistAllFileName",
                Material = new MaterialForCreationDto
                {
                    Units = new List<UnitForCreationDto>
                    {
                        new UnitForCreationDto
                        {
                            VisualId = "VisualId11",
                            PartType = "HH 6HY789 A J",
                            OriginalPartType = string.Empty,
                            Lot = "LotName",
                            Lab = "LabName",
                            Location = new LocationForCreationDto
                            {
                                LocationType = "LocationType",
                                LocationName = "LocationName",
                            },
                        },
                    },
                    Lots = new List<LotForCreationDto>(),
                },
                Stages = new List<StageForCreationDto>
                {
                    new StageForCreationDto
                    {
                        StageType = StageType.Class,
                        StageData = new ClassStageDataForCreationDto
                        {
                            Conditions = new List<ConditionForCreationDto>
                            {
                                new ConditionForCreationDto
                                {
                                    Thermals = new List<ThermalForCreationDto> { new ThermalForCreationDto { Name = "HOT", SequenceId = 1} },
                                    EngineeringId = "9A",
                                    LocationCode = "19",
                                    SequenceId = 1,
                                },
                            },
                        },
                        SequenceId = 1,
                    },
                },
                SequenceId = 1,
                Recipe = new RecipeForCreationDto { RecipeSource = RecipeSource.TpGenerated, RecipePath = string.Empty },
            };
        }

        public ExperimentForCreationDto GenerateExperimentsForCreationDto_ValidOriginalPartType()
        {
            return new ExperimentForCreationDto()
            {
                BomGroupName = $"BomGroup{new Random().Next(1, 100)}",
                ExperimentType = ExperimentType.Engineering,
                ActivityType = ActivityType.ModuleValidation,
                Step = $"A{new Random().Next(1, 10)}",
                TplFileName = "TplFileName",
                StplFileName = "StplFileName",
                EnvironmentFileName = "EnvironmentFileName",
                PlistAllFileName = "PlistAllFileName",
                Material = new MaterialForCreationDto
                {
                    Units = new List<UnitForCreationDto>
                    {
                        new UnitForCreationDto
                        {
                            VisualId = "VisualId11",
                            PartType = "HH 6HY789 A J",
                            OriginalPartType = "HH 6HY789 A G",
                            Lot = "LotName",
                            Lab = "LabName",
                            Location = new LocationForCreationDto
                            {
                                LocationType = "LocationType",
                                LocationName = "LocationName",
                            },
                        },
                    },
                    Lots = new List<LotForCreationDto>(),
                },
                Stages = new List<StageForCreationDto>
                {
                    new StageForCreationDto
                    {
                        StageType = StageType.Class,
                        SequenceId = 1,
                        StageData = new ClassStageDataForCreationDto
                        {
                            Conditions = new List<ConditionForCreationDto>
                            {
                                new ConditionForCreationDto
                                {
                                    Thermals = new List<ThermalForCreationDto> { new ThermalForCreationDto { Name = "HOT", SequenceId = 1} },
                                    EngineeringId = "9A",
                                    LocationCode = "19",
                                    SequenceId = 1,
                                },
                            },
                        },
                    },
                },
                SequenceId = 1,
                Recipe = new RecipeForCreationDto { RecipeSource = RecipeSource.TpGenerated, RecipePath = string.Empty },
            };
        }

        public ExperimentGroupForCreationDto GenerateExperimentGroupForCreationDtoWithEmptyActivityType()
        {
            return new ExperimentGroupForCreationDto
            {
                Wwid = 1234,
                Username = "userName1",
                TestProgramData = GenerateTestProgramDataForCreationDto(),
                Experiments = new List<ExperimentForCreationDto> { GenerateExperimentsForCreationDtoWithoutActivityType() },
            };
        }

        public ExperimentForCreationDto GenerateExperimentsForCreationDtoWithoutActivityType()
        {
            return new ExperimentForCreationDto()
            {
                BomGroupName = $"BomGroup{new Random().Next(1, 100)}",
                ExperimentType = ExperimentType.Engineering,
                Step = $"A{new Random().Next(1, 10)}",
                TplFileName = "TplFileName",
                StplFileName = "StplFileName",
                EnvironmentFileName = "EnvironmentFileName",
                PlistAllFileName = "PlistAllFileName",
                Material = new MaterialForCreationDto
                {
                    Units = new List<UnitForCreationDto>
                    {
                        new UnitForCreationDto
                        {
                            VisualId = "VisualId11",
                            PartType = "HH 6HY789 A J",
                            Lot = "LotName",
                            Lab = "LabName",
                            Location = new LocationForCreationDto
                            {
                                LocationType = "LocationType",
                                LocationName = "LocationName",
                            },
                        },
                    },
                    Lots = new List<LotForCreationDto>(),
                },
                Stages = new List<StageForCreationDto>
                {
                    new StageForCreationDto
                    {
                        StageType = StageType.Class,
                        SequenceId = 1,
                        StageData = new ClassStageDataForCreationDto
                        {
                            Conditions = new List<ConditionForCreationDto>
                            {
                                new ConditionForCreationDto
                                {
                                    Thermals = new List<ThermalForCreationDto> { new ThermalForCreationDto { Name = "HOT", SequenceId = 1} },
                                    EngineeringId = "9A",
                                    LocationCode = "19",
                                    SequenceId = 1,
                                },
                            },
                        },
                    },
                },
                SequenceId = 1,
                Recipe = new RecipeForCreationDto
                {
                    RecipePath = null,
                    RecipeSource = RecipeSource.TpGenerated,
                },
            };
        }

        public ExperimentGroup GenerateExperimentForCreationDtoWithMPSCondition()
        {
            var experimentGroup = new ExperimentGroup
            {
                Username = "user1",
                Segment = "test",
                Team = "spark",
                SubmissionTime = DateTime.UtcNow,

                TestProgramData = new TestProgramData
                {
                    BaseTestProgramName = "BaseTestProgramName1",
                    ProgramFamily = "ProgramFamily1",
                    ProgramSubFamily = "ProgramSubFamily1",
                    StplDirectory = "TestProgramUncPath1",
                    TestProgramRootDirectory = "TestProgramRootDirectory",
                },
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        BomGroupName = "BomGroup11",
                        EnvironmentFileName = "EnvironmentFileName11",
                        TestTimeInSecPerUnit = 100,
                        PlistAllFileName = "PlistAllFileName11",
                        Step = "Step11",
                        TplFileName = "TplFileName11",
                        StplFileName = "StplFileName11",
                        Material = new Material
                        {
                            Lots = new List<Lot>(),
                            Units = new List<Unit>
                            {
                                new Unit
                                {
                                    PartType = "Bom111",
                                    Lot = "Name111",
                                    Lab = "Lab1",
                                    VisualId = "VisualId11",
                                    Location = new Location()
                                    {
                                        LocationName = "LocationName",
                                        LocationType = "LocationType",
                                    },
                                },
                            },
                        },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                SequenceId = 1,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId11",
                                            LocationCode = "6262",
                                            Thermals = new List<Thermal>() {CreateConditionThermalData("HOT", 1), CreateConditionThermalData("COLD", 2)},
                                            Status = ProcessStatus.Running,
                                        },
                                    },
                                },
                            },
                        },
                    },
                },

                WasSubmittedToQueue = false,
            };

            return experimentGroup;
        }

        public TestProgramDataForCreationDto GenerateTestProgramDataForCreationDto()
        {
            return new TestProgramDataForCreationDto
            {
                BaseTestProgramName = "BaseTestProgramName",
                ProgramFamily = "ProgramFamily",
                TpName = "TpName",
                ProgramSubFamily = "ProgramSubFamily",
                StplDirectory = @"\\amr.corp.intel.com\ThisIs\StplDirectory",
                DirectoryChecksum = "DirectoryChecksum",
                TestProgramRootDirectory = @"\\amr.corp.intel.com\ThisIs\TestProgramRootDirectory",
            };
        }

        public BomGroupForCreationDto GenerateBomGroupForCreationDto()
        {
            return new BomGroupForCreationDto
            {
                Name = "BomGroupName",
                Boms = new List<BomForCreationDto> { GenerateBomForCreationDto() },
            };
        }

        public BomForCreationDto GenerateBomForCreationDto()
        {
            return new BomForCreationDto
            {
                Name = "BomName",
                Packages = "Packages",
                Device = "Device",
                Step = "A1",
                Rev = "Rev",
            };
        }

        public IEnumerable<ExperimentForCreationDto> GenerateListOfExperimentsForCreationDtos(
            uint numberOfExperiments = 1)
        {
            return TestDataGenerationHelper.GenerateList(GenerateExperimentsForCreationDto, numberOfExperiments);
        }

        public ExperimentGroup CreateExperimentGroupWithCompletedAndRunningExperiment()
        {
            var experimentGroup = new ExperimentGroup
            {
                Username = "user1",
                Segment = "test",
                Team = "spark",
                SubmissionTime = new DateTime(2019, 4, 1, 13, 00, 00).ToUniversalTime(),

                TestProgramData = new TestProgramData
                {
                    BaseTestProgramName = "BaseTestProgramName1",
                    ProgramFamily = "ProgramFamily1",
                    ProgramSubFamily = "ProgramSubFamily1",
                    StplDirectory = "TestProgramUncPath1",
                    TestProgramRootDirectory = "TestProgramRootDirectory",
                },
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        BomGroupName = "BomGroup11",
                        EnvironmentFileName = "EnvironmentFileName11",
                        TestTimeInSecPerUnit = 100,
                        PlistAllFileName = "PlistAllFileName11",
                        Step = "Step11",
                        TplFileName = "TplFileName11",
                        StplFileName = "StplFileName11",
                        Material = new Material
                        {
                            Lots = new List<Lot>(),
                            Units = new List<Unit>
                            {
                                new Unit
                                {
                                    PartType = "Bom111",
                                    Lot = "Name111",
                                    Lab = "Lab1",
                                    VisualId = "VisualId11",
                                    Location = new Location()
                                    {
                                        LocationName = "LocationName",
                                        LocationType = "LocationType",
                                    },
                                },
                                new Unit
                                {
                                    PartType = "Bom111",
                                    Lot = "Name111",
                                    Lab = "Lab1",
                                    VisualId = "VisualId12",
                                    Location = new Location()
                                    {
                                        LocationName = "LocationName1",
                                        LocationType = "LocationType1",
                                    },
                                },
                                new Unit
                                {
                                    PartType = "Bom112",
                                    Lot = "Name112",
                                    Lab = "Lab1",
                                    VisualId = "VisualId121",
                                    Location = new Location()
                                    {
                                        LocationName = "LocationName2",
                                        LocationType = "LocationType2",
                                    },
                                },
                            },
                        },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                SequenceId = 1,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId11",
                                            LocationCode = "6262",
                                            Thermals = new List<Thermal>() {CreateConditionThermalData("Thermal12")},
                                            Status = ProcessStatus.Running,
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId12",
                                            LocationCode = "6262",
                                            Thermals = new List<Thermal>() {CreateConditionThermalData("Thermal12")},
                                            Status = ProcessStatus.Completed,
                                            ModificationTime = new DateTime(2019, 4, 1, 14, 00, 00).ToUniversalTime(),
                                            CompletionTime = new DateTime(2019, 4, 1, 15, 00, 00).ToUniversalTime(),
                                        },
                                    },
                                },
                            },
                            new Stage
                            {
                                StageType = StageType.Olb,
                                SequenceId = 2,
                                StageData = new OLBStageData
                                {
                                    Comment = "Superman and Spiderman are best friends",
                                    MoveUnits = MoveUnits.All,
                                    Operation = "operation",
                                    Qdf = "qdf1",
                                    Recipe = "recipe",
                                    Status = ProcessStatus.Completed,
                                    ModificationTime = new DateTime(2019, 4, 1, 14, 00, 00).ToUniversalTime(),
                                    CompletionTime = new DateTime(2019, 4, 1, 15, 00, 00).ToUniversalTime(),
                                },
                            },
                            new Stage
                            {
                                StageType = StageType.Ppv,
                                SequenceId = 3,
                                StageData = new PPVStageData
                                {
                                    Comment = "captain America and Iron Man are very popular",
                                    Operation = "operation",
                                    Qdf = "qdf1",
                                    Recipe = "recipe",
                                    Status = ProcessStatus.Completed,
                                    ModificationTime = new DateTime(2019, 4, 1, 14, 00, 00).ToUniversalTime(),
                                    CompletionTime = new DateTime(2019, 4, 1, 15, 00, 00).ToUniversalTime(),
                                },
                            },
                            new Stage
                            {
                                StageType = StageType.Maestro,
                                SequenceId = 4,
                                StageData = new MaestroStageData
                                {
                                    Comment = "captain America and Iron Man are very popular",
                                    Operation = "operation",
                                    EngId = "AB",
                                    TpEnvironment = TpEnvironment.Production,
                                    Status = ProcessStatus.Completed,
                                    ModificationTime = new DateTime(2019, 4, 1, 14, 00, 00).ToUniversalTime(),
                                    CompletionTime = new DateTime(2019, 4, 1, 15, 00, 00).ToUniversalTime(),
                                },
                            },
                        },
                        Vpo = "vpo1",
                    },
                    new Experiment
                    {
                        BomGroupName = "BomGroup12",
                        EnvironmentFileName = "EnvironmentFileName12",
                        TestTimeInSecPerUnit = 200,
                        PlistAllFileName = "PlistAllFileName12",
                        Step = "Step12",
                        TplFileName = "TplFileName12",
                        StplFileName = "StplFileName12",
                        Material = new Material
                        {
                            Lots = new List<Lot>(),
                            Units = new List<Unit>
                            {
                                new Unit
                                {
                                    PartType = "Bom211",
                                    Lot = "Name211",
                                    Lab = "Lab1",
                                    VisualId = "VisualId21",
                                    Location = new Location()
                                    {
                                        LocationName = "LocationName1",
                                        LocationType = "LocationType1",
                                    },
                                },
                                new Unit
                                {
                                    PartType = "Bom212",
                                    Lot = "Name212",
                                    Lab = "Lab1",
                                    VisualId = "VisualId221",
                                    Location = new Location()
                                    {
                                        LocationName = "LocationName2",
                                        LocationType = "LocationType2",
                                    },
                                },
                            },
                        },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                SequenceId = 1,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId12",
                                            LocationCode = "6262",
                                            Thermals = new List<Thermal>() {CreateConditionThermalData("Thermal13")},
                                            Status = ProcessStatus.Running,
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId14",
                                            LocationCode = "6261",
                                            Thermals = new List<Thermal>() {CreateConditionThermalData("Thermal14")},
                                            Status = ProcessStatus.Completed,
                                            ModificationTime = new DateTime(2019, 4, 2, 14, 00, 00).ToUniversalTime(),
                                            CompletionTime = new DateTime(2019, 4, 2, 15, 00, 00).ToUniversalTime(),
                                        },
                                    },
                                },
                            },
                            new Stage
                            {
                                StageType = StageType.Olb,
                                SequenceId = 2,
                                StageData = new OLBStageData
                                {
                                    Comment = "Superman and Spiderman are best friends",
                                    MoveUnits = MoveUnits.All,
                                    Operation = "operation",
                                    Qdf = "qdf1",
                                    Recipe = "recipe",
                                    Status = ProcessStatus.Completed,
                                    ModificationTime = new DateTime(2019, 4, 2, 14, 00, 00).ToUniversalTime(),
                                    CompletionTime = new DateTime(2019, 4, 2, 15, 00, 00).ToUniversalTime(),
                                },
                            },
                            new Stage
                            {
                                StageType = StageType.Ppv,
                                SequenceId = 3,
                                StageData = new PPVStageData
                                {
                                    Comment = "captain America and Iron Man are very popular",
                                    Operation = "operation",
                                    Qdf = "qdf1",
                                    Recipe = "recipe",
                                    Status = ProcessStatus.Completed,
                                    ModificationTime = new DateTime(2019, 4, 2, 14, 00, 00).ToUniversalTime(),
                                    CompletionTime = new DateTime(2019, 4, 2, 15, 00, 00).ToUniversalTime(),
                                },
                            },
                            new Stage
                            {
                                StageType = StageType.Maestro,
                                SequenceId = 4,
                                StageData = new MaestroStageData
                                {
                                    Comment = "captain America and Iron Man are very popular",
                                    Operation = "operation",
                                    EngId = "AB",
                                    TpEnvironment = TpEnvironment.Production,
                                    Status = ProcessStatus.Completed,
                                    ModificationTime = new DateTime(2019, 4, 2, 14, 00, 00).ToUniversalTime(),
                                    CompletionTime = new DateTime(2019, 4, 2, 15, 00, 00).ToUniversalTime(),
                                },
                            },
                        },
                    },
                },

                WasSubmittedToQueue = false,
            };

            return experimentGroup;
        }

        public ExperimentGroup CreateExperimentGroupWithCompletedAndRunningExperimentWithoutTestProgramRootDirectory()
        {
            var experimentGroup = new ExperimentGroup
            {
                Username = "user1",
                Segment = "test",
                Team = "spark",
                SubmissionTime = new DateTime(2019, 4, 1, 13, 00, 00).ToUniversalTime(),

                TestProgramData = new TestProgramData
                {
                    BaseTestProgramName = "BaseTestProgramName1",
                    ProgramFamily = "ProgramFamily1",
                    ProgramSubFamily = "ProgramSubFamily1",
                    StplDirectory = "TestProgramUncPath1",
                },
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        BomGroupName = "BomGroup11",
                        EnvironmentFileName = "EnvironmentFileName11",
                        TestTimeInSecPerUnit = 100,
                        PlistAllFileName = "PlistAllFileName11",
                        Step = "Step11",
                        TplFileName = "TplFileName11",
                        StplFileName = "StplFileName11",
                        Material = new Material
                        {
                            Lots = new List<Lot>(),
                            Units = new List<Unit>
                            {
                                new Unit
                                {
                                    PartType = "Bom111",
                                    Lot = "Name111",
                                    Lab = "Lab1",
                                    VisualId = "VisualId11",
                                    Location = new Location()
                                    {
                                        LocationName = "LocationName",
                                        LocationType = "LocationType",
                                    },
                                },
                                new Unit
                                {
                                    PartType = "Bom111",
                                    Lot = "Name111",
                                    Lab = "Lab1",
                                    VisualId = "VisualId12",
                                    Location = new Location()
                                    {
                                        LocationName = "LocationName1",
                                        LocationType = "LocationType1",
                                    },
                                },
                                new Unit
                                {
                                    PartType = "Bom112",
                                    Lot = "Name112",
                                    Lab = "Lab1",
                                    VisualId = "VisualId121",
                                    Location = new Location()
                                    {
                                        LocationName = "LocationName2",
                                        LocationType = "LocationType2",
                                    },
                                },
                            },
                        },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                SequenceId = 1,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId11",
                                            LocationCode = "6262",
                                            Thermals = new List<Thermal>() {CreateConditionThermalData("Thermal12")},
                                            Status = ProcessStatus.Running,
                                            FuseEnabled = true,
                                            DieSelection = "DummyDieSelection",
                                            MoveUnits = MoveUnits.All,
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId12",
                                            LocationCode = "6262",
                                            Thermals = new List<Thermal>() {CreateConditionThermalData("Thermal12")},
                                            Status = ProcessStatus.Completed,
                                            FuseEnabled = true,
                                            DieSelection = "DummyDieSelection",
                                            MoveUnits = MoveUnits.All,
                                            ModificationTime = new DateTime(2019, 4, 1, 14, 00, 00).ToUniversalTime(),
                                            CompletionTime = new DateTime(2019, 4, 1, 15, 00, 00).ToUniversalTime(),
                                        },
                                    },
                                },
                            },
                        },
                        Vpo = "vpo1",
                    },
                    new Experiment
                    {
                        BomGroupName = "BomGroup12",
                        EnvironmentFileName = "EnvironmentFileName12",
                        TestTimeInSecPerUnit = 200,
                        PlistAllFileName = "PlistAllFileName12",
                        Step = "Step12",
                        TplFileName = "TplFileName12",
                        StplFileName = "StplFileName12",
                        Material = new Material
                        {
                            Lots = new List<Lot>(),
                            Units = new List<Unit>
                            {
                                new Unit
                                {
                                    PartType = "Bom211",
                                    Lot = "Name211",
                                    Lab = "Lab1",
                                    VisualId = "VisualId21",
                                    Location = new Location()
                                    {
                                        LocationName = "LocationName1",
                                        LocationType = "LocationType1",
                                    },
                                },
                                new Unit
                                {
                                    PartType = "Bom212",
                                    Lot = "Name212",
                                    Lab = "Lab1",
                                    VisualId = "VisualId221",
                                    Location = new Location()
                                    {
                                        LocationName = "LocationName2",
                                        LocationType = "LocationType2",
                                    },
                                },
                            },
                        },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                SequenceId = 1,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId12",
                                            LocationCode = "6262",
                                            Thermals = new List<Thermal>() {CreateConditionThermalData("Thermal13")},
                                            Status = ProcessStatus.Running,
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId14",
                                            LocationCode = "6261",
                                            Thermals = new List<Thermal>() {CreateConditionThermalData("Thermal14")},
                                            Status = ProcessStatus.Completed,
                                            ModificationTime = new DateTime(2019, 4, 2, 14, 00, 00).ToUniversalTime(),
                                            CompletionTime = new DateTime(2019, 4, 2, 15, 00, 00).ToUniversalTime(),
                                        },
                                    },
                                },
                            },
                        },
                    },
                },

                WasSubmittedToQueue = false,
            };

            return experimentGroup;
        }

        public ExperimentGroup CreateExperimentGroupWithRunningExperiment()
        {
            var experimentGroup = new ExperimentGroup
            {
                Username = "user1",
                Segment = "test",
                Team = "spark",
                SubmissionTime = new DateTime(2019, 4, 1, 13, 00, 00).ToUniversalTime(),

                TestProgramData = new TestProgramData
                {
                    BaseTestProgramName = "BaseTestProgramName1",
                    ProgramFamily = "ProgramFamily1",
                    ProgramSubFamily = "ProgramSubFamily1",
                    StplDirectory = "TestProgramUncPath1",
                    TestProgramRootDirectory = "TestProgramRootDirectory",
                },
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        BomGroupName = "BomGroup11",
                        EnvironmentFileName = "EnvironmentFileName11",
                        TestTimeInSecPerUnit = 100,
                        PlistAllFileName = "PlistAllFileName11",
                        Step = "Step11",
                        TplFileName = "TplFileName11",
                        StplFileName = "StplFileName11",
                        Material = new Material
                        {
                            Lots = new List<Lot>(),
                            Units = new List<Unit>
                            {
                                new Unit
                                {
                                    PartType = "Bom111",
                                    Lot = "Name111",
                                    Lab = "Lab1",
                                    VisualId = "VisualId11",
                                    Location = new Location()
                                    {
                                        LocationName = "LocationName",
                                        LocationType = "LocationType",
                                    },
                                },
                                new Unit
                                {
                                    PartType = "Bom111",
                                    Lot = "Name111",
                                    Lab = "Lab1",
                                    VisualId = "VisualId12",
                                    Location = new Location()
                                    {
                                        LocationName = "LocationName1",
                                        LocationType = "LocationType1",
                                    },
                                },
                                new Unit
                                {
                                    PartType = "Bom112",
                                    Lot = "Name112",
                                    Lab = "Lab1",
                                    VisualId = "VisualId121",
                                    Location = new Location()
                                    {
                                        LocationName = "LocationName2",
                                        LocationType = "LocationType2",
                                    },
                                },
                            },
                        },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                SequenceId = 1,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId11",
                                            LocationCode = "6262",
                                            Thermals = new List<Thermal>() {CreateConditionThermalData("Thermal12")},
                                            Status = ProcessStatus.Running,
                                        },
                                    },
                                },
                            },
                            new Stage
                            {
                                StageType = StageType.Olb,
                                SequenceId = 2,
                                StageData = new OLBStageData
                                {
                                    Comment = "Superman and Spiderman are best friends",
                                    MoveUnits = MoveUnits.All,
                                    Operation = "operation",
                                    Qdf = "qdf1",
                                    Recipe = "recipe",
                                    Status = ProcessStatus.Running,
                                },
                            },
                            new Stage
                            {
                                StageType = StageType.Ppv,
                                SequenceId = 3,
                                StageData = new PPVStageData
                                {
                                    Comment = "captain America and Iron Man are very popular",
                                    Operation = "operation",
                                    Qdf = "qdf1",
                                    Recipe = "recipe",
                                    Status = ProcessStatus.Running,
                                },
                            },
                            new Stage
                            {
                                StageType = StageType.Maestro,
                                SequenceId = 4,
                                StageData = new MaestroStageData
                                {
                                    Comment = "captain America and Iron Man are very popular",
                                    Operation = "operation",
                                    EngId = "AB",
                                    TpEnvironment = TpEnvironment.Production,
                                    Status = ProcessStatus.Running,
                                },
                            },
                        },
                        Vpo = "vpo1",
                    },
                },

                WasSubmittedToQueue = false,
            };

            return experimentGroup;
        }

        public IEnumerable<ExperimentGroup> CreateExperimentGroups()
        {
            var groups = new List<ExperimentGroup>
            {
                CreateExperimentGroupWithCompletedAndRunningExperiment(),
                CreateCompletedExperimentGroup(),
                CreateExperimentGroupsWithCompletedAndCanceledExperiment(),
            };

            return groups;
        }

        public ExperimentGroup CreateExperimentGroupWithOlbStage()
        {
            var experimentGroup = new ExperimentGroup
            {
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        BomGroupName = "BomGroup21",
                        EnvironmentFileName = "EnvironmentFileName21",
                        TestTimeInSecPerUnit = 300,
                        PlistAllFileName = "PlistAllFileName21",
                        Step = "Step21",
                        TplFileName = "TplFileName21",
                        StplFileName = "StplFileName21",
                        Material = new Material { },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Olb,
                                SequenceId = 2,
                                StageData = new OLBStageData()
                                {
                                   Status = ProcessStatus.NotStarted,
                                   Comment = "Superman and Spiderman are best friends",
                                   MoveUnits = MoveUnits.All,
                                   Operation = "operation",
                                   Qdf = "qdf1",
                                   Recipe = "recipe",
                                },
                            },
                        },
                    },
                },

                WasSubmittedToQueue = false,
            };
            return experimentGroup;
        }

        public ExperimentGroup CreateExperimentGroupWithPpvStage()
        {
            var experimentGroup = new ExperimentGroup
            {
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        BomGroupName = "BomGroup21",
                        EnvironmentFileName = "EnvironmentFileName21",
                        TestTimeInSecPerUnit = 300,
                        PlistAllFileName = "PlistAllFileName21",
                        Step = "Step21",
                        TplFileName = "TplFileName21",
                        StplFileName = "StplFileName21",
                        Material = new Material { },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Ppv,
                                SequenceId = 2,
                                StageData = new PPVStageData()
                                {
                                    Status = ProcessStatus.NotStarted,
                                    Comment = "captain America and Iron Man are very popular",
                                    Operation = "operation",
                                    Qdf = "qdf1",
                                    Recipe = "recipe",
                                },
                            },
                        },
                    },
                },

                WasSubmittedToQueue = false,
            };
            return experimentGroup;
        }

        public ExperimentGroup CreateExperimentGroupWithMaestroStage()
        {
            var experimentGroup = new ExperimentGroup
            {
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        BomGroupName = "BomGroup21",
                        EnvironmentFileName = "EnvironmentFileName21",
                        TestTimeInSecPerUnit = 300,
                        PlistAllFileName = "PlistAllFileName21",
                        Step = "Step21",
                        TplFileName = "TplFileName21",
                        StplFileName = "StplFileName21",
                        Material = new Material { },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Maestro,
                                SequenceId = 2,
                                StageData = new MaestroStageData()
                                {
                                    Status = ProcessStatus.NotStarted,
                                    Comment = "captain America and Iron Man are very popular",
                                    Operation = "operation",
                                    EngId = "AB",
                                    TpEnvironment = TpEnvironment.Production,
                                },
                            },
                        },
                    },
                },

                WasSubmittedToQueue = false,
            };
            return experimentGroup;
        }

        public ExperimentGroup CreateCompletedExperimentGroup()
        {
            var experimentGroup = new ExperimentGroup
            {
                Username = "user2",
                Segment = "test2",
                Team = "Cift",
                SubmissionTime = new DateTime(2019, 4, 3, 13, 00, 00).ToUniversalTime(),

                TestProgramData = new TestProgramData
                {
                    BaseTestProgramName = "BaseTestProgramName2",
                    ProgramFamily = "ProgramFamily2",
                    ProgramSubFamily = "ProgramSubFamily2",
                    StplDirectory = "TestProgramUncPath2",
                    TestProgramRootDirectory = "TestProgramRootDirectory",
                },
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        BomGroupName = "BomGroup21",
                        EnvironmentFileName = "EnvironmentFileName21",
                        TestTimeInSecPerUnit = 300,
                        PlistAllFileName = "PlistAllFileName21",
                        Step = "Step21",
                        TplFileName = "TplFileName21",
                        StplFileName = "StplFileName21",
                        Material = new Material
                        {
                            Lots = new List<Lot>(),
                            Units = new List<Unit>
                            {
                                new Unit
                                {
                                    PartType = "Bom221",
                                    Lot = "Name221",
                                    Lab = "Lab2",
                                    VisualId = "VisualId22",
                                    Location = new Location()
                                    {
                                        LocationName = "LocationName2",
                                        LocationType = "LocationType2",
                                    },
                                },
                            },
                        },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                SequenceId = 1,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId14",
                                            LocationCode = "6881",
                                            Thermals = new List<Thermal>() {CreateConditionThermalData("Thermal15")},
                                            Status = ProcessStatus.Completed,
                                            ModificationTime = new DateTime(2019, 4, 3, 14, 00, 00).ToUniversalTime(),
                                            CompletionTime = new DateTime(2019, 4, 3, 15, 00, 00).ToUniversalTime(),
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId12",
                                            LocationCode = "7712",
                                            Thermals = new List<Thermal>() {CreateConditionThermalData("Thermal16")},
                                            Status = ProcessStatus.Completed,
                                            ModificationTime = new DateTime(2019, 4, 4, 14, 00, 00).ToUniversalTime(),
                                            CompletionTime = new DateTime(2019, 4, 4, 15, 00, 00).ToUniversalTime(),
                                        },
                                    },
                                },
                            },
                        },
                    },
                },

                WasSubmittedToQueue = false,
            };
            return experimentGroup;
        }

        public ExperimentGroup CreateExperimentGroupsWithCompletedAndCanceledExperiment()
        {
            var experimentGroup = new ExperimentGroup
            {
                Username = "batel",
                SubmissionTime = new DateTime(2019, 4, 3, 13, 00, 00).ToUniversalTime(),

                TestProgramData = new TestProgramData
                {
                    BaseTestProgramName = "BaseTestProgramName2",
                    ProgramFamily = "ProgramFamily2",
                    ProgramSubFamily = "ProgramSubFamily2",
                    StplDirectory = "TestProgramUncPath2",
                    TestProgramRootDirectory = "TestProgramRootDirectory",
                },
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        BomGroupName = "BomGroup21",
                        EnvironmentFileName = "EnvironmentFileName21",
                        PlistAllFileName = "PlistAllFileName21",
                        Step = "Step21",
                        TplFileName = "TplFileName21",
                        StplFileName = "StplFileName21",
                        Material = new Material
                        {
                            Lots = new List<Lot>(),
                            Units = new List<Unit>
                            {
                                new Unit
                                {
                                    PartType = "Bom221",
                                    Lot = "Name221",
                                    Lab = "Lab2",
                                    VisualId = "VisualId22",
                                    Location = new Location()
                                    {
                                        LocationName = "LocationName2",
                                        LocationType = "LocationType2",
                                    },
                                },
                            },
                        },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                SequenceId = 1,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId14",
                                            LocationCode = "6881",
                                            Thermals = new List<Thermal>() {CreateConditionThermalData("Thermal15")},
                                            Status = ProcessStatus.Completed,
                                            ModificationTime = new DateTime(2019, 4, 3, 14, 00, 00).ToUniversalTime(),
                                            CompletionTime = new DateTime(2019, 4, 3, 15, 00, 00).ToUniversalTime(),
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId12",
                                            LocationCode = "7712",
                                            Thermals = new List<Thermal>() {CreateConditionThermalData("Thermal16")},
                                            Status = ProcessStatus.Canceled,
                                            ModificationTime = new DateTime(2019, 4, 4, 14, 00, 00).ToUniversalTime(),
                                            CompletionTime = new DateTime(2019, 4, 4, 15, 00, 00).ToUniversalTime(),
                                        },
                                    },
                                },
                            },
                        },
                    },
                },

                WasSubmittedToQueue = false,
            };
            return experimentGroup;
        }

        public ExperimentGroup CreateExperimentGroupsWithCanceledExperiment()
        {
            var experimentGroup = new ExperimentGroup
            {
                Username = "batel",
                SubmissionTime = new DateTime(2019, 4, 3, 13, 00, 00).ToUniversalTime(),

                TestProgramData = new TestProgramData
                {
                    BaseTestProgramName = "BaseTestProgramName2",
                    ProgramFamily = "ProgramFamily2",
                    ProgramSubFamily = "ProgramSubFamily2",
                    StplDirectory = "TestProgramUncPath2",
                    TestProgramRootDirectory = "TestProgramRootDirectory",
                },
                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        BomGroupName = "BomGroup21",
                        EnvironmentFileName = "EnvironmentFileName21",
                        PlistAllFileName = "PlistAllFileName21",
                        Step = "Step21",
                        TplFileName = "TplFileName21",
                        StplFileName = "StplFileName21",
                        Material = new Material
                        {
                            Lots = new List<Lot>(),
                            Units = new List<Unit>
                            {
                                new Unit
                                {
                                    PartType = "Bom221",
                                    Lot = "Name221",
                                    Lab = "Lab2",
                                    VisualId = "VisualId22",
                                    Location = new Location()
                                    {
                                        LocationName = "LocationName2",
                                        LocationType = "LocationType2",
                                    },
                                },
                            },
                        },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                SequenceId = 1,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId14",
                                            LocationCode = "6881",
                                            Thermals = new List<Thermal> { new Thermal {Name = "Thermal15", SequenceId = 1 } },
                                            Status = ProcessStatus.Canceled,
                                            ModificationTime = new DateTime(2019, 4, 3, 14, 00, 00).ToUniversalTime(),
                                            CompletionTime = new DateTime(2019, 4, 3, 15, 00, 00).ToUniversalTime(),
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId12",
                                            LocationCode = "7712",
                                            Thermals = new List<Thermal>() {CreateConditionThermalData("Thermal16")},
                                            Status = ProcessStatus.Canceled,
                                            ModificationTime = new DateTime(2019, 4, 4, 14, 00, 00).ToUniversalTime(),
                                            CompletionTime = new DateTime(2019, 4, 4, 15, 00, 00).ToUniversalTime(),
                                        },
                                    },
                                },
                            },
                        },
                    },
                },

                WasSubmittedToQueue = false,
            };
            return experimentGroup;
        }

        public ExperimentGroup CreateIclExperimentGroupForAggregationUser1()
        {
            var experimentGroup = new ExperimentGroup
            {
                Username = "user1",

                TestProgramData = new TestProgramData
                {
                    ProgramFamily = "ICL",
                },

                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                SequenceId = 1,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId11", // EngineeringId11 : 1
                                            LocationCode = "6262", // "6262" : 1
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId12", // EngineeringId12 : 1
                                            LocationCode = "6262", // "6262" : 2
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId13", // EngineeringId13 : 1
                                            LocationCode = "6262", // "6262" : 3
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId12", // EngineeringId12 : 2
                                            LocationCode = "7712", // "7712" : 1
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId15", // EngineeringId15 : 1
                                            LocationCode = "7718", // 7718 : 1
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId17", // EngineeringId17 : 1
                                            LocationCode = "7718", // 7718 : 2
                                        },
                                    },
                                },
                            },
                        },
                    },
                    new Experiment
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                SequenceId = 1,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId12", // EngineeringId12 : 3
                                            LocationCode = "6262", // "6262" : 4
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId14", // EngineeringId14 : 1
                                            LocationCode = "6261", // 6261 : 1
                                        },
                                    },
                                },
                            },
                        },
                    },
                    new Experiment
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                SequenceId = 1,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId15", // EngineeringId15 : 2
                                            LocationCode = "6262", // "6262" : 5
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId14", // EngineeringId14 : 2
                                            LocationCode = "6261", // 6261 : 2
                                        },
                                    },
                                },
                            },
                        },
                    },
                    new Experiment
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                SequenceId = 1,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId18", // EngineeringId18 : 1
                                            LocationCode = "6263", // 6263 : 1
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId10", // EngineeringId10 : 1
                                            LocationCode = "7712", // "7712" : 2
                                        },
                                    },
                                },
                            },
                        },
                    },
                    new Experiment
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                SequenceId = 1,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId17", // EngineeringId17 : 2
                                            LocationCode = "6264", // 6264 : 1
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId16", // EngineeringId16 : 1
                                            LocationCode = "7717", // 7717 : 1
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            };

            return experimentGroup;
        }

        public ExperimentGroup CreateIclExperimentGroupForAggregationUser2()
        {
            var experimentGroup = new ExperimentGroup
            {
                Username = "user2",

                TestProgramData = new TestProgramData
                {
                    ProgramFamily = "ICL",
                },

                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                SequenceId = 1,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId15", // EngineeringId15 : 3
                                            LocationCode = "6881", // "6881" : 1
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId19", // EngineeringId19 : 1
                                            LocationCode = "7712", // "7712" : 3
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId17", // EngineeringId17 : 3
                                            LocationCode = "6262", // "6262" : 6
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId18", // EngineeringId18 : 2
                                            LocationCode = "7712", // "7712" : 4
                                        },
                                    },
                                },
                            },
                        },
                    },
                    new Experiment
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                SequenceId = 1,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId12", // EngineeringId12 : 4
                                            LocationCode = "6881", // "6881" : 2
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId16", // EngineeringId16 : 2
                                            LocationCode = "7712", // "7712" : 5
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            };
            return experimentGroup;
        }

        public ExperimentGroup CreateIclExperimentGroupForAggregationUser3()
        {
            var experimentGroup = new ExperimentGroup
            {
                Username = "user3",

                TestProgramData = new TestProgramData
                {
                    ProgramFamily = "ICL",
                },

                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                SequenceId = 1,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId15", // EngineeringId15 : 3
                                            LocationCode = "6881", // "6881" : 3
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId19", // EngineeringId19 : 2
                                            LocationCode = "7712", // "7712" : 6
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId17", // EngineeringId17 : 4
                                            LocationCode = "6262", // "6262" : 7
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId18", // EngineeringId18 : 3
                                            LocationCode = "7712", // "7712" : 7
                                        },
                                    },
                                },
                            },
                        },
                    },
                    new Experiment
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                SequenceId = 1,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId12", // EngineeringId12 : 5
                                            LocationCode = "6881", // "6881" : 4
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId16", // EngineeringId16 : 3
                                            LocationCode = "7712", // "7712" : 8
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            };
            return experimentGroup;
        }

        public ExperimentGroup CreateClxExperimentGroupForAggregationUser4()
        {
            var experimentGroup = new ExperimentGroup
            {
                Username = "user4",

                TestProgramData = new TestProgramData
                {
                    ProgramFamily = "CLX",
                },

                Experiments = new List<Experiment>
                {
                    new Experiment
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                SequenceId = 1,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId15", // EngineeringId15 : 5
                                            LocationCode = "6881", // "6881" : 5
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId17", // EngineeringId17 : 5
                                            LocationCode = "7712", // "7712" : 9
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId17", // EngineeringId17 : 6
                                            LocationCode = "6262", // "6262" : 8
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId18", // EngineeringId18 : 4
                                            LocationCode = "7712", // "7712" : 10
                                        },
                                    },
                                },
                            },
                        },
                    },
                    new Experiment
                    {
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                SequenceId = 1,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>
                                    {
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId12", // EngineeringId12 : 6
                                            LocationCode = "6881", // "6881" : 6
                                        },
                                        new Condition
                                        {
                                            EngineeringId = "EngineeringId18", // EngineeringId18 : 5
                                            LocationCode = "7712", // "7712" : 11
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            };
            return experimentGroup;
        }

        public List<LotForCreationDto> GenerateNotUniqueLotForCreationDto()
        {
            var lot = new LotForCreationDto
            {
                PartType = "HH 6HY789 A J",
                NumberOfUnitsToRun = 1,
                Name = "LotName",
                Lab = "LabName",
                Locations = new List<LocationWithQuantityForCreationDto>
                {
                    new LocationWithQuantityForCreationDto
                    {
                        LocationType = "LocationType",
                        LocationName = "LocationName",
                        Quantity = 1,
                    },
                },
            };

            var lots = new List<LotForCreationDto> { lot, lot };
            return lots;
        }

        public List<UnitForCreationDto> GenerateNotUniqueUnitForCreationDto()
        {
            var unit = new UnitForCreationDto
            {
                VisualId = "VisualId11",
                PartType = "HH 6HY789 A J",
                Lot = "LotName",
                Lab = "LabName",
                Location = new LocationForCreationDto
                {
                    LocationType = "LocationType",
                    LocationName = "LocationName",
                },
            };

            var units = new List<UnitForCreationDto> { unit, unit };
            return units;
        }

        public ExperimentGroupForUpdateDto GenerateExperimentGroupForTpEditDto()
        {
            return new ExperimentGroupForUpdateDto()
            {
                TestProgramData = new TestProgramDataForUpdateDto()
                {
                    BaseTestProgramName = "59Tp2Updated",
                    TpName = "TpNameUpdated",
                    ProgramFamily = "IceLakeUpdated",
                    ProgramSubFamily = "Class_ICLU_TE_StepUpdated",
                    StplDirectory = "\\\\ger.corp.intel.com\\ec\\proj\\mdl\\ha\\intel\\hdmxprogs\\icl\\OfCourseTPUpdated\\TPL",
                    TestProgramRootDirectory = "\\\\ger.corp.intel.com\\ec\\proj\\mdl\\ha\\intel\\hdmxprogs\\icl\\OfCourseTPUpdated\\TPL",
                },
                Experiments = new List<ExperimentForUpdateDto>()
                {
                    new ExperimentForUpdateDto()
                    {
                        BomGroupName = "CLASS_ICL_42_UN",
                        Step = "D3",
                        TplFileName = "BaseTestPlan.tpl",
                        EnvironmentFileName = "EnvironmentFile_CLASS_ICL_42_UN.env",
                        ExperimentState = ExperimentState.Draft,
                        PlistAllFileName = "PLIST_ALL_CLASS_ICL_42_UN.xml",
                        StplFileName = "someStplFileName.stpl",
                        TestTimeInSecPerUnit = 100,
                        Material = new MaterialForUpdateDto()
                        {
                            MaterialIssue = new MaterialIssueForUpdateDto()
                            {
                                MaterialIssueIsRequired = false,
                            },
                            Units = new List<UnitForUpdateDto>()
                            {
                                new UnitForUpdateDto()
                                {
                                    Lab = "HVC",
                                    OriginalPartType = "H6 4EXHRV C D",
                                    PartType = "H6 4EXHRV C D",
                                    VisualId = "79GU515700503",
                                    Lot = "someLot",
                                    Location = new LocationForUpdateDto()
                                    {
                                        LocationType = "LocationType",
                                        LocationName = "LocationName",
                                    },
                                },
                            },
                            Sspec = "ABCD",
                        },
                        Conditions = new List<ConditionForUpdateDto>()
                        {
                            new ConditionForUpdateDto()
                            {
                                Thermals = new List<ThermalForUpdateDto> { new ThermalForUpdateDto { Name = "HOT", SequenceId = 1} },
                                EngineeringId = "9A",
                                LocationCode = "19",
                                SequenceId = 1,
                                FuseEnabled = false,
                                DieSelection = "DummyDieSelection",
                                MoveUnits = MoveUnits.All,
                                Comment = "Comment",
                                ConditionId = default(Guid),
                            },
                        },
                        SequenceId = 1,
                        DisplayName = "TestProgram",
                        RetestRate = 1,
                        FlexbomHri = "string",
                        FlexbomMrv = "string",
                        ExperimentType = ExperimentType.Engineering,
                        ActivityType = ActivityType.ModuleValidation,
                        Recipe = new RecipeForUpdateDto()
                        {
                            RecipePath = string.Empty,
                            RecipeSource = RecipeSource.TpGenerated,
                        },
                        Tags = new string[] { "string" },
                        Description = "description",
                        ContactEmails = new string[] {"email@intel.com" },
                    },
                },
                Wwid = 11941076,
                Username = "ejmunozz",
                Segment = "someSegment",
                Team = "some team",
                DisplayName = "soe display name",
                DisplayNameSource = DisplayNameSource.SparkProvided,
            };
        }

        public ExperimentGroup GenerateExperimentGroup()
        {
            return new ExperimentGroup()
            {
                TestProgramData = new TestProgramData()
                {
                    BaseTestProgramName = "59Tp2",
                    ProgramFamily = "IceLake",
                    ProgramSubFamily = "Class_ICLU_TE_Step",
                    DirectoryChecksum = "checksum",
                    StplDirectory = "\\\\ger.corp.intel.com\\ec\\proj\\mdl\\ha\\intel\\hdmxprogs\\icl\\ICLXXXXDXH59T20S035\\TPL",
                    TestProgramRootDirectory = "\\\\ger.corp.intel.com\\ec\\proj\\mdl\\ha\\intel\\hdmxprogs\\icl\\ICLXXXXDXH59T20S035\\TPL",
                },
                Experiments = new List<Experiment>()
                {
                    new Experiment()
                    {
                        Id = default(Guid),
                        Vpo = "VpoNumber",
                        BomGroupName = "CLASS_ICL_42_UN",
                        Step = "D3",
                        EnvironmentFileName = "EnvironmentFile_CLASS_ICL_42_UN.env",
                        ExperimentState = ExperimentState.Draft,
                        TestTimeInSecPerUnit = 100,
                        Material = new Material()
                        {
                            MaterialIssue = new MaterialIssue()
                            {
                                MaterialIssueIsRequired = false,
                            },
                            Units = new List<Unit>()
                            {
                                new Unit()
                                {
                                    Lab = "HVC",
                                    VisualId = "79GU515700503",
                                    Location = new Location()
                                    {
                                        LocationName = "INACTIVE",
                                        LocationType = "BI HOLD",
                                    },
                                },
                            },
                            Lots = new List<Lot>(),
                            Sspec = "string",
                        },
                        Stages = new List<Stage>
                        {
                            new Stage
                            {
                                StageType = StageType.Class,
                                SequenceId = 1,
                                StageData = new ClassStageData
                                {
                                    Conditions = new List<Condition>()
                                    {
                                        new Condition()
                                        {
                                            Id = default(Guid),
                                            LocationCode = "6261",
                                            Thermals = new List<Thermal>()
                                            {
                                                new Thermal()
                                                {
                                                    Name = "CLASSHOT",
                                                    SequenceId = 1,
                                                },
                                            },
                                            EngineeringId = "--",
                                            DieSelection = "DieSelection",
                                            SequenceId = 1,
                                            Comment = "Comment",
                                            MoveUnits = MoveUnits.All,
                                        },
                                    },
                                },
                            },
                        },
                        SequenceId = 0,
                        DisplayName = "TestProgram",
                        RetestRate = 1,
                        FlexbomHri = "string",
                        FlexbomMrv = "string",
                        ExperimentType = ExperimentType.Engineering,
                        ActivityType = ActivityType.ModuleValidation,
                        Recipe = new Recipe()
                        {
                            RecipePath = "string",
                            RecipeSource = RecipeSource.TpGenerated,
                        },
                        ExperimentSplitId = default(Guid),
                        Tags = new string[] { "string" },
                        Description = "description",
                        ContactEmails = new string[] {"email@intel.com" },
                        IsArchived = false,
                        PlistAllFileName = "somePlist",
                        StplFileName = "someStpl",
                        TplFileName = "someTpl",
                    },
                },
                Wwid = 11941076,
                Id = default(Guid),
                Segment = "someSegment",
                Team = "scrum",
                DisplayName = "myDisplayName",
                DisplayNameSource = DisplayNameSource.SparkProvided,
                Username = "fjcoulon",
                WasSubmittedToQueue = false,
            };
        }

        public ExperimentGroup GenerateExperimentGroupWithSplitExperiment()
        {
            return new ExperimentGroup
            {
                Username = "user1",
                Segment = "test",
                Team = "spark",
                SubmissionTime = DateTime.UtcNow,
                TestProgramData = new TestProgramData
                {
                    BaseTestProgramName = "BaseTestProgramName1",
                    ProgramFamily = "ProgramFamily1",
                    ProgramSubFamily = "ProgramSubFamily1",
                    StplDirectory = "TestProgramUncPath1",
                    TestProgramRootDirectory = "TestProgramRootDirectory",
                },
                Experiments = new List<Experiment>
                    {
                        new Experiment
                        {
                            ExperimentType = ExperimentType.Engineering,
                            BomGroupName = "BomGroup11",
                            EnvironmentFileName = "EnvironmentFileName11",
                            TestTimeInSecPerUnit = 25,
                            PlistAllFileName = "PlistAllFileName11",
                            Step = "Step11",
                            TplFileName = "TplFileName11",
                            StplFileName = "StplFileName11",
                            Material = new Material
                            {
                                Lots = new List<Lot>(),
                                Units = new List<Unit>
                                {
                                    new Unit
                                    {
                                        PartType = "Bom111",
                                        Lot = "Name111",
                                        Lab = "Lab1",
                                        VisualId = "VisualId11",
                                        Location = new Location()
                                        {
                                            LocationName = "LocationName",
                                            LocationType = "LocationType",
                                        },
                                    },
                                    new Unit
                                    {
                                        PartType = "Bom111",
                                        Lot = "Name111",
                                        Lab = "Lab1",
                                        VisualId = "VisualId12",
                                        Location = new Location()
                                        {
                                            LocationName = "LocationName1",
                                            LocationType = "LocationType1",
                                        },
                                    },
                                    new Unit
                                    {
                                        PartType = "Bom112",
                                        Lot = "Name112",
                                        Lab = "Lab1",
                                        VisualId = "VisualId121",
                                        Location = new Location()
                                        {
                                            LocationName = "LocationName2",
                                            LocationType = "LocationType2",
                                        },
                                    },
                                },
                            },
                            SequenceId = 0,
                            Vpo = "vpo1",
                            Stages = new List<Stage>
                            {
                                new Stage
                                {
                                    StageType = StageType.Class,
                                    SequenceId = 1,
                                    StageData = new ClassStageData
                                    {
                                        Conditions = new List<Condition>
                                        {
                                            new Condition
                                            {
                                                EngineeringId = "EngineeringId11",
                                                LocationCode = "6262",
                                                Thermals = new List<Thermal>
                                                {
                                                    new Thermal
                                                    {
                                                        Name = "Thermal12",
                                                        SequenceId = 1,
                                                        IsByPassed = false,
                                                    },
                                                },
                                                Status = ProcessStatus.Running,
                                                SequenceId = 0,
                                            },
                                            new Condition
                                            {
                                                EngineeringId = "EngineeringId12",
                                                LocationCode = "6262",
                                                Thermals = new List<Thermal>
                                                {
                                                    new Thermal
                                                    {
                                                        Name = "Thermal12",
                                                        SequenceId = 1,
                                                        IsByPassed = false,
                                                    },
                                                },
                                                Status = ProcessStatus.Completed,
                                                SequenceId = 0,
                                                CustomAttributes = new List<CustomAttribute>(),
                                            },
                                        },
                                    },
                                },
                            },
                            Recipe = new Recipe
                            {
                                RecipeSource = RecipeSource.TpGenerated,
                            },
                            Tags = new List<string>(),
                            ContactEmails = new List<string>(),
                        },
                        new Experiment
                        {
                            ExperimentType = ExperimentType.Engineering,
                            BomGroupName = "BomGroup12",
                            EnvironmentFileName = "EnvironmentFileName12",
                            TestTimeInSecPerUnit = 25,
                            PlistAllFileName = "PlistAllFileName12",
                            Step = "Step12",
                            TplFileName = "TplFileName12",
                            StplFileName = "StplFileName12",
                            Material = new Material
                            {
                                Lots = new List<Lot>(),
                                Units = new List<Unit>
                                {
                                    new Unit
                                    {
                                        PartType = "Bom211",
                                        Lot = "Name211",
                                        Lab = "Lab1",
                                        VisualId = "VisualId21",
                                        Location = new Location()
                                        {
                                            LocationName = "LocationName1",
                                            LocationType = "LocationType1",
                                        },
                                    },
                                    new Unit
                                    {
                                        PartType = "Bom212",
                                        Lot = "Name212",
                                        Lab = "Lab1",
                                        VisualId = "VisualId221",
                                        Location = new Location()
                                        {
                                            LocationName = "LocationName2",
                                            LocationType = "LocationType2",
                                        },
                                    },
                                },
                            },
                            SequenceId = 0,
                            Stages = new List<Stage>
                            {
                                new Stage
                                {
                                    StageType = StageType.Class,
                                    SequenceId = 1,
                                    StageData = new ClassStageData
                                    {
                                        Conditions = new List<Condition>
                                        {
                                            new Condition
                                            {
                                                EngineeringId = "EngineeringId12",
                                                LocationCode = "6262",
                                                Thermals = new List<Thermal>
                                                {
                                                    new Thermal
                                                    {
                                                        Name = "Thermal13",
                                                        SequenceId = 1,
                                                        IsByPassed = false,
                                                    },
                                                },
                                                Status = ProcessStatus.Running,
                                                SequenceId = 0,
                                                CustomAttributes = new List<CustomAttribute>(),
                                            },
                                            new Condition
                                            {
                                                EngineeringId = "EngineeringId12",
                                                LocationCode = "6261",
                                                Thermals = new List<Thermal>
                                                {
                                                    new Thermal
                                                    {
                                                        Name = "Thermal14",
                                                        SequenceId = 1,
                                                        IsByPassed = false,
                                                    },
                                                },
                                                Status = ProcessStatus.Completed,
                                                SequenceId = 0,
                                                CustomAttributes = new List<CustomAttribute>(),
                                            },
                                        },
                                    },
                                },
                            },
                            Recipe = new Recipe
                            {
                                RecipeSource = RecipeSource.TpGenerated,
                            },
                            Tags = new List<string>(),
                            ContactEmails = new List<string>(),
                        },
                    },
                WasSubmittedToQueue = false,
                Wwid = 0,
            };
        }

        public PredictionRecordDto GenerateExperimentGroupDtoWithSplitExperiment()
        {
            return new PredictionRecordDto
            {
                Username = "user1",
                TestProgramData = new TestProgramDataDto
                {
                    BaseTestProgramName = "BaseTestProgramName1",
                    ProgramFamily = "ProgramFamily1",
                    ProgramSubFamily = "ProgramSubFamily1",
                    StplDirectory = "TestProgramUncPath1",
                    TestProgramRootDirectory = "TestProgramRootDirectory",
                },
                Experiments = new List<ExperimentDto>
                    {
                        new ExperimentDto
                        {
                            ExperimentType = ExperimentType.Engineering,
                            BomGroupName = "BomGroup11",
                            EnvironmentFileName = "EnvironmentFileName11",
                            TestTimeInSecPerUnit = 25,
                            PlistAllFileName = "PlistAllFileName11",
                            Step = "Step11",
                            TplFileName = "TplFileName11",
                            StplFileName = "StplFileName11",
                            Material = new MaterialDto
                            {
                                Lots = new List<LotDto>(),
                                Units = new List<UnitDto>
                                {
                                    new UnitDto
                                    {
                                        PartType = "Bom111",
                                        Lot = "Name111",
                                        Lab = "Lab1",
                                        VisualId = "VisualId11",
                                        Location = new LocationDto()
                                        {
                                            LocationName = "LocationName",
                                            LocationType = "LocationType",
                                        },
                                    },
                                    new UnitDto
                                    {
                                        PartType = "Bom111",
                                        Lot = "Name111",
                                        Lab = "Lab1",
                                        VisualId = "VisualId12",
                                        Location = new LocationDto()
                                        {
                                            LocationName = "LocationName1",
                                            LocationType = "LocationType1",
                                        },
                                    },
                                    new UnitDto
                                    {
                                        PartType = "Bom112",
                                        Lot = "Name112",
                                        Lab = "Lab1",
                                        VisualId = "VisualId121",
                                        Location = new LocationDto()
                                        {
                                            LocationName = "LocationName2",
                                            LocationType = "LocationType2",
                                        },
                                    },
                                },
                            },
                            SequenceId = 0,
                            Vpo = "vpo1",
                            Stages = new List<StageDto>
                            {
                                new StageDto
                                {
                                    StageType = StageType.Class,
                                    SequenceId = 1,
                                    StageData = new ClassStageDataDto
                                    {
                                        Conditions = new List<ConditionDto>
                                        {
                                            new ConditionDto
                                            {
                                                EngineeringId = "EngineeringId11",
                                                LocationCode = "6262",
                                                Thermals = new List<ThermalDto>
                                                {
                                                    new ThermalDto
                                                    {
                                                        Name = "Thermal12",
                                                        SequenceId = 1,
                                                        IsByPassed = false,
                                                    },
                                                },
                                                Status = ProcessStatus.Running,
                                                SequenceId = 0,
                                                CustomAttributes = new List<CustomAttributeDto>(),
                                            },
                                        },
                                        ConditionsWithResults = new List<ConditionWithResultsDto>
                                        {
                                            new ConditionWithResultsDto
                                            {
                                                EngineeringId = "EngineeringId12",
                                                LocationCode = "6262",
                                                Thermals = new List<ThermalDto>
                                                {
                                                    new ThermalDto
                                                    {
                                                        Name = "Thermal12",
                                                        SequenceId = 1,
                                                        IsByPassed = false,
                                                    },
                                                },
                                                Status = ProcessStatus.Completed,
                                                SequenceId = 0,
                                                CustomAttributes = new List<CustomAttributeDto>(),
                                            },
                                        },
                                    },
                                },
                            },
                            Recipe = new RecipeDto
                            {
                                RecipeSource = RecipeSource.TpGenerated,
                            },
                            Tags = new List<string>(),
                            ContactEmails = new List<string>(),
                        },
                        new ExperimentDto
                        {
                            ExperimentType = ExperimentType.Engineering,
                            BomGroupName = "BomGroup12",
                            EnvironmentFileName = "EnvironmentFileName12",
                            TestTimeInSecPerUnit = 25,
                            PlistAllFileName = "PlistAllFileName12",
                            Step = "Step12",
                            TplFileName = "TplFileName12",
                            StplFileName = "StplFileName12",
                            Material = new MaterialDto
                            {
                                Lots = new List<LotDto>(),
                                Units = new List<UnitDto>
                                {
                                    new UnitDto
                                    {
                                        PartType = "Bom211",
                                        Lot = "Name211",
                                        Lab = "Lab1",
                                        VisualId = "VisualId21",
                                        Location = new LocationDto()
                                        {
                                            LocationName = "LocationName1",
                                            LocationType = "LocationType1",
                                        },
                                    },
                                    new UnitDto
                                    {
                                        PartType = "Bom212",
                                        Lot = "Name212",
                                        Lab = "Lab1",
                                        VisualId = "VisualId221",
                                        Location = new LocationDto()
                                        {
                                            LocationName = "LocationName2",
                                            LocationType = "LocationType2",
                                        },
                                    },
                                },
                            },
                            SequenceId = 0,
                            Stages = new List<StageDto>
                            {
                                new StageDto
                                {
                                    StageType = StageType.Class,
                                    SequenceId = 1,
                                    StageData = new ClassStageDataDto
                                    {
                                        Conditions = new List<ConditionDto>
                                        {
                                            new ConditionDto
                                            {
                                                EngineeringId = "EngineeringId12",
                                                LocationCode = "6262",
                                                Thermals = new List<ThermalDto>
                                                {
                                                    new ThermalDto
                                                    {
                                                        Name = "Thermal13",
                                                        SequenceId = 1,
                                                        IsByPassed = false,
                                                    },
                                                },
                                                Status = ProcessStatus.Running,
                                                SequenceId = 0,
                                                CustomAttributes = new List<CustomAttributeDto>(),
                                            },
                                        },
                                        ConditionsWithResults = new List<ConditionWithResultsDto>
                                        {
                                            new ConditionWithResultsDto
                                            {
                                                EngineeringId = "EngineeringId12",
                                                LocationCode = "6261",
                                                Thermals = new List<ThermalDto>
                                                {
                                                    new ThermalDto
                                                    {
                                                        Name = "Thermal14",
                                                        SequenceId = 1,
                                                        IsByPassed = false,
                                                    },
                                                },
                                                Status = ProcessStatus.Completed,
                                                SequenceId = 0,
                                                CustomAttributes = new List<CustomAttributeDto>(),
                                            },
                                        },
                                    },
                                },
                            },
                            Recipe = new RecipeDto
                            {
                                RecipeSource = RecipeSource.TpGenerated,
                            },
                            Tags = new List<string>(),
                            ContactEmails = new List<string>(),
                        },
                    },
                Wwid = 0,
            };
        }

        public ExperimentGroup GenerateExperimentGroupWithDuplicatedConditions()
        {
            return new ExperimentGroup
            {
                Username = "user1",
                Segment = "test",
                Team = "spark",
                SubmissionTime = DateTime.UtcNow,
                TestProgramData = new TestProgramData
                {
                    BaseTestProgramName = "BaseTestProgramName1",
                    ProgramFamily = "ProgramFamily1",
                    ProgramSubFamily = "ProgramSubFamily1",
                    StplDirectory = "TestProgramUncPath1",
                    TestProgramRootDirectory = "TestProgramRootDirectory",
                    DirectoryChecksum = "checksuc5116b87mValc5116b87ue",
                },
                Experiments = new List<Experiment>
                    {
                        new Experiment
                        {
                            ExperimentType = ExperimentType.Engineering,
                            BomGroupName = "BomGroup11",
                            EnvironmentFileName = "EnvironmentFileName11",
                            TestTimeInSecPerUnit = 25,
                            PlistAllFileName = "PlistAllFileName11",
                            Step = "Step11",
                            ActivityType = ActivityType.MassiveValidation,
                            ExperimentState = ExperimentState.Ready,
                            TplFileName = "TplFileName11",
                            StplFileName = "StplFileName11",
                            Material = new Material
                            {
                                Sspec = "ABCD",
                                MaterialIssue = new MaterialIssue
                                {
                                    MaterialIssueIsRequired = false,
                                    MaterialIssueErrorComments = "just for testing, please ignore.",
                                },
                                Lots = new List<Lot>(),
                                Units = new List<Unit>
                                {
                                    new Unit
                                    {
                                        PartType = "Bom111",
                                        Lot = "Name111",
                                        Lab = "Lab1",
                                        VisualId = "VisualId11",
                                        Location = new Location()
                                        {
                                            LocationName = "LocationName",
                                            LocationType = "LocationType",
                                        },
                                    },
                                    new Unit
                                    {
                                        PartType = "Bom111",
                                        Lot = "Name111",
                                        Lab = "Lab1",
                                        VisualId = "VisualId12",
                                        Location = new Location()
                                        {
                                            LocationName = "LocationName1",
                                            LocationType = "LocationType1",
                                        },
                                    },
                                    new Unit
                                    {
                                        PartType = "Bom112",
                                        Lot = "Name112",
                                        Lab = "Lab1",
                                        VisualId = "VisualId121",
                                        Location = new Location()
                                        {
                                            LocationName = "LocationName2",
                                            LocationType = "LocationType2",
                                        },
                                    },
                                },
                            },
                            SequenceId = 0,
                            Vpo = "vpo1",
                            Stages = new List<Stage>
                            {
                                new Stage
                                {
                                    StageType = StageType.Class,
                                    SequenceId = 1,
                                    StageData = new ClassStageData
                                    {
                                        Conditions = new List<Condition>
                                        {
                                            new Condition
                                            {
                                                EngineeringId = "EngineeringId11",
                                                LocationCode = "6262",
                                                Thermals = new List<Thermal>
                                                {
                                                    new Thermal
                                                    {
                                                        Name = "Thermal12",
                                                        SequenceId = 1,
                                                        IsByPassed = false,
                                                    },
                                                },
                                                Status = ProcessStatus.Committed,
                                                StatusChangeComment = "to committed",
                                                Results = new ConditionResult
                                                {
                                                    NumberOfGoodBins = 5,
                                                    NumberOfBadBins = 63,
                                                },
                                                SequenceId = 0,
                                                Comment = "sample file",
                                                FuseEnabled = true,
                                                DieSelection = "SS",
                                                MoveUnits = MoveUnits.Rejects,
                                                Fuse = new FuseForCreation
                                                {
                                                    FuseType = FuseType.OneStep,
                                                    Lrf = new LrfData
                                                    {
                                                        FuseReleaseRevision = 12,
                                                        FuseReleaseName = "testing fuse name",
                                                        BinQdfs = new List<BinQdf>
                                                        {
                                                            new BinQdf
                                                            {
                                                                PassBin = 1001,
                                                                PlanningBin = "test",
                                                                Qdf = "RSDA",
                                                            },
                                                        },
                                                    },
                                                },
                                                VpoCustomSuffix = "RA",
                                            },
                                            new Condition
                                            {
                                                EngineeringId = "EngineeringId12",
                                                LocationCode = "6262",
                                                Thermals = new List<Thermal>
                                                {
                                                    new Thermal
                                                    {
                                                        Name = "Thermal12",
                                                        SequenceId = 1,
                                                        IsByPassed = false,
                                                    },
                                                },
                                                Status = ProcessStatus.Paused,
                                                SequenceId = 0,
                                                CustomAttributes = new List<CustomAttribute>(),
                                            },
                                        },
                                    },
                                },
                            },
                            Recipe = new Recipe
                            {
                                RecipePath = "dummyRecipePath",
                                RecipeSource = RecipeSource.ByoGenerated,
                            },
                            Tags = new List<string>
                            {
                                "tag1", "tag2",
                            },
                            Description = "this is a dummy description for test",
                            ContactEmails = new List<string>
                            {
                                "someemail@intel.com",
                                "anotheremail@intel.com",
                            },
                            DisplayName = "A beatiful and meaningful name",
                        },
                    },
                WasSubmittedToQueue = true,
                Wwid = 1235645,
                DisplayName = "experiment group level name",
                DisplayNameSource = DisplayNameSource.SparkProvided,
            };
        }

        public PredictionRecordDto GenerateExperimentGroupDtoWithDuplicatedConditions()
        {
            return new PredictionRecordDto
            {
                Username = "user1",
                TestProgramData = new TestProgramDataDto
                {
                    BaseTestProgramName = "BaseTestProgramName1",
                    ProgramFamily = "ProgramFamily1",
                    ProgramSubFamily = "ProgramSubFamily1",
                    StplDirectory = "TestProgramUncPath1",
                    TestProgramRootDirectory = "TestProgramRootDirectory",
                    DirectoryChecksum = "checksuc5116b87mValc5116b87ue",
                },
                Experiments = new List<ExperimentDto>
                    {
                        new ExperimentDto
                        {
                            ExperimentType = ExperimentType.Engineering,
                            BomGroupName = "BomGroup11",
                            EnvironmentFileName = "EnvironmentFileName11",
                            ActivityType = ActivityType.MassiveValidation,
                            ExperimentState = ExperimentState.Ready,
                            TestTimeInSecPerUnit = 25,
                            PlistAllFileName = "PlistAllFileName11",
                            Step = "Step11",
                            TplFileName = "TplFileName11",
                            StplFileName = "StplFileName11",
                            Material = new MaterialDto
                            {
                                Sspec = "ABCD",
                                MaterialIssue = new MaterialIssueDto
                                {
                                    MaterialIssueIsRequired = false,
                                    MaterialIssueErrorComments = "just for testing, please ignore.",
                                },
                                Lots = new List<LotDto>(),
                                Units = new List<UnitDto>
                                {
                                    new UnitDto
                                    {
                                        PartType = "Bom111",
                                        Lot = "Name111",
                                        Lab = "Lab1",
                                        VisualId = "VisualId11",
                                        Location = new LocationDto()
                                        {
                                            LocationName = "LocationName",
                                            LocationType = "LocationType",
                                        },
                                    },
                                    new UnitDto
                                    {
                                        PartType = "Bom111",
                                        Lot = "Name111",
                                        Lab = "Lab1",
                                        VisualId = "VisualId12",
                                        Location = new LocationDto()
                                        {
                                            LocationName = "LocationName1",
                                            LocationType = "LocationType1",
                                        },
                                    },
                                    new UnitDto
                                    {
                                        PartType = "Bom112",
                                        Lot = "Name112",
                                        Lab = "Lab1",
                                        VisualId = "VisualId121",
                                        Location = new LocationDto()
                                        {
                                            LocationName = "LocationName2",
                                            LocationType = "LocationType2",
                                        },
                                    },
                                },
                            },
                            SequenceId = 0,
                            Vpo = "vpo1",
                            Stages = new List<StageDto>
                            {
                                new StageDto
                                {
                                    StageType = StageType.Class,
                                    SequenceId = 1,
                                    StageData = new ClassStageDataDto
                                    {
                                        Conditions = new List<ConditionDto>
                                        {
                                            new ConditionDto
                                            {
                                                EngineeringId = "EngineeringId11",
                                                LocationCode = "6262",
                                                Status = ProcessStatus.Committed,
                                                StatusChangeComment = "to committed",
                                                SequenceId = 0,
                                                Comment = "sample file",
                                                FuseEnabled = true,
                                                DieSelection = "SS",
                                                MoveUnits = MoveUnits.Rejects,
                                                Fuse = new FuseDto
                                                {
                                                    FuseType = FuseType.OneStep,
                                                    Lrf = new LrfDto
                                                    {
                                                        FuseReleaseRevision = 12,
                                                        FuseReleaseName = "testing fuse name",
                                                        BinQdfs = new List<BinQdfDto>
                                                        {
                                                            new BinQdfDto
                                                            {
                                                                PassBin = 1001,
                                                                PlanningBin = "test",
                                                                Qdf = "RSDA",
                                                            },
                                                        },
                                                    },
                                                },
                                                VpoCustomSuffix = "RA",
                                                Thermals = new List<ThermalDto>
                                                {
                                                    new ThermalDto
                                                    {
                                                        Name = "Thermal12",
                                                        SequenceId = 1,
                                                        IsByPassed = false,
                                                    },
                                                },
                                                CustomAttributes = new List<CustomAttributeDto>(),
                                            },
                                            new ConditionDto
                                            {
                                                EngineeringId = "EngineeringId12",
                                                LocationCode = "6262",
                                                Thermals = new List<ThermalDto>
                                                {
                                                    new ThermalDto
                                                    {
                                                        Name = "Thermal12",
                                                        SequenceId = 1,
                                                        IsByPassed = false,
                                                    },
                                                },
                                                Status = ProcessStatus.Paused,
                                                SequenceId = 0,
                                                CustomAttributes = new List<CustomAttributeDto>(),
                                            },
                                        },
                                        ConditionsWithResults = new List<ConditionWithResultsDto>(),
                                    },
                                },
                            },
                            Recipe = new RecipeDto
                            {
                                RecipePath = "dummyRecipePath",
                                RecipeSource = RecipeSource.ByoGenerated,
                            },
                            Tags = new List<string>
                            {
                                "tag1", "tag2",
                            },
                            Description = "this is a dummy description for test",
                            ContactEmails = new List<string>
                            {
                                "someemail@intel.com",
                                "anotheremail@intel.com",
                            },
                            DisplayName = "A beatiful and meaningful name",
                        },
                    },
                Wwid = 1235645,
                DisplayName = "experiment group level name",
                DisplayNameSource = DisplayNameSource.SparkProvided,
            };
        }

        public PredictionRecordDto GenerateExperimentGroupDtoWithDuplicatedConditionsInSameStage()
        {
            return new PredictionRecordDto
            {
                Username = "username1",
                TestProgramData = new TestProgramDataDto
                {
                    BaseTestProgramName = "BaseTestProgramName",
                    ProgramFamily = "ProgramFamily",
                    TpName = "TpName",
                    ProgramSubFamily = "ProgramSubFamily",
                    StplDirectory = "\\\\amr.corp.intel.com\\ThisIs\\StplDirectory",
                    TestProgramRootDirectory = "\\\\amr.corp.intel.com\\ThisIs\\TestProgramRootDirectory",
                    DirectoryChecksum = "DirectoryChecksum",
                },
                Experiments = new List<ExperimentDto>
                    {
                        new ExperimentDto
                        {
                            BomGroupName = "BomGroup9",
                            Step = "A2",
                            TplFileName = "TplFileName",
                            StplFileName = "StplFileName",
                            EnvironmentFileName = "EnvironmentFileName",
                            ExperimentState = ExperimentState.Ready,
                            PlistAllFileName = "PlistAllFileName",
                            ActivityType = ActivityType.ModuleValidation,
                            TestTimeInSecPerUnit = 100,
                            SequenceId = 1,
                            DisplayName = "BaseTestProgramName - BomGroup9 A2",
                            RetestRate = 50,
                            FlexbomHri = FlexbomDefaults.Hri,
                            FlexbomMrv = FlexbomDefaults.Mrv,
                            ExperimentType = ExperimentType.Engineering,
                            Material = new MaterialDto
                            {
                                MaterialIssue = new MaterialIssueDto
                                {
                                    MaterialIssueIsRequired = true,
                                },
                                Lots = new List<LotDto>(),
                                Units = new List<UnitDto>
                                {
                                    new UnitDto
                                    {
                                        PartType = "HH 6HY789 A J",
                                        OriginalPartType = "HH 6HY789 A R",
                                        Lot = "LotName",
                                        Lab = "LabName",
                                        VisualId = "VisualId11",
                                        Location = new LocationDto()
                                        {
                                            LocationName = "LocationName",
                                            LocationType = "LocationType",
                                        },
                                    },
                                },
                            },
                            Stages = new List<StageDto>
                            {
                                new StageDto
                                {
                                    StageType = StageType.Class,
                                    SequenceId = 1,
                                    StageData = new ClassStageDataDto
                                    {
                                        Conditions = new List<ConditionDto>
                                        {
                                            new ConditionDto
                                            {
                                                EngineeringId = "9A",
                                                LocationCode = "19",
                                                SequenceId = 1,
                                                Comment = "A simple comment for condition",
                                                FuseEnabled = false,
                                                DieSelection = "DummyDieSelection",
                                                MoveUnits = MoveUnits.Rejects,
                                                Fuse = new FuseDto
                                                {
                                                    FuseType = FuseType.OneStep,
                                                    Lrf = new LrfDto
                                                    {
                                                        FuseReleaseRevision = 5,
                                                        FuseReleaseName = "DummyRelease",
                                                        BinQdfs = new List<BinQdfDto>
                                                        {
                                                            new BinQdfDto
                                                            {
                                                                PassBin = 1002,
                                                                PlanningBin = "AB",
                                                                Qdf = "DADD",
                                                            },
                                                        },
                                                    },
                                                },
                                                Thermals = new List<ThermalDto>
                                                {
                                                    new ThermalDto
                                                    {
                                                        Name = "HOT",
                                                        SequenceId = 1,
                                                        IsByPassed = false,
                                                    },
                                                },
                                                CustomAttributes = new List<CustomAttributeDto>(),
                                            },
                                            new ConditionDto
                                            {
                                                EngineeringId = "9A",
                                                LocationCode = "6987",
                                                SequenceId = 2,
                                                Comment = "some custom comment here",
                                                FuseEnabled = false,
                                                DieSelection = "DummyDieSelection",
                                                MoveUnits = MoveUnits.Good,
                                                Thermals = new List<ThermalDto>
                                                {
                                                    new ThermalDto
                                                    {
                                                        Name = "HOT",
                                                        SequenceId = 1,
                                                        IsByPassed = false,
                                                    },
                                                },
                                                CustomAttributes = new List<CustomAttributeDto>(),
                                                VpoCustomSuffix = "RA",
                                            },
                                            new ConditionDto
                                            {
                                                EngineeringId = "9A",
                                                LocationCode = "6987",
                                                SequenceId = 3,
                                                Comment = "some custom comment here2222222",
                                                FuseEnabled = false,
                                                DieSelection = "DummyDieSelection",
                                                MoveUnits = MoveUnits.All,
                                                Thermals = new List<ThermalDto>
                                                {
                                                    new ThermalDto
                                                    {
                                                        Name = "HOT",
                                                        SequenceId = 1,
                                                        IsByPassed = false,
                                                    },
                                                },
                                                CustomAttributes = new List<CustomAttributeDto>(),
                                            },
                                            new ConditionDto
                                            {
                                                EngineeringId = "9A",
                                                LocationCode = "6987",
                                                SequenceId = 4,
                                                Comment = "some custom comment here22222223",
                                                FuseEnabled = false,
                                                DieSelection = "DummyDieSelection",
                                                MoveUnits = MoveUnits.Rejects,
                                                Thermals = new List<ThermalDto>
                                                {
                                                    new ThermalDto
                                                    {
                                                        Name = "HOT",
                                                        SequenceId = 1,
                                                        IsByPassed = false,
                                                    },
                                                },
                                                CustomAttributes = new List<CustomAttributeDto>
                                                {
                                                    new CustomAttributeDto
                                                    {
                                                        AttributeName = "someName",
                                                        AttributeValue = "someValue",
                                                    },
                                                },
                                                VpoCustomSuffix = "RB",
                                            },
                                        },
                                        ConditionsWithResults = new List<ConditionWithResultsDto>(),
                                    },
                                },
                            },
                            Recipe = new RecipeDto{RecipeSource = RecipeSource.TpGenerated, RecipePath = string.Empty},
                            Tags = new List<string>(){ "tag1", "tag2" },
                            Description = "this is just a simple description",
                            ContactEmails = new List<string>(){ "contact@intel.com", "contact2@intel.com" },
                        },
                    },
                Wwid = 1234,
                DisplayName = "This is just a simple validation item.",
                DisplayNameSource = DisplayNameSource.SparkProvided,
            };
        }

        private Thermal CreateConditionThermalData(string thermalName, uint sequenceId = 1)
        {
            return new Thermal() { Name = thermalName, SequenceId = sequenceId };
        }
    }
}