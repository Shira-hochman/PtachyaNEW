using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Dal.Models;

public partial class PtachiyaContext : DbContext
{
    public PtachiyaContext()
    {
    }

    public PtachiyaContext(DbContextOptions<PtachiyaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Child> Children { get; set; }
    public virtual DbSet<Customer> Customers { get; set; }
    public virtual DbSet<Form> Forms { get; set; }
    public virtual DbSet<Kindergarten> Kindergartens { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<Payment> Payments { get; set; }
    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Child>(entity =>
        {
            entity.HasKey(e => e.ChildId).HasName("PK__Children__BEFA0716585D1C92");

            entity.HasIndex(e => e.IdNumber, "UQ__Children__62DF80332B6EC341").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(100);
            // 🛑 הוסר: entity.Property(e => e.FormLink).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IdNumber).HasMaxLength(20);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.SchoolYear).HasMaxLength(20);

            entity.HasOne(d => d.Kindergarten).WithMany(p => p.Children)
                .HasForeignKey(d => d.KindergartenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Children__Kinder__52593CB8");

            entity.HasOne(d => d.Payment).WithMany(p => p.Children)
                .HasForeignKey(d => d.PaymentId)
                .HasConstraintName("FK_Children_Payments");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("customers");

            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.FirstName)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("last_name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone");
        });

        modelBuilder.Entity<Form>(entity =>
        {
            entity.HasKey(e => e.FormId).HasName("PK__Forms__FB05B7DD3B2B1448");

            // 🛑 הוסר: entity.Property(e => e.FormLink).HasMaxLength(255);

            // ⭐️ מיפוי לשדות החדשים:
            entity.Property(e => e.FileContent)
                .HasColumnType("varbinary(max)"); // מאפשר אחסון קבצים גדולים
            entity.Property(e => e.ContentType)
                .HasMaxLength(255)
                .IsRequired(true);

            entity.Property(e => e.SubmittedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Child).WithMany(p => p.Forms)
                .HasForeignKey(d => d.ChildId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Forms__ChildId__59063A47");
        });

        modelBuilder.Entity<Kindergarten>(entity =>
        {
            entity.HasKey(e => e.KindergartenId).HasName("PK__Kinderga__2B06D7BC1EB3F444");

            entity.HasIndex(e => e.Code, "UQ__Kinderga__A25C5AA7B71BED89").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("orders");

            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.OrderDate)
                .HasColumnType("datetime")
                .HasColumnName("order_date");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A389869C495");

            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("pending");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C0814D012");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4E7221715").IsUnique();

            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}