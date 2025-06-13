using NoteMoodUOW.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Interfaces
{
    public interface IProfileRepository

    {
        public Task<ProfileDto> GetProfile(string email);

        public Task<IEnumerable<ProfileDto>> GetAllProfiles();

        public Task<IEnumerable<ProfileDto>> SearchProfiles(string query);

        public Task<ProfileDto> UpdateProfile(ProfileDto profileDto);


    }
}
