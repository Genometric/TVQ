using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Genometric.TVQ.API.Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Repositories",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<int>(nullable: true),
                    URI = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repositories", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tools",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RepositoryID = table.Column<int>(nullable: false),
                    IDinRepo = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Homepage = table.Column<string>(nullable: true),
                    CodeRepo = table.Column<string>(nullable: true),
                    Owner = table.Column<string>(nullable: true),
                    UserID = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    TimesDownloaded = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tools", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tools_Repositories_RepositoryID",
                        column: x => x.RepositoryID,
                        principalTable: "Repositories",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Publications",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToolID = table.Column<int>(nullable: false),
                    PubMedID = table.Column<string>(nullable: true),
                    EID = table.Column<string>(nullable: true),
                    ScopusID = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Year = table.Column<string>(nullable: true),
                    CitedBy = table.Column<int>(nullable: false),
                    DOI = table.Column<string>(nullable: true),
                    BibTeXEntry = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publications", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Publications_Tools_ToolID",
                        column: x => x.ToolID,
                        principalTable: "Tools",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ToolDownloadRecords",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToolID = table.Column<int>(nullable: false),
                    Count = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolDownloadRecords", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ToolDownloadRecords_Tools_ToolID",
                        column: x => x.ToolID,
                        principalTable: "Tools",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Citations",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublicationID = table.Column<int>(nullable: false),
                    Count = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Source = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Citations_Publications_PublicationID",
                        column: x => x.PublicationID,
                        principalTable: "Publications",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Citations_PublicationID",
                table: "Citations",
                column: "PublicationID");

            migrationBuilder.CreateIndex(
                name: "IX_Publications_ToolID",
                table: "Publications",
                column: "ToolID");

            migrationBuilder.CreateIndex(
                name: "IX_ToolDownloadRecords_ToolID",
                table: "ToolDownloadRecords",
                column: "ToolID");

            migrationBuilder.CreateIndex(
                name: "IX_Tools_RepositoryID",
                table: "Tools",
                column: "RepositoryID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Citations");

            migrationBuilder.DropTable(
                name: "ToolDownloadRecords");

            migrationBuilder.DropTable(
                name: "Publications");

            migrationBuilder.DropTable(
                name: "Tools");

            migrationBuilder.DropTable(
                name: "Repositories");
        }
    }
}
