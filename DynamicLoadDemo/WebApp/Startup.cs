using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

            builders.ConfigureApplicationPartManager(apm =>
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

                }
            });
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
