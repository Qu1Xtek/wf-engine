using Newtonsoft.Json;
using WorkflowConfiguration.Models;

namespace WorkflowConfigurator.Models.Activity
{
    public class PageData
    {
        public List<Hotkey> Hotkeys { get; set; } = new List<Hotkey>();

        public List<TextValue> TextValues { get; set; } = new List<TextValue>();

        public List<ActionData> ActionData { get; set; } = new List<ActionData>();

        public string? Icon { get; set; }

        public string? Image { get; set; }

        public int? ActionReoccuring { get; set; }

        public InputVariables? InputVars { get; set; }
    }
}