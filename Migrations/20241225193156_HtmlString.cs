using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebEmailSendler.Migrations
{
    /// <inheritdoc />
    public partial class HtmlString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SampleJson",
                table: "Samles",
                newName: "JsonString");

            migrationBuilder.AddColumn<string>(
                name: "HtmlString",
                table: "Samles",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HtmlString",
                table: "Samles");

            migrationBuilder.RenameColumn(
                name: "JsonString",
                table: "Samles",
                newName: "SampleJson");
        }
    }
}
