using System.IO.Pipes;
using System.Text;
using FocusManager.Agent.Services;
using Xunit;

namespace FocusManager.Agent.Tests;

public sealed class IpcPipeFactoryTests
{
    [Fact]
    public async Task CreateServerStream_AllowsLocalClientToConnectAndExchangeData()
    {
        var pipeName = $"FocusManager.Agent.Tests.{Guid.NewGuid():N}";

        using var server = IpcPipeFactory.CreateServerStream(pipeName);
        using var client = new NamedPipeClientStream(
            ".",
            pipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous);

        var serverWaitTask = server.WaitForConnectionAsync();
        await client.ConnectAsync(timeout: 3000);
        await serverWaitTask.WaitAsync(TimeSpan.FromSeconds(3));

        using var clientWriter = new StreamWriter(client, new UTF8Encoding(false), leaveOpen: true)
        {
            AutoFlush = true
        };
        using var serverReader = new StreamReader(server, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);

        await clientWriter.WriteLineAsync("ping");

        var message = await serverReader.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(3));

        Assert.Equal("ping", message);
    }
}
