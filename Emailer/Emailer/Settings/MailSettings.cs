using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Emailer.Settings
{
    public class MailSettings
    {
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public int RetryTimer { get; set; }
        public int RetryCount { get; set; }
        public string LogPath { get; set; }
        public string LogFileName { get; set; }
    }
}
