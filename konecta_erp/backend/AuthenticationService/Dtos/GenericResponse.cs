namespace AuthenticationService.Dtos
{
   
    public class GenericResponse
    {
        public object? Result { get; set; }


        public string Code { get; set; } = string.Empty;

        public string C_Message { get; set; } = string.Empty;

   
        public string S_Message { get; set; } = string.Empty;
    }
}
