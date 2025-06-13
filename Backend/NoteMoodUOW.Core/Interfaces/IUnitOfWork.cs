using Microsoft.AspNetCore.Identity;
using NoteMoodUOW.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoteMoodUOW.Core.Interfaces;
using NoteMoodUOW.Core.Dtos;
using NoteMoodUOW.Core.Configurations;


namespace NoteMoodUOW.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        //IBaseRepository<Entry> Entries { get; }
        IAuthService Auth { set; get; }
        ITokenService TokenService { get; set; }
        IRefreshTokenService RefreshTokenService { get; set; }
        IProfileRepository Profile { set; get; }
        IEmailService EmailService { set; get; }
        IEntryRepository Entry { set; get; }
        IDailySentimentService DailySentiment { set; get; }
        IAspectAnalysisService AspectAnalysisService { set; get; }
        ITopicAnalysisService TopicAnalysisService { set; get; }
        public Configuration configuration { set; get;} 



        int Complete();
    }
}
