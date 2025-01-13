using Microsoft.EntityFrameworkCore;
using WebEmailSendler.Context;
using WebEmailSendler.Models;

namespace WebEmailSendler.Managers
{
    public class MyConfigurationManager(MyDbContext context)
    {
        private readonly MyDbContext _context = context;

        public AppConfiguration GetConfigurationAsync()
        {
            return _context.AppConfigurations.Single();
        }
        public void UpdateConfiguration(AppConfiguration appConfiguration)
        {
            _context.AppConfigurations.Update(appConfiguration);
            _context.SaveChanges();
        }
    }
}
