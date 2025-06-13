using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NoteMoodUOW.Core.Configurations;
using NoteMoodUOW.Core.Dtos;
using NoteMoodUOW.Core.Interfaces;
using NoteMoodUOW.Core.Models;
using NoteMoodUOW.EF.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.EF
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        //public IBaseRepository<Entry> Entries { get; private set; }
        public IAuthService Auth { get; set; }
        public ITokenService TokenService { get; set; }
        public IRefreshTokenService RefreshTokenService { get; set; }

        public IProfileRepository Profile { get; set; }

        public IEntryRepository Entry { get; set; }
        public IEmailService EmailService { get; set;}
        public IDailySentimentService DailySentiment { get; set; }
        public IAspectAnalysisService AspectAnalysisService { get; set; }
        public ITopicAnalysisService TopicAnalysisService { get; set; }

        public Configuration configuration { get; set; }

        public UnitOfWork(ApplicationDbContext context , UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            IOptions<JWTConfiguration> jwt, EmailConfiguration emailConfiguration, IHttpContextAccessor httpContextAccessor, ISearchService searchService, Configuration urlConfiguration, IMachineAPI machineAPI)
        {
            _context = context;
            //Entries = new BaseRepository<Entry>(_context);
            TokenService = new TokenService(userManager, jwt, httpContextAccessor);
            RefreshTokenService = new RefreshTokenService(userManager, jwt, TokenService,httpContextAccessor);
            Auth = new AuthService( userManager,signInManager, RefreshTokenService, TokenService);
            Profile = new ProfileRepository(userManager);
            Entry = new EntryRepository(_context,searchService, machineAPI);
            EmailService = new EmailService(emailConfiguration);
            DailySentiment = new DailySentimentService(_context);
            AspectAnalysisService = new AspectAnalysisService(_context, machineAPI);
            TopicAnalysisService = new TopicAnalysisService(_context, machineAPI);
            configuration = urlConfiguration;
        }
        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
