using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using System.Globalization;
using System.Net;
using WebEmailSendler;
using WebEmailSendler.Context;
using WebEmailSendler.Dependencies;
using WebEmailSendler.Models;
using WebEmailSendler.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson(x =>
{
    x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
});

#if DEBUG
    builder.Configuration
        .SetBasePath($"{Directory.GetCurrentDirectory()}/Config")
        .AddJsonFile("configuration.json")
        .Build();
#endif

builder.Services.AddDbContext<MyDbContext>(
    options =>
    {
        string connection = Environment.GetEnvironmentVariable("ConnectionString")
        ?? builder.Configuration.GetConnectionString("Connection")
        ?? throw new Exception("Отсутствует строка подключения.");
        options.UseNpgsql(connection);
    }
);

builder.Services.AddHangfire(x =>
    {
        string connection = Environment.GetEnvironmentVariable("ConnectionString")
                    ?? builder.Configuration.GetConnectionString("Connection")
                    ?? throw new Exception("Отсутствует строка подключения.");
        x.UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connection),
            new PostgreSqlStorageOptions
            {
                InvisibilityTimeout = TimeSpan.FromHours(30), // Тайм-аут невидимости задач (увеличьте для длительных задач)
                QueuePollInterval = TimeSpan.FromSeconds(15), // Интервал опроса очереди задач
                DistributedLockTimeout = TimeSpan.FromMinutes(30), // Тайм-аут для распределённой блокировки
            }
        );
    }
);

builder.Services.AddHangfireServer((options) =>
{
    new BackgroundJobServerOptions()
    {
        WorkerCount = 1,
        Queues = new[] { "default" }
    };
});

builder.Services.Inject();
builder.Services.AddSignalR()
        .AddHubOptions<SignalHub>(options =>
        {
            options.EnableDetailedErrors = true;  // Включить подробные ошибки для отладки
            options.ClientTimeoutInterval = TimeSpan.FromMinutes(1);  // Тайм-аут для клиентов
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);  // Интервал проверки соединения
            options.HandshakeTimeout = TimeSpan.FromSeconds(15);  // Тайм-аут для рукопожатий
        });
///swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string[] cors = [Environment.GetEnvironmentVariable("ExternalUrl") ?? IPAddress.Loopback.ToString()];
//?? builder.Configuration?.GetSection("Cores").Get<string[]>() ?? [IPAddress.Loopback.ToString()];

#if DEBUG
// добавляем сервисы CORS
builder.Services
      .AddCors(options =>
      {
          options.AddPolicy("default",
              builder => builder
            .WithOrigins(cors)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
           );
      });
#endif

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Error)
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Error)
    .MinimumLevel.Override("Hangfire", LogEventLevel.Error)
    .WriteTo.File($"Logs/logs_.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Console()
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

// Установите культуру для всего приложения
CultureInfo russianCulture = new CultureInfo("ru-RU");
Thread.CurrentThread.CurrentCulture = russianCulture;
Thread.CurrentThread.CurrentUICulture = russianCulture;

Log.Information($"Start App");
Console.WriteLine(DateTimeOffset.Now.ToString("F"));

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    db.Database.Migrate();
    var conf = scope.ServiceProvider.GetRequiredService<ConfigurationService>();
    conf.Init();
}


var options = new DashboardOptions()
{
    DisplayStorageConnectionString = false,
    Authorization = new[] { new MyAuthorizationFilter() }
};

app.UseHangfireDashboard("/task", options);

#if DEBUG
    app.UseCors("default");
#endif

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.MapHub<SignalHub>("/hub");
app.Run();
