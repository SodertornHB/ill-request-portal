
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using AutoMapper;
using Localization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection; 
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IllRequestPortal.Logic.DataAccess;
using IllRequestPortal.Logic.Http;
using IllRequestPortal.Logic.Model;
using IllRequestPortal.Logic.Services;
using IllRequestPortal.Logic.Settings;
using IllRequestPortal.Web.ViewModel;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using System;
using System.IO;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;

namespace IllRequestPortal.Web
{
    public class Startup
    {       
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            #region register IllRequest
            services.AddTransient<IIllRequestService, IllRequestService>();
            services.AddTransient<IIllRequestDataAccess, IllRequestDataAccess>();
            services.AddSingleton<SqlStringBuilder<IllRequest>>();
            #endregion

            #region register Log
            services.AddTransient<ILogService, LogService>();
            services.AddTransient<ILogDataAccess, LogDataAccess>();
            services.AddSingleton<SqlStringBuilder<Log>>();
            #endregion

            #region register Migration
            services.AddTransient<IMigrationService, MigrationService>();
            services.AddTransient<IMigrationDataAccess, MigrationDataAccess>();
            services.AddSingleton<SqlStringBuilder<Migration>>();
            #endregion

            #region register Middleware services
            services.AddTransient<ICleanUpService, CleanUpService>();
            services.AddTransient<IVersionInfoService, VersionInfoService>();
            #endregion

            services.AddSingleton<LocService>();
            services.AddTransient<IHttpClient, Logic.Http.HttpClient>();
            services.Configure<AuthenticationSettings>(Configuration.GetSection("Authentication"));
            services.Configure<ApplicationSettings>(Configuration.GetSection("Application"));
            services.AddTransient<ISqlDataAccess, SqlDataAccess>();
            
            services.AddHttpClient();
            services.AddSingleton(GetMapper());
            ConfigureLocalization(services);
            services.AddLocalization(x => x.ResourcesPath = "Resources");
            CustomServiceConfiguration(services);
            services.AddControllersWithViews()
                .AddViewLocalization();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<ApplicationSettings> applicationSettingsOptions)
        {
            app.UsePathBase(applicationSettingsOptions.Value.Name);
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseRequestLocalization();  
            RegisterMiddleware(app);
            ConfigureExceptionHandler(app);
            CustomConfiguration(app, env); 

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        protected virtual void CustomServiceConfiguration(IServiceCollection services)
        {
            //override for custom behaviour
        }

        protected virtual void CustomConfiguration(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //override for custom behaviour
        }

        protected virtual void RegisterMiddleware(IApplicationBuilder app)
        {
            app.UseMiddleware<Version>();
            app.UseMiddleware<CleanUp>();
            app.UseMiddleware<RedirectNotFound>();
            app.UseMiddleware<RedirectTablelang>();
        }
        
         protected void ConfigureLocalization(IServiceCollection services)
        {
            var supportedCultures = GetSupportedLanguages();

            services.Configure((Action<RequestLocalizationOptions>)(options =>
            {
                options.DefaultRequestCulture = GetDefaultCulture();
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                options.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                  new QueryStringRequestCultureProvider(),
                  new CookieRequestCultureProvider()
                };
            }));
        }

        protected virtual RequestCulture GetDefaultCulture() => new RequestCulture("en-gb");

        protected virtual IList<CultureInfo> GetSupportedLanguages()
        {
            return new List<CultureInfo> {
                new CultureInfo("en-gb")
            };
        }

        protected virtual void ConfigureExceptionHandler(IApplicationBuilder app)
        {
            app.UseExceptionHandler(builder =>
            {
                builder.Run(async context =>
                {
                    var e = context.Features.Get<IExceptionHandlerFeature>();
                    if (e == null) return;
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "text/html";
                    var factory = builder.ApplicationServices.GetService<ILoggerFactory>();
                    var logger = factory.CreateLogger("ExceptionLogger");
                    logger.LogError(e.Error, e.Error.Message);
                    context.Response.Redirect("/error");
                });
            });
        }

