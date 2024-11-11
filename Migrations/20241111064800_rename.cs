using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebEmailSendler.Migrations
{
    /// <inheritdoc />
    public partial class rename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SentDate",
                table: "EmailSendResults",
                newName: "SendDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SendDate",
                table: "EmailSendResults",
                newName: "SentDate");
        }
    }
}
