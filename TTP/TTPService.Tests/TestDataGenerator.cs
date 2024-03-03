using System;
using System.Collections.Generic;
using System.Linq;
using TTPService.Dtos;
using TTPService.Dtos.Creation;
using TTPService.Dtos.Notification.Enums;
using TTPService.Dtos.Update;
using TTPService.Entities;
using TTPService.Enums;
using ClassStageDataForCreationDto = TTPService.Dtos.Creation.ClassStageDataForCreationDto;
using ConditionForCreationDto = TTPService.Dtos.Creation.ConditionForCreationDto;
using ExperimentForUpdateDto = TTPService.Dtos.Orchestrator.ExperimentGroupUpdateDtos.ExperimentForUpdateDto;
using LocationForCreationDto = TTPService.Dtos.Creation.LocationForCreationDto;
using LocationWithQuantityForCreationDto = TTPService.Dtos.Creation.LocationWithQuantityForCreationDto;
using LotForCreationDto = TTPService.Dtos.Creation.LotForCreationDto;
using MaterialForCreationDto = TTPService.Dtos.Creation.MaterialForCreationDto;
using StageForCreationDto = TTPService.Dtos.Creation.StageForCreationDto;
using UnitForCreationDto = TTPService.Dtos.Creation.UnitForCreationDto;

namespace TTPService.Tests
{
    public class TestDataGenerator
    {
        private static readonly Random Random = new Random();

        public PredictionRecordDto GenerateExperimentGroupDto(uint numOfExperiments = 1)
        {
            return new PredictionRecordDto()
            {
                Id = Guid.NewGuid(),
                Username = TestDataGenerationHelper.GenerateUserName(),
                Experiments = GenerateListOfExperimentDtos(numOfExperiments),
            };
        }

        public ExperimentStatusUpdateDto GenerateExperimentProgressNotifyDto()
        {
            Array values = Enum.GetValues(typeof(ExperimentStatus));
            var randomStatus = (ExperimentStatus)values.GetValue(Random.Next(values.Length));
            return new ExperimentStatusUpdateDto()
            {
                CorrelationId = Guid.NewGuid(),
                ExperimentStatus = randomStatus,
            };
        }

        public ExperimentGroupForCreationDto GenerateExperimentGroupForCreationDto()
        {
            return new ExperimentGroupForCreationDto()
            {
                Username = TestDataGenerationHelper.GenerateUserName(),
                DisplayName = string.Empty,
                Experiments = GenerateListOfExperimentsForCreationDtos(),
                TestProgramData = GenerateTestProgramDataForCrreationDto(),
            };
        }

        public IEnumerable<PredictionRecordDto> GenerateListOfExperimentGroupDtos(uint numberOfExperimentGroups = 1)
        {
            return TestDataGenerationHelper.GenerateList(GenerateExperimentGroupDtoWithoutExperiments, numberOfExperimentGroups);
        }

        public ExperimentGroup GenerateExperimentGroup(uint numOfExperiments = 1, bool includeExperimentSplitId = false)
        {
            return new ExperimentGroup()
            {
                Id = Guid.NewGuid(),
                TestProgramData = GenerateTestProgramData(),
                Username = TestDataGenerationHelper.GenerateUserName(),
                Experiments = GenerateListOfExperiments(numOfExperiments, includeExperimentSplitId),
                WasSubmittedToQueue = false,
                Segment = "Echo",
                Team = "CIFT",
                SubmissionTime = DateTime.Now,
            };
        }

        public IEnumerable<Experiment> GenerateListOfExperiments(uint numOfExperiments = 1, bool includeExperimentSplitId = false)
        {
            return TestDataGenerationHelper.GenerateList(() => GenerateExperiment(includeExperimentSplitId), numOfExperiments);
        }

        public IEnumerable<ExperimentGroup> GenerateListOfExperimentGroups(uint numberOfExperimentGroups = 1)
        {
            return TestDataGenerationHelper.GenerateList(TestDataGenerationHelper.GenerateExperimentGroupWithoutExperiments, numberOfExperimentGroups);
        }

