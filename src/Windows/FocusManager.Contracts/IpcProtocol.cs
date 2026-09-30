namespace FocusManager.Contracts;

public static class IpcProtocol
{
    // Local machine named pipe used for App <-> Agent communication.
    public const string PipeName = "FocusManager.Agent.Pipe";
}
