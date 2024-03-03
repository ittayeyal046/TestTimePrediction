using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTPService.Dtos;
using TTPService.Enums;
using TTPService.Helpers;

namespace TTPService.Tests.Helpers
{
    [TestClass]
    public class StageDtoJsonConverterFixtureL0
    {
        private static readonly Guid Id = Guid.NewGuid();
        private static readonly Guid ConditionId1 = Guid.NewGuid();
        private static readonly Guid ConditionId2 = Guid.NewGuid();
        private StageDtoJsonConverter _sut;
        private JsonSerializerOptions _options;

        [TestInitialize]
        public void Init()
        {
            _sut = new StageDtoJsonConverter();

            _options = new JsonSerializerOptions();
            _options.Converters.Add(new JsonStringEnumConverter());
        }

        [TestMethod]
        [DynamicData(nameof(GetDtoDataForStageDto), DynamicDataSourceType.Method)]
        public void Write_HappyFlow(StageDto dto, string expected)
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
            var dto = new StageDto
            {
                Id = Id,
                StageType = (StageType)50,
                SequenceId = 1,
                StageData = new ClassStageDataDto(),
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
        [DynamicData(nameof(GetDtoDataForStageDto), DynamicDataSourceType.Method)]
        public void Read_HappyFlow(StageDto expected, string jsonString)
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
            var actual = _sut.Read(ref reader, typeof(StageDto), _options);

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
            var jsonString = "{\"Id\":\"" + Id + "\",\"StageType\":\"Ppv\",\"SequenceId\":3,\"StageData\":{\"TestType\":\"Ppvs\",\"Operation\":\"operation\",\"Recipe\":\"recipe\",\"Qdf\":\"Q8VB\",\"Comment\":\"DO NOT RUN\",\"ModificationTime\":\"0001-01-01T00:00:00\",\"CompletionTime\":null,\"Status\":\"NotStarted\"}}";
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
            ReadOnlySpan<byte> jsonUtf8 = new ReadOnlySpan<byte>(byteArray);
            var reader = new Utf8JsonReader(jsonUtf8, true, default);
            using var jsonDocument = JsonDocument.ParseValue(ref reader);

            try
            {
                // Act
                _sut.Read(ref reader, typeof(StageDto), _options);
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
            var jsonString = "{\"Id\":\"" + Id + "\",\"StageType\":\"Ppv\",\"SequenceId\":3,\"StageData\":{\"TestType\":\"Ppvs\",\"Operation\":\"operation\",\"Recipe\":\"recipe\",\"Qdf\":\"Q8VB\",\"Comment\":\"DO NOT RUN\",\"ModificationTime\":\"0001-01-01T00:00:00\",\"CompletionTime\":null,\"Status\":\"NotStarted\"}}";
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
            ReadOnlySpan<byte> jsonUtf8 = new ReadOnlySpan<byte>(byteArray);
            var reader = new Utf8JsonReader(jsonUtf8, true, default);

            try
            {
                // Act
                _sut.Read(ref reader, typeof(StageDto), _options);
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
            var jsonString = "{\"Id\":\"" + Id + "\",\"StageType\":\"" + unknownStageType + "\",\"SequenceId\":3,\"StageData\":{\"TestType\":\"Ppvs\",\"Operation\":\"operation\",\"Recipe\":\"recipe\",\"Qdf\":\"Q8VB\",\"Comment\":\"DO NOT RUN\",\"ModificationTime\":\"0001-01-01T00:00:00\",\"CompletionTime\":null,\"Status\":\"NotStarted\"}}";
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
                _sut.Read(ref reader, typeof(StageDto), _options);
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
            var jsonString = "{\"Id\":\"" + Id + "\",\"StageType\":\"Ppv\",\"SequenceId\":3}";
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
                _sut.Read(ref reader, typeof(StageDto), _options);
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
            var jsonString = "{\"Id\":\"" + Id + "\",\"StageType\":\"Ppv\",\"StageData\":{\"TestType\":\"Ppvs\",\"Operation\":\"operation\",\"Recipe\":\"recipe\",\"Qdf\":\"Q8VB\",\"Comment\":\"DO NOT RUN\",\"ModificationTime\":\"0001-01-01T00:00:00\",\"CompletionTime\":null,\"Status\":\"NotStarted\"}}";
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
                _sut.Read(ref reader, typeof(StageDto), _options);
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
        public void Read_MissingId_ThrowsException()
        {
            // Arrange
            var jsonString = "{\"StageType\":\"Ppv\",\"StageData\":{\"TestType\":\"Ppvs\",\"Operation\":\"operation\",\"Recipe\":\"recipe\",\"Qdf\":\"Q8VB\",\"Comment\":\"DO NOT RUN\",\"ModificationTime\":\"0001-01-01T00:00:00\",\"CompletionTime\":null,\"Status\":\"NotStarted\"}}";
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
                _sut.Read(ref reader, typeof(StageDto), _options);
            }
            catch (JsonException ex)
            {
                ex.Message.Should().Be("Could not get Id");
                return;
            }

            // Assert
            Assert.Fail("The test should throw JsonException");
        }

        [TestMethod]
        public void Read_MissingStageType_ThrowsException()
        {
            // Arrange
            var jsonString = "{\"Id\":\"" + Id + "\",\"SequenceId\":3,\"StageData\":{\"TestType\":\"Ppvs\",\"Operation\":\"operation\",\"Recipe\":\"recipe\",\"Qdf\":\"Q8VB\",\"Comment\":\"DO NOT RUN\",\"ModificationTime\":\"0001-01-01T00:00:00\",\"CompletionTime\":null,\"Status\":\"NotStarted\"}}";
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
                _sut.Read(ref reader, typeof(StageDto), _options);
            }
            catch (JsonException ex)
            {
                ex.Message.Should().Be("Could not get StageType");
                return;
            }

            // Assert
            Assert.Fail("The test should throw JsonException");
        }

        private static IEnumerable<object[]> GetDtoDataForStageDto()
        {
            yield return new object[]
            {
                new StageDto()
                {
                    Id = Id,
                    StageType = StageType.Class,
                    SequenceId = 1,
                    StageData = new ClassStageDataDto()
                    {
                        Conditions = new List<ConditionDto>
                        {
                            new ConditionDto
                            {
                                Id = ConditionId1,
                                ModificationTime = DateTime.Parse("0001-01-01T00:00:00"),
                                CompletionTime = null,
                                LocationCode = null,
                                Thermals = new[]
                                {
                                    new ThermalDto()
                                    {
                                        Name = "23.3",
                                        SequenceId = 1,
                                        IsByPassed = false,
                                    },
                                },
                                EngineeringId = "--",
                                Status = ProcessStatus.NotStarted,
                                StatusChangeComment = null,
                                Comment = "turn on the air conditioner",
                                SequenceId = 1,
                                FuseEnabled = null,
                                DieSelection = "N/A",
                                MoveUnits = null,
                                Fuse = null,
                            },
                            new ConditionDto
                            {
                                Id = ConditionId2,
                                ModificationTime = DateTime.Parse("0001-01-01T00:00:00"),
                                CompletionTime = null,
                                LocationCode = null,
                                Thermals = new[]
                                {
                                    new ThermalDto()
                                    {
                                        Name = "23.3",
                                        SequenceId = 1,
                                        IsByPassed = false,
                                    },
                                },
                                EngineeringId = "--",
                                Status = ProcessStatus.NotStarted,
                                StatusChangeComment = null,
                                Comment = "turn on the air conditioner please",
                                SequenceId = 2,
                                FuseEnabled = null,
                                DieSelection = "N/A",
                                MoveUnits = null,
                                Fuse = null,
                            },
                        },
                    },
                },
                "{\"Id\":\"" + Id + "\",\"stageType\":\"Class\",\"sequenceId\":1,\"stageData\":{\"Conditions\":[{\"Id\":\"" + ConditionId1 + "\",\"ModificationTime\":\"0001-01-01T00:00:00\",\"CompletionTime\":null,\"LocationCode\":null,\"Thermals\":[{\"Name\":\"23.3\",\"SequenceId\":1,\"IsByPassed\":false}],\"EngineeringId\":\"--\",\"Status\":\"NotStarted\",\"StatusChangeComment\":null,\"Comment\":\"turn on the air conditioner\",\"SequenceId\":1,\"FuseEnabled\":null,\"DieSelection\":\"N/A\",\"MoveUnits\":null,\"Fuse\":null,\"VpoCustomSuffix\":null,\"CustomAttributes\":null},{\"Id\":\"" + ConditionId2 + "\",\"ModificationTime\":\"0001-01-01T00:00:00\",\"CompletionTime\":null,\"LocationCode\":null,\"Thermals\":[{\"Name\":\"23.3\",\"SequenceId\":1,\"IsByPassed\":false}],\"EngineeringId\":\"--\",\"Status\":\"NotStarted\",\"StatusChangeComment\":null,\"Comment\":\"turn on the air conditioner please\",\"SequenceId\":2,\"FuseEnabled\":null,\"DieSelection\":\"N/A\",\"MoveUnits\":null,\"Fuse\":null,\"VpoCustomSuffix\":null,\"CustomAttributes\":null}],\"ConditionsWithResults\":null}}",
            };

            yield return new object[]
            {
                new StageDto()
                {
                    Id = Id,
                    StageType = StageType.Olb,
                    SequenceId = 2,
                    StageData = new OlbStageDataDto()
                    {
                        MoveUnits = MoveUnits.All,
                        Operation = "op",
                        Qdf = "qdf",
                        Recipe = "recipe",
                        BllFiles = new BllFilesDto
                        {
                            ScriptFile = "script",
                            ConstFile = "const",
                        },
                        Comment = "I need some ice-cream",
                        ModificationTime = DateTime.Parse("0001-01-01T00:00:00"),
                        CompletionTime = null,
                        Status = ProcessStatus.NotStarted,
                    },
                },
                "{\"id\":\"" + Id + "\",\"stageType\":\"Olb\",\"sequenceId\":2,\"stageData\":{\"MoveUnits\":\"All\",\"Operation\":\"op\",\"Qdf\":\"qdf\",\"Recipe\":\"recipe\",\"BllFiles\":{\"ScriptFile\":\"script\",\"ConstFile\":\"const\"},\"Comment\":\"I need some ice-cream\",\"ModificationTime\":\"0001-01-01T00:00:00\",\"CompletionTime\":null,\"Status\":\"NotStarted\"}}",
            };

            yield return new object[]
            {
                new StageDto()
                {
                    Id = Id,
                    StageType = StageType.Ppv,
                    SequenceId = 3,
                    StageData = new PpvStageDataDto
                    {
                        TestType = TestType.Ppvm,
                        Operation = "op",
                        Recipe = "recipe",
                        Qdf = "qdf",
                        Comment = "Maybe some chocolate ice-cream",
                        ModificationTime = DateTime.Parse("0001-01-01T00:00:00"),
                        CompletionTime = null,
                        Status = ProcessStatus.NotStarted,
                    },
                },
                "{\"id\":\"" + Id + "\",\"stageType\":\"Ppv\",\"sequenceId\":3,\"stageData\":{\"TestType\":\"Ppvm\",\"Operation\":\"op\",\"Recipe\":\"recipe\",\"Qdf\":\"qdf\",\"Comment\":\"Maybe some chocolate ice-cream\",\"ModificationTime\":\"0001-01-01T00:00:00\",\"CompletionTime\":null,\"Status\":\"NotStarted\"}}",
            };

            yield return new object[]
            {
                new StageDto()
                {
                    Id = Id,
                    StageType = StageType.Maestro,
                    SequenceId = 4,
                    StageData = new MaestroStageDataDto
                    {
                        Operation = "1234",
                        EngId = "AB",
                        TpEnvironment = TpEnvironment.Production,
                        Comment = "Bamba",
                        ModificationTime = DateTime.Parse("0001-01-01T00:00:00"),
                        CompletionTime = null,
                        Status = ProcessStatus.NotStarted,
                    },
                },
                "{\"id\":\"" + Id + "\",\"stageType\":\"Maestro\",\"sequenceId\":4,\"stageData\":{\"Operation\":\"1234\",\"EngId\":\"AB\",\"TpEnvironment\":\"Production\",\"Comment\":\"Bamba\",\"ModificationTime\":\"0001-01-01T00:00:00\",\"CompletionTime\":null,\"Status\":\"NotStarted\"}}",
            };
        }
    }
}
