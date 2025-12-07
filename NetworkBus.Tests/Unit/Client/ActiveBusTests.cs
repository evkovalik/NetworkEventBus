using Moq;
using NetworkBus.Client;
using NetworkBus.Models;

namespace NetworkBus.Tests.Unit.Client;

public class ActiveBusTests
{
    private const string SignalName = "TestSignal";
    private const string BadSignalName = "BadTestSignal";
    private record FakeDto(int Number=default, string Str="");
    private record BadFakeDto(int Number=default);

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public void Meet_MultipleSignalHandlerAndExpectedInput_Calls(int count)
    {
        var transport = new Mock<INetworkTransport>();
        var sut = new ActiveBus(transport.Object);
        int calls = 0;
        for (int i = 0; i < count; i++) sut.Meet(SignalName, () => calls++);

        // Act
        transport.Raise(t => t.OnReceive += null, Signal.AsPacket(SignalName));

        Assert.Equal(count, calls);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public void Meet_MultipleSignalHandlerAndNotExpectedInput_NoCalls(int count)
    {
        var transport = new Mock<INetworkTransport>();
        var sut = new ActiveBus(transport.Object);
        int calls = 0;
        for (int i = 0; i < count; i++) sut.Meet(SignalName, () => calls++);

        // Act
        transport.Raise(t => t.OnReceive += null, Signal.AsPacket(BadSignalName));

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
        var sut = new ActiveBus(transport.Object);

        var fakeDto = new FakeDto(10);
        int calls = 0;
        int sum = 0;

        for (int i = 0; i < handlerCount; i++)
            sut.Meet<FakeDto>(dto =>
            {
                sum += dto.Number;
                calls++;
            });

        // Act
        if(expectedInput)
            transport.Raise(t => t.OnReceive += null, Packet.Create(fakeDto));
        else
            transport.Raise(t => t.OnReceive += null, Packet.Create(new BadFakeDto()));

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
        var sut = new ActiveBus(transport.Object);

        int sum = 0;
        Action handler1 = () => sum += 1;
        Action handler2 = () => sum += 5;

        sut.Meet(SignalName, handler1);
        sut.Meet(SignalName, handler2);

        // Act
        sut.Forget(SignalName, handler1);
        transport.Raise(t => t.OnReceive += null, Signal.AsPacket(SignalName));

        Assert.Equal(5, sum);
    }

    [Fact]
    public void Forget_SignalHandlersAndNoTarget_NoDeletion()
    {
        var transport = new Mock<INetworkTransport>();
        var sut = new ActiveBus(transport.Object);

        int sum = 0;
        Action handler1 = () => sum += 1;
        Action handler2 = () => sum += 5;
        Action badHandler = () => sum += 10;

        sut.Meet(SignalName, handler1);
        sut.Meet(SignalName, handler2);

        // Act
        sut.Forget(SignalName, badHandler);
        transport.Raise(t => t.OnReceive += null, Signal.AsPacket(SignalName));

        Assert.Equal(6, sum);
    }

    [Fact]
    public void Forget_DtoHandlersAndIsTarget_Deletion()
    {
        var transport = new Mock<INetworkTransport>();
        var sut = new ActiveBus(transport.Object);

        var fakeDto = new FakeDto(10);
        int sum = 0;
        Action<FakeDto> handler1 = dto => sum += dto.Number;
        Action<FakeDto> handler2 = dto => sum += dto.Number * 5;

        sut.Meet(handler1);
        sut.Meet(handler2);

        // Act
        sut.Forget<FakeDto>(handler1);
        transport.Raise(t => t.OnReceive += null, Packet.Create(fakeDto));

        Assert.Equal(5 * fakeDto.Number, sum);
    }

    [Fact]
    public void Forget_DtoHandlersAndNoTarget_NoDeletion()
    {
        var transport = new Mock<INetworkTransport>();
        var sut = new ActiveBus(transport.Object);

        var fakeDto = new FakeDto(10);
        int sum = 0;
        Action<FakeDto> handler1 = dto => sum += dto.Number;
        Action<FakeDto> handler2 = dto => sum += dto.Number * 5;
        Action<FakeDto> badHandler = dto => sum += dto.Number * 10;

        sut.Meet(handler1);
        sut.Meet(handler2);

        // Act
        sut.Forget<FakeDto>(badHandler);
        transport.Raise(t => t.OnReceive += null, Packet.Create(fakeDto));

        Assert.Equal(fakeDto.Number + 5 * fakeDto.Number, sum);
    }

    [Fact]
    public void Send_Signal_Success()
    {
        var transport = new Mock<INetworkTransport>();
        var sut = new ActiveBus(transport.Object);

        Packet standard = Signal.AsPacket(SignalName);
        Packet? received = null;
        transport.Setup(t => t.Send(It.IsAny<Packet>()))
            .Callback<Packet>(packet => received = packet);

        // Act
        sut.Send(SignalName);

        Assert.NotNull(received);
        Assert.Equal(received.Value.Name, standard.Name);
        Assert.Equal(received.Value.JsonData, standard.JsonData);
    }

    [Fact]
    public void Send_Dto_Success()
    {
        var transport = new Mock<INetworkTransport>();
        var sut = new ActiveBus(transport.Object);

        Packet standard = Packet.Create(new FakeDto(10));
        Packet? received = null;
        transport.Setup(t => t.Send(It.IsAny<Packet>()))
            .Callback<Packet>(packet => received = packet);

        // Act
        sut.Send(new FakeDto(10));

        Assert.NotNull(received);
        Assert.Equal(received.Value.Name, standard.Name);
        Assert.Equal(received.Value.JsonData, standard.JsonData);
    }

    [Fact]
    public void FullLoop_Signal_Success()
    {
        var transport1 = new Mock<INetworkTransport>();
        var transport2 = new Mock<INetworkTransport>();
        var sut1 = new ActiveBus(transport1.Object);
        var sut2 = new ActiveBus(transport2.Object);
        int calls = 0;
        sut2.Meet(SignalName, () => calls++);
        sut2.Meet(BadSignalName, () => calls++);

        transport1.Setup(t => t.Send(It.IsAny<Packet>()))
            .Callback<Packet>(packet => transport2.Raise(t => t.OnReceive += null, packet));

        // Act
        sut1.Send(SignalName);

        Assert.Equal(1, calls);
    }

    [Fact]
    public void FullLoop_Dto_Success()
    {
        var transport1 = new Mock<INetworkTransport>();
        var transport2 = new Mock<INetworkTransport>();
        var sut1 = new ActiveBus(transport1.Object);
        var sut2 = new ActiveBus(transport2.Object);
        int result = 0;
        sut2.Meet<FakeDto>(dto => result += dto.Number);
        sut2.Meet<BadFakeDto>(dto => result += dto.Number);

        transport1.Setup(t => t.Send(It.IsAny<Packet>()))
            .Callback<Packet>(packet => transport2.Raise(t => t.OnReceive += null, packet));

        // Act
        sut1.Send(new FakeDto(10));

        Assert.Equal(10, result);
    }
}
