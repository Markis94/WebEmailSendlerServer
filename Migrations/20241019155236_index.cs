using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebEmailSendler.Migrations
{
    /// <inheritdoc />
    public partial class index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EmailSendResults_Email",
                table: "EmailSendResults",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSendResults_IsSuccess",
                table: "EmailSendResults",
                column: "IsSuccess");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailSendResults_Email",
                table: "EmailSendResults");

            migrationBuilder.DropIndex(
                name: "IX_EmailSendResults_IsSuccess",
                table: "EmailSendResults");
        }
    }
}
