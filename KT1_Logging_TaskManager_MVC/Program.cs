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
    Log.Information("[INFO] Приложение запускается...");

    // Add services to the container.
    builder.Services.AddControllersWithViews();

    builder.Host.UseSerilog();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite("Data Source=TaskManagerDatabase.db"));

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseRouting();

    app.UseAuthorization();

    app.MapStaticAssets();

    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            Log.Error("[ERROR] Необработанное исключение в приложении");
            await context.Response.WriteAsync("Произошла ошибка. Пожалуйста, попробуйте ещё.");
        });
    });

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=CurrentTasks}/{action=Index}/{id?}")
        .WithStaticAssets();

    app.Run();
}

catch (Exception ex)
{
    Log.Fatal(ex, "[CRITICAL] Критическая ошибка при запуске приложения");
}

finally
{
    Log.CloseAndFlush();
}