using System.Buffers.Binary;
using System.Text;
using Org.BouncyCastle.Crypto.Agreement.JPake;

namespace DataDrop.Core;

/// <summary>
/// Encodes and decodes byte sequences into enum commands and payload
/// Message structure: [command:1b][contentLength:8b][content:{contentLength} - optional]
/// </summary>

public static class Protocol
{
    /// <summary>
    /// SendClipboard(0x01) - send clipboard content
    /// SendFile(0x02) - send file
    /// SendRound1Payload(0x03) - send round 1 crypto payload
    /// SendRound2Payload(0x04) - send round 2 crypto payload
    /// SendRound3Payload(0x05) - send round 3 crypto payload
    /// </summary>
    public enum Command : byte
    {
        SendClipboard = 0x01,
        SendFile = 0x02,
        SendRound1Payload = 0x03,
        SendRound2Payload = 0x04,
        SendRound3Payload = 0x05
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

    public static ReadOnlyMemory<byte> EncodePayload1Send(JPakeRound1Payload payload)
    {
        long length = 0;
        
        var participantIdBA = Encoding.UTF8.GetBytes(payload.ParticipantId);
        length += participantIdBA.Length;
        
        var gx1BA = payload.Gx1.ToByteArray();
        length += gx1BA.Length;
        var gx2BA = payload.Gx2.ToByteArray();
        length += gx2BA.Length;
        
        List<byte[]> knowlegeProofX1BA = new List<byte[]>(payload.KnowledgeProofForX1.Length);
        List<byte[]> knowlegeProofX2BA = new List<byte[]>(payload.KnowledgeProofForX2.Length);
        foreach(var i in payload.KnowledgeProofForX1)
        {
            knowlegeProofX1BA.Add(i.ToByteArray());
            length += i.GetLengthofByteArray();
        }
        foreach(var i in payload.KnowledgeProofForX2)
        {
            knowlegeProofX2BA.Add(i.ToByteArray());
            length += i.GetLengthofByteArray();
        }

        Span<byte> message = new byte[length];  // Final protocol message with command id and length
        message[0] = (byte)Command.SendRound1Payload;
        BinaryPrimitives.WriteInt64LittleEndian(message[1..], length);
        participantIdBA.CopyTo(message[9..]);
        gx1BA.CopyTo(message[(9 + participantIdBA.Length)..]);
        gx2BA.CopyTo(message[(9 + participantIdBA.Length + gx1BA.Length)..]);
        // TODO : finish that and other 2 rounds

        return new ReadOnlyMemory<byte>([.. message]);
    }
}
