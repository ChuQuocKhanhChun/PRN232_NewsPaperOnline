using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PRN232_FinalProject.Identity;
using System;
using System.Collections.Generic;

namespace PRN232_FinalProject.Models
{
    public partial class Prn232FinalProjectContext : IdentityDbContext<ApplicationUser>
    {
        public Prn232FinalProjectContext() { }

        public Prn232FinalProjectContext(DbContextOptions<Prn232FinalProjectContext> options)
            : base(options) { }

        public virtual DbSet<Article> Articles { get; set; }
        public virtual DbSet<ArticleHistory> ArticleHistories { get; set; }
        public virtual DbSet<ArticleLike> ArticleLikes { get; set; }
        public virtual DbSet<Bookmark> Bookmarks { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build()
                    .GetConnectionString("MyCnn");

                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Identity

            modelBuilder.Entity<Article>(entity =>
            {
                entity.HasKey(e => e.ArticleId);
                entity.Property(e => e.ArticleId).HasColumnName("ArticleID");
                entity.Property(e => e.AuthorId).HasColumnName("AuthorID");
                entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
                entity.Property(e => e.ImageUrl).HasMaxLength(255);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
                entity.Property(e => e.PublishedDate).HasColumnType("datetime");
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Draft");
                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
                entity.Property(e => e.Views).HasDefaultValue(0);

                entity.HasOne(d => d.Author)
                    .WithMany()
                    .HasForeignKey(d => d.AuthorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Articles__Author__45F365D3");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Articles)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Articles__Catego__46E78A0C");

                entity.HasMany(d => d.Tags)
                    .WithMany(p => p.Articles)
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
                entity.HasKey(e => e.HistoryId);
                entity.ToTable("ArticleHistory");

                entity.Property(e => e.HistoryId).HasColumnName("HistoryID");
                entity.Property(e => e.ArticleId).HasColumnName("ArticleID");
                entity.Property(e => e.EditedBy).HasColumnName("EditedBy");
                entity.Property(e => e.EditedDate).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
                entity.Property(e => e.OldTitle).HasMaxLength(200);

                entity.HasOne(d => d.Article)
                    .WithMany(p => p.ArticleHistories)
                    .HasForeignKey(d => d.ArticleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ArticleHi__Artic__571DF1D5");

                entity.HasOne(d => d.EditedByNavigation)
                    .WithMany()
                    .HasForeignKey(d => d.EditedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ArticleHi__Edite__5812160E");
            });

            modelBuilder.Entity<ArticleLike>(entity =>
            {
                entity.HasKey(e => new { e.ArticleId, e.UserId });
                entity.Property(e => e.ArticleId).HasColumnName("ArticleID");
                entity.Property(e => e.UserId).HasColumnName("UserID");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");

                entity.HasOne(d => d.Article)
                    .WithMany(p => p.ArticleLikes)
                    .HasForeignKey(d => d.ArticleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ArticleLi__Artic__60A75C0F");

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ArticleLi__UserI__619B8048");
            });

            modelBuilder.Entity<Bookmark>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.ArticleId });
                entity.Property(e => e.UserId).HasColumnName("UserID");
                entity.Property(e => e.ArticleId).HasColumnName("ArticleID");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");

                entity.HasOne(d => d.Article)
                    .WithMany(p => p.Bookmarks)
                    .HasForeignKey(d => d.ArticleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Bookmarks__Artic__5CD6CB2B");

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Bookmarks__UserI__5BE2A6F2");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.CategoryId);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.CommentId);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()"); // Hoặc GETDATE()

                // Quan hệ với ApplicationUser (Author)
                entity.HasOne(d => d.Author)
                      .WithMany() // Hoặc WithMany(u => u.Comments) nếu bạn thêm ICollection<Comment> vào ApplicationUser
                      .HasForeignKey(d => d.AuthorId)
                      .OnDelete(DeleteBehavior.Restrict); // Ngăn chặn xóa tầng nếu người dùng có bình luận

                // Quan hệ với Article
                entity.HasOne(d => d.Article)
                      .WithMany(p => p.Comments)
                      .HasForeignKey(d => d.ArticleId)
                      .OnDelete(DeleteBehavior.Cascade); // Xóa bình luận nếu bài viết bị xóa

                // Quan hệ tự tham chiếu cho ParentComment
                entity.HasOne(d => d.ParentComment)
                      .WithMany(p => p.InverseParentComment)
                      .HasForeignKey(d => d.ParentCommentId)
                      .OnDelete(DeleteBehavior.Restrict); // Ngăn chặn xóa tầng nếu bình luận cha bị xóa
            });

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(e => e.TagId);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.TagId).HasColumnName("TagID");
                entity.Property(e => e.Name).HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
