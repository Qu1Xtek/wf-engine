namespace WorkflowConfigurator.Services.Service
{
    public class ServiceResponse
    {
        public int ErrorCode {get; set; }
        public string Message {get; set; }

        public object ResponseObject {get; set; }
    }
}
