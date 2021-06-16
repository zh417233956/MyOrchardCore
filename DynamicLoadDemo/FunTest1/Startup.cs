using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace FunTest1
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            //routes.MapAreaControllerRoute(
            //    name: "Home",
            //    areaName: "FunTest1",
            //    pattern: "Home/Index",
            //    defaults: new { controller = "Home", action = "Index" }
            //);
            routes.MapControllerRoute(
                name:"module api",
                pattern: "api/{controller}/{action}",
                defaults: new { controller = "Home", action = "Index" }
                );
        }
    }
}
