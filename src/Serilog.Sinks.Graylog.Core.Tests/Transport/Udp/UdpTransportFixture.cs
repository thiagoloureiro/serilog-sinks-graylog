﻿using AutoFixture;
using Moq;
using Serilog.Sinks.Graylog.Core.Extensions;
using Serilog.Sinks.Graylog.Core.Transport;
using Serilog.Sinks.Graylog.Core.Transport.Udp;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Serilog.Sinks.Graylog.Core.Tests.Transport.Udp
{
    public class UdpTransportFixture
    {
        [Fact]
        public void WhenSend_ThenCallMethods()
        {
            var transportClient = new Mock<ITransportClient<byte[]>>();
            var dataToChunkConverter = new Mock<IDataToChunkConverter>();
            var fixture = new Fixture();

            var stringData = fixture.Create<string>();

            byte[] data = stringData.Compress();

            List<byte[]> chunks = fixture.CreateMany<byte[]>(3).ToList();

            dataToChunkConverter.Setup(c => c.ConvertToChunks(data)).Returns(chunks);

            UdpTransport target = new UdpTransport(transportClient.Object, dataToChunkConverter.Object);

            target.Send(stringData);

            dataToChunkConverter.Verify(c => c.ConvertToChunks(data), Times.Once);

            foreach (byte[] chunk in chunks)
            {
                transportClient.Verify(c => c.Send(chunk), Times.Once);
            }
        }
    }
}