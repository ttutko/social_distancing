using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MongoDB.Driver;
using AspNetCore.Identity.Mongo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Logging;
using Microsoft.AspNetCore.HttpOverrides;

namespace api
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
            IdentityModelEventSource.ShowPII = true;
            
            var client = new MongoClient();
            
            services.AddSingleton<IMongoClient>(client);

            services.Configure<ForwardedHeadersOptions>(options => {
                options.ForwardedHeaders = 
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            services.AddHttpContextAccessor();

            // services.AddIdentityMongoDbProvider<ApplicationUser>(identity =>
            // {
                
            // },
            // mongo => 
            // {
            //     mongo.UsersCollection = "users";
            //     mongo.ConnectionString = "mongodb://localhost:27017";
            // });
            
            services.AddAuthentication(options => 
            {
                // options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                // options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                
            })
            .AddCookie()
            // .AddOpenIdConnect(options =>
            // {
            //     options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //     options.Authority = "http://localhost:8888/auth/realms/test";
            //     options.RequireHttpsMetadata = false;
            //     options.ClientId = "webapi";
            //     options.ClientSecret = "clientsecret";
            //     options.ResponseType = OpenIdConnectResponseType.Code;
            //     options.GetClaimsFromUserInfoEndpoint = true;
            //     options.Scope.Add("openid");
            //     options.Scope.Add("profile");
            //     options.SaveTokens = true;
            //     options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            //     {
            //         NameClaimType = "name",
            //         RoleClaimType = "groups",
            //         ValidateIssuer = true
            //     };
            //     // options.Events.OnUserInformationReceived = ctx => {
            //     //     Console.WriteLine($"***** USER **** {ctx.User.ToString()}");

            //     //     return Task.CompletedTask;
            //     // };            
            // })
            .AddJwtBearer( o => 
            {
                o.Authority = "https://localhost:8888/auth/realms/test";
                o.Audience = "postman";
                o.RequireHttpsMetadata = true;
                o.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = c =>
                    {
                        c.NoResult();
                        c.Response.StatusCode = 500;
                        c.Response.ContentType = "text/plain";
                        // if(Environment.IsDevelopment())
                        // {
                        //     return c.Response.WriteAsync(c.Exception.ToString());
                        // }
                        // return c.Response.Body.WriteAsync("An error occured processing your authentication.");
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("/index.html");
            });
        }
    }
}
