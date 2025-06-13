using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NoteMoodUOW.Core.Configurations;
using NoteMoodUOW.Core.Interfaces;
using NoteMoodUOW.Core.Models;
using NoteMoodUOW.EF;
using NoteMoodUOW.EF.Repositories;
using System.Text;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
namespace NoteMoodAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    policy => policy.AllowAnyOrigin()
                                    .AllowAnyMethod()
                                    .AllowAnyHeader()
                                    .WithExposedHeaders("Access-Control-Allow-Origin", "Access-Control-Allow-Methods", "Access-Control-Allow-Headers"));
            });


            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            // Add Identity

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider)
                .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            // inject Search Service as Singleton
            builder.Services.AddSingleton<ISearchService, LuceneSearchService> ();

            
            builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();

            // Mapping JWT settings

            builder.Services.Configure<JWTConfiguration>(builder.Configuration.GetSection("JWTSettings"));

            // Add Email Configuration
            var emailConfig = builder.Configuration.GetSection("EmailConfiguration")
                .Get<EmailConfiguration>();

            builder.Services.AddSingleton(emailConfig);

            // Add URL Configuration

            var urlConfig = builder.Configuration.GetSection("Configuration")
                .Get<Configuration>();

            builder.Services.AddSingleton(urlConfig);


            // Add MachineAPI
            builder.Services.AddTransient<IMachineAPI, MachineAPI>();


            // Add JWT Authentication
            builder.Services.AddAuthentication(options =>
            { 
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
             })
            .AddJwtBearer(o =>
            {
                o.SaveToken = true;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JWTSettings:ValidIssuer"],
                    ValidAudience = builder.Configuration["JWTSettings:ValidAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWTSettings:SecretKey"])),
                    ClockSkew = TimeSpan.Zero
                };
            });


            // Add Configuration for required email 
            builder.Services.Configure<IdentityOptions>(
                op => op.SignIn.RequireConfirmedEmail = true);

            builder.Services.Configure<DataProtectionTokenProviderOptions>(
                               op => op.TokenLifespan = TimeSpan.FromHours(2));

            // Add Hangfire Configuration
            builder.Services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseDefaultTypeSerializer()
                .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    UsePageLocksOnDequeue = true,
                    DisableGlobalLocks = true
                }));

            builder.Services.AddHangfireServer();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();



           var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            //else
            //{
            //    // UseExceptionHandler in production environment
            //    app.UseExceptionHandler(appBuilder =>
            //    {
            //        appBuilder.Run(async context =>
            //        {
            //            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            //            context.Response.ContentType = "application/json";

            //            var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
            //            if (exceptionHandlerFeature != null)
            //            {
            //                var exception = exceptionHandlerFeature.Error;
            //                // Enhanced logging with exception details
            //                var logger = app.Services.GetRequiredService<ILogger<Program>>();
            //                logger.LogError(exception, "An unexpected error occurred. Exception: {ExceptionMessage}, StackTrace: {StackTrace}", exception.Message, exception.StackTrace);

            //                var response = new
            //                {
            //                    StatusCode = context.Response.StatusCode,
            //                    Message = "An unexpected fault occurred. Try again later."
            //                };

            //                var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);
            //                await context.Response.WriteAsync(jsonResponse);
            //            }
            //        });
            //    });
            //}
            app.UseHttpsRedirection();

            // Add CORS
            app.UseCors("AllowAllOrigins");
           
            //add authentication middleware

            app.UseAuthentication();

            app.UseAuthorization();
            app.UseHangfireDashboard();
            // Add explicit handling for OPTIONS requests
            app.Use((context, next) =>
            {
                if (context.Request.Method == "OPTIONS")
                {
                    context.Response.StatusCode = 200;
                    context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                    context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                    context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization");
                    return Task.CompletedTask;
                }

                return next();
            });

            app.MapControllers();

            app.Run();
        }
    }
}
