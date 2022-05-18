// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System;
using System.Threading.Tasks;

namespace WebApp_OpenIDConnect_DotNet
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private async Task OnRedirectToIdentityProviderFunc(RedirectContext context)
        {
            context.ProtocolMessage.Parameters.Add("campaignId", "123");

            // More code
            await Task.CompletedTask.ConfigureAwait(false);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
                options.HandleSameSiteCookieCompatibility();
            });

            //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            //{
            //    options.ExpireTimeSpan = new TimeSpan(7, 0, 0, 0);
            //});
            // Configuration to sign-in users with Azure AD B2C
            services.AddMicrosoftIdentityWebAppAuthentication(Configuration, "AzureAdB2C");

            //  begin mik changes
            //services.AddAuthentication("AzureAdB2C")
            //        .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAdB2C"), Microsoft.Identity.Web.Constants.AzureAdB2C, null);

            //services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme) // This means default scheme is "OpenIdConnect"
            //    .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAdB2C2"), Microsoft.Identity.Web.Constants.AzureAd, null);

            //services.AddAuthentication() // No default scheme
            //    .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAdB2C2"), "AzureAdB2C2", null);

            services.AddAuthentication()
        .AddMicrosoftIdentityWebApp(options =>
        {
            Configuration.Bind("AzureAdB2C2", options);
            options.Events ??= new OpenIdConnectEvents();
            options.Events.OnRedirectToIdentityProvider += OnRedirectToIdentityProviderFunc;
          
        }, cookieScheme:"Cookie1", openIdConnectScheme: "AzureAdB2C2");


            //services.AddAuthentication(options =>
            //{
            //    options.DefaultScheme = "Cookies";
            //    options.DefaultChallengeScheme = "oidc";
            //});


            //  services.AddAuthentication(options =>
            //{
            //    options.AddScheme = OpenIdConnectDefaults.AuthenticationScheme;
            //})
            //         .AddOpenIdConnect(options =>
            //         {

            //             options.Authority = "https://identityb2c.b2clogin.com/tfp/identityb2c.onmicrosoft.com/B2C_1A_B2CASENTRYPOINTSIGNUP_SIGNIN-o365/v2.0";
            //             options.ClientId = "e50289d6-72f8-4482-ab0b-efd1655ed605";
            //             options.CallbackPath = "/signin-oidc" + "-o365";
            //             options.TokenValidationParameters.NameClaimType = "name";
            //         });


            //  end mik changes


            services.AddControllersWithViews()
                .AddMicrosoftIdentityUI();

            services.AddRazorPages();

            //Configuring appsettings section AzureAdB2C, into IOptions
            services.AddOptions();
            services.Configure<OpenIdConnectOptions>(Configuration.GetSection("AzureAdB2C"));
            services.AddAuthorization(options =>
            {
                options.AddPolicy("nuance_verify_decision", policy => policy.RequireClaim("nuance_verify_decision", "DECISION_AUTHENTIC")); //Nuance

            });
            //mik

        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
            app.UseCookiePolicy();

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
    }
}