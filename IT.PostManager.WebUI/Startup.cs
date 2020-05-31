using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using IT.PostManager.Core.Logic;
using IT.PostManager.Infra.TelegraphConnect;
using IT.PostManager.WebUI.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
namespace IT.PostManager.WebUI
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment hostEnvironment)
        {
            Configuration = configuration;
            HostEnvironment = hostEnvironment;

            CreateILoggerConfiguration();
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment HostEnvironment { get; }
        

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddScoped<ITelegraphClient, TelegraphClient>();
            services.AddScoped<ICoreLogicService, CoreLogicService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
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
                endpoints.MapRazorPages();
            });
        }
        
        private void CreateILoggerConfiguration()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Logger(lc => lc.Filter
                    .ByIncludingOnly(e => e.Level == LogEventLevel.Information || e.Level == LogEventLevel.Debug)
                    .WriteTo.File(
                        new RenderedCompactJsonFormatter(), 
                        Path.Combine(HostEnvironment.ContentRootPath, "logs/applog.ndjson"), 
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7,
                        shared: true))
                .WriteTo.Logger(lc => lc.Filter
                    .ByIncludingOnly(e => e.Level == LogEventLevel.Error)
                    .WriteTo.File(new RenderedCompactJsonFormatter(), 
                        Path.Combine(HostEnvironment.ContentRootPath, "logs/errorlog.ndjson"), 
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7,
                        shared: true))
                #if RELEASE
                .WriteTo.Seq("http://seq-it")
                #endif
                .CreateLogger();
        }
    }
}