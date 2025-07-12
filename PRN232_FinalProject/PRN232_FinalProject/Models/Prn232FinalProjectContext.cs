using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PRN232_FinalProject.Models;

public partial class Prn232FinalProjectContext : DbContext
{
    public Prn232FinalProjectContext()
    {
    }

    public Prn232FinalProjectContext(DbContextOptions<Prn232FinalProjectContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Article> Articles { get; set; }

    public virtual DbSet<ArticleHistory> ArticleHistories { get; set; }

    public virtual DbSet<ArticleLike> ArticleLikes { get; set; }

    public virtual DbSet<Bookmark> Bookmarks { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetConnectionString("MyCnn");
            optionsBuilder.UseSqlServer(connectionString);
        }

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(e => e.ArticleId).HasName("PK__Articles__9C6270C8E3555466");

            entity.Property(e => e.ArticleId).HasColumnName("ArticleID");
            entity.Property(e => e.AuthorId).HasColumnName("AuthorID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.PublishedDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Draft");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.Views).HasDefaultValue(0);

            entity.HasOne(d => d.Author).WithMany(p => p.Articles)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Articles__Author__45F365D3");

            entity.HasOne(d => d.Category).WithMany(p => p.Articles)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Articles__Catego__46E78A0C");

            entity.HasMany(d => d.Tags).WithMany(p => p.Articles)
                .UsingEntity<Dictionary<string, object>>(
                    "ArticleTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .HasConstraintName("FK__ArticleTa__TagID__4D94879B"),
                    l => l.HasOne<Article>().WithMany()
                        .HasForeignKey("ArticleId")
                        .HasConstraintName("FK__ArticleTa__Artic__4CA06362"),
                    j =>
                    {
                        j.HasKey("ArticleId", "TagId").HasName("PK__ArticleT__4A35BF6C6586C92B");
                        j.ToTable("ArticleTags");
                        j.IndexerProperty<int>("ArticleId").HasColumnName("ArticleID");
                        j.IndexerProperty<int>("TagId").HasColumnName("TagID");
                    });
        });

        modelBuilder.Entity<ArticleHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__ArticleH__4D7B4ADD618FEBBE");

            entity.ToTable("ArticleHistory");

            entity.Property(e => e.HistoryId).HasColumnName("HistoryID");
            entity.Property(e => e.ArticleId).HasColumnName("ArticleID");
            entity.Property(e => e.EditedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OldTitle).HasMaxLength(200);

            entity.HasOne(d => d.Article).WithMany(p => p.ArticleHistories)
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ArticleHi__Artic__571DF1D5");

            entity.HasOne(d => d.EditedByNavigation).WithMany(p => p.ArticleHistories)
                .HasForeignKey(d => d.EditedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ArticleHi__Edite__5812160E");
        });

        modelBuilder.Entity<ArticleLike>(entity =>
        {
            entity.HasKey(e => new { e.ArticleId, e.UserId }).HasName("PK__ArticleL__4D1AFC02F610E46C");

            entity.Property(e => e.ArticleId).HasColumnName("ArticleID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Article).WithMany(p => p.ArticleLikes)
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ArticleLi__Artic__60A75C0F");

            entity.HasOne(d => d.User).WithMany(p => p.ArticleLikes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ArticleLi__UserI__619B8048");
        });

        modelBuilder.Entity<Bookmark>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.ArticleId }).HasName("PK__Bookmark__8E4EEBA0F91DE894");

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.ArticleId).HasColumnName("ArticleID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Article).WithMany(p => p.Bookmarks)
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Bookmarks__Artic__5CD6CB2B");

            entity.HasOne(d => d.User).WithMany(p => p.Bookmarks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Bookmarks__UserI__5BE2A6F2");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A2BB55C5E76");

            entity.HasIndex(e => e.Name, "UQ__Categori__737584F659C0CED2").IsUnique();

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Comments__C3B4DFAAA95EB961");

            entity.Property(e => e.CommentId).HasColumnName("CommentID");
            entity.Property(e => e.ArticleId).HasColumnName("ArticleID");
            entity.Property(e => e.CommentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Content).HasMaxLength(1000);
            entity.Property(e => e.IsApproved).HasDefaultValue(false);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Article).WithMany(p => p.Comments)
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Comments__Articl__534D60F1");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Comments__UserID__52593CB8");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.TagId).HasName("PK__Tags__657CFA4C3C6A2509");

            entity.HasIndex(e => e.Name, "UQ__Tags__737584F6A8A47994").IsUnique();

            entity.Property(e => e.TagId).HasColumnName("TagID");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC7CD7B83D");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534AF94F0E1").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(256);
            entity.Property(e => e.Role).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
