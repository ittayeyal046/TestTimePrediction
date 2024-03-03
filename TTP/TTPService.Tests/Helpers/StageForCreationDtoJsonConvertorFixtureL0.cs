using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos.Creation;
using TTPService.Enums;
using TTPService.Helpers;

namespace TTPService.Tests.Helpers
{
    [TestClass]
    public class StageForCreationDtoJsonConvertorFixtureL0
    {
        private StageForCreationDtoJsonConverter _sut;
        private JsonSerializerOptions _options;

        [TestInitialize]
        public void Init()
        {
            _sut = new StageForCreationDtoJsonConverter();

            _options = new JsonSerializerOptions();
            _options.
                Converters.Add(new JsonStringEnumConverter());
        }

        [TestMethod]
        [DynamicData(nameof(GetDtoDataForStageForCreationDto), DynamicDataSourceType.Method)]
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
            var dto = new StageForCreationDto
            {
                StageType = (StageType)50,
                SequenceId = 1,
                StageData = new ClassStageDataForCreationDto(),
            };

            // Act
            try
            {
                _sut.Write(writer, dto, _options);
            }
            catch (JsonException ex)
            {
                ex.Message.Should().Be("50 is not a known stage type");
                return;
            }

            Assert.Fail("The test should throw JsonException");
        }

        [TestMethod]
        [DynamicData(nameof(GetDtoDataForStageForCreationDto), DynamicDataSourceType.Method)]
        public void Read_HappyFlow(StageForCreationDto expected, string jsonString)
        {
            // Arrange
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
            ReadOnlySpan<byte> jsonUtf8 = new ReadOnlySpan<byte>(byteArray);
            var reader = new Utf8JsonReader(jsonUtf8, true, default);

            // Advance the Utf8JsonReader until it is positioned on JsonTokenType.StartObject
            while (reader.TokenType == JsonTokenType.None)
            {
                if (!reader.Read())
                {
                    break;
                }
            }

            // Act
            var actual = _sut.Read(ref reader, typeof(StageForCreationDto), _options);

            // Assert
            actual.Should().BeEquivalentTo(expected, opt =>
            {
                opt.IncludingAllRuntimeProperties();
                opt.IncludingNestedObjects();
                opt.AllowingInfiniteRecursion();
                return opt;
            });
        }

        [TestMethod]
        public void Read_InvalidTokenType_ThrowsException()
        {
            // Arrange
            var jsonString = "{\"StageType\":\"Ppv\",\"SequenceId\":3,\"StageData\":{\"TestType\":\"Ppvs\",\"Operation\":\"operation\",\"Recipe\":\"recipe\",\"Qdf\":\"Q8VB\",\"Comment\":\"DO NOT RUN\"}}";
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
            ReadOnlySpan<byte> jsonUtf8 = new ReadOnlySpan<byte>(byteArray);
            var reader = new Utf8JsonReader(jsonUtf8, true, default);
            using var jsonDocument = JsonDocument.ParseValue(ref reader);

            try
            {
                // Act
                _sut.Read(ref reader, typeof(StageForCreationDto), _options);
            }
            catch (JsonException ex)
            {
                ex.Message.Should().Be("Token type is not StartObject");
                return;
            }

            // Assert
            Assert.Fail("The test should throw JsonException");
        }

        [TestMethod]
        public void Read_MissingTokenTypeStartObject_ThrowsException()
        {
            // Arrange
            var jsonString = "{\"StageType\":\"Ppv\",\"SequenceId\":3,\"StageData\":{\"TestType\":\"Ppvs\",\"Operation\":\"operation\",\"Recipe\":\"recipe\",\"Qdf\":\"Q8VB\",\"Comment\":\"DO NOT RUN\"}}";
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
            ReadOnlySpan<byte> jsonUtf8 = new ReadOnlySpan<byte>(byteArray);
            var reader = new Utf8JsonReader(jsonUtf8, true, default);

            try
            {
                // Act
                _sut.Read(ref reader, typeof(StageForCreationDto), _options);
            }
            catch (JsonException ex)
            {
                ex.Message.Should().Be("Token type is not StartObject");
                return;
            }

            // Assert
            Assert.Fail("The test should throw JsonException");
        }

        [TestMethod]
        public void Read_UnknownStageType_ThrowsException()
        {
            // Arrange
            var unknownStageType = 50;
            var jsonString = "{\"StageType\":\"" + unknownStageType + "\",\"SequenceId\":3,\"StageData\":{\"TestType\":\"Ppvs\",\"Operation\":\"operation\",\"Recipe\":\"recipe\",\"Qdf\":\"Q8VB\",\"Comment\":\"DO NOT RUN\"}}";
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
            ReadOnlySpan<byte> jsonUtf8 = new ReadOnlySpan<byte>(byteArray);
            var reader = new Utf8JsonReader(jsonUtf8, true, default);

            // Advance the Utf8JsonReader until it is positioned on JsonTokenType.StartObject
            while (reader.TokenType == JsonTokenType.None)
            {
                if (!reader.Read())
                {
                    break;
                }
            }

            try
            {
                // Act
                _sut.Read(ref reader, typeof(StageForCreationDto), _options);
            }
            catch (JsonException ex)
            {
                ex.Message.Should().Be($"Invalid StageType: {unknownStageType}");
                return;
            }

            // Assert
            Assert.Fail("The test should throw JsonException");
        }

