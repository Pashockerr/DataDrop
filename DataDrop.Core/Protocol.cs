using System.Buffers.Binary;
using System.Text;

namespace DataDrop.Core;

/// <summary>
/// Encodes and decodes byte sequences into enum commands and payload
/// Message structure: [command:1b][contentLength:8b][content:{contentLength} - optional]
/// </summary>

public static class Protocol
{
    /// <summary>
    /// SendClipboard(0x04) - send clipboard content
    /// SendFile(0x05) - send file
    /// </summary>
    public enum Command : byte
    {
        SendClipboard = 0x01,
        SendFile = 0x02
    }

    public static Command ParseCommand(ReadOnlySpan<byte> bytes)
    {
        if (!Enum.IsDefined(typeof(Command), bytes[0]))
            throw new ArgumentOutOfRangeException(nameof(bytes), "First byte of message doesn't match any of commands");
        return (Command)bytes[0];
    }

    public static long ParseLength(ReadOnlySpan<byte> bytes)
    {
        return BinaryPrimitives.ReadInt64LittleEndian(bytes[1..]);
    }

    public static ReadOnlyMemory<byte> EncodeClipboardSend(string content)
    {
        int contentLength = Encoding.UTF8.GetByteCount(content);
        byte[] buffer = new byte[contentLength + 9];
        buffer[0] = (byte)Command.SendClipboard;
        BinaryPrimitives.WriteInt64LittleEndian(buffer.AsSpan(1), contentLength);
        Encoding.UTF8.GetBytes(content, buffer.AsSpan(9));
        return buffer;
    }

    public static string DecodeClipboardSend(ReadOnlySpan<byte> bytes)
    {
        return Encoding.UTF8.GetString(bytes[9..]); // Skip command and length bytes
    }

    public static void EncodeCommand(long length, Command command, Span<byte> bytes)
    {
        if (bytes.Length < 9)
        {
            throw new IndexOutOfRangeException(
                "Buffer length is not enough to write command code and message length!");
        }

        bytes[0] = (byte)command;
        BinaryPrimitives.WriteInt64LittleEndian(bytes[1..], length);
    }

    public static (long Length, Command Command) DecodeCommand(Span<byte> bytes)
    {
        if (bytes.Length < 9)
        {
            throw new IndexOutOfRangeException(
                "Buffer length is not enough to write command code and message length!");
        }

        return (
            Length: BinaryPrimitives.ReadInt64LittleEndian(bytes[1..]),
            Command: (Command)bytes[0]
        );
    }
}
