using Microsoft.EntityFrameworkCore;
using WebEmailSendler.Models;

namespace WebEmailSendler.Context
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }
        public DbSet<EmailSendTask> EmailSendTask { get; set; }
        public DbSet<EmailSendResult> EmailSendResults { get; set; }

    }
}
