using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Orchestrator.ExperimentGroupCreationDtos;
using TTPService.Enums;
using TTPService.Helpers;

namespace TTPService.Tests.Helpers
{
    [TestClass]
    public class StageForCreationOrchestratorDtoJsonConvertorFixtureL0
    {
        private static readonly Guid StageId = Guid.NewGuid();
        private StageForCreationOrchestratorDtoJsonConverter _sut;
        private JsonSerializerOptions _options;

        [TestInitialize]
        public void Init()
        {
            _sut = new StageForCreationOrchestratorDtoJsonConverter();

            _options = new JsonSerializerOptions();
            _options.Converters.Add(new JsonStringEnumConverter());
        }

        [TestMethod]
        [DynamicData(nameof(GetDtoDataForStageDto), DynamicDataSourceType.Method)]
        public void Write_HappyFlow(StageForCreationDto dto, string expected)
        {
            // Arrange
            var bufferWriter = new ArrayBufferWriter<byte>();
            var writer = new Utf8JsonWriter(bufferWriter);

            // Act
            _sut.Write(writer, dto, _options);
            writer.Dispose();
            var writtenSpan = bufferWriter.WrittenSpan;
            var actual = Encoding.UTF8.GetString(writtenSpan);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void Write_UnknownStageType_ThrowsError()
        {
            // Arrange
            var bufferWriter = new ArrayBufferWriter<byte>();
            var writer = new Utf8JsonWriter(bufferWriter);
            var dto = new StageForCreationDto()
            {
                StageType = (StageType)50,
                SequenceId = 1,
                StageData = new ClassStageDataForCreationDto(),
            };

            try
            {
                // Act
                _sut.Write(writer, dto, _options);
            }
            catch (JsonException ex)
            {
                ex.Message.Should().Be("50 is not a known step type");
                return;
            }

            // Assert
            Assert.Fail("The test should throw JsonException");
        }

        private static IEnumerable<object[]> GetDtoDataForStageDto()
        {
            yield return new object[]
            {
                new StageForCreationDto()
                {
                    StageId = StageId,
                    StageType = StageType.Class,
                    SequenceId = 1,
                    StageData = new ClassStageDataForCreationDto()
                    {
                        Conditions = new ConditionForCreationDto[]
                        {
                            new ConditionForCreationDto()
                            {
                                EngineeringId = "--",
                                Thermals = new List<ThermalForCreationDto>()
                                {
                                    new ThermalForCreationDto()
                                    {
                                        Name = "chuck norris",
                                        SequenceId = 1,
                                    },
                                },
                                FuseEnabled = false,
                                DieSelection = "N/A",
                                MoveUnits = MoveUnits.All,
                                LocationCode = "666",
                                Comment = "turn on the air conditioner",
                            },
                        },
                    },
                },
                "{\"stageType\":\"Class\",\"sequenceId\":1,\"stageId\":\"" + StageId + "\",\"stageData\":{\"Conditions\":[{\"ConditionId\":\"00000000-0000-0000-0000-000000000000\",\"LocationCode\":\"666\",\"SequenceId\":0,\"Thermals\":[{\"Name\":\"chuck norris\",\"SequenceId\":1}],\"EngineeringId\":\"--\",\"Status\":null,\"Comment\":\"turn on the air conditioner\",\"FuseEnabled\":false,\"DieSelection\":\"N/A\",\"MoveUnits\":\"All\",\"Fuse\":null,\"VpoCustomSuffix\":null,\"CustomAttributes\":null}]}}",
            };

            yield return new object[]
            {
                new StageForCreationDto()
                {
                    StageId = StageId,
                    StageType = StageType.Olb,
                    SequenceId = 2,
                    StageData = new OlbStageDataForCreationDto()
                    {
                        MoveUnits = MoveUnits.All,
                        Operation = "op",
                        Qdf = "qdf",
                        Recipe = "recipe",
                        Comment = "I need some ice-cream",
                        BllFiles = new BllFilesForCreationDto
                        {
                            ScriptFile = "script",
                            ConstFile = "const",
                        },
                    },
                },
                "{\"stageType\":\"Olb\",\"sequenceId\":2,\"stageId\":\"" + StageId + "\",\"stageData\":{\"MoveUnits\":\"All\",\"Operation\":\"op\",\"Qdf\":\"qdf\",\"Recipe\":\"recipe\",\"BllFiles\":{\"ScriptFile\":\"script\",\"ConstFile\":\"const\"},\"Comment\":\"I need some ice-cream\"}}",
            };

            yield return new object[]
            {
                new StageForCreationDto()
                {
                    StageId = StageId,
                    SequenceId = 3,
                    StageType = StageType.Ppv,
                    StageData = new PpvStageDataForCreationDto()
                    {
                        TestType = TestType.Ppvm,
                        Operation = "op",
                        Qdf = "qdf",
                        Recipe = "recipe",
                        Comment = "Maybe some chocolate ice-cream",
                    },
                },
                "{\"stageType\":\"Ppv\",\"sequenceId\":3,\"stageId\":\"" + StageId + "\",\"stageData\":{\"TestType\":\"Ppvm\",\"Operation\":\"op\",\"Recipe\":\"recipe\",\"Qdf\":\"qdf\",\"Comment\":\"Maybe some chocolate ice-cream\"}}",
            };

            yield return new object[]
            {
                new StageForCreationDto()
                {
                    StageId = StageId,
                    SequenceId = 4,
                    StageType = StageType.Maestro,
                    StageData = new MaestroStageDataForCreationDto()
                    {
                        Operation = "1234",
                        EngId = "AB",
                        TpEnvironment = TpEnvironment.Production,
                        Comment = "bamba is yummy.",
                    },
                },
                "{\"stageType\":\"Maestro\",\"sequenceId\":4,\"stageId\":\"" + StageId + "\",\"stageData\":{\"Operation\":\"1234\",\"EngId\":\"AB\",\"TpEnvironment\":\"Production\",\"Comment\":\"bamba is yummy.\"}}",
            };
        }
    }
}
