using System.Text;

namespace Common.Networking;

public static class MessageEncodingExtensions
{
    public static ArraySegment<byte> Encode(this string message) 
        => new(Encoding.UTF8.GetBytes(message));

    public static string Decode(this byte[] bytes, int count)
        => Encoding.UTF8.GetString(bytes, 0, count); //todo: check offset
}