        public ExperimentDto GenerateExperimentDto()
        {
            return new ExperimentDto()
            {
                Id = Guid.NewGuid(),
            };
        }

        public Experiment GenerateExperiment(bool includeExperimentSplitId = false)
        {
            return new Experiment()
            {
                Id = Guid.NewGuid(),
                ExperimentSplitId = includeExperimentSplitId ? new Guid?(Guid.Parse("fef6fddd-b422-4757-a320-20e9738ea7bb")) : (Guid?)null,
                BomGroupName = "CLASS_ICL42_U",
                EnvironmentFileName = "env_file1.env",
                PlistAllFileName = "plist_file1.xml",
                TestTimeInSecPerUnit = 1,
                StplFileName = "sub_file1.stpl",
                TplFileName = "base_tpl1.tpl",
                Step = "A1",
                Stages = new List<Stage>()
                {
                    new Stage()
                    {
                        StageType = StageType.Class,
                        Id = Guid.NewGuid(),
                        SequenceId = 1,
                        StageData = new ClassStageData()
                        {
                            Conditions = GenerateConditions(),
                        },
                    },
                },
                Material = GenerateMaterial(),
                FlexbomHri = "HRI1",
                FlexbomMrv = "00000000000D",
                ExperimentType = ExperimentType.Engineering,
                ActivityType = ActivityType.ModuleValidation,
                Tags = new List<string>() { "tag1", "tag2" },
                ExperimentState = ExperimentState.Draft,
                ContactEmails = new[] { "a", "b", "c" },
                Description = "desc",
                DisplayName = "exp display name",
                IsArchived = false,
                Recipe = new Recipe()
                {
                    RecipePath = "someRecipePath",
                    RecipeSource = RecipeSource.ByoGenerated,
                },
                RetestRate = 88,
                SequenceId = 8,
            };
        }

        public ExperimentForCreationDto GenerateExperimentsForCreationDto()
        {
            return new ExperimentForCreationDto()
            {
                BomGroupName = $"BomGroup{new Random().Next(1, 100)}",
                Step = $"A{new Random().Next(1, 10)}",
                EnvironmentFileName = "env_file1.env",
                PlistAllFileName = "plist_file1.xml",
                TestTimeInSecPerUnit = 1,
                StplFileName = "sub_file1.stpl",
                TplFileName = "base_tpl1.tpl",
                Stages = new List<StageForCreationDto>()
                {
                    new StageForCreationDto()
                    {
                        StageType = StageType.Class,
                        SequenceId = 1,
                        StageData = new ClassStageDataForCreationDto()
                        {
                            Conditions = GenerateConditionsForCreationDtos(),
                        },
                    },
                },
                Material = GenerateMaterialForCreationDto(),
                FlexbomHri = "HRI1",
                FlexbomMrv = "00000000000D",
                DisplayName = string.Empty,
                ActivityType = ActivityType.ModuleValidation,
                Tags = new List<string>() { "tag1", "tag2" },
            };
        }

        public ExperimentForCreationDto GenerateExperimentsForCreationDtoWithoutActivityType()
        {
            return new ExperimentForCreationDto()
            {
                BomGroupName = $"BomGroup{new Random().Next(1, 100)}",
                Step = $"A{new Random().Next(1, 10)}",
                EnvironmentFileName = "env_file1.env",
                PlistAllFileName = "plist_file1.xml",
                TestTimeInSecPerUnit = 1,
                StplFileName = "sub_file1.stpl",
                TplFileName = "base_tpl1.tpl",
                Stages = new List<StageForCreationDto>()
                {
                    new StageForCreationDto()
                    {
                        StageType = StageType.Class,
                        SequenceId = 1,
                        StageData = new ClassStageDataForCreationDto()
                        {
                            Conditions = GenerateConditionsForCreationDtos(),
                        },
                    },
                },
                Material = GenerateMaterialForCreationDto(),
                FlexbomHri = "HRI1",
                FlexbomMrv = "00000000000D",
                DisplayName = string.Empty,
            };
        }

