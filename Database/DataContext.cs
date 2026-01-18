using Microsoft.EntityFrameworkCore;
using LibraryCoreApi.Entities;


namespace LibraryCoreApi.Database;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Party> Parties { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<PartyRole> PartyRoles { get; set; } = null!;
    public DbSet<Book> Books { get; set; } = null!;
    public DbSet<BookCopy> BookCopies { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Reservation> Reservations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // PartyRole - Many-to-Many relationship
        modelBuilder.Entity<PartyRole>()
            .HasKey(pr => pr.Id);

        modelBuilder.Entity<PartyRole>()
            .HasOne(pr => pr.Party)
            .WithMany(p => p.PartyRoles)
            .HasForeignKey(pr => pr.PartyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PartyRole>()
            .HasOne(pr => pr.Role)
            .WithMany(r => r.PartyRoles)
            .HasForeignKey(pr => pr.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ensure unique Party-Role combination
        modelBuilder.Entity<PartyRole>()
            .HasIndex(pr => new { pr.PartyId, pr.RoleId })
            .IsUnique();

        // Book relationships
        modelBuilder.Entity<Book>()
            .HasOne(b => b.Author)
            .WithMany(p => p.AuthoredBooks)
            .HasForeignKey(b => b.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Book>()
            .HasOne(b => b.Category)
            .WithMany(c => c.Books)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // BookCopy relationships
        modelBuilder.Entity<BookCopy>()
            .HasOne(bc => bc.Book)
            .WithMany(b => b.Copies)
            .HasForeignKey(bc => bc.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        // BookCopy to Reservation relationship (one-to-one, optional)
        modelBuilder.Entity<BookCopy>()
            .HasOne(bc => bc.CurrentReservation)
            .WithOne(r => r.BookCopy)
            .HasForeignKey<Reservation>(r => r.BookCopyId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Ensure unique CopyNumber per book
        modelBuilder.Entity<BookCopy>()
            .HasIndex(bc => new { bc.BookId, bc.CopyNumber })
            .IsUnique();

        // Reservation relationships
        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Customer)
            .WithMany(p => p.Reservations)
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for performance
        modelBuilder.Entity<Book>()
            .HasIndex(b => b.Title);
        
        modelBuilder.Entity<Book>()
            .HasIndex(b => b.ISBN)
            .IsUnique();

        modelBuilder.Entity<BookCopy>()
            .HasIndex(bc => bc.IsAvailable);

        modelBuilder.Entity<Reservation>()
            .HasIndex(r => r.Status);

        // Ensure unique role name
        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique();
    }
}