using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentScheduler;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using WebApp.Data;
using WebApp.Models;
using WebApp.Services;
using System.Linq;

namespace WebApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public static IConfigurationRoot Configuration { get; set; }
        /// <summary>
        /// This method gets called by the runtime. Used to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 3;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(config =>
            {
                config.Cookies.ApplicationCookie.Events =
                    new CookieAuthenticationEvents
                    {
                        OnRedirectToLogin = ctx =>
                        {
                            if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                            {
                                ctx.Response.StatusCode = 401;
                                return Task.FromResult<object>(null);
                            }

                            ctx.Response.Redirect(ctx.RedirectUri);
                            return Task.FromResult<object>(null);
                        }
                    };
            });

            services.AddMemoryCache();
//            services.AddMvc();
            services.AddMvc(options =>
            {
                options.RespectBrowserAcceptHeader = true;
                options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
                options.OutputFormatters.Add(new JsonOutputFormatter(new JsonSerializerSettings(), ArrayPool<char>.Shared));
            });

            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddTransient<ISourceSaver, XmlSourceSaver>();
            services.AddTransient<IXmlParser, XmlFeedParser>();
            services.AddTransient<ApplicationDbContext>();

            services.AddTransient<IJobFactory, JobFactory>();
            services.AddTransient<Registry, MyRegistry>();
            services.AddTransient<Sheluder>();
            services.AddTransient<Job>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "KodiSoft API", Version = "v1" });
                var xmlPath = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "WebApp.xml");
                c.IncludeXmlComments(xmlPath);
            });
        }
        /// <summary>
        /// This method gets called by the runtime. Used to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="loggerFactory"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();

                //using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
                //{
                //    var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                //    context.Database.EnsureDeleted();
                //    context.Database.EnsureCreated();
                //    context.Database.Migrate();
                //}
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseIdentity();

            app.UseFacebookAuthentication(new FacebookOptions
            {
                AppId = "150880478813274",
                AppSecret = "fc702c7362622d5e7dc9638c5b44b098",
                Scope = { "email", "public_profile" }
            });
            app.UseGoogleAuthentication(new GoogleOptions
            {
                ClientId = "336420643951-4trkuc944lvjkdcgot29m2l5tjtfuvn2.apps.googleusercontent.com",
                ClientSecret = "xqaofKQPnVJAYa-07CsaOV9B"
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "KodiSoft API V1");
            });

            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var jobFactory = new JobFactory(serviceScope.ServiceProvider.GetRequiredService<IServiceProvider>());

                JobManager.JobFactory = jobFactory;
                JobManager.Initialize(new MyRegistry());
            }

        }
    }
}
