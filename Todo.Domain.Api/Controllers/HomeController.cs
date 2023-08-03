using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Todo.Domain.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAppInfo()
        {
            string appName = Assembly.GetEntryAssembly().GetName().Name;
            string appVersion = Assembly.GetEntryAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

            var appInfo = new
            {
                ApplicationName = appName,
                Version = appVersion
            };

            return Ok(appInfo);
        }
    }
}
