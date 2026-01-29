namespace TrackerAPI.Data
{
    public class ResponseModel<T>
    {
        public string Code { get; set; } = "200";
        public string Message { get; set; } = "Ok";
        public T? Detalle { get; set; }
    }
}