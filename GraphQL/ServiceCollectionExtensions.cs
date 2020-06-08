using api.GraphQL.Types;
using api.GraphQL.Schemas;
using GraphiQl;
using GraphQL;
using GraphQL.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace api.GraphQL
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGraphQLServices(this IServiceCollection services)
        {
            services.Configure<KestrelServerOptions>(options =>
                {
                    options.AllowSynchronousIO = true;
                });
            services.AddScoped<UserType>();
            services.AddScoped<Query>();
            services.AddScoped<MeridiaSchema>();
            services.AddScoped<IDependencyResolver>(
               c => new FuncDependencyResolver(type =>
                   c.GetRequiredService(type)));
            services.AddGraphQL(options =>
            {
                options.EnableMetrics = true;
                options.ExposeExceptions = true;
            });
        }

        public static void ConfigureGraphQL(this IApplicationBuilder app)
        {
            app.UseGraphiQl();
            app.UseGraphQL<MeridiaSchema>();
        }
    }
}