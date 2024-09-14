namespace WorkflowConfigurator.Models.Activity
{
    public class PicaviResponse
    {
        public string ScreenId { get; set; }

        public bool Success { get; set; }

        public Error Error { get; set; }

        public PageData PageData { get; set; }

    }

    public class InvokerResponse
    {
        public string ScreenId { get; set; }

        public bool Success { get; set; }

        public Error Error { get; set; }

        public string ActivityMetadata { get; set; }
    }

    public class ActivityStateResult
    {
        public string ActivityMetadata { get; set; }

        public string ActivityType { get; set; }

        public bool Success { get; set; }

        public Error Error { get; set; }

        public string ScreenId { get; set; }

        public string Input { get; set; }
    }
}