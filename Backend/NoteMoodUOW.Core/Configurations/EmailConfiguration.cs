﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Configurations
{
    public class EmailConfiguration
    {
        public string? SmtpServer { get; set; }
        public int Port { get; set; }
        public string? From { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
