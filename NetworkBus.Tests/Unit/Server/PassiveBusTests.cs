using Moq;
using NetworkBus.Models;
using NetworkBus.Server;
using NetworkBus.Server.Interfaces;

namespace NetworkBus.Tests.Unit.Server;

public class PassiveBusTests
{
    private const string SignalName = "TestSignal";
    private const string BadSignalName = "BadTestSignal";
    private record FakeDto(int Number=default, string Str="");
    private record BadFakeDto(int Number=default);

    [Fact]
    public void SendTo_Signal_Success()
    {
        var transport = new Mock<INetworkTransport>();
        var sut = new PassiveBus(transport.Object);

        Packet standard = Signal.AsPacket(SignalName);
        string? receivedId = null;
        Packet? receivedData = null;
        transport.Setup(t => t.Send(It.IsAny<string>(), It.IsAny<Packet>()))
            .Callback<string, Packet>((id, packet) =>
            {
                receivedId = id;
                receivedData = packet;
            });

        // Act
        sut.SendTo("id", SignalName);
        sut.Release();

        // Assert
        Assert.NotNull(receivedId);
        Assert.NotNull(receivedData);
        Assert.Equal(receivedData.Value.Name, standard.Name);
        Assert.Equal(receivedData.Value.JsonData, standard.JsonData);
    }

    [Fact]
    public void SendTo_Dto_Success()
    {
        var transport = new Mock<INetworkTransport>();
        var sut = new PassiveBus(transport.Object);

        Packet standard = Packet.Create(new FakeDto(10));
        string? receivedId = null;
        Packet? receivedData = null;
        transport.Setup(t => t.Send(It.IsAny<string>(), It.IsAny<Packet>()))
            .Callback<string, Packet>((id, packet) =>
            {
                receivedId = id;
                receivedData = packet;
            });

        // Act
        sut.SendTo("id", new FakeDto(10));
        sut.Release();

        Assert.NotNull(receivedId);
        Assert.NotNull(receivedData);
        Assert.Equal(receivedData.Value.Name, standard.Name);
        Assert.Equal(receivedData.Value.JsonData, standard.JsonData);
    }

    [Fact]
    public void SendToAll_MultipleClients_Success()
    {
        var transport = new Mock<INetworkTransport>();
        var sut = new PassiveBus(transport.Object);

        sut.AddRecipients(["id1", "id2", "id3"]);
        List<(string, Packet)> received = [];
        transport.Setup(t => t.Send(It.IsAny<string>(), It.IsAny<Packet>()))
            .Callback<string, Packet>((id, packet) => received.Add((id, packet)));

        // Act
        sut.SendToAll(new FakeDto(10));
        sut.Release();

        Assert.NotEmpty(received);
        Assert.Single(received, tuple => tuple.Item1 == "id1");
        Assert.Single(received, tuple => tuple.Item1 == "id2");
        Assert.Single(received, tuple => tuple.Item1 == "id3");
    }
}