        public IEnumerable<ExperimentForCreationDto> GenerateListOfExperimentsForCreationDtos(uint numberOfExperiments = 1)
        {
            return TestDataGenerationHelper.GenerateList(GenerateExperimentsForCreationDto, numberOfExperiments);
        }

        public IEnumerable<ExperimentForCreationDto> GenerateListOfExperimentsForCreationDtosWithoutActivtyType(uint numberOfExperiments = 1)
        {
            return TestDataGenerationHelper.GenerateList(GenerateExperimentsForCreationDtoWithoutActivityType, numberOfExperiments);
        }

        public IEnumerable<ExperimentDto> GenerateListOfExperimentDtos(uint numberOfExperiments = 1)
        {
            return TestDataGenerationHelper.GenerateList(GenerateExperimentDto, numberOfExperiments);
        }

        public StatusForUpdateDto GenerateStatusForUpdateDto()
        {
            Array values = Enum.GetValues(typeof(ProcessStatus));
            var randomStatus = (ProcessStatus)values.GetValue(Random.Next(values.Length));
            return new StatusForUpdateDto()
            {
                CorrelationId = Guid.NewGuid(),
                Status = randomStatus,
            };
        }

        public Dtos.Orchestrator.ExperimentGroupUpdateDtos.ExperimentGroupForUpdateDto GenerateExperimentGroupWorkFlowUpdateDto()
        {
            return new Dtos.Orchestrator.ExperimentGroupUpdateDtos.ExperimentGroupForUpdateDto
            {
                ExperimentGroupId = Guid.NewGuid(),
                Wwid = uint.MinValue,
                Username = "abcdefg",
                StplDirectory = "StplDirectory",
                Experiments = Enumerable.Empty<ExperimentForUpdateDto>(),
                Segment = "Segment",
                Team = "Team",
            };
        }

        public VpoForUpdateDto GenerateVpoForUpdateDto()
        {
            return new VpoForUpdateDto()
            {
                CorrelationId = Guid.NewGuid(),
                Vpo = "mockVpo",
                IsFinishedSuccessfully = true,
            };
        }

        public VpoForUpdateDto GenerateVpoFailureForUpdateDto()
        {
            return new VpoForUpdateDto()
            {
                CorrelationId = Guid.NewGuid(),
                ErrorMessage = "some error",
            };
        }

        public ResultsForUpdateDto GenerateResultForUpdateDto()
        {
            return new ResultsForUpdateDto()
            {
                ConditionId = Guid.NewGuid(),
                ConditionResultForUpdate = new ConditionResultForUpdateDto()
                {
                    NumberOfGoodBins = TestDataGenerationHelper.GenerateRandomUint(),
                    NumberOfBadBins = TestDataGenerationHelper.GenerateRandomUint(),
                },
            };
        }

        public IEnumerable<Condition> GenerateConditions()
        {
            return new List<Condition>()
            {
                new Condition()
                {
                    Id = Guid.NewGuid(),
                    Status = ProcessStatus.PendingCommit,
                    EngineeringId = "AA",
                    LocationCode = "7721",
                    Thermals = new List<Thermal> { new Thermal { Name = "HOT", SequenceId = 1 } },
                    CompletionTime = null,
                    ModificationTime = null,
                    Comment = string.Empty,
                    CustomAttributes = new List<CustomAttribute>
                    {
                        new CustomAttribute
                        {
                            AttributeName = "Attribute1",
                            AttributeValue = "Attribute Value 1",
                        },
                    },
                },
                new Condition()
                {
                    Id = Guid.NewGuid(),
                    Status = ProcessStatus.Completed,
                    EngineeringId = "AA",
                    LocationCode = "7721",
                    Thermals = new List<Thermal> { new Thermal {Name = "HOT", SequenceId = 1 } },
                    CompletionTime = null,
                    ModificationTime = null,
                    Comment = string.Empty,
                    Results = new ConditionResult(),
                },
                new Condition()
                {
                    Id = Guid.NewGuid(),
                    Status = ProcessStatus.NotStarted,
                    EngineeringId = "AA",
                    LocationCode = "7721",
                    Thermals = new List<Thermal> { new Thermal { Name = "HOT", SequenceId = 1 } },
                    CompletionTime = null,
                    ModificationTime = null,
                    Comment = string.Empty,
                },
                new Condition()
                {
                    Id = Guid.NewGuid(),
                    Status = ProcessStatus.Paused,
                    EngineeringId = "AA",
                    LocationCode = "7721",
                    Thermals = new List<Thermal> { new Thermal { Name = "HOT", SequenceId = 1 } },
                    CompletionTime = null,
                    ModificationTime = null,
                    Comment = string.Empty,
                },
            };
        }

