using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pokazuha.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPostadEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Postads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Condition = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ShowEmailToPublic = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    ViewCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Postads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Postads_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PostadImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PostadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    FileName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostadImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostadImages_Postads_PostadId",
                        column: x => x.PostadId,
                        principalTable: "Postads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostadImages_PostadId",
                table: "PostadImages",
                column: "PostadId");

            migrationBuilder.CreateIndex(
                name: "IX_PostadImages_PostadId_IsPrimary",
                table: "PostadImages",
                columns: new[] { "PostadId", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_Postads_Category",
                table: "Postads",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Postads_CreatedAt",
                table: "Postads",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Postads_Location",
                table: "Postads",
                column: "Location");

            migrationBuilder.CreateIndex(
                name: "IX_Postads_Price",
                table: "Postads",
                column: "Price");

            migrationBuilder.CreateIndex(
                name: "IX_Postads_Status",
                table: "Postads",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Postads_UserId",
                table: "Postads",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostadImages");

            migrationBuilder.DropTable(
                name: "Postads");
        }
    }
}
