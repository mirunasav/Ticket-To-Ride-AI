
using TicketToRide.Model.GameBoard;
using TicketToRide.Services;

namespace TicketToRide
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSingleton<GameService>();
            builder.Services.AddSingleton<RouteService>();
            builder.Services.AddSingleton<MoveValidatorService>();
            builder.Services.AddSingleton<GameProvider>();

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddSession();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
            }

            app.UseStaticFiles();

            //app.UseSession();
            app.UseRouting();
            app.UseAuthorization();

            app.UseSession();

#pragma warning disable ASP0014 // Suggest using top level route registrations
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                     name: "default",
                  pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapFallbackToFile("/index.html");
            });
#pragma warning restore ASP0014 // Suggest using top level route registrations


            app.MapControllers();
            app.Run();
        }
    }
}
