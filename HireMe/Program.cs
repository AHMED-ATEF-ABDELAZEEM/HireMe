
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using Hangfire;
using HangfireBasicAuthenticationFilter;
using HireMe.Authentication;
using HireMe.BackgroundJobs;
using HireMe.Cache;
using HireMe.Consts;
using HireMe.CustomErrors;
using HireMe.EmailSettings;
using HireMe.Helpers;
using HireMe.Models;
using HireMe.Persistence;
using HireMe.SeedingData;
using HireMe.Services;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Connections.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

namespace HireMe
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer(); 

            builder.Services.AddSwaggerGen(options =>
            {
                // Enable XML comments
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
                options.IncludeXmlComments(xmlPath);
            });

            // Add Mapster
            var MappingConfig = TypeAdapterConfig.GlobalSettings;
            MappingConfig.Scan(Assembly.GetExecutingAssembly());
            builder.Services.AddSingleton<IMapper>(new Mapper(MappingConfig));


            // fluent validation
            builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            builder.Services.AddFluentValidationAutoValidation();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("connectionString")));


            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 8;
                options.SignIn.RequireConfirmedEmail = false;
                options.User.RequireUniqueEmail = true;
            });


            builder.Services.AddSingleton<IJwtProvider, JwtProvider>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IPasswordService, PasswordService>();
            builder.Services.AddScoped<IRegistrationService, RegistrationService>();
            builder.Services.AddScoped<IGovernorateService, GovernorateService>();
            builder.Services.AddScoped<IJobService, JobService>(); 
            builder.Services.AddScoped<IQuestionService, QuestionService>();
            builder.Services.AddScoped<IAnswerService, AnswerService>();
            builder.Services.AddScoped<IApplicationService, ApplicationService>();
            builder.Services.AddScoped<IApplicationStatusBackgroundJob, ApplicationStatusBackgroundJob>();
            builder.Services.AddScoped<IJobConnectionCompletionBackgroundJob, JobConnectionCompletionBackgroundJob>();
            builder.Services.AddScoped<IJobConnectionService, JobConnectionService>();
            builder.Services.AddScoped<IEmployerDashboardService, EmployerDashboardService>();
            builder.Services.AddScoped<IWorkerDashboardService, WorkerDashboardService>();


            builder.Services.AddScoped<IImageProfileService, ImageProfileService>();

            builder.Services.AddScoped<ITemporarySessionStore, MemoryTemporarySessionStore>();
            builder.Services.AddScoped<IRefreshTokenHelper, RefreshTokenHelper>();
            builder.Services.AddScoped<IAuthServiceHelper, AuthServiceHelper>();
            builder.Services.AddScoped<IUserCreationHelper, UserCreationHelper>();
            builder.Services.AddScoped<IEmailHelper, EmailHelper>();

            builder.Services.AddTransient<AppDbSeeder>();

            builder.Services.AddMemoryCache();

            builder.Services.AddHttpContextAccessor();

            builder.Services.Configure<MailSettings>(builder.Configuration.GetSection(nameof(MailSettings)));

            builder.Services.AddHttpClient("EmailApi");

            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JWT"));

            var JwtSetting = builder.Configuration.GetSection("JWT").Get<JwtOptions>();


            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSetting!.Key)),
                    ValidIssuer = JwtSetting.Issuer,
                    ValidAudience = JwtSetting.Audience,

                };
            })
            .AddGoogle(options =>
            {
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;

                options.Scope.Add("openid");
                options.Scope.Add("email");
                options.Scope.Add("profile");

                options.ClaimActions.MapJsonKey("email_verified", "email_verified", "bool");
            });

            // Rate Limiter
            builder.Services.AddRateLimiter(rateLimiterOptions =>
            {
                rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                rateLimiterOptions.AddPolicy(policyName: RateLimiters.IpLimit, httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter<string>(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString()!,
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 20,
                            Window = TimeSpan.FromMinutes(1)
                        }

                    )
                );

                rateLimiterOptions.AddPolicy(policyName: RateLimiters.UserLimit, httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter<string>(
                        partitionKey: httpContext.User.Identity?.Name?.ToString()!,
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1)
                        }

                    )
                );

                rateLimiterOptions.AddConcurrencyLimiter(policyName: RateLimiters.ConcurrencyLimit, options =>
                {
                    options.PermitLimit = 1000;
                    options.QueueLimit = 100;
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                });

            });

            // Exception Handler
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            // To Use Serilog Package For Logging
            builder.Host.UseSerilog((context, configuration) =>
            {
                // Read Configuration from appsettings.json
                configuration.ReadFrom.Configuration(context.Configuration);

            });

            // Add Hangfire services.
            builder.Services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection")));

            // Add the processing server as IHostedService
            builder.Services.AddHangfireServer();

            var app = builder.Build();

            using (var _scope = app.Services.CreateScope())
            {
                var seeder = _scope.ServiceProvider.GetRequiredService<AppDbSeeder>();
                seeder.SeedAsync().GetAwaiter().GetResult(); ;
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }




            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();

            app.UseHangfireDashboard("/background-jobs", new DashboardOptions
            {
                Authorization =
                [
                    new HangfireCustomBasicAuthenticationFilter
                    {
                        User = app.Configuration.GetValue<string>("HangfireSettings:UserName"),
                        Pass = app.Configuration.GetValue<string>("HangfireSettings:Password")
                    }
                ]
            });

            var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            var refreshTokenHelper = scope.ServiceProvider.GetRequiredService<IRefreshTokenHelper>();

            RecurringJob.AddOrUpdate("RemoveExpiredRefreshTokens", () => refreshTokenHelper.RemoveExpiredRefreshTokensAsync(), Cron.Daily);

            app.UseRateLimiter();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.UseExceptionHandler();

            app.UseStaticFiles();

            app.Run();
        }
    }
}