        [TestMethod]
        public void Read_MissingStageData_ThrowsException()
        {
            // Arrange
            var jsonString = "{\"StageType\":\"Ppv\",\"SequenceId\":3}";
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
            ReadOnlySpan<byte> jsonUtf8 = new ReadOnlySpan<byte>(byteArray);
            var reader = new Utf8JsonReader(jsonUtf8, true, default);

            // Advance the Utf8JsonReader until it is positioned on JsonTokenType.StartObject
            while (reader.TokenType == JsonTokenType.None)
            {
                if (!reader.Read())
                {
                    break;
                }
            }

            try
            {
                // Act
                _sut.Read(ref reader, typeof(StageForCreationDto), _options);
            }
            catch (JsonException ex)
            {
                ex.Message.Should().Be("Could not get StageData");
                return;
            }

            // Assert
            Assert.Fail("The test should throw JsonException");
        }

        [TestMethod]
        public void Read_MissingSequenceId_ThrowsException()
        {
            // Arrange
            var jsonString = "{\"StageType\":\"Ppv\",\"StageData\":{\"TestType\":\"Ppvs\",\"Operation\":\"operation\",\"Recipe\":\"recipe\",\"Qdf\":\"Q8VB\",\"Comment\":\"DO NOT RUN\"}}";
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
            ReadOnlySpan<byte> jsonUtf8 = new ReadOnlySpan<byte>(byteArray);
            var reader = new Utf8JsonReader(jsonUtf8, true, default);

            // Advance the Utf8JsonReader until it is positioned on JsonTokenType.StartObject
            while (reader.TokenType == JsonTokenType.None)
            {
                if (!reader.Read())
                {
                    break;
                }
            }

            try
            {
                // Act
                _sut.Read(ref reader, typeof(StageForCreationDto), _options);
            }
            catch (JsonException ex)
            {
                ex.Message.Should().Be("Could not get SequenceId");
                return;
            }

            // Assert
            Assert.Fail("The test should throw JsonException");
        }

        [TestMethod]
        public void Read_MissingStageType_ThrowsException()
        {
            // Arrange
            var jsonString = "{\"SequenceId\":3,\"StageData\":{\"TestType\":\"Ppvs\",\"Operation\":\"operation\",\"Recipe\":\"recipe\",\"Qdf\":\"Q8VB\",\"Comment\":\"DO NOT RUN\"}}";
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
            ReadOnlySpan<byte> jsonUtf8 = new ReadOnlySpan<byte>(byteArray);
            var reader = new Utf8JsonReader(jsonUtf8, true, default);

            // Advance the Utf8JsonReader until it is positioned on JsonTokenType.StartObject
            while (reader.TokenType == JsonTokenType.None)
            {
                if (!reader.Read())
                {
                    break;
                }
            }

            try
            {
                // Act
                _sut.Read(ref reader, typeof(StageForCreationDto), _options);
            }
            catch (JsonException ex)
            {
                ex.Message.Should().Be("Could not get StageType");
                return;
            }

            // Assert
            Assert.Fail("The test should throw JsonException");
        }

        [TestMethod]
        public void Read_StageTypeIsNull_ThrowsException()
        {
            // Arrange
            var jsonString = "{\"StageType\":null,\"SequenceId\":3,\"StageData\":{\"TestType\":\"Ppvs\",\"Operation\":\"operation\",\"Recipe\":\"recipe\",\"Qdf\":\"Q8VB\",\"Comment\":\"DO NOT RUN\"}}";
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
            ReadOnlySpan<byte> jsonUtf8 = new ReadOnlySpan<byte>(byteArray);
            var reader = new Utf8JsonReader(jsonUtf8, true, default);

            // Advance the Utf8JsonReader until it is positioned on JsonTokenType.StartObject
            while (reader.TokenType == JsonTokenType.None)
            {
                if (!reader.Read())
                {
                    break;
                }
            }

            try
            {
                // Act
                _sut.Read(ref reader, typeof(StageForCreationDto), _options);
            }
            catch (JsonException ex)
            {
                ex.Message.Should().Be("Could not get StageType");
                return;
            }

            // Assert
            Assert.Fail("The test should throw JsonException");
        }

