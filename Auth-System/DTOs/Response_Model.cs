namespace Auth_System.DTOs
{
    public class Response_Model<T>
    {
        public T? Data { get; set; }
        public string? Message { get; set; }
        public bool Status { get; set; } = false;

        public static Response_Model<T> Success(T? data, string message, bool status)
        {
            return new Response_Model<T>
            {
                Data = data,
                Message = message,
                Status = true
            };


        }
        public static Response_Model<T> Failure(string message, bool status)
        {
            return new Response_Model<T>
            {
                Message = message,
                Status = false
            };


        }
    }
}
