using BOS.Auth.Client.ServiceExtension;
using BOS.IA.Client.ServiceExtension;
using BOS.LaunchPad.ConfigurationHelpers;
using BOS.LaunchPad.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System;

namespace BOS.LaunchPad
{
    public class Startup
    {
        public IConfiguration _Configuration { get; }
        public Startup(IConfiguration _configuration)
        {
            _Configuration = _configuration;
            var loggerConfig = new LoggerConfiguration().ReadFrom.Configuration(_configuration);
            if (!string.IsNullOrWhiteSpace(_configuration["ElasticSearchUri"]))
            {
                var esOptions = new ElasticsearchSinkOptions(new Uri(_configuration["ElasticSearchUri"]));
                esOptions.AutoRegisterTemplate = true;
                esOptions.AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6;
                loggerConfig = loggerConfig.WriteTo.Elasticsearch(esOptions);
            }
            Log.Logger = loggerConfig.CreateLogger();
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.Configure<ViewConfig>(_Configuration.GetSection("ViewConfiguration"));
            services.AddTransient<IEmailSender>(e => new EmailSender(_Configuration["SendGrid:From"], _Configuration["SendGrid:ApiKey"]));
            services.AddBOSAuthClient(_Configuration["BOS:ApiKey"], _Configuration["BOS:AuthUrl"]);
            services.AddBOSIAClient(_Configuration["BOS:ApiKey"], _Configuration["BOS:IAUrl"]);
            services.AddDefaultIdentity<IdentityUser>();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(c =>
            {
                c.LoginPath = $"/Identity/Account/Login";
                c.AccessDeniedPath = $"/Identity/Account/AccessDenied";
                c.LogoutPath = $"/Identity/Account/Logout";
            });
            services.AddDistributedMemoryCache(); // Adds a default in-memory implementation of IDistributedCache
            services.AddSession(
                s =>
                {
                    s.IdleTimeout = TimeSpan.FromMinutes(30);
                    s.Cookie.HttpOnly = true;
                }
            );
            services.AddMvc().AddFeatureFolders().AddAreaFeatureFolders().AddJsonOptions(option => {
                if (option.SerializerSettings.ContractResolver != null)
                {
                    var resolver = option.SerializerSettings.ContractResolver as DefaultContractResolver;
                    resolver.NamingStrategy = null;
                }
            });
        }
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseCookiePolicy(new CookiePolicyOptions()
            {
                
            });
            app.UseAuthentication();
            var cachePeriod = env.IsDevelopment() ? "600" : "604800";
            app.UseCors("CorsPolicy");
            app.UseStaticFiles(
              new StaticFileOptions
              {
                  OnPrepareResponse = ctx =>
                  {                      //Add caching for static files  10 min for development
                      ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={cachePeriod}");
                  }
              });
            app.UseCookiePolicy();
            app.UseSession();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "areaRoute",
                    template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                     "NotFound",
                     "{*url}",
                      new { controller = "Error", action = "PageNotFound" }
                     );
            });
        }
    }
}
