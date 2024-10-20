using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using System.Reflection;
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

string connection = builder.Configuration.GetConnectionString("Connection") ?? "";

builder.Services.Configure<SmtpConfiguration>(builder.Configuration.GetSection("SmtpConfiguration"));
builder.Services.AddDbContext<MyDbContext>(options => options.UseNpgsql(connection));

builder.Services.AddHangfire(x => x.UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connection)));

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
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCor", LogEventLevel.Warning)
    .WriteTo.File($"Logs/{Assembly.GetExecutingAssembly().GetName().Name}.log")
    .WriteTo.Console()
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

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

app.UseCors("default");
// Configure the HTTP request pipeline.
app.UseHangfireDashboard("", new DashboardOptions
{
    Authorization = new[] { new AllowAllAuthorizationFilter() }
});

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();


app.Run();
