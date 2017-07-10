using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace WebApp.Migrations
{
    public partial class M3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "SourceTags",
                columns: table => new
                {
                    SourceId = table.Column<int>(nullable: false),
                    TagId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceTags", x => new { x.SourceId, x.TagId });
                    table.ForeignKey(
                        name: "FK_SourceTags_Sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SourceTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SourceTags_TagId",
                table: "SourceTags",
                column: "TagId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserReads_AspNetUsers_ApplicationUserId",
                table: "UserReads",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserReads_AspNetUsers_ApplicationUserId",
                table: "UserReads");

            migrationBuilder.DropTable(
                name: "SourceTags");

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
    }
}
