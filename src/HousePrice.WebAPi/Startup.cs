using HousePrice.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestSharp;

namespace HousePrice.WebAPi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }
       private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddScoped<IImporter, Importer>();
            services.AddScoped<IHousePriceLookup, HousePriceLookup>();
            var connection = new MongoConnection($"mongodb://{Configuration["connectionString"]}",
                "HousePrice");
            services.AddSingleton<IMongoConnection>(connection);

            services.AddScoped<IRestClient, RestClient>();

            services.AddScoped<IPostcodeLookupConfig, PostcodeLookupConfig>((ctx) =>
            {
                var restClient = ctx.GetService<IRestClient>();

                return new PostcodeLookupConfig(restClient, Configuration["postcodelookupservicename"]);
            });
            services.AddScoped<IPostcodeLookup, PostcodeLookup>();

            services.AddScoped<IHousePriceLookupConfig, HousePriceLookupConfig>((ctx) => new HousePriceLookupConfig(
                ctx.GetService<IMongoContext>(), ctx.GetService<IPostcodeLookup>()));


            services.AddScoped<IMongoContext, MongoContext>();
            services.AddSingleton(Configuration);


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors(builder =>
                builder.AllowAnyOrigin());
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            //  app.UseHttpsRedirection();
            app.UseMvc();

            var mongoContext = new MongoContext( new MongoConnection($"mongodb://{Configuration["connectionString"]}",
                "HousePrice"));
            Importer.AddIndex(mongoContext);
            Importer.AddPostcodeIndex(mongoContext);
            Importer.AddTransferDateIndex(mongoContext);


        }
    }
}