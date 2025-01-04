using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebEmailSendler.Migrations
{
    /// <inheritdoc />
    public partial class EmailSendDataRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailSendResults_EmailSendTask_EmailSendTaskId",
                table: "EmailSendResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmailSendResults",
                table: "EmailSendResults");

            migrationBuilder.RenameTable(
                name: "EmailSendResults",
                newName: "EmailSendData");

            migrationBuilder.RenameIndex(
                name: "IX_EmailSendResults_IsSuccess",
                table: "EmailSendData",
                newName: "IX_EmailSendData_IsSuccess");

            migrationBuilder.RenameIndex(
                name: "IX_EmailSendResults_EmailSendTaskId",
                table: "EmailSendData",
                newName: "IX_EmailSendData_EmailSendTaskId");

            migrationBuilder.RenameIndex(
                name: "IX_EmailSendResults_Email",
                table: "EmailSendData",
                newName: "IX_EmailSendData_Email");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmailSendData",
                table: "EmailSendData",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailSendData_EmailSendTask_EmailSendTaskId",
                table: "EmailSendData",
                column: "EmailSendTaskId",
                principalTable: "EmailSendTask",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailSendData_EmailSendTask_EmailSendTaskId",
                table: "EmailSendData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmailSendData",
                table: "EmailSendData");

            migrationBuilder.RenameTable(
                name: "EmailSendData",
                newName: "EmailSendResults");

            migrationBuilder.RenameIndex(
                name: "IX_EmailSendData_IsSuccess",
                table: "EmailSendResults",
                newName: "IX_EmailSendResults_IsSuccess");

            migrationBuilder.RenameIndex(
                name: "IX_EmailSendData_EmailSendTaskId",
                table: "EmailSendResults",
                newName: "IX_EmailSendResults_EmailSendTaskId");

            migrationBuilder.RenameIndex(
                name: "IX_EmailSendData_Email",
                table: "EmailSendResults",
                newName: "IX_EmailSendResults_Email");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmailSendResults",
                table: "EmailSendResults",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailSendResults_EmailSendTask_EmailSendTaskId",
                table: "EmailSendResults",
                column: "EmailSendTaskId",
                principalTable: "EmailSendTask",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
