using System;
using System.Threading.Tasks;
using AutoEstate.Data.DatabaseContexts.ApplicationDbContext;
using AutoEstate.Data.DatabaseContexts.AuthenticationDbContext;
using AutoEstate.Data.Entities;
using AutoEstate.Web.Interfaces;
using AutoEstate.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AutoEstate.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextPool<AuthenticationDbContext>(
                options => options.UseSqlServer(Configuration.GetConnectionString("AuthenticationConnection"),
                sqlServerOptions => {
                    sqlServerOptions.MigrationsAssembly("AutoEstate.Data");
                }));
            
            services.AddDbContextPool<ApplicationDbContext>(
                options => options.UseSqlServer(Configuration.GetConnectionString("ApplicationConnection"),
                sqlServerOptions => {
                    sqlServerOptions.MigrationsAssembly("AutoEstate.Data");
                }));
            
            services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<AuthenticationDbContext>()
            .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            });

            services.AddControllersWithViews();
            services.AddTransient<IAccountsService, AccountsService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider svp)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            MigrateDatabaseContexts(svp);
            CreateDefaultRolesAndUsers(svp).GetAwaiter().GetResult();
        }
        public void MigrateDatabaseContexts(IServiceProvider svp)
        {
            var AuthenticationDbContext = svp.GetRequiredService<AuthenticationDbContext>();
            AuthenticationDbContext.Database.Migrate();

            var ApplicationDbContext = svp.GetRequiredService<ApplicationDbContext>();
            ApplicationDbContext.Database.Migrate();
        }

        public async Task CreateDefaultRolesAndUsers(IServiceProvider svp)
        {
            string[] roles = new string[] { "SystemAdministrator", "Agent", "User" };
            var userEmail = "admin@autoestate.com";
            var userPassword = "SuperSecretPassword@2020";

            var roleManager = svp.GetRequiredService<RoleManager<IdentityRole>>();
            foreach(var role in roles)
            {
                var roleExists = await roleManager.RoleExistsAsync(role);
                if(!roleExists)
                {
                    await roleManager.CreateAsync(new IdentityRole{Name = role});
                }
            }

            var userManager = svp.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(userEmail);
            if(user is null)
            {
                user = new ApplicationUser
                {
                    Email = userEmail,
                    UserName = userEmail,
                    EmailConfirmed = true,
                    PhoneNumber = "+2347063735920",
                    PhoneNumberConfirmed = true
                };
                await userManager.CreateAsync(user, userPassword);
               
                await userManager.AddToRolesAsync(user, roles);
            }
            
        }
    }
}
