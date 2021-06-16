using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

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

                if (Directory.Exists(location))
                {
                    //foreach (var file in Directory.EnumerateFiles(location))
                    //{
                    //    var assemblyPath = Path.Combine(location, file);
                    //    //var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
                    //    var mycon = new AssemblyLoadContext("mycon", true);                       
                    //    var assembly = mycon.LoadFromAssemblyPath(assemblyPath);
                    //    var assemblyPart = new AssemblyPart(assembly);
                    //    apm.ApplicationParts.Add(assemblyPart);
                    //    if (MyConfig.assemblys.Exists(m => m.FullName == assembly.FullName) == false)
                    //    {
                    //        MyConfig.assemblys.Add(assembly);
                    //    }
                    //}

                    foreach (var file in Directory.EnumerateFiles(location))
                    {
                        var assemblyPath = Path.Combine(location, file);
                        //var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
                        var mycon = new AssemblyLoadContext("mycon", true);
                        //TODO:重复加载问题
                        using (var fs = new FileStream(assemblyPath, FileMode.Open))
                        {
                            var assembly = mycon.LoadFromStream(fs);
                            var assemblyPart = new AssemblyPart(assembly);
                            apm.ApplicationParts.Add(assemblyPart);
                            if (MyConfig.assemblys.Exists(m => m.FullName == assembly.FullName) == false)
                            {
                                MyConfig.assemblys.Add(assembly);
                            }
                        }
                    }
                }
            });

            MyActionDescriptorChangeProvider.Instance.HasChanged = true;
            MyActionDescriptorChangeProvider.Instance.TokenSource.Cancel();

            return new string[] { "value1", "value2" };
        }

        // GET api/<ModuleController>/0
        [HttpGet("{id}")]
        public string Get(int id = 0)
        {
            try
            {
                Startup.builders.ConfigureApplicationPartManager(apm =>
                {
                    var assembly = MyConfig.assemblys[id];
                    var assemblyPart = new AssemblyPart(assembly);
                    AssemblyPart removeAssemblyPart = null;
                    foreach (AssemblyPart item in apm.ApplicationParts)
                    {
                        if (item.Assembly.FullName.Equals(assemblyPart.Assembly.FullName))
                        {
                            removeAssemblyPart = item;
                            break;
                        }
                    }
                    if (removeAssemblyPart != null)
                    {
                        apm.ApplicationParts.Remove(removeAssemblyPart);
                        AssemblyLoadContext.GetLoadContext(assembly)?.Unload();

                        MyActionDescriptorChangeProvider.Instance.HasChanged = true;
                        MyActionDescriptorChangeProvider.Instance.TokenSource.Cancel();
                    }
                });
            }
            catch (Exception)
            {
            }

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
