using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyBuddy.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class SystemBlock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemBlockReasons",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemBlockReasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemBlocks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    BlockedUserId = table.Column<string>(type: "text", nullable: false),
                    SystemBlockReasonId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemBlocks_SystemBlockReasons_SystemBlockReasonId",
                        column: x => x.SystemBlockReasonId,
                        principalTable: "SystemBlockReasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SystemBlocks_Users_BlockedUserId",
                        column: x => x.BlockedUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemBlocks_BlockedUserId",
                table: "SystemBlocks",
                column: "BlockedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemBlocks_SystemBlockReasonId",
                table: "SystemBlocks",
                column: "SystemBlockReasonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemBlocks");

            migrationBuilder.DropTable(
                name: "SystemBlockReasons");
        }
    }
}
