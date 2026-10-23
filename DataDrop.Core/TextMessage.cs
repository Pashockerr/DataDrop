namespace DataDrop.Core;

public class TextMessage(string text) : IMessage
{
    public string? Text { get; set; } = text;
}