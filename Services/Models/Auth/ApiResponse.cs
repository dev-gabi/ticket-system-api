using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


    public class ApiResponse
    {
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string StatusCodeTitle { get; set; }
        public string Errors { get; set; }

    internal LoginManagerResponse ConvertToLoginManagerResponse()
    {
        return new LoginManagerResponse()
        {
            Message = this.Message,
            Errors = this.Errors,
            StatusCode = this.StatusCode,
            StatusCodeTitle = this.StatusCodeTitle,
            IsSuccess = this.IsSuccess
        };
    }
}

