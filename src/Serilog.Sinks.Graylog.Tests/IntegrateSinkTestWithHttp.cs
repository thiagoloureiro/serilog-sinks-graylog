﻿using AutoFixture;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Graylog.Core.Helpers;
using Serilog.Sinks.Graylog.Core.Transport;
using Serilog.Sinks.Graylog.Tests.ComplexIntegrationTest;
using System;
using System.Linq;
using Xunit;

namespace Serilog.Sinks.Graylog.Tests
{
    [Trait("Category", "Integration")]
    public class IntegrateSinkTestWithHttp
    {
        [Fact]
        [Trait("Category", "Integration")]
        public void TestComplex()
        {
            var loggerConfig = new LoggerConfiguration();

            loggerConfig.WriteTo.Graylog(new GraylogSinkOptions
            {
                ShortMessageMaxLength = 50,
                MinimumLogEventLevel = LogEventLevel.Information,
                TransportType = TransportType.Http,
                Facility = "VolkovTestFacility",
                HostnameOrAddress = "http://logs.aeroclub.int",
                Port = 12201
            });

            var logger = loggerConfig.CreateLogger();

            var test = new TestClass
            {
                Id = 1,
                Bar = new Bar
                {
                    Id = 2,
                    Prop = "123"
                },
                TestPropertyOne = "1",
                TestPropertyThree = "3",
                TestPropertyTwo = "2"
            };

            logger.Information("SomeComplexTestEntry {@test}", test);
        }

        [Fact]
        public void WhenHostIsWrong_ThenLoggerCreationShouldNotBeFail()
        {
            var loggerConfig = new LoggerConfiguration();

            loggerConfig.WriteTo.Graylog(new GraylogSinkOptions
            {
                ShortMessageMaxLength = 50,
                MinimumLogEventLevel = LogEventLevel.Information,
                TransportType = TransportType.Http,
                Facility = "VolkovTestFacility",
                HostnameOrAddress = "abracadabra",
                Port = 12201
            });

            var logger = loggerConfig.CreateLogger();

            var test = new TestClass
            {
                Id = 1,
                Bar = new Bar
                {
                    Id = 2,
                    Prop = "123"
                },
                TestPropertyOne = "1",
                TestPropertyThree = "3",
                TestPropertyTwo = "2"
            };

            logger.Information("SomeComplexTestEntry {@test}", test);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void LogInformationWithLevel()
        {
            var fixture = new Fixture();
            fixture.Behaviors.Clear();
            fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));
            var profile = fixture.Create<Profile>();

            var loggerConfig = new LoggerConfiguration();

            loggerConfig.WriteTo.Graylog(new GraylogSinkOptions
            {
                MinimumLogEventLevel = LogEventLevel.Error,
                MessageGeneratorType = MessageIdGeneratortype.Timestamp,
                TransportType = TransportType.Http,
                Facility = "VolkovTestFacility",
                HostnameOrAddress = "http://logs.aeroclub.int",
                Port = 12201
            });

            var logger = loggerConfig.CreateLogger();

            logger.Information("battle profile:  {@BattleProfile}", profile);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void LogInformationWithOneProfile()
        {
            var fixture = new Fixture();
            fixture.Behaviors.Clear();
            fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));
            var profile = fixture.Create<Profile>();

            var loggerConfig = new LoggerConfiguration();

            loggerConfig.WriteTo.Graylog(new GraylogSinkOptions
            {
                MinimumLogEventLevel = LogEventLevel.Information,
                MessageGeneratorType = MessageIdGeneratortype.Timestamp,
                TransportType = TransportType.Http,
                Facility = "VolkovTestFacility",
                HostnameOrAddress = "http://logs.aeroclub.int",
                Port = 12201
            });

            var logger = loggerConfig.CreateLogger();

            logger.Information("battle profile:  {@BattleProfile}", profile);
        }

        [Fact]
        [Trait("Ignore", "Integration")]
        public void Log10Profiles()
        {
            var fixture = new Fixture();
            fixture.Behaviors.Clear();
            fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));
            var profiles = fixture.CreateMany<Profile>(10).ToList();

            var loggerConfig = new LoggerConfiguration();

            loggerConfig.WriteTo.Graylog(new GraylogSinkOptions
            {
                MinimumLogEventLevel = LogEventLevel.Information,
                MessageGeneratorType = MessageIdGeneratortype.Timestamp,
                TransportType = TransportType.Http,
                Facility = "VolkovTestFacility",
                HostnameOrAddress = "http://logs.aeroclub.int",
                Port = 12201
            });

            var logger = loggerConfig.CreateLogger();

            profiles.AsParallel().ForAll(profile =>
            {
                logger.Information("TestSend {@BattleProfile}", profile);
            });
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void TestException()
        {
            var loggerConfig = new LoggerConfiguration();

            loggerConfig
                .Enrich.WithExceptionDetails()
                .WriteTo.Graylog(new GraylogSinkOptions
                {
                    MinimumLogEventLevel = LogEventLevel.Information,
                    MessageGeneratorType = MessageIdGeneratortype.Timestamp,
                    TransportType = TransportType.Http,
                    Facility = "VolkovTestFacility",
                    HostnameOrAddress = "http://logs.aeroclub.int",
                    Port = 12201
                });

            var test = new TestClass
            {
                Id = 1,
                SomeTestDateTime = DateTime.UtcNow,
                Bar = new Bar
                {
                    Id = 2
                },
                TestPropertyOne = "1",
                TestPropertyThree = "3",
                TestPropertyTwo = "2"
            };

            var logger = loggerConfig.CreateLogger();

            try
            {
                try
                {
                    throw new InvalidOperationException("Level One exception");
                }
                catch (Exception exc)
                {
                    throw new NotImplementedException("Nested Exception", exc);
                }
            }
            catch (Exception exc)
            {
                logger.Error(exc, "test exception with object {@test}", test);
            }
        }
    }
}