        public Experiment GenerateExperimentWithStatusCondition(Guid experimentId, params ProcessStatus[] status)
        {
            return new Experiment()
            {
                Id = experimentId,
                Stages = new List<Stage>()
                {
                    new Stage()
                    {
                        StageType = StageType.Class,
                        StageData = new ClassStageData()
                        {
                            Conditions = status.Select(s => new Condition() { Status = s }),
                        },
                    },
                },
            };
        }

        public ExperimentGroupForUpdateDto GenerateExperimentGroupForEdit()
        {
            return new ExperimentGroupForUpdateDto()
            {
                TestProgramData = new TestProgramDataForUpdateDto()
                {
                    BaseTestProgramName = "59Tp2",
                    TpName = "TpNameUpdated",
                    ProgramFamily = "IceLake",
                    ProgramSubFamily = "Class_ICLU_TE_Step",
                    StplDirectory = "\\\\ger.corp.intel.com\\ec\\proj\\mdl\\ha\\intel\\hdmxprogs\\icl\\ICLXXXXDXH59T20S035\\TPL",
                    TestProgramRootDirectory = "\\\\ger.corp.intel.com\\ec\\proj\\mdl\\ha\\intel\\hdmxprogs\\icl\\ICLXXXXDXH59T20S035\\TPL",
                },
                Experiments = new List<Dtos.Update.ExperimentForUpdateDto>()
                {
                    new Dtos.Update.ExperimentForUpdateDto()
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
                                    OriginalPartType = "HH 6HY789 A D",
                                    VisualId = "VisualId11",
                                    PartType = "HH 6HY789 A J",
                                    Lot = "LotName",
                                    Lab = "LabName",
                                    Location = new LocationForUpdateDto
                                    {
                                        LocationType = "LocationType",
                                        LocationName = "LocationName",
                                    },
                                },
                            },
                            Lots = new List<LotForUpdateDto>()
                            {
                                 new LotForUpdateDto
                                {
                                    PartType = "HH 6HY789 A Z",
                                    NumberOfUnitsToRun = 1,
                                    Name = "LotName",
                                    Lab = "LabName",
                                    OriginalPartType = "HH 6HY789 A J",
                                    Locations = new List<LocationWithQuantityForUpdateDto>
                                    {
                                        new LocationWithQuantityForUpdateDto
                                        {
                                            LocationType = "LocationType",
                                            LocationName = "LocationName",
                                            Quantity = 1,
                                        },
                                    },
                                },
                            },
                            Sspec = "string",
                        },
                        Conditions = new List<ConditionForUpdateDto>()
                        {
                            new ConditionForUpdateDto
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
                        SequenceId = 0,
                        DisplayName = "TestProgram",
                        RetestRate = 1,
                        FlexbomHri = "string",
                        FlexbomMrv = "string",
                        ExperimentType = ExperimentType.Engineering,
                        ActivityType = ActivityType.ModuleValidation,
                        Recipe = new RecipeForUpdateDto()
                        {
                            RecipePath = "string",
                            RecipeSource = RecipeSource.TpGenerated,
                        },
                        ExperimentSplitId = default(Guid),
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

        private static IEnumerable<ConditionForCreationDto> GenerateConditionsForCreationDtos()
        {
            return new List<ConditionForCreationDto>
            {
                new ConditionForCreationDto
                {
                    EngineeringId = "AA",
                    LocationCode = "7721",
                    Thermals = new List<ThermalForCreationDto> { new ThermalForCreationDto { Name = "HOT", SequenceId = 1 } },
                    CustomAttributes = new List<CustomAttributeForCreationDto>
                    {
                        new CustomAttributeForCreationDto
                        {
                            AttributeName = "Attribute1",
                            AttributeValue = "Value For Attribute 1",
                        },
                    },
                },
            };
        }

        private static MaterialForCreationDto GenerateMaterialForCreationDto()
        {
            return new MaterialForCreationDto
            {
                Units = new List<UnitForCreationDto>
                {
                    new UnitForCreationDto
                    {
                        PartType = "H64DXHPVAA",
                        Lab = "CRML",
                        Location = new LocationForCreationDto { LocationName = "FLOOR", LocationType = "ACTIVcE"},
                        Lot = "Y1234567EN",
                        VisualId = "1234567890",
                    },
                },
                Lots = new List<LotForCreationDto>
                {
                    new LotForCreationDto
                    {
                        PartType = "H64DXHPVAA",
                        Lab = "CRML",
                        Locations = new List<LocationWithQuantityForCreationDto>
                        {
                            new LocationWithQuantityForCreationDto
                            {
                                LocationName = "FLOOR", LocationType = "ACTIVcE", Quantity = 50,
                            },
                        },
                        Name = "Y1234567EN",
                        NumberOfUnitsToRun = 20,
                    },
                },
            };
        }

        private static TestProgramData GenerateTestProgramData()
        {
            return new TestProgramData
            {
                BaseTestProgramName = "baseName",
                TpName = "tpName2",
                ProgramFamily = "ICL",
                ProgramSubFamily = "ICL 42",
                StplDirectory = @"\\ger.corp.intel.com\ex\proj\mdl\ha\intel\hdmxprogs\ICL\1234567\TPL",
                DirectoryChecksum = "checksum",
                TestProgramRootDirectory = "TestProgramRootDirectory",
            };
        }

        private static TestProgramDataForCreationDto GenerateTestProgramDataForCrreationDto()
        {
            return new TestProgramDataForCreationDto
            {
                BaseTestProgramName = "baseName",
                ProgramFamily = "ICL",
                TpName = "TpName",
                ProgramSubFamily = "ICL 42",
                StplDirectory = @"\\ger.corp.intel.com\ex\proj\mdl\ha\intel\hdmxprogs\ICL\1234567\TPL",
                DirectoryChecksum = "checksum",
                TestProgramRootDirectory = "TestProgramRootDirectory",
            };
        }

        private static Material GenerateMaterial()
        {
            return new Material
            {
                Units = new List<Unit>
                {
                    new Unit
                    {
                        PartType = "H64DXHPVAA",
                        Lab = "CRML",
                        Location = new Location {LocationName = "FLOOR", LocationType = "ACTIVcE"},
                        Lot = "Y1234567EN",
                        VisualId = "1234567890",
                    },
                },
                Lots = new List<Lot>
                {
                    new Lot
                    {
                        PartType = "H64DXHPVAA",
                        Lab = "CRML",
                        Locations = new List<LocationWithQuantity>
                        {
                            new LocationWithQuantity
                            {
                                LocationName = "FLOOR", LocationType = "ACTIVcE", Quantity = 50,
                            },
                        },
                        Name = "Y1234567EN",
                        NumberOfUnitsToRun = 20,
                    },
                },
            };
        }

        private PredictionRecordDto GenerateExperimentGroupDtoWithoutExperiments()
        {
            return new PredictionRecordDto()
            {
                Id = Guid.NewGuid(),
                Username = TestDataGenerationHelper.GenerateUserName(),
            };
        }
    }
}