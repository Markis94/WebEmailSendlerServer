using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebEmailSendler.Migrations
{
    /// <inheritdoc />
    public partial class configuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppConfigurations",
                columns: table => new
                {
                    ThreadCount = table.Column<int>(type: "integer", nullable: false),
                    ThreadSleep = table.Column<int>(type: "integer", nullable: false),
                    EmailPackSize = table.Column<int>(type: "integer", nullable: false),
                    Server = table.Column<string>(type: "text", nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    EnableSsl = table.Column<bool>(type: "boolean", nullable: false),
                    Login = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    HostEmailAddress = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                });

            // Добавляем начальные значения
            migrationBuilder.InsertData(
                table: "AppConfigurations",
                columns: new[] { "ThreadCount", "ThreadSleep", "EmailPackSize", "Server", "Port", "EnableSsl", "Login", "Password", "HostEmailAddress", "DisplayName" },
                values: new object[] { 5, 0, 50, "smtp.example.com", 587, true, "your-login@example.com", "your-password", "host-email@example.com", "My App Display Name" }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppConfigurations");
        }
    }
}
