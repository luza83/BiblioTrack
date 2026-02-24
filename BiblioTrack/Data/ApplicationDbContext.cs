using BiblioTrack.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BiblioTrack.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) 
        {
        }

        public DbSet<Book> Book { get; set; }
        public DbSet<BookCopy> BookCopy { get; set; }
        public DbSet<Borrowings> Borrowings { get; set; }
        public DbSet<UserFavoriteBookModel> UserFavoriteBook { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserFavoriteBookModel>()
                .HasIndex(f => new { f.UserId, f.BookId })
                .IsUnique();
        }
    }
}
