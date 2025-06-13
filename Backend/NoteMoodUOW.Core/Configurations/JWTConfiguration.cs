using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Configurations
{
    public class JWTConfiguration
    {
        public string ValidAudience { get; set; }
        public string ValidIssuer { get; set; }
        public string SecretKey { get; set; }
        public double ExpiryInMinutes { get; set; }

        public double RefreshTokenExpiryInDays { get; set; }

    }
}
