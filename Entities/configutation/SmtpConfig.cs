using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.configutation
{
    public class SmtpConfig
    {
        public string EmailAddress { get; set; }
        public string EmailPass { get; set; }
        public string From { get; set; }
        public string Smtp { get; set; }
        public string Port { get; set; }
        public string WebsiteName { get; set; }
    }
}
