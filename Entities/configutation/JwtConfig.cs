using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.configutation
{
    public class JwtConfig
    {
        public string EncKey { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public string TokenExpirationDurationInSeconds { get; set; }
    }
}
