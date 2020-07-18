﻿// <auto-generated />
using System;
using Genometric.TVQ.WebService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Genometric.TVQ.WebService.Infrastructure.Migrations
{
    [DbContext(typeof(TVQContext))]
    partial class TVQContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.AnalysisJob", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("RepositoryID")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("ID");

                    b.HasIndex("RepositoryID");

                    b.ToTable("AnalysisJobs");
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.Associations.AuthorPublicationAssociation", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AuthorID")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("PublicationID")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("ID");

                    b.HasIndex("AuthorID");

                    b.HasIndex("PublicationID");

                    b.ToTable("AuthorsPublicationAssociations");
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.Associations.CategoryRepoAssociation", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CategoryID")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("IDinRepo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RepositoryID")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("ID");

                    b.HasIndex("CategoryID");

                    b.HasIndex("RepositoryID");

                    b.ToTable("CategoryRepoAssociations");
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.Associations.PublicationKeywordAssociation", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("KeywordID")
                        .HasColumnType("int");

                    b.Property<int>("PublicationID")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("ID");

                    b.HasIndex("KeywordID");

                    b.HasIndex("PublicationID");

                    b.ToTable("PublicationKeywordAssociations");
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.Associations.ToolCategoryAssociation", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CategoryID")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("ToolID")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("ID");

                    b.HasIndex("CategoryID");

                    b.HasIndex("ToolID");

                    b.ToTable("ToolCategoryAssociations");
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.Associations.ToolPublicationAssociation", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("PublicationID")
                        .HasColumnType("int");

                    b.Property<int>("ToolID")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("ID");

                    b.HasIndex("PublicationID");

                    b.HasIndex("ToolID");

                    b.ToTable("ToolPublicationAssociations");
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.Associations.ToolRepoAssociation", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateAddedToRepository")
                        .HasColumnType("datetime2");

                    b.Property<string>("IDinRepo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Owner")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RepositoryID")
                        .HasColumnType("int");

                    b.Property<int?>("TimesDownloaded")
                        .HasColumnType("int");

                    b.Property<int>("ToolID")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserID")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.HasIndex("RepositoryID");

                    b.HasIndex("ToolID");

                    b.ToTable("ToolRepoAssociations");
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.Author", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("ID");

                    b.ToTable("Authors");
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.Category", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("URI")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("ID");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.Citation", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AccumulatedCount")
                        .HasColumnType("int");

                    b.Property<int>("Count")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<int>("PublicationID")
                        .HasColumnType("int");

                    b.Property<int?>("Source")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("ID");

                    b.HasIndex("PublicationID");

                    b.ToTable("Citations");
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.Keyword", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Label")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("ID");

                    b.ToTable("Keywords");
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.LiteratureCrawlingJob", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("ScanAllPublications")
                        .HasColumnType("bit");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("ID");

                    b.ToTable("LiteratureCrawlingJobs");
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.Publication", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("BibTeXEntry")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Chapter")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("CitedBy")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("DOI")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Day")
                        .HasColumnType("int");

                    b.Property<string>("EID")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Journal")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("LiteratureCrawlingJobID")
                        .HasColumnType("int");

                    b.Property<int?>("Month")
                        .HasColumnType("int");

                    b.Property<int?>("Number")
                        .HasColumnType("int");

                    b.Property<string>("Pages")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PubMedID")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Publisher")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ScopusID")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Volume")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Year")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("LiteratureCrawlingJobID");

                    b.ToTable("Publications");
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.RepoCrawlingJob", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RepositoryID")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("ID");

                    b.HasIndex("RepositoryID");

                    b.ToTable("RepoCrawlingJobs");
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.Repository", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("Name")
                        .HasColumnType("int");

                    b.Property<string>("URI")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("ID");

                    b.ToTable("Repositories");
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.Service", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("MaxDegreeOfParallelism")
                        .HasColumnType("int");

                    b.Property<int>("Name")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("ID");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Services");
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.Statistics", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<double?>("CriticalValue")
                        .HasColumnType("float");

                    b.Property<double?>("DegreeOfFreedom")
                        .HasColumnType("float");

                    b.Property<bool?>("MeansSignificantlyDifferent")
                        .HasColumnType("bit");

                    b.Property<double?>("PValue")
                        .HasColumnType("float");

                    b.Property<int>("RepositoryID")
                        .HasColumnType("int");

                    b.Property<double?>("TScore")
                        .HasColumnType("float");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("ID");

                    b.HasIndex("RepositoryID")
                        .IsUnique();

                    b.ToTable("Statistics");
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.Tool", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CodeRepo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Homepage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("ID");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasFilter("[Name] IS NOT NULL");

                    b.ToTable("Tools");
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.ToolDownloadRecord", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Count")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<int>("ToolID")
                        .HasColumnType("int");

                    b.Property<int?>("ToolRepoAssociationID")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("ID");

                    b.HasIndex("ToolID");

                    b.HasIndex("ToolRepoAssociationID");

                    b.ToTable("ToolDownloadRecords");
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.AnalysisJob", b =>
                {
                    b.HasOne("Genometric.TVQ.WebService.Model.Repository", "Repository")
                        .WithMany()
                        .HasForeignKey("RepositoryID");
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.Associations.AuthorPublicationAssociation", b =>
                {
                    b.HasOne("Genometric.TVQ.WebService.Model.Author", "Author")
                        .WithMany("AuthorPublications")
                        .HasForeignKey("AuthorID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Genometric.TVQ.WebService.Model.Publication", "Publication")
                        .WithMany("AuthorAssociations")
                        .HasForeignKey("PublicationID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.Associations.CategoryRepoAssociation", b =>
                {
                    b.HasOne("Genometric.TVQ.WebService.Model.Category", "Category")
                        .WithMany("RepoAssociations")
                        .HasForeignKey("CategoryID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Genometric.TVQ.WebService.Model.Repository", "Repository")
                        .WithMany("CategoryAssociations")
                        .HasForeignKey("RepositoryID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.Associations.PublicationKeywordAssociation", b =>
                {
                    b.HasOne("Genometric.TVQ.WebService.Model.Keyword", "Keyword")
                        .WithMany("PublicationAssociations")
                        .HasForeignKey("KeywordID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Genometric.TVQ.WebService.Model.Publication", "Publication")
                        .WithMany("KeywordAssociations")
                        .HasForeignKey("PublicationID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.Associations.ToolCategoryAssociation", b =>
                {
                    b.HasOne("Genometric.TVQ.WebService.Model.Category", "Category")
                        .WithMany("ToolAssociations")
                        .HasForeignKey("CategoryID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Genometric.TVQ.WebService.Model.Tool", "Tool")
                        .WithMany("CategoryAssociations")
                        .HasForeignKey("ToolID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.Associations.ToolPublicationAssociation", b =>
                {
                    b.HasOne("Genometric.TVQ.WebService.Model.Publication", "Publication")
                        .WithMany("ToolAssociations")
                        .HasForeignKey("PublicationID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Genometric.TVQ.WebService.Model.Tool", "Tool")
                        .WithMany("PublicationAssociations")
                        .HasForeignKey("ToolID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.Associations.ToolRepoAssociation", b =>
                {
                    b.HasOne("Genometric.TVQ.WebService.Model.Repository", "Repository")
                        .WithMany("ToolAssociations")
                        .HasForeignKey("RepositoryID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Genometric.TVQ.WebService.Model.Tool", "Tool")
                        .WithMany("RepoAssociations")
                        .HasForeignKey("ToolID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.Citation", b =>
                {
                    b.HasOne("Genometric.TVQ.WebService.Model.Publication", "Publication")
                        .WithMany("Citations")
                        .HasForeignKey("PublicationID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.Publication", b =>
                {
                    b.HasOne("Genometric.TVQ.WebService.Model.LiteratureCrawlingJob", null)
                        .WithMany("Publications")
                        .HasForeignKey("LiteratureCrawlingJobID");
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.RepoCrawlingJob", b =>
                {
                    b.HasOne("Genometric.TVQ.WebService.Model.Repository", "Repository")
                        .WithMany()
                        .HasForeignKey("RepositoryID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.Statistics", b =>
                {
                    b.HasOne("Genometric.TVQ.WebService.Model.Repository", "Repository")
                        .WithOne("Statistics")
                        .HasForeignKey("Genometric.TVQ.WebService.Model.Statistics", "RepositoryID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Genometric.TVQ.WebService.Model.ToolDownloadRecord", b =>
                {
                    b.HasOne("Genometric.TVQ.WebService.Model.Tool", "Tool")
                        .WithMany()
                        .HasForeignKey("ToolID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Genometric.TVQ.WebService.Model.Associations.ToolRepoAssociation", null)
                        .WithMany("Downloads")
                        .HasForeignKey("ToolRepoAssociationID");
                });
#pragma warning restore 612, 618
        }
    }
}