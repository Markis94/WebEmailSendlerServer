using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebEmailSendler.Migrations
{
    /// <inheritdoc />
    public partial class EmailSendInfoExtend : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SendCount",
                table: "EmailSendTask",
                newName: "SuccessSendCount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SuccessSendCount",
                table: "EmailSendTask",
                newName: "SendCount");
        }
    }
}
