using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;

namespace FocusManager.Agent.Services;

public static class IpcPipeFactory
{
    public static NamedPipeServerStream CreateServerStream(string pipeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pipeName);

        var currentUserSid = WindowsIdentity.GetCurrent().User
            ?? throw new InvalidOperationException("Current Windows user SID could not be resolved for IPC pipe security.");

        var pipeSecurity = new PipeSecurity();
        pipeSecurity.AddAccessRule(new PipeAccessRule(
            currentUserSid,
            PipeAccessRights.ReadWrite,
            AccessControlType.Allow));
        pipeSecurity.AddAccessRule(new PipeAccessRule(
            new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
            PipeAccessRights.FullControl,
            AccessControlType.Allow));
        pipeSecurity.AddAccessRule(new PipeAccessRule(
            new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
            PipeAccessRights.FullControl,
            AccessControlType.Allow));

        return NamedPipeServerStreamAcl.Create(
            pipeName,
            PipeDirection.InOut,
            NamedPipeServerStream.MaxAllowedServerInstances,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous,
            inBufferSize: 4096,
            outBufferSize: 4096,
            pipeSecurity,
            HandleInheritability.None,
            additionalAccessRights: (PipeAccessRights)0);
    }
}
