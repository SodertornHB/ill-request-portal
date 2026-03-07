// This is an organization specific file 
using AutoMapper;
using IllRequestPortal.Logic.Http;
using IllRequestPortal.Logic.Model;
using IllRequestPortal.Logic.Services;
using IllRequestPortal.Logic.Settings;
using IllRequestPortal.Web;
using IllRequestPortal.Web.ViewModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Web
{
    public class StartupExtended : Startup
    {
        public StartupExtended(IConfiguration configuration, IWebHostEnvironment env) : base(configuration, env)
        { }

        protected override IList<CultureInfo> GetSupportedLanguages()
        {
            return new List<CultureInfo> {
                new CultureInfo("sv-se"),
                new CultureInfo("en-gb"),
            };
        }

        protected override RequestCulture GetDefaultCulture()
        {
            return new RequestCulture("sv-se");
        }

        protected override void CustomServiceConfiguration(IServiceCollection services)
        {
            services.Configure<KohaApiSettings>(Configuration.GetSection("KohaApiSettings"));

            services.AddTransient<IIllRequestService, IllRequestServiceExtended>();
            services.AddTransient<IKohaGetHttpService, KohaGetHttpService>();
            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
            });

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
                Formatting = Formatting.Indented
            };
        }

        protected override void CustomConfiguration(IApplicationBuilder app, IWebHostEnvironment env)
        {
           
        }

        protected override void ConfigureExceptionHandler(IApplicationBuilder app)
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
                    if (e.Error.Message.Contains("No connection could be made because the target machine actively refused it")) context.Response.Redirect("/no-auth");
                });
            });
        }
        public override IMapper GetMapper()
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                var profile = new MappingConfiguration();
                profile = AddAdditionalMappingConfig(profile);

                mc.AddProfile(profile);
            });

            return mapperConfig.CreateMapper();
        }

        public static MappingConfiguration AddAdditionalMappingConfig(MappingConfiguration profile)
        {

            profile.CreateMap<IllRequestViewModelExtended, IllRequestExtended>();

           return profile;
        }

    }

    public class CleanUpServiceExtended : CleanUpService
    {
        private const int WEEDING_TIME_IN_DAYS = -14;
        private readonly ILogService logService;

        public CleanUpServiceExtended(IOptions<ApplicationSettings> options,
            ILogService logService) : base(options)
        {
            this.logService = logService;
        }

        public override async Task ProcessCleanUp()
        {
            await base.ProcessCleanUp();
            await DeleteLogs();
        }

        private async Task DeleteLogs()
        {
            var all = await logService.GetAll();
            foreach (var item in all.Where(x => x.CreatedOn < DateTime.Now.AddDays(WEEDING_TIME_IN_DAYS)))
            {
                await logService.Delete(item.Id);
            }
        }
    }
}