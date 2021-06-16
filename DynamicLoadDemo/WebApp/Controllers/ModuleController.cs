using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModuleController : ControllerBase
    {
        // GET: api/<ModuleController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            Startup.builders.ConfigureApplicationPartManager(apm =>
            {
                var baseDirectory = AppContext.BaseDirectory;

                var location = Path.Combine(baseDirectory, "modules");

                if (!Directory.Exists(location))
                {
                    return;
                }

                foreach (var file in Directory.EnumerateFiles(location))
                {
                    var assemblyPath = Path.Combine(location, file);
                    //var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
                    var mycon = new AssemblyLoadContext("mycon", true);
                    var assembly = mycon.LoadFromAssemblyPath(assemblyPath);
                    var assemblyPart = new AssemblyPart(assembly);
                    apm.ApplicationParts.Add(assemblyPart);
                    MyConfig.assemblys.Add(assembly);
                }
            });

            return new string[] { "value1", "value2" };
        }

        // GET api/<ModuleController>/0
        [HttpGet("{id}")]
        public string Get(int id=0)
        {
            Startup.builders.ConfigureApplicationPartManager(apm =>
            {             
                var assembly = MyConfig.assemblys[id];
                var assemblyPart = new AssemblyPart(assembly);
                apm.ApplicationParts.Remove(assemblyPart);
                AssemblyLoadContext.GetLoadContext(assembly)?.Unload();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            });
            return "value";
        }

        // POST api/<ModuleController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ModuleController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ModuleController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }


    public static class MyConfig
    {
        public static List<Assembly> assemblys = new List<Assembly>();
    }
}
