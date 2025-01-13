
using WebEmailSendler.Managers;
using WebEmailSendler.Services;

namespace WebEmailSendler.Dependencies
{
    public static class Injection
    {
        public static void Inject(this IServiceCollection services)
        {
            services.AddScoped<DataManager>();

            services.AddScoped<FileService>();
            services.AddScoped<SendlerService>();
            services.AddScoped<DataService>();
            services.AddScoped<JobService>();
            services.AddScoped<SampleService>();
            services.AddScoped<MyConfigurationManager>();
            services.AddSingleton<ConfigurationService>();

            services.AddSingleton<SignalHub>();
        }
    }
}
