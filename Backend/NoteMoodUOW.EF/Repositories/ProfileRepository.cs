using Microsoft.AspNetCore.Identity;
using NoteMoodUOW.Core.Constants;
using NoteMoodUOW.Core.Dtos;
using NoteMoodUOW.Core.Interfaces;
using NoteMoodUOW.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.EF.Repositories
{

    public class ProfileRepository : IProfileRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileRepository(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Retrieves all profiles.
        /// </summary>
        /// <returns>An asynchronous task that represents the operation and contains the collection of profile DTOs.</returns>
        public Task<IEnumerable<ProfileDto>> GetAllProfiles()
        {
            var users = _userManager.Users.ToList();
            if (users == null)
            {
                return null;
            }
            return Task.FromResult(users.Select(u => new ProfileDto
            {
                Email = u.Email,
                FullName = u.FullName,
                Country = u.Country,
                Day = u.DateOfBirth.Day,
                Month = Enum.TryParse<Month>(u.DateOfBirth.Month.ToString(), true, out var tempMonth) ? tempMonth.ToString() : "Unknown",
                Year = u.DateOfBirth.Year,
                Gender = u.Gender.ToString(),
            }));
        }

        /// <summary>
        /// Retrieves a profile by email.
        /// </summary>
        /// <param name="email">The email of the profile to retrieve.</param>
        /// <returns>An asynchronous task that represents the operation and contains the profile DTO.</returns>
        public Task<ProfileDto> GetProfile(string email)
        {
            var user = _userManager.FindByEmailAsync(email).Result;
            if (user == null)
            {
                return null;
            }
            return Task.FromResult(new ProfileDto
            {
                Email = user.Email,
                FullName = user.FullName,
                Country = user.Country,
                Day = user.DateOfBirth.Day,
                Month = Enum.TryParse<Month>(user.DateOfBirth.Month.ToString(), true, out var tempMonth) ? tempMonth.ToString() : "Unknown",
                Year = user.DateOfBirth.Year,
                Gender = user.Gender.ToString()
                ////password is hashed so we can't get it 
            });
        }

        /// <summary>
        /// Retrieves the profile ID by email.
        /// </summary>
        /// <param name="email">The email of the profile to retrieve the ID for.</param>
        /// <returns>An asynchronous task that represents the operation and contains the profile ID.</returns>
        public Task<string> GetProfileIdByEmail(string email)
        {
            var user = _userManager.FindByEmailAsync(email).Result;
            if (user == null)
            {
                return null;
            }
            return Task.FromResult(user.Id);
        }

        /// <summary>
        /// Searches for profiles based on a query.
        /// </summary>
        /// <param name="query">The query to search for profiles.</param>
        /// <returns>An asynchronous task that represents the operation and contains the collection of profile DTOs.</returns>
        public Task<IEnumerable<ProfileDto>> SearchProfiles(string query)
        {
            var users = _userManager.Users.Where(u => u.Email.Contains(query) || u.FullName.Contains(query)).ToList();
            if (users == null)
            {
                return null;
            }
            return Task.FromResult(users.Select(u => new ProfileDto
            {
                Email = u.Email,
                FullName = u.FullName,
                Country = u.Country,
                Day = u.DateOfBirth.Day,
                Month = Enum.TryParse<Month>(u.DateOfBirth.Month.ToString(), true, out var tempMonth) ? tempMonth.ToString() : "Unknown",
                Year = u.DateOfBirth.Year,
                Gender = u.Gender.ToString()
            }));
        }

        /// <summary>
        /// Updates a profile.
        /// </summary>
        /// <param name="profileDto">The profile DTO containing the updated profile information.</param>
        /// <returns>An asynchronous task that represents the operation and contains the updated profile DTO.</returns>
        public async Task<ProfileDto> UpdateProfile(ProfileDto profileDto)
        {
            var user = _userManager.FindByEmailAsync(profileDto.Email).Result;
            if (user == null)
            {
                return null;
            }
            Month month = Enum.TryParse<Month>(profileDto.Month, true, out var tempMonth) ? tempMonth : Month.Unknown;
            user.FullName = profileDto.FullName ?? user.FullName;
            user.Country = profileDto.Country ?? user.Country;
            user.DateOfBirth = new DateOnly(profileDto.Year, (int)month, profileDto.Day);
            user.Gender = Enum.TryParse<Gender>(profileDto.Gender, true, out var tempGender) ? tempGender : Gender.Unknown;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return null;
            }

            return profileDto;
        }
    }
    
}
