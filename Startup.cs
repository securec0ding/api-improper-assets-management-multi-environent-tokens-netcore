using Backend.Data;
using Backend.Model;
using Backend.Services;
using JwtSharp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Backend
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // configure Entity Framework with SQLite
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite("DataSource=ProdDb.sqlite")
            );

            services.AddDbContext<ApplicationDbContextTesting>(options =>
                options.UseSqlite("DataSource=TestingDb.sqlite")
            );

            // add membership system for .NET
            Action<IdentityOptions> getIdentityOptions = options =>
            {
                options.Password.RequiredLength = 4;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            };

            services.AddIdentityCore<IdentityUser>(getIdentityOptions)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddIdentityCore<IdentityUserTesting>(getIdentityOptions)
            .AddRoles<IdentityRoleTesting>()
            .AddEntityFrameworkStores<ApplicationDbContextTesting>();

            // this is to create tokens
            var jwtIssuerOptions = new JwtIssuerOptions()
            {
                Audience = JwtConfiguration.Audience,
                Issuer = JwtConfiguration.Issuer,
                SecurityKey = JwtConfiguration.SigningKey,
                ExpireSeconds = JwtConfiguration.ExpireSeconds
            };
            var jwtIssuer = new JwtIssuer(jwtIssuerOptions);
            services.AddSingleton(jwtIssuer);

            // authentication configuration for .NET
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(cfg =>
            {
                cfg.TokenValidationParameters = JwtConfiguration.GetTokenValidationParameters();
                cfg.Events = new JwtBearerEvents
                {
                    // event for custom responses for not authenticated users
                    OnChallenge = async (context) =>
                    {
                        context.HandleResponse();

                        context.Response.StatusCode = 401;
                        context.Response.Headers.Append(
                            HeaderNames.WWWAuthenticate,
                            context.Options.Challenge);

                        await context.Response.WriteAsync(JsonConvert.SerializeObject(new { Message = "Invalid token" }));
                    }
                };
            });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap[JwtRegisteredClaimNames.Sub] = ClaimTypes.Name;

            services.AddAuthorization(options =>
            {
                options.AddPolicy("OnlyForAccountHolders", policy => policy.RequireClaim("role", "ACCOUNT_HOLDERS"));
                options.AddPolicy("OnlyForAuditors", policy => policy.RequireClaim("role", "AUDITORS"));
            });

            // add controllers and services to Dependency Injection Container
            services.AddControllers();
            services.AddTransient<IIdentityService, IdentityService>();
            services.AddTransient<IIdentityServiceTesting, IdentityServiceTesting>();
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<IAccountServiceTesting, AccountServiceTesting>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, 
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUserTesting> userManagerTesting,
            RoleManager<IdentityRoleTesting> roleManagerTesting,
            ApplicationDbContext dbContextProd, 
            ApplicationDbContextTesting dbContextTesting)
        {
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            dbContextProd.Database.EnsureDeleted();
            dbContextProd.Database.Migrate();

            dbContextTesting.Database.EnsureDeleted();
            dbContextTesting.Database.Migrate();

            //SeedUsers(userManager, roleManager, dbContextProd, dbContextTesting).Wait();
            SeedUsersForProduction(userManager, roleManager, dbContextProd).Wait();
            SeedUsersForTesting(userManagerTesting, roleManagerTesting, dbContextTesting).Wait();
        }

        private async Task SeedUsersForProduction(
            UserManager<IdentityUser> userManager, 
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext dbContext)
        {
            var ACCOUNT_HOLDERS = "ACCOUNT_HOLDERS";
            var AUDITORS = "AUDITORS";
            var usersRole = new IdentityRole(ACCOUNT_HOLDERS);
            var auditorsRole = new IdentityRole(AUDITORS);
            await roleManager.CreateAsync(usersRole);
            await roleManager.CreateAsync(auditorsRole);

            var billyUser = new IdentityUser("Billy.Jean@me.com");
            await userManager.CreateAsync(billyUser, "myPassword");
            await userManager.AddToRoleAsync(billyUser, ACCOUNT_HOLDERS);

            var emilyUser = new IdentityUser("Emily.White@gmail.com");
            await userManager.CreateAsync(emilyUser, "PaSswOrD");
            await userManager.AddToRoleAsync(emilyUser, ACCOUNT_HOLDERS);

            var annaUser = new IdentityUser("Anna4564564@company.com");
            await userManager.CreateAsync(annaUser, "12345Pass");
            await userManager.AddToRoleAsync(annaUser, ACCOUNT_HOLDERS);

            var theAuditor = new IdentityUser("John.Black@auditors.com");
            await userManager.CreateAsync(theAuditor, "secret#$%345345345");
            await userManager.AddToRoleAsync(theAuditor, AUDITORS);

            var billyUserAccount = new BankAccount
            {
                Id = Guid.Parse("BF861F2B-A238-4D37-8C4D-E634B47577F0"),
                UserId = billyUser.Id,
                UserName = "Billy.Jean@me.com",
                SSN = "123-45-6789",
                Balance = 5440.50M,
            };

            var emilyUserAccount = new BankAccount
            {
                Id = Guid.Parse("F63A109F-A7AF-44DC-8DCD-52FBA219C9D0"),
                UserId = emilyUser.Id,
                UserName = "Emily.White@gmail.com",
                SSN = "456-78-901",
                Balance = 15700.00M,
            };

            var annaUserAccount = new BankAccount
            {
                Id = Guid.Parse("92F70A00-FC13-4A57-863A-C30E3F397FA4"),
                UserId = annaUser.Id,
                UserName = "Anna4564564@company.com",
                SSN = "368-56-975",
                Balance = 8700.00M,
            };

            dbContext.Accounts.Add(billyUserAccount);
            dbContext.Accounts.Add(emilyUserAccount);
            dbContext.Accounts.Add(annaUserAccount);
            await dbContext.SaveChangesAsync();
        }

        private async Task SeedUsersForTesting(
            UserManager<IdentityUserTesting> userManager,
            RoleManager<IdentityRoleTesting> roleManager,
            ApplicationDbContextTesting dbContext)
        {
            var AUDITORS = "AUDITORS";
            var ACCOUNT_HOLDERS = "ACCOUNT_HOLDERS";
            var usersRole = new IdentityRoleTesting(ACCOUNT_HOLDERS);
            var auditorsRole = new IdentityRoleTesting(AUDITORS);
            await roleManager.CreateAsync(usersRole);
            await roleManager.CreateAsync(auditorsRole);

            var billyUser = new IdentityUserTesting("Billy.Jean@me.com");
            await userManager.CreateAsync(billyUser, "myPassword");
            await userManager.AddToRoleAsync(billyUser, ACCOUNT_HOLDERS);

            var emilyUser = new IdentityUserTesting("Emily.White@gmail.com");
            await userManager.CreateAsync(emilyUser, "PaSswOrD");
            await userManager.AddToRoleAsync(emilyUser, ACCOUNT_HOLDERS);

            var annaUser = new IdentityUserTesting("Anna4564564@company.com");
            await userManager.CreateAsync(annaUser, "12345Pass");
            await userManager.AddToRoleAsync(annaUser, ACCOUNT_HOLDERS);

            var theAuditor = new IdentityUserTesting("John.Black@auditors.com");
            await userManager.CreateAsync(theAuditor, "secret#$%345345345");
            await userManager.AddToRoleAsync(theAuditor, AUDITORS);

            var testingUser = new IdentityUserTesting("test");
            await userManager.CreateAsync(testingUser, "test");
            await userManager.AddToRoleAsync(testingUser, AUDITORS);

            var billyUserAccount = new BankAccount
            {
                Id = Guid.Parse("BF861F2B-A238-4D37-8C4D-E634B47577F0"),
                UserId = billyUser.Id,
                UserName = "Billy.Jean@me.com",
                SSN = "123-45-6789",
                Balance = 5440.50M,
            };

            var emilyUserAccount = new BankAccount
            {
                Id = Guid.Parse("F63A109F-A7AF-44DC-8DCD-52FBA219C9D0"),
                UserId = emilyUser.Id,
                UserName = "Emily.White@gmail.com",
                SSN = "456-78-901",
                Balance = 15700.00M,
            };

            var annaUserAccount = new BankAccount
            {
                Id = Guid.Parse("92F70A00-FC13-4A57-863A-C30E3F397FA4"),
                UserId = annaUser.Id,
                UserName = "Anna4564564@company.com",
                SSN = "368-56-975",
                Balance = 8700.00M,
            };

            dbContext.Accounts.Add(billyUserAccount);
            dbContext.Accounts.Add(emilyUserAccount);
            dbContext.Accounts.Add(annaUserAccount);
            await dbContext.SaveChangesAsync();
        }
    }
}