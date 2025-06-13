using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Dtos.AuthDtos
{
    public class AuthStateDto
    {
        public string? Message { get; set; }

        public string? Email { get; set; }

        public List<string>? Roles { get; set; }

        public bool IsAuthenticated { get; set; }
        public string? Token { get; set; }

        //this part will be updated to include refresh token
        public DateTime? Expires { get; set; }

        [JsonIgnore]
        public string? RefreshToken { get; set; }

        public DateTime RefreshTokenExpires { get; set; }
    }
}
