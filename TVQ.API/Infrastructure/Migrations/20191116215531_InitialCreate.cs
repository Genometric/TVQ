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
                    Status = table.Column<int>(nullable: false),
                    Name = table.Column<int>(nullable: true),
                    URI = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repositories", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Statistics",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(nullable: false),
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
                    Type = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Year = table.Column<int>(nullable: true),
                    Month = table.Column<int>(nullable: true),
                    Day = table.Column<int>(nullable: true),
                    CitedBy = table.Column<int>(nullable: true),
                    DOI = table.Column<string>(nullable: true),
                    BibTeXEntry = table.Column<string>(nullable: true),
                    Journal = table.Column<string>(nullable: true),
                    Volume = table.Column<int>(nullable: true),
                    Number = table.Column<int>(nullable: true),
                    Chapter = table.Column<string>(nullable: true),
                    Pages = table.Column<string>(nullable: true),
                    Publisher = table.Column<string>(nullable: true)
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
                name: "Authors",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    PublicationID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Authors_Publications_PublicationID",
                        column: x => x.PublicationID,
                        principalTable: "Publications",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.CreateTable(
                name: "Keywords",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublicationID = table.Column<int>(nullable: false),
                    Label = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Keywords", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Keywords_Publications_PublicationID",
                        column: x => x.PublicationID,
                        principalTable: "Publications",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthorsPublications",
                columns: table => new
                {
                    AuthorID = table.Column<int>(nullable: false),
                    PublicationID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorsPublications", x => new { x.PublicationID, x.AuthorID });
                    table.ForeignKey(
                        name: "FK_AuthorsPublications_Authors_AuthorID",
                        column: x => x.AuthorID,
                        principalTable: "Authors",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthorsPublications_Publications_PublicationID",
                        column: x => x.PublicationID,
                        principalTable: "Publications",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Authors_PublicationID",
                table: "Authors",
                column: "PublicationID");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorsPublications_AuthorID",
                table: "AuthorsPublications",
                column: "AuthorID");

            migrationBuilder.CreateIndex(
                name: "IX_Citations_PublicationID",
                table: "Citations",
                column: "PublicationID");

            migrationBuilder.CreateIndex(
                name: "IX_Keywords_PublicationID",
                table: "Keywords",
                column: "PublicationID");

            migrationBuilder.CreateIndex(
                name: "IX_Publications_ToolID",
                table: "Publications",
                column: "ToolID");

            migrationBuilder.CreateIndex(
                name: "IX_Statistics_RepositoryID",
                table: "Statistics",
                column: "RepositoryID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ToolDownloadRecords_ToolID",
                table: "ToolDownloadRecords",
                column: "ToolID");

            migrationBuilder.CreateIndex(
                name: "IX_Tools_Name",
                table: "Tools",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tools_RepositoryID",
                table: "Tools",
                column: "RepositoryID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorsPublications");

            migrationBuilder.DropTable(
                name: "Citations");

            migrationBuilder.DropTable(
                name: "Keywords");

            migrationBuilder.DropTable(
                name: "Statistics");

            migrationBuilder.DropTable(
                name: "ToolDownloadRecords");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "Publications");

            migrationBuilder.DropTable(
                name: "Tools");

            migrationBuilder.DropTable(
                name: "Repositories");
        }
    }
}
