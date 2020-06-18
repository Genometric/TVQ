using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Genometric.TVQ.API.Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    URI = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Keywords",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    Label = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Keywords", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "LiteratureCrawlingJobs",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    ScanAllPublications = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiteratureCrawlingJobs", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Repositories",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    Name = table.Column<int>(nullable: true),
                    URI = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repositories", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    Name = table.Column<int>(nullable: false),
                    MaxDegreeOfParallelism = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Tools",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Homepage = table.Column<string>(nullable: true),
                    CodeRepo = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tools", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Publications",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
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
                    Volume = table.Column<string>(nullable: true),
                    Number = table.Column<int>(nullable: true),
                    Chapter = table.Column<string>(nullable: true),
                    Pages = table.Column<string>(nullable: true),
                    Publisher = table.Column<string>(nullable: true),
                    LiteratureCrawlingJobID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publications", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Publications_LiteratureCrawlingJobs_LiteratureCrawlingJobID",
                        column: x => x.LiteratureCrawlingJobID,
                        principalTable: "LiteratureCrawlingJobs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisJobs",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    RepositoryID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisJobs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AnalysisJobs_Repositories_RepositoryID",
                        column: x => x.RepositoryID,
                        principalTable: "Repositories",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CategoryRepoAssociations",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    IDinRepo = table.Column<string>(nullable: true),
                    CategoryID = table.Column<int>(nullable: false),
                    RepositoryID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryRepoAssociations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CategoryRepoAssociations_Categories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "Categories",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryRepoAssociations_Repositories_RepositoryID",
                        column: x => x.RepositoryID,
                        principalTable: "Repositories",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RepoCrawlingJobs",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    RepositoryID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepoCrawlingJobs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RepoCrawlingJobs_Repositories_RepositoryID",
                        column: x => x.RepositoryID,
                        principalTable: "Repositories",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Statistics",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    RepositoryID = table.Column<int>(nullable: false),
                    TScore = table.Column<double>(nullable: true),
                    PValue = table.Column<double>(nullable: true),
                    DegreeOfFreedom = table.Column<double>(nullable: true),
                    CriticalValue = table.Column<double>(nullable: true),
                    MeansSignificantlyDifferent = table.Column<bool>(nullable: true)
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
                name: "ToolCategoryAssociations",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    ToolID = table.Column<int>(nullable: false),
                    CategoryID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolCategoryAssociations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ToolCategoryAssociations_Categories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "Categories",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ToolCategoryAssociations_Tools_ToolID",
                        column: x => x.ToolID,
                        principalTable: "Tools",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ToolRepoAssociations",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    IDinRepo = table.Column<string>(nullable: true),
                    ToolID = table.Column<int>(nullable: false),
                    RepositoryID = table.Column<int>(nullable: false),
                    Owner = table.Column<string>(nullable: true),
                    UserID = table.Column<string>(nullable: true),
                    TimesDownloaded = table.Column<int>(nullable: true),
                    DateAddedToRepository = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolRepoAssociations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ToolRepoAssociations_Repositories_RepositoryID",
                        column: x => x.RepositoryID,
                        principalTable: "Repositories",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ToolRepoAssociations_Tools_ToolID",
                        column: x => x.ToolID,
                        principalTable: "Tools",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthorsPublicationAssociations",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    AuthorID = table.Column<int>(nullable: false),
                    PublicationID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorsPublicationAssociations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AuthorsPublicationAssociations_Authors_AuthorID",
                        column: x => x.AuthorID,
                        principalTable: "Authors",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthorsPublicationAssociations_Publications_PublicationID",
                        column: x => x.PublicationID,
                        principalTable: "Publications",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Citations",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    PublicationID = table.Column<int>(nullable: false),
                    Count = table.Column<int>(nullable: false),
                    AccumulatedCount = table.Column<int>(nullable: false),
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
                name: "PublicationKeywordAssociations",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    PublicationID = table.Column<int>(nullable: false),
                    KeywordID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicationKeywordAssociations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PublicationKeywordAssociations_Keywords_KeywordID",
                        column: x => x.KeywordID,
                        principalTable: "Keywords",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PublicationKeywordAssociations_Publications_PublicationID",
                        column: x => x.PublicationID,
                        principalTable: "Publications",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ToolPublicationAssociations",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    ToolID = table.Column<int>(nullable: false),
                    PublicationID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolPublicationAssociations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ToolPublicationAssociations_Publications_PublicationID",
                        column: x => x.PublicationID,
                        principalTable: "Publications",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ToolPublicationAssociations_Tools_ToolID",
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
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    ToolID = table.Column<int>(nullable: false),
                    Count = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    ToolRepoAssociationID = table.Column<int>(nullable: true)
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
                    table.ForeignKey(
                        name: "FK_ToolDownloadRecords_ToolRepoAssociations_ToolRepoAssociationID",
                        column: x => x.ToolRepoAssociationID,
                        principalTable: "ToolRepoAssociations",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisJobs_RepositoryID",
                table: "AnalysisJobs",
                column: "RepositoryID");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorsPublicationAssociations_AuthorID",
                table: "AuthorsPublicationAssociations",
                column: "AuthorID");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorsPublicationAssociations_PublicationID",
                table: "AuthorsPublicationAssociations",
                column: "PublicationID");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryRepoAssociations_CategoryID",
                table: "CategoryRepoAssociations",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryRepoAssociations_RepositoryID",
                table: "CategoryRepoAssociations",
                column: "RepositoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Citations_PublicationID",
                table: "Citations",
                column: "PublicationID");

            migrationBuilder.CreateIndex(
                name: "IX_PublicationKeywordAssociations_KeywordID",
                table: "PublicationKeywordAssociations",
                column: "KeywordID");

            migrationBuilder.CreateIndex(
                name: "IX_PublicationKeywordAssociations_PublicationID",
                table: "PublicationKeywordAssociations",
                column: "PublicationID");

            migrationBuilder.CreateIndex(
                name: "IX_Publications_LiteratureCrawlingJobID",
                table: "Publications",
                column: "LiteratureCrawlingJobID");

            migrationBuilder.CreateIndex(
                name: "IX_RepoCrawlingJobs_RepositoryID",
                table: "RepoCrawlingJobs",
                column: "RepositoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Services_Name",
                table: "Services",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Statistics_RepositoryID",
                table: "Statistics",
                column: "RepositoryID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ToolCategoryAssociations_CategoryID",
                table: "ToolCategoryAssociations",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_ToolCategoryAssociations_ToolID",
                table: "ToolCategoryAssociations",
                column: "ToolID");

            migrationBuilder.CreateIndex(
                name: "IX_ToolDownloadRecords_ToolID",
                table: "ToolDownloadRecords",
                column: "ToolID");

            migrationBuilder.CreateIndex(
                name: "IX_ToolDownloadRecords_ToolRepoAssociationID",
                table: "ToolDownloadRecords",
                column: "ToolRepoAssociationID");

            migrationBuilder.CreateIndex(
                name: "IX_ToolPublicationAssociations_PublicationID",
                table: "ToolPublicationAssociations",
                column: "PublicationID");

            migrationBuilder.CreateIndex(
                name: "IX_ToolPublicationAssociations_ToolID",
                table: "ToolPublicationAssociations",
                column: "ToolID");

            migrationBuilder.CreateIndex(
                name: "IX_ToolRepoAssociations_RepositoryID",
                table: "ToolRepoAssociations",
                column: "RepositoryID");

            migrationBuilder.CreateIndex(
                name: "IX_ToolRepoAssociations_ToolID",
                table: "ToolRepoAssociations",
                column: "ToolID");

            migrationBuilder.CreateIndex(
                name: "IX_Tools_Name",
                table: "Tools",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalysisJobs");

            migrationBuilder.DropTable(
                name: "AuthorsPublicationAssociations");

            migrationBuilder.DropTable(
                name: "CategoryRepoAssociations");

            migrationBuilder.DropTable(
                name: "Citations");

            migrationBuilder.DropTable(
                name: "PublicationKeywordAssociations");

            migrationBuilder.DropTable(
                name: "RepoCrawlingJobs");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "Statistics");

            migrationBuilder.DropTable(
                name: "ToolCategoryAssociations");

            migrationBuilder.DropTable(
                name: "ToolDownloadRecords");

            migrationBuilder.DropTable(
                name: "ToolPublicationAssociations");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "Keywords");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "ToolRepoAssociations");

            migrationBuilder.DropTable(
                name: "Publications");

            migrationBuilder.DropTable(
                name: "Repositories");

            migrationBuilder.DropTable(
                name: "Tools");

            migrationBuilder.DropTable(
                name: "LiteratureCrawlingJobs");
        }
    }
}
