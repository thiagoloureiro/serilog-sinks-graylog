﻿using AutoFixture;
using FluentAssertions;
using Moq;
using Serilog.Sinks.Graylog.Core.Helpers;
using Serilog.Sinks.Graylog.Core.Transport.Udp;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Serilog.Sinks.Graylog.Core.Tests.Transport.Udp
{
    public class DataToChunkConverterFixture
    {
        private readonly ChunkSettings _settings;
        private readonly Fixture _fixture;
        private readonly Mock<IMessageIdGeneratorResolver> _resolver;

        public DataToChunkConverterFixture()
        {
            _settings = new ChunkSettings(MessageIdGeneratortype.Md5, 8192);
            _fixture = new Fixture();
            _resolver = new Mock<IMessageIdGeneratorResolver>();
        }

        [Fact]
        public void WhenConvertToChunkWithSmallData_ThenReturnsOneChunk()
        {
            var target = new DataToChunkConverter(_settings, _resolver.Object);

            byte[] giwenData = _fixture.CreateMany<byte>(1000).ToArray();
            IList<byte[]> actual = target.ConvertToChunks(giwenData);

            var expected = new List<byte[]>()
            {
                giwenData
            };

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void WhenChunksWasTooMany_ThenThrowsException()
        {
            byte[] giwenData = new byte[10000000];

            var target = new DataToChunkConverter(_settings, _resolver.Object);

            Assert.Throws<ArgumentException>(() => target.ConvertToChunks(giwenData));
        }

        [Fact]
        public void WhenMessageIsLong_ThenSplitItToChunks()
        {
            byte[] giwenData = new byte[100000];

            var idGenerator = new Mock<IMessageIdGenerator>();

            var messageId = _fixture.CreateMany<byte>(8).ToArray();

            idGenerator.Setup(c => c.GenerateMessageId(giwenData)).Returns(messageId);

            _resolver.Setup(c => c.Resolve(_settings.MessageIdGeneratorType))
                .Returns(idGenerator.Object);

            var target = new DataToChunkConverter(_settings, _resolver.Object);

            var actual = target.ConvertToChunks(giwenData);

            Assert.True(actual.Count == 13);

            for (int i = 0; i < actual.Count; i++)
            {
                actual[i].Take(2).ToArray().Should().BeEquivalentTo(new[] { 0x1e, 0x0f });
                actual[i].Skip(2).Take(8).ToArray().Should().BeEquivalentTo(messageId);
                actual[i].Skip(10).Take(1).First().Should().Be((byte)i);
                actual[i].Skip(11).Take(1).First().Should().Be(13);
                Assert.True(actual[i].Skip(12).All(c => c == 0));
            }
        }
    }
}