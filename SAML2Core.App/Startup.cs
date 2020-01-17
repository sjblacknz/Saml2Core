using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using SamlCore.AspNetCore.Authentication.Saml2.Metadata;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SAML2Core.App
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
            services.AddControllersWithViews();
            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
             .AddSamlCore(options =>
             {
                 // SignOutPath- The endpoint for the idp to perform its signout action. default is "/signedout"
                 //options.SignOutPath = "/signmeout";             
                 //options.CallbackPath = new PathString("/login/callback");
                 // EntityId (REQUIRED) - The Relying Party Identifier e.g. https://my.la.gov.local
                 options.ServiceProvider.EntityId = Configuration["AppConfiguration:ServiceProvider:EntityId"];

                 // There are two ways to provide FederationMetadata:
                 // Option 1 - A FederationMetadata.xml already exists for your application
                 options.MetadataAddress = @"federatedmetadata.xml";

                 // Option 2 - Have the middleware generate the FederationMetadata.xml file for you
                 //options.MetadataAddress = Configuration["AppConfiguration:IdentityProvider:MetadataAddress"];

                 options.RequireMessageSigned = false;
                 options.WantAssertionsSigned = true;

                 // Have the middleware create the metadata file for you
                 // The default is false. If you don't want a file generated by the middleware, comment the line below.
                 options.CreateMetadataFile = true;

                 // If you want to specify the filename and path for the generated metadata file do so below:
                 //options.DefaultMetadataFileName = "MyMetadataFilename.xml";
                 //options.DefaultMetadataFolderLocation = "MyPath";

                 // (REQUIRED IF) signing AuthnRequest with Sp certificate to Idp. The value here is the certifcate serial number.
                 //if the certificate is in the project. make sure the path to ti is correct. 
                 //password value is needed to access private keys for signature and decryption.
                 //options.ServiceProvider.X509Certificate2 = new X509Certificate2(@"democert.pfx", "1234");

                 //if you want to search in cert store - can be used for production
                 options.ServiceProvider.X509Certificate2 = new Cryptography.X509Certificates.Extension.X509Certificate2(
                     Configuration["AppConfiguration:ServiceProvider:CertificateSerialNumber"],
                     StoreName.My,
                     StoreLocation.LocalMachine,
                     X509FindType.FindBySerialNumber, false);

                 // Force Authentication (optional) - Is authentication required?
                 options.ForceAuthn = true;

                 options.WantAssertionsSigned = false;
                 options.RequireMessageSigned = true;

                 // Service Provider Properties (optional) - These set the appropriate tags in the metadata.xml file
                 options.ServiceProvider.ServiceName = "My Test Site";
                 options.ServiceProvider.Language = "en-US";
                 options.ServiceProvider.OrganizationDisplayName = "Louisiana State Government";
                 options.ServiceProvider.OrganizationName = "Louisiana State Government";
                 options.ServiceProvider.OrganizationURL = "https://ots.la.gov";
                 options.ServiceProvider.ContactPerson = new ContactType()
                 {
                     Company = "Louisiana State Government - OTS",
                     GivenName = "Dina Heidar",
                     EmailAddress = new[] { "dina.heidar@la.gov" },
                     contactType = ContactTypeType.technical,
                     TelephoneNumber = new[] { "+1 234 5678" }
                 };
                 //Events - Modify events below if you want to log errors, add custom claims, etc.
                 options.Events.OnRemoteFailure = context =>
               {
                   return Task.FromResult(0);
               };
                 options.Events.OnTicketReceived = context =>
                 {  //if you need to add custom claims here

                     //example:
                     //var identity = context.Principal.Identity as ClaimsIdentity;
                     //var claims = context.Principal.Claims;                 
                     //if (claims.Any(c => c.Type == "userID"))
                     //{
                     //    var userId = claims.FirstOrDefault(c => c.Type == "userID").Value;
                     //    var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                     //    identity.TryRemoveClaim(name);
                     //    identity.AddClaim(new Claim(ClaimTypes.Name, userId));
                     //}
                     return Task.FromResult(0);
                 };
             })
             .AddCookie(opt =>
             {
                 opt.AccessDeniedPath = new PathString("/Account/AccessDenied");
                 opt.SlidingExpiration = true;
                 opt.LogoutPath = new PathString("/Account/Logout");
                 opt.LoginPath = new PathString("/");
                 opt.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                 opt.Cookie.Expiration = TimeSpan.FromMinutes(30);
             });
            //.AddSamlCore(options =>
            //{
            //    options.SignOutPath = "/signedout";
            //    options.ForceAuthn = true;
            //    options.ServiceProvider.EntityId = "localhost.admin.omv.la.gov";
            //    options.ServiceProvider.ServiceName = "DPS OMV Admin Portal";
            //    options.ServiceProvider.Language = "en-US";
            //    options.ServiceProvider.OrganizationDisplayName = "Office of Management and Finance";
            //    options.ServiceProvider.OrganizationName = "DPS-OMV";
            //    options.ServiceProvider.OrganizationURL = "https://www.expresslane.org/";
            //    options.ServiceProvider.ContactPerson = new ContactType
            //    {
            //        Company = "OTS",
            //        GivenName = "OMV",
            //        SurName = "Modernization",
            //        EmailAddress = new[] { "DPS-OMVModernization@la.gov" },
            //        contactType = ContactTypeType.technical
            //    };

            //    options.Events.OnTicketReceived = context => { return Task.CompletedTask; };

            //    options.Events.OnRemoteFailure = context =>
            //    {
            //        context.Response.Redirect("/Home/AuthError");
            //        context.HandleResponse();
            //        return Task.CompletedTask;
            //    };

            //    options.MetadataAddress = Configuration["AppConfiguration:IdentityProvider:MetadataAddress"];
            //    options.CreateMetadataFile = true;
            //    options.UseTokenLifetime = false;
            //    options.ServiceProvider.X509Certificate2 = new X509Certificate2(@"OmvAdmin-DevTest-public_privatekey.pfx", "");
            //    options.ServiceProvider.X509Certificate2 = new X509Certificate2(Configuration["OtsCertificates:Personal"]);
            //})
            //.AddCookie();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                IdentityModelEventSource.ShowPII = true; //To show detail of error and see the problem
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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}