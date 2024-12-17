using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using WebEmailSendler.Context;
using WebEmailSendler.Dependencies;
using WebEmailSendler.Models;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers().AddNewtonsoftJson(x =>
{
    x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
});
//// Add services to the container.
//builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
//   .AddNegotiate();

//builder.Services.AddAuthorization(options =>
//{
//    // By default, all incoming requests will be authorized according to the default policy.
//    options.FallbackPolicy = options.DefaultPolicy;
//});

#if !DEBUG
    builder.Configuration
        .SetBasePath($"{Directory.GetCurrentDirectory()}/Config")
        .AddJsonFile("configuration.json")
        .Build();
#endif

string connection = builder.Configuration.GetConnectionString("Connection") ?? "";

builder.Services.Configure<SmtpConfiguration>(builder.Configuration.GetSection("SmtpConfiguration"));
builder.Services.AddDbContext<MyDbContext>(options => options.UseNpgsql(connection));

builder.Services.AddHangfire(x => x.UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connection), 
    new PostgreSqlStorageOptions
    {
        InvisibilityTimeout = TimeSpan.FromHours(30), // Тайм-аут невидимости задач (увеличьте для длительных задач)
        QueuePollInterval = TimeSpan.FromSeconds(15), // Интервал опроса очереди задач
        DistributedLockTimeout = TimeSpan.FromMinutes(30), // Тайм-аут для распределённой блокировки
    }
    )
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
///swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string[] cors = builder.Configuration.GetSection("Cores").Get<string[]>();

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

Log.Information("Start App");
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
}


var options = new DashboardOptions()
{
    DisplayStorageConnectionString = false,
    Authorization = new[] { new MyAuthorizationFilter() }
};

app.UseHangfireDashboard("/task", options);

app.UseCors("default");
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UsePathBase("/task");
app.MapControllers();
app.Run();
