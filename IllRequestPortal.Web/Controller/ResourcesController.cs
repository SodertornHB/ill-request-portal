
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public partial class ResourcesController : ControllerBase
    {
        private readonly IStringLocalizer localizer;
        private readonly ILogger<ResourcesController> logger;

        public ResourcesController(IStringLocalizerFactory factory,
            ILogger<ResourcesController> logger)
        {
            var type = typeof(SharedResource);
            var assemblyName = new AssemblyName(type.GetTypeInfo().Assembly.FullName);
            localizer = factory.Create("SharedResource", assemblyName.Name);
            this.logger = logger;
        }

        [HttpGet()]
        public IActionResult Get()
        {
            try
            {
                var locatedStrings = GetLocatedStrings();
                var json = ConvertToSerializedJson(locatedStrings);
                return Ok(json);
            }
            catch (System.Resources.MissingManifestResourceException e)
            {
                logger.LogWarning(e.Message);
                return Ok(e.Message);
            }
        }

        [HttpGet("{key}")]
        public IActionResult Get(string key)
        {
            try
            {
                var locatedString = GetLocatedStrings(key);
                var json = ConvertToSerializedJson(locatedString);
                return Ok(json);
            }
            catch (System.Resources.MissingManifestResourceException e)
            {
                logger.LogWarning(e.Message);
                return Ok(e.Message);
            }
        }

        #region private 

        private IEnumerable<KeyValuePair<string, string>> GetLocatedStrings(string key = "")
        {
            var allLocalizedStrings = localizer.GetAllStrings().Where(x => key == "" || x.Name.Equals(key,System.StringComparison.OrdinalIgnoreCase));
            return allLocalizedStrings.Select(x => new KeyValuePair<string, string>(x.Name, x.Value));
        }

        private static string ConvertToSerializedJson(IEnumerable<KeyValuePair<string, string>> keyValuePairCollection)
        {
            var dictionary = new Dictionary<string, string>(keyValuePairCollection);
            return JsonConvert.SerializeObject(dictionary);
        }

        #endregion
    }
}