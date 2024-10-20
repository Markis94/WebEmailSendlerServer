
using WebEmailSendler.Managers;
using WebEmailSendler.Services;

namespace WebEmailSendler.Dependencies
{
    public static class Injection
    {
        public static void Inject(this IServiceCollection services)
        {
            services.AddScoped<FileService>();
            services.AddScoped<SendlerService>();
            services.AddScoped<DataManager>();
            services.AddScoped<DataService>();
        }
    }
}
