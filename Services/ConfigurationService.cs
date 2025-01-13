using Serilog;
using WebEmailSendler.Managers;
using WebEmailSendler.Models;

namespace WebEmailSendler.Services
{
    public class ConfigurationService
    {
        private readonly IServiceProvider serviceProvider;
        private AppConfiguration Configuration { get; set; } = new AppConfiguration() { HostEmailAddress = "", Login = "", Password = "", Server = "" };

        public ConfigurationService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            Console.WriteLine("init");
        }

        public void Init()
        {
            using var scope = serviceProvider.CreateScope();
            var configurationManager = scope.ServiceProvider.GetRequiredService<MyConfigurationManager>();
            Configuration = configurationManager.GetConfigurationAsync();
        }

        public AppConfiguration GetConfiguration()
        {
            return Configuration;
        }

        public void UpdateConfiguration(AppConfiguration appConfiguration)
        {
            using var scope = serviceProvider.CreateScope();
            var configurationManager = scope.ServiceProvider.GetRequiredService<MyConfigurationManager>();
            configurationManager.UpdateConfiguration(appConfiguration);
            Configuration = appConfiguration;
        }
    }

}
