using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using OrchardCore.Modules;
using OrchardCore.Modules.Manifest;

namespace WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public static IMvcBuilder builders;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOrchardCore();

            //services.AddControllers();
            //services.AddSingleton<IModuleNamesProvider, DynamicModuleNamesProvider>();

            builders = services.AddControllers();

            //builders.ConfigureApplicationPartManager(apm =>
            //{
            //    var baseDirectory = AppContext.BaseDirectory;

            //    var location = Path.Combine(baseDirectory, "modules");

            //    if (!Directory.Exists(location))
            //    {
            //        return;
            //    }

            //    foreach (var file in Directory.EnumerateFiles(location))
            //    {
            //        var assemblyPath = Path.Combine(location, file);
            //        //var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            //        var mycon = new AssemblyLoadContext("mycon", true);
            //        var assembly = mycon.LoadFromAssemblyPath(assemblyPath);
            //        var assemblyPart = new AssemblyPart(assembly);
            //        apm.ApplicationParts.Add(assemblyPart);

            //    }
            //});

            services.AddSingleton<IActionDescriptorChangeProvider>(MyActionDescriptorChangeProvider.Instance);
            services.AddSingleton(MyActionDescriptorChangeProvider.Instance);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseOrchardCore();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
    /// <summary>
    /// 使用IActionDescriptorChangeProvider在运行时激活控制器
    /// 需要在Startup.cs的ConfigureServices方法中，将MyActionDescriptorChangeProvider.Instance属性以单例的方式注册到依赖注入容器中
    /// </summary>
    public class MyActionDescriptorChangeProvider : IActionDescriptorChangeProvider
    {
        public static MyActionDescriptorChangeProvider Instance { get; } = new MyActionDescriptorChangeProvider();

        public CancellationTokenSource TokenSource { get; private set; }

        public bool HasChanged { get; set; }

        public IChangeToken GetChangeToken()
        {
            TokenSource = new CancellationTokenSource();
            return new CancellationChangeToken(TokenSource.Token);
        }
    }
    public class DynamicModuleNamesProvider : IModuleNamesProvider
    {
        private readonly List<string> _moduleNames = new List<string>();

        public DynamicModuleNamesProvider()
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

                var assembly = Assembly.LoadFrom(assemblyPath);

                _moduleNames.AddRange(assembly.GetCustomAttributes<ModuleMarkerAttribute>().Select(m => m.Name));
            }
        }

        public IEnumerable<string> GetModuleNames()
        {
            return _moduleNames;
        }
    }
}
