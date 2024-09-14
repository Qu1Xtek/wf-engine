using MongoDB.Bson.Serialization.Attributes;

namespace WorkflowConfiguration.Models
{
    public class Hotkey
    {
        [BsonElement("id")]
        public string HotkeyId { get; set; }
        public string Value { get; set; }
        public string? MenuIcon { get; set; }

        public Hotkey(string id, string value, string? menuIcon)
        {
            HotkeyId = id;
            Value = value;
            MenuIcon = menuIcon;
        }
    }
}