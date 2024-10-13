namespace SignalrServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();
            //添加SingalR服务
            builder.Services.AddSignalR();
            //执行后台服务
            builder.Services.AddHostedService<UseInputService>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseAuthorization();

            //注册Hub，括号里是url路由
            app.MapHub<MessageHub>("/message");

            /*
            //var summaries = new[]
            //{
            //    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            //};

            //app.MapGet("/weatherforecast", (HttpContext httpContext) =>
            //{
            //    var forecast = Enumerable.Range(1, 5).Select(index =>
            //        new WeatherForecast
            //        {
            //            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            //            TemperatureC = Random.Shared.Next(-20, 55),
            //            Summary = summaries[Random.Shared.Next(summaries.Length)]
            //        })
            //        .ToArray();
            //    return forecast;
            //});
            */
            app.Run();
        }
    }
}
