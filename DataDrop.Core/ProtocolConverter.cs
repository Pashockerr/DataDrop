using System.Buffers.Binary;
using System.Text;

namespace DataDrop.Core;

/// <summary>
/// Encodes and decodes byte sequences into enum commands and payload
/// Only supports text for clipboard yet!
/// Message structure: [command:1b][contentLength:8b][content:{contentLength} - optional]
/// </summary>

public class ProtocolConverter
{
    /// <summary>
    /// AskSendClipboard(0x00) - ask server if it's ready to receive clipboard content as next message
    /// AskSendFile(0x01) - ask server if it's ready to receive file content as next message
    /// Approve(0x02) - approve sending file or clipboard content to server
    /// Deny(0x03) - deny sending file or clipboard content to server
    /// SendClipboard(0x04) - send clipboard content(either client->server and server->client)
    /// SendFile(0x05) - send file(either client->server and server->client)
    /// RequestContent(0x06) - check available content(only client->server)
    /// SendContent(0x07) - list available content as [index(from 0)]:[value](answer for ping: server->client)
    ///     0:clipboard size
    ///     1:file name(size)
    ///     ...
    ///     n:file name(size)
    /// RequestFile(0x08) - request one of available files(client -> server)
    /// RequestClipboard(0x09) - request clipboard content(client -> server)
    /// </summary>
    public enum Command : byte
    {
        AskSendClipboard = 0x00,
        AskSendFile = 0x01,
        Approve = 0x02,
        Deny = 0x03,
        SendClipboard = 0x04,
        SendFile = 0x05,
        RequestContent = 0x06,
        SendContent = 0x07,
        RequestFile = 0x08,
        RequestClipboard = 0x09
    }

    public Command ParseCommand(ReadOnlySpan<byte> bytes)
    {
        if (!Enum.IsDefined(typeof(Command), bytes[0]))
            throw new ArgumentOutOfRangeException(nameof(bytes), "First byte of message doesn't match any of commands");
        return (Command)bytes[0];
    }

    public ReadOnlyMemory<byte> EncodeClipboardContent(string content)
    {
        byte[] buffer = new byte[Encoding.UTF8.GetByteCount(content) + 9]; // 1 for command, 8 for content length
        buffer[0] = (byte)Command.SendClipboard;
        BinaryPrimitives.WriteInt64LittleEndian(buffer.AsSpan(1), content.Length);
        Encoding.UTF8.GetBytes(content, buffer.AsSpan(2));
        return buffer;
    }

    public ReadOnlyMemory<byte> SendContent(long clipboardSize)     // TODO : Add files support
    {
        string result = $"0:{clipboardSize}";
        var buffer = new byte[9 + Encoding.UTF8.GetByteCount(result)].AsMemory();
        buffer.Span[0] = (byte)Command.SendContent;
        BinaryPrimitives.WriteInt64LittleEndian(buffer.Span[1..], result.Length);
        return buffer;
    }

    public string DecodeClipboardContent(ReadOnlySpan<byte> bytes)
    {
        return Encoding.UTF8.GetString(bytes[9..]); // Skip command and length bytes
    }

    public ReadOnlyMemory<byte> AskSendClipboard(long length)
    {
        return CommandLengthMessage(length, Command.AskSendClipboard);
    }
    
    public ReadOnlyMemory<byte> AskSendFile(long length)
    {
        return CommandLengthMessage(length, Command.AskSendFile);
    }

    public ReadOnlyMemory<byte> Approve(long length)
    {
        return CommandLengthMessage(length, Command.Approve);
    }
    
    public ReadOnlyMemory<byte> Deny(long length)
    {
        return CommandLengthMessage(length, Command.Deny);
    }
    
    public ReadOnlyMemory<byte> RequestClipboard(long length)
    {
        return CommandLengthMessage(length, Command.RequestClipboard);
    }

    public ReadOnlyMemory<byte> RequestContent()
    {
        return CommandLengthMessage(0, Command.RequestContent);
    }

    private static ReadOnlyMemory<byte> CommandLengthMessage(long length, Command command)
    {
        var buffer = new byte[9].AsMemory();
        buffer.Span[0] = (byte)command;
        BinaryPrimitives.WriteInt64LittleEndian(buffer[1..].Span, length);
        return buffer;
    }
}