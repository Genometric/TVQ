using Microsoft.EntityFrameworkCore.Migrations;

namespace Genometric.TVQ.API.Infrastructure.Migrations
{
    public partial class AddTTest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Statistics",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RepositoryID = table.Column<int>(nullable: false),
                    TValue = table.Column<double>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statistics", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Statistics_Repositories_RepositoryID",
                        column: x => x.RepositoryID,
                        principalTable: "Repositories",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Statistics_RepositoryID",
                table: "Statistics",
                column: "RepositoryID",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Statistics");
        }
    }
}
