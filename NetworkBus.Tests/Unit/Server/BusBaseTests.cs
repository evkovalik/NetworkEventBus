using Moq;
using NetworkBus.Models;
using NetworkBus.Server;
using NetworkBus.Server.Interfaces;

namespace NetworkBus.Tests.Unit.Server;

public class BusBaseTests
{
    private const string SignalName = "TestSignal";
    private const string BadSignalName = "BadTestSignal";
    private record FakeDto(int Number=default, string Str="");
    private record BadFakeDto(int Number=default);

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(5, true)]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(5, false)]
    public void Meet_MultipleSignalHandlerAndDiffInput_CallIfExpected(int handlerCount, bool expectedInput)
    {
        var transport = new Mock<INetworkTransport>();
        var sut = new Mock<BusBase>(transport.Object);

        int calls = 0;
        for (int i = 0; i < handlerCount; i++)
            sut.Object.Meet(SignalName, id => calls++);

        // Act
        if(expectedInput)
            transport.Raise(t => t.OnReceive += null, "id", Signal.AsPacket(SignalName));
        else
            transport.Raise(t => t.OnReceive += null, "id", Signal.AsPacket(BadSignalName));

        // Assert
        if(expectedInput)
            Assert.Equal(handlerCount, calls);
        else
            Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(5, true)]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(5, false)]
    public void Meet_MultipleDtoHandlerAndDiffInput_CallIfExpected(int handlerCount, bool expectedInput)
    {
        var transport = new Mock<INetworkTransport>();
        var sut = new Mock<BusBase>(transport.Object);

        var fakeDto = new FakeDto(10);
        int calls = 0;
        int sum = 0;

        for (int i = 0; i < handlerCount; i++)
            sut.Object.Meet<FakeDto>((id, dto) =>
            {
                calls++;
                sum += dto.Number;
            });

        // Act
        if(expectedInput)
            transport.Raise(t => t.OnReceive += null, "id", Packet.Create(fakeDto));
        else
            transport.Raise(t => t.OnReceive += null, "id", Packet.Create(new BadFakeDto()));

        // Assert
        if(expectedInput)
        {
            Assert.Equal(handlerCount, calls);
            Assert.Equal(handlerCount * fakeDto.Number, sum);
        }
        else
        {
            Assert.Equal(0, calls);
            Assert.Equal(0, sum);
        }
    }

    [Fact]
    public void Forget_SignalHandlersAndIsTarget_Deletion()
    {
        var transport = new Mock<INetworkTransport>();
        var sut = new Mock<BusBase>(transport.Object);

        int sum = 0;
        Action<string> handler1 = (id) => sum += 1;
        Action<string> handler2 = (id) => sum += 5;

        sut.Object.Meet(SignalName, handler1);
        sut.Object.Meet(SignalName, handler2);

        // Act
        sut.Object.Forget(SignalName, handler1);
        transport.Raise(t => t.OnReceive += null, "id", Signal.AsPacket(SignalName));

        Assert.Equal(5, sum);
    }

    [Fact]
    public void Forget_SignalHandlersAndNoTarget_NoDeletion()
    {
        var transport = new Mock<INetworkTransport>();
        var sut = new Mock<BusBase>(transport.Object);

        int sum = 0;
        Action<string> handler1 = (id) => sum += 1;
        Action<string> handler2 = (id) => sum += 5;
        Action<string> badHandler = (id) => sum += 10;

        sut.Object.Meet(SignalName, handler1);
        sut.Object.Meet(SignalName, handler2);

        // Act
        sut.Object.Forget(SignalName, badHandler);
        transport.Raise(t => t.OnReceive += null, "id", Signal.AsPacket(SignalName));

        Assert.Equal(6, sum);
    }

    [Fact]
    public void Forget_DtoHandlersAndIsTarget_Deletion()
    {
        var transport = new Mock<INetworkTransport>();
        var sut = new Mock<BusBase>(transport.Object);

        var fakeDto = new FakeDto(10);
        int sum = 0;
        Action<string, FakeDto> handler1 = (id, dto) => sum += dto.Number;
        Action<string, FakeDto> handler2 = (id, dto) => sum += dto.Number * 5;

        sut.Object.Meet(handler1);
        sut.Object.Meet(handler2);

        // Act
        sut.Object.Forget<FakeDto>(handler1);
        transport.Raise(t => t.OnReceive += null, "id", Packet.Create(fakeDto));

        Assert.Equal(5 * fakeDto.Number, sum);
    }

    [Fact]
    public void Forget_DtoHandlersAndNoTarget_NoDeletion()
    {
        var transport = new Mock<INetworkTransport>();
        var sut = new Mock<BusBase>(transport.Object);

        var fakeDto = new FakeDto(10);
        int sum = 0;
        Action<string, FakeDto> handler1 = (id, dto) => sum += dto.Number;
        Action<string, FakeDto> handler2 = (id, dto) => sum += dto.Number * 5;
        Action<string, FakeDto> badHandler = (id, dto) => sum += dto.Number * 10;

        sut.Object.Meet(handler1);
        sut.Object.Meet(handler2);

        // Act
        sut.Object.Forget<FakeDto>(badHandler);
        transport.Raise(t => t.OnReceive += null, "id", Packet.Create(fakeDto));

        Assert.Equal(fakeDto.Number + 5 * fakeDto.Number, sum);
    }
}