using KT1_Logging_TaskManager_MVC;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Приложение запускается...");

    builder.Services.AddControllersWithViews();
    builder.Host.UseSerilog();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite("Data Source=TaskManagerDatabase.db"));

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthorization();

    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            Log.Error("Необработанное исключение в приложении");
            await context.Response.WriteAsync("Произошла ошибка. Пожалуйста, попробуйте позже.");
        });
    });

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=CurrentTasks}/{action=Index}/{id?}");

    Log.Information("Конвейер приложения настроен");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Критическая ошибка при запуске приложения");
}
finally
{
    Log.CloseAndFlush();
}