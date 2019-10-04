﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.Graylog.Core.MessageBuilders;
using Serilog.Sinks.Graylog.Tests;
using System;
using System.Collections.Generic;
using Xunit;

namespace Serilog.Sinks.Graylog.Core.Tests.MessageBuilders
{
    public class GelfMessageBuilderFixture
    {
        [Fact]
        public void WhenGetSimpleEvent_ThenResult_ShouldBeExpected()
        {
            var options = new GraylogSinkOptions();
            var target = new GelfMessageBuilder("localhost", options);

            var date = DateTimeOffset.Now;

            var expected = new
            {
                facility = "GELF",
                full_message = "abcdef\"zxc\"",
                host = "localhost",
                level = 2,
                short_message = "abcdef\"zxc\"",
                timestamp = date.DateTime,
                version = "1.1",
                _stringLevel = "Information",
                _TestProp = "\"zxc\"",
                _id_ = "\"asd\""
            };

            LogEvent logEvent = LogEventSource.GetSimpleLogEvent(date);

            string expectedString = JsonConvert.SerializeObject(expected, Newtonsoft.Json.Formatting.None);
            string actual = target.Build(logEvent).ToString(Newtonsoft.Json.Formatting.None);
            //actual.ShouldBeEquivalentTo(expectedString);
        }

        [Fact]
        [Trait("Category", "Debug")]
        public void TryComplexEvent()
        {
            var options = new GraylogSinkOptions();
            var target = new GelfMessageBuilder("localhost", options);

            DateTimeOffset date = DateTimeOffset.Now;

            LogEvent logEvent = LogEventSource.GetComplexEvent(date);

            string actual = target.Build(logEvent).ToString(Newtonsoft.Json.Formatting.None);
        }

        [Fact]
        public void GetSimpleLogEvent_GraylogSinkOptionsContainsHost_ReturnsOptionsHost()
        {
            //arrange
            GraylogSinkOptions options = new GraylogSinkOptions()
            {
                Host = "my_host"
            };
            GelfMessageBuilder messageBuilder = new GelfMessageBuilder("localhost", options);
            DateTime date = DateTime.UtcNow;
            string expectedHost = "my_host";

            //act
            LogEvent logEvent = LogEventSource.GetSimpleLogEvent(date);
            JObject actual = messageBuilder.Build(logEvent);
            string actualHost = actual.Value<string>("host");

            //assert
            Assert.Equal(expectedHost, actualHost);
        }

        [Fact]
        public static void WhenTryCreateLogEventWithNullKeyOrValue_ThenThrow()
        {
            //If in future this test fail then should add check for null in GelfMessageBuilder

            Assert.Throws<ArgumentNullException>(() =>
            {
                var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null,
                    new MessageTemplate("abcdef{TestProp}", new List<MessageTemplateToken>
                    {
                        new TextToken("abcdef", 0),
                        new PropertyToken("TestProp", "zxc", alignment: new Alignment(AlignmentDirection.Left, 3))
                    }), new List<LogEventProperty>
                    {
                        new LogEventProperty("TestProp", new ScalarValue("zxc")),
                        new LogEventProperty("id", new ScalarValue("asd")),
                        new LogEventProperty("Oo", null),
                        new LogEventProperty(null, null),
                        new LogEventProperty("StructuredProperty",
                            new StructureValue(new List<LogEventProperty>
                            {
                                new LogEventProperty("id", new ScalarValue(1)),
                                new LogEventProperty("_TestProp", new ScalarValue(3)),
                            }, "TypeTag"))
                    });
            });
        }
    }
}