        private static IEnumerable<object[]> GetDtoDataForStageForCreationDto()
        {
            yield return new object[]
            {
                new StageForCreationDto
                {
                    StageType = StageType.Class,
                    SequenceId = 1,
                    StageData = new ClassStageDataForCreationDto()
                    {
                        Conditions = new List<ConditionForCreationDto>
                        {
                            new ConditionForCreationDto()
                            {
                                LocationCode = "operation",
                                Thermals = new List<ThermalForCreationDto>()
                                {
                                    new ThermalForCreationDto()
                                    {
                                        Name = "ClassCold",
                                        SequenceId = 1,
                                    },
                                },
                                SequenceId = 1,
                                EngineeringId = "c1",
                                Fuse = new FuseForCreationDto()
                                {
                                    FuseType = FuseType.TwoStepQdf,
                                    Qdf = "qdf1",
                                },
                                Comment = "DO NOT RUN",
                                FuseEnabled = false,
                                DieSelection = "die",
                                MoveUnits = MoveUnits.All,
                            },
                        },
                    },
                },
                "{\"stageType\":\"Class\",\"sequenceId\":1,\"stageData\":{\"Conditions\":[{\"LocationCode\":\"operation\",\"Thermals\":[{\"Name\":\"ClassCold\",\"SequenceId\":1,\"IsByPassed\":false}],\"EngineeringId\":\"c1\",\"SequenceId\":1,\"Comment\":\"DO NOT RUN\",\"FuseEnabled\":false,\"DieSelection\":\"die\",\"MoveUnits\":\"All\",\"Fuse\":{\"FuseType\":\"TwoStepQdf\",\"Lrf\":null,\"Qdf\":\"qdf1\"},\"VpoCustomSuffix\":null,\"CustomAttributes\":null}]}}",
            };

            yield return new object[]
            {
                new StageForCreationDto
                {
                    StageType = StageType.Olb,
                    SequenceId = 2,
                    StageData = new OlbStageDataForCreationDto()
                    {
                        Comment = "DO NOT RUN",
                        Operation = "operation",
                        Recipe = "recipe",
                        Qdf = "Q8VB",
                        MoveUnits = MoveUnits.All,
                        BllFiles = new BllFilesForCreationDto()
                        {
                            ScriptFile = "\\\\somepath\\yabadabadu\\wow\\kawabanga",
                            ConstFile = "\\\\somepath\\yabadabadu\\wow\\bazinga",
                        },
                    },
                },
                "{\"stageType\":\"Olb\",\"sequenceId\":2,\"stageData\":{\"MoveUnits\":\"All\",\"Operation\":\"operation\",\"Qdf\":\"Q8VB\",\"Recipe\":\"recipe\",\"BllFiles\":{\"ScriptFile\":\"\\\\\\\\somepath\\\\yabadabadu\\\\wow\\\\kawabanga\",\"ConstFile\":\"\\\\\\\\somepath\\\\yabadabadu\\\\wow\\\\bazinga\"},\"Comment\":\"DO NOT RUN\"}}",
            };

            yield return new object[]
            {
                new StageForCreationDto
                {
                    StageType = StageType.Ppv,
                    SequenceId = 3,
                    StageData = new PpvStageDataForCreationDto()
                    {
                        Comment = "DO NOT RUN",
                        Operation = "operation",
                        Recipe = "recipe",
                        Qdf = "Q8VB",
                        TestType = TestType.Ppvs,
                    },
                },
                "{\"stageType\":\"Ppv\",\"sequenceId\":3,\"stageData\":{\"TestType\":\"Ppvs\",\"Operation\":\"operation\",\"Recipe\":\"recipe\",\"Qdf\":\"Q8VB\",\"Comment\":\"DO NOT RUN\"}}",
            };

            yield return new object[]
            {
                new StageForCreationDto
                {
                    SequenceId = 3,
                    StageType = StageType.Ppv,
                    StageData = new PpvStageDataForCreationDto()
                    {
                        Comment = "Stage type not at the beginning",
                        Operation = "operation",
                        Recipe = "recipe",
                        Qdf = "Q8VB",
                        TestType = TestType.Ppvs,
                    },
                },
                "{\"stageType\":\"Ppv\",\"sequenceId\":3,\"stageData\":{\"TestType\":\"Ppvs\",\"Operation\":\"operation\",\"Recipe\":\"recipe\",\"Qdf\":\"Q8VB\",\"Comment\":\"Stage type not at the beginning\"}}",
            };

            yield return new object[]
            {
                new StageForCreationDto
                {
                    StageType = StageType.Maestro,
                    SequenceId = 4,
                    StageData = new MaestroStageDataForCreationDto()
                    {
                        Operation = "1234",
                        EngId = "AB",
                        TpEnvironment = TpEnvironment.Production,
                        Comment = "some comment",
                    },
                },
                "{\"stageType\":\"Maestro\",\"sequenceId\":4,\"stageData\":{\"Operation\":\"1234\",\"EngId\":\"AB\",\"TpEnvironment\":\"Production\",\"Comment\":\"some comment\"}}",
            };
        }
    }
}
