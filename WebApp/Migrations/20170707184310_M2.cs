using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace WebApp.Migrations
{
    public partial class M2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserReads_AspNetUsers_ApplicationUserId",
                table: "UserReads");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserReads",
                table: "UserReads");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "UserReads",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "UserReads",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserReads",
                table: "UserReads",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserReads_ApplicationUserId",
                table: "UserReads",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserReads_AspNetUsers_ApplicationUserId",
                table: "UserReads",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserReads_AspNetUsers_ApplicationUserId",
                table: "UserReads");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserReads",
                table: "UserReads");

            migrationBuilder.DropIndex(
                name: "IX_UserReads_ApplicationUserId",
                table: "UserReads");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserReads");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "UserReads",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserReads",
                table: "UserReads",
                columns: new[] { "ApplicationUserId", "ArticleId" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserReads_AspNetUsers_ApplicationUserId",
                table: "UserReads",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
