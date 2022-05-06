public class LoginManagerResponse : ApiResponse
    {
        public string Token { get; set; }
        public string ExpireInSeconds { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public string Id { get; set; }
} 

