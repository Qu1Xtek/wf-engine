namespace WorkflowConfigurator.Models
{
    /// <summary>
    /// Global response model, used all code files and http responses
    /// </summary>
    public class GRes<T>
    {
        public GRes() { }

        public int Code { get; set; }

        public string Message { get; set; }

        public bool IsSuccesful { get; set; }

        public bool IsFault => !IsSuccesful;

        public T Data { get; set; }

        public object[] Errors { get; set; } // This might not be used, proper type should be assigned if decision to be used is taken


        public static GRes<T> OK(T data, string message = "")
        {
            return new GRes<T>
            {
                Data = data,
                IsSuccesful = true,
                Message = message
            };
        }

        public static GRes<T> Fault<T>(T data, string message, int code)
        {
            return new GRes<T>
            {
                Data = data,
                IsSuccesful = false,
                Message = message,
                Code = code
            };
        }
    }
}
