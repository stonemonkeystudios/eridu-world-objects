using MessagePack.Resolvers;
using MessagePack;
using Grpc.Net.Client;
using MagicOnion.Server;

namespace HQDotNet.Presence
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddControllersWithViews();
            services.AddGrpc();
            services.AddMagicOnion(); // Add this line
            ConfigureResolvers();
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            /*if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            else {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }*/

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                /*var servDef = app.ApplicationServices.GetService<MagicOnionServiceDefinition>();
                if(servDef != null) {
                    endpoints.MapMagicOnionHttpGateway("_", servDef.MethodHandlers, GrpcChannel.ForAddress("http://localhost:443"));
                    endpoints.MapMagicOnionSwagger("swagger", servDef.MethodHandlers, "/_/");
                }*/


                // Replace to this line instead of MapGrpcService<GreeterService>()
                endpoints.MapMagicOnionService();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync(Liveliness());
                    //await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }

        private static string Liveliness() {
            return "ok";
        }

        private static void ConfigureResolvers() {
            // Set extensions to default resolver.
            /*var resolver = StaticCompositeResolver.Instance.Register(
                Magicon
                );*/
            var resolver = MessagePack.Resolvers.CompositeResolver.Create(
                // enable extension packages first
                MessagePack.Unity.Extension.UnityBlitResolver.Instance,
                MessagePack.Unity.UnityResolver.Instance,

                // finally use standard (default) resolver
                StandardResolver.Instance
            );
            var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);

            // Pass options every time or set as default
            MessagePackSerializer.DefaultOptions = options;
        }
    }
}