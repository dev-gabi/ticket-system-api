using Dal;
using Entities.configutation;
using Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Services;
using Services.logs;
using System;
using System.Text;
using WebApi.Filters;
using static Services.IEmailService;

namespace WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        readonly string CorsOrigins = "origins";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddCors(options =>
            {
                options.AddPolicy(CorsOrigins,
                                  builder =>
                                  {
                                      builder.WithOrigins("https://test-111.xyz", "http://localhost:4200")/**/
                                        .WithHeaders("Access-Control-Allow-Headers", "Access-Control-Allow-Origin", "Content-Type", "Authorization")
                                      .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS");
                                  });
            });


            services.AddDbContext<TicketsContext>(options =>
             options.UseSqlServer(
               Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentityCore<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
              .AddRoles<IdentityRole>()
               .AddEntityFrameworkStores<TicketsContext>()
               .AddDefaultTokenProviders();

            services.Configure<DataProtectionTokenProviderOptions>(options => {
                options.TokenLifespan = TimeSpan.FromHours(1);
            });
            //Configure Identity
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = true;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 5;

                // User settings
                options.User.RequireUniqueEmail = true;
            });

            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidAudience = Configuration["JWTSettings:Audience"],
                    ValidIssuer = Configuration["JWTSettings:Issuer"],
                    RequireExpirationTime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWTSettings:EncKey"])),
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddScoped(typeof(GenericRepository<>));
            services.AddScoped<ISeedService,SeedService>();
            services.AddScoped<IErrorLogService, ErrorLogService>();
            services.AddScoped<IAuthLogService, AuthLogService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IEmailService, SmtpService>();
            services.AddScoped<ISanitizerService, SanitizerService>();
            services.Configure<ConnectionsConfig>(Configuration.GetSection("ConnectionStrings"));
            services.Configure<DomainConfig>(Configuration.GetSection("DomainConfig"));
            services.Configure<JwtConfig>(Configuration.GetSection("JWTSettings"));
            services.Configure<SmtpConfig>(Configuration.GetSection("SmtpSettings"));
            services.Configure<TicketCategoriesConfig>(Configuration.GetSection("TicketCategories"));
            services.Configure<DirectoriesConfig>(Configuration.GetSection("Directories"));
            services.Configure<TicketStatusConfig>(Configuration.GetSection("TicketStatus"));
            services.Configure<EmployeesSettingsConfig>(Configuration.GetSection("EmployeesSettings"));
      
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            if (env.IsProduction())
            {
                app.UseMiddleware<BlockNonBrowserRequestsMiddlware>();
            }
            app.UseCors(CorsOrigins);

            app.UseMiddleware<JwtMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
