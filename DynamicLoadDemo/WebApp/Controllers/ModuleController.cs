using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
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
        MyAssemblyConfig _myAssemblyConfig;
        IMvcBuilder _builders;
        public ModuleController(MyAssemblyConfig myAssemblyConfig)
        {
            _myAssemblyConfig = myAssemblyConfig;
            _builders = myAssemblyConfig.mvcbuilders;
        }
        // GET: api/Enabled
        [HttpGet("Enabled")]
        public ActionResult Enabled()
        {
            var guid = Guid.NewGuid().ToString();
            var result = new { code = 200, msg = "", uuid = guid };
            try
            {
                _builders.ConfigureApplicationPartManager(apm =>
                {
                    var baseDirectory = AppContext.BaseDirectory;

                    var location = Path.Combine(baseDirectory, "modules");

                    if (Directory.Exists(location))
                    {
                        var locationPackage = location + "\\packages";
                        if (Directory.Exists(locationPackage))
                        {
                            foreach (var file in Directory.EnumerateFiles(locationPackage))
                            {
                                var assemblyPath = Path.Combine(locationPackage, file);
                                //默认上下文只允许加载一次且不会卸载，解决重复加载问题
                                using (var fs = new FileStream(assemblyPath, FileMode.Open))
                                {
                                    var assembly = AssemblyLoadContext.Default.LoadFromStream(fs);
                                    var assemblyPart = new AssemblyPart(assembly);
                                    if (_myAssemblyConfig.assemblyPackages.Exists(m => m.FullName == assembly.FullName) == false)
                                    {
                                        apm.ApplicationParts.Add(assemblyPart);
                                        _myAssemblyConfig.assemblyPackages.Add(assembly);
                                    }
                                }
                            }
                        }
                        foreach (var file in Directory.EnumerateFiles(location))
                        {
                            var assemblyPath = Path.Combine(location, file);
                            //LoadFromAssemblyPath，当程序卸载后，无法删除，通过LoadFromStream可以解决此问题
                            //AssemblyLoadContext.Default默认上下文是无法卸载的，需要新建上下文来解决
                            //var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);                                            
                            using (var fs = new FileStream(assemblyPath, FileMode.Open))
                            {
                                var mycon = new AssemblyLoadContext(file, true);
                                var assembly = mycon.LoadFromStream(fs);
                                var assemblyPart = new AssemblyPart(assembly);
                                if (_myAssemblyConfig.assemblys.Exists(m => m.FullName == assembly.FullName) == false)
                                {
                                    apm.ApplicationParts.Add(assemblyPart);
                                    _myAssemblyConfig.assemblys.Add(assembly);
                                }
                            }
                        }
                    }
                });

                MyActionDescriptorChangeProvider.Instance.HasChanged = true;
                MyActionDescriptorChangeProvider.Instance.TokenSource.Cancel();
            }
            catch (Exception ex)
            {
                result = new { code = 403, msg = ex.ToString(), uuid = guid };
            }
            return Content(JsonConvert.SerializeObject(result));
        }

        // GET api/Disabled
        [HttpGet("Disabled")]
        public ActionResult Disabled()
        {
            var guid = Guid.NewGuid().ToString();
            var result = new { code = 200, msg = "", uuid = guid };
            try
            {
                _builders.ConfigureApplicationPartManager(apm =>
                {
                    var assemblyPartRemove = new List<ApplicationPart>();
                    var assemblyRemove = new List<Assembly>();
                    foreach (var assembly in _myAssemblyConfig.assemblys)
                    {
                        var assemblyPart = new AssemblyPart(assembly);
                        foreach (AssemblyPart item in apm.ApplicationParts)
                        {
                            if (item.Assembly.FullName.Equals(assemblyPart.Assembly.FullName))
                            {
                                assemblyRemove.Add(assembly);
                                assemblyPartRemove.Add(item);
                                break;
                            }
                        }
                    }
                    foreach (var assemblyPartItem in assemblyPartRemove)
                    {
                        apm.ApplicationParts.Remove(assemblyPartItem);
                    }
                    foreach (var assemblyItem in assemblyRemove)
                    {
                        _myAssemblyConfig.assemblys.Remove(assemblyItem);
                        AssemblyLoadContext.GetLoadContext(assemblyItem)?.Unload();
                    }
                    MyActionDescriptorChangeProvider.Instance.HasChanged = true;
                    MyActionDescriptorChangeProvider.Instance.TokenSource.Cancel();
                });
            }
            catch (Exception ex)
            {
                result = new { code = 403, msg = ex.ToString(), uuid = guid };
            }

            return Content(JsonConvert.SerializeObject(result));
        }
    }

    /// <summary>
    /// 模块加载配置
    /// </summary>
    public class MyAssemblyConfig
    {
        /// <summary>
        /// 构造函数，初始化加载模块信息
        /// </summary>
        public MyAssemblyConfig(IMvcBuilder mvcBuilders)
        {
            _mvcbuilders = mvcBuilders;
            assemblys = new List<Assembly>();
            assemblyPackages = new List<Assembly>();
        }
        public List<Assembly> assemblys { get; set; }
        public List<Assembly> assemblyPackages { get; set; }
        private IMvcBuilder _mvcbuilders;

        public IMvcBuilder mvcbuilders
        {
            get { return _mvcbuilders; }
        }
    }
}
