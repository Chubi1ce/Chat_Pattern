using System.Text.Json;

namespace UDP_Chat
{
    internal class Message
    {
        public string? FromNickName { get; set; }
        public string? ToNickName { get; set; }
        public string? Text { get; set; }
        public DateTime Time { get; set; }

        public Message(string? fromNickName, string? text, DateTime time)
        {
            this.FromNickName = fromNickName;
            this.Text = text;
            this.Time = time;
        }

        public Message()
        {

        }

        public override string ToString()
        {
            return $"{Time} {FromNickName}: {Text}";
        }

        public string ConvertToJSON()
        {
            return JsonSerializer.Serialize(this);
        }

        public static Message? ConvertFromJSON(string message)
        {
            return JsonSerializer.Deserialize<Message>(message);
        }
    }
}
