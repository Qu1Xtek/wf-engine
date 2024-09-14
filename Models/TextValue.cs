using MongoDB.Bson.Serialization.Attributes;
using WorkflowConfigurator.Models;

namespace WorkflowConfiguration.Models
{
    public class TextValue
    { 
        public string Type { get; set; }

        public dynamic Value { get; set; }

        public string? Background { get; set; }

        public string? FontColor { get; set; }

        public string? FontSize { get; set; }

        public TextValue(string type, dynamic value)
        {
            Type = type;
            Value = value;
        }

        public TextValue(string type, object value, string background, string fontColor, string fontSize)
        {
            Type = type;
            Value= value;
            Background = background;
            FontColor = fontColor;
            FontSize = fontSize;
        }
    }
}