        public virtual IMapper GetMapper()
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingConfiguration());
            });

            return mapperConfig.CreateMapper();
        }
    }

    public class MappingConfiguration : Profile
    {
        public MappingConfiguration()
        {
        
                CreateMap<IllRequest, IllRequestViewModel>();

                CreateMap<IllRequestViewModel, IllRequest>();
        
                CreateMap<Log, LogViewModel>();

                CreateMap<LogViewModel, Log>();
        
                CreateMap<Migration, MigrationViewModel>();

                CreateMap<MigrationViewModel, Migration>();
        
    
        }
    }

    public static class CustomHtmlHelpers
    {
        public static async Task<IHtmlContent> GetExternalHtmlAsync(this IHtmlHelper helper, string url)
        {
            try
            {
                using var httpClient = new System.Net.Http.HttpClient();
                var html = await httpClient.GetStringAsync(url);
                return new HtmlString(html);
            }
            catch
            {
                return new HtmlString("<!-- Error loading external HTML -->");
            }
        }
    }

    #region middleware

    public class RedirectNotFound
    {
        private readonly RequestDelegate next;
        private readonly ApplicationSettings applicationSettings;

        public RedirectNotFound(RequestDelegate next, IOptions<ApplicationSettings> options)
        {
            this.next = next;
            applicationSettings = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await next.Invoke(context);
            
            if (context.Response.StatusCode == 404 && !context.Response.HasStarted && !context.Request.Path.Value.Contains("notfound"))
            {
                context.Response.Redirect($"/notfound?page={context.Request.Path}");
            }
        }
    }

    public class RedirectTablelang
    {
        private readonly RequestDelegate next;
        private readonly ApplicationSettings applicationSettings;

        public RedirectTablelang(RequestDelegate next, IOptions<ApplicationSettings> options)
        {
            this.next = next;
            applicationSettings = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            const string resourceBase = "/resources";
            var applicationName = applicationSettings.Name;
            var path = context.Request.Path;
            var tableLangPath = $"{applicationName}{resourceBase}";
            if (path.Value.EndsWith(resourceBase, StringComparison.InvariantCultureIgnoreCase) &&
                !path.Value.Equals(tableLangPath, StringComparison.InvariantCultureIgnoreCase) &&
                !path.Value.Equals(resourceBase, StringComparison.InvariantCultureIgnoreCase))
            {
                context.Response.StatusCode = 302;
                context.Response.Headers["Location"] = tableLangPath;
                return;
            }
            await next.Invoke(context);
        }
    }

    

    #region clean-up
    public interface ICleanUp
    {
        Task InvokeAsync(HttpContext context);
    }

    public class CleanUp : ICleanUp
    {
        private readonly RequestDelegate next;
        private readonly ICleanUpService cleanUpService;

        public CleanUp(RequestDelegate next, ICleanUpService cleanUpService)
        {
            this.next = next;
            this.cleanUpService = cleanUpService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.Value.EndsWith("clean-up", StringComparison.InvariantCultureIgnoreCase))
            {
                await cleanUpService.ProcessCleanUp();
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsync("Clean-up ok.");
            }
            else
            {
                await next.Invoke(context);
            }
        }
    }
    public interface ICleanUpService
    {
        Task ProcessCleanUp();
    }
    public class CleanUpService : ICleanUpService
    {
        private readonly ApplicationSettings applicationSettings;

        public CleanUpService(IOptions<ApplicationSettings> options)
        {
            this.applicationSettings = options.Value;
        }

        public virtual async Task ProcessCleanUp()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "logs");

            var directory = new DirectoryInfo(path);

            foreach (var file in directory.GetFiles())
            {
                if (file.LastAccessTime < DateTime.Today.AddDays(-applicationSettings.KeepLogsInDays))
                {
                    await Task.Run(() => file.Delete());
                }
            }
        }

    }

    #endregion
    
    #region version middleware


    public class Version
    {
        private readonly RequestDelegate next;
        private readonly IVersionInfoService versionInfoService;

        public Version(RequestDelegate next,
            IVersionInfoService versionInfoService)
        {
            this.next = next;
            this.versionInfoService = versionInfoService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.Value.EndsWith("version", StringComparison.InvariantCultureIgnoreCase))
            {
                var versionInformation = await versionInfoService.GetVersionInformation();
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsync(versionInformation);
            }
            else
            {
                await next.Invoke(context);
            }
        }


    }

    public interface IVersionInfoService
    {
        Task<string> GetVersionInformation();
    }

    public class VersionInfoService : IVersionInfoService
    {
        private readonly IMigrationDataAccess migrationDataAccess;
        protected readonly ApplicationSettings applicationSettings;

        public VersionInfoService(IOptions<ApplicationSettings> options,
            IMigrationDataAccess migrationDataAccess)
        {
            this.migrationDataAccess = migrationDataAccess;
            applicationSettings = options.Value;
        }

        public virtual async Task<string> GetVersionInformation()
        {
            var assembly = Assembly.GetEntryAssembly();
            var version = assembly.GetName().Version.ToString();

            var versionInfo = new VersionInfo
            {
                ServiceName = applicationSettings.Name
            };

            var migration = (await migrationDataAccess.GetAll()).OrderByDescending(x => x.CreatedOn).FirstOrDefault();
            if (migration != null)
            {
                versionInfo.ClientVersion = migration.ClientVersion;
                versionInfo.DatabaseVersion = migration.DatabaseVersion;
            }

            versionInfo.Assemblies.Add(new AssemblyInfo
            {
                Name = assembly.GetName().Name,
                Version = version
            });

            var referencedAssemblies = assembly.GetReferencedAssemblies();
            foreach (var referencedAssembly in referencedAssemblies)
            {
                if (referencedAssembly.Name.StartsWith("Sh.Library.") || referencedAssembly.Name.StartsWith("Logic"))
                {
                    versionInfo.Assemblies.Add(new AssemblyInfo
                    {
                        Name = referencedAssembly.Name,
                        Version = referencedAssembly.Version.ToString()
                    });
                }
            }
            var settings = new JsonSerializerSettings { Formatting = Formatting.Indented };
            return JsonConvert.SerializeObject(versionInfo, settings);
        }
    }

    public class VersionInfo
    {
        public string ServiceName { get; set; }
        public string ClientVersion { get; set; }
        public string DatabaseVersion { get; set; }
        public List<AssemblyInfo> Assemblies { get; set; } = new List<AssemblyInfo>();
    }

    public class AssemblyInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
    }

    #endregion

    #endregion
}
