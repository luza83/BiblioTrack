using BiblioTrack.Data;
using BiblioTrack.Models;
using BiblioTrack.Models.Dto;
using BiblioTrack.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BiblioTrack.Services
{
    public class DashBoardService : IDashboardService
    {
        private readonly ApplicationDbContext _db;

        public DashBoardService(ApplicationDbContext db)
        {
            _db = db;
      
        }
        public async Task<DashboardResponseDto> GetDashboardData()
        {
            try
            {
                var lastMonth = DateTime.UtcNow.AddMonths(-1);
                var seed = DateTime.UtcNow.Date.DayOfYear;

                // Preload available copies per book
                var availableCopies = await _db.BookCopy
                    .AsNoTracking()
                    .Where(c => c.Status == SD.Book_Copy_Status_Available)
                    .GroupBy(c => c.BookId)
                    .Select(g => new { BookId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.BookId, x => x.Count);

                // Dashboard counts
                var totalBooks = await _db.Book
                    .AsNoTracking()
                    .CountAsync();

                var totalFavoriteBooks = await _db.UserFavoriteBook
                    .AsNoTracking()
                    .Select(b => b.BookId)
                    .Distinct()
                    .CountAsync();

                var totalBorrowedBooks = await _db.Borrowings
                    .AsNoTracking()
                    .CountAsync(b => b.Status == SD.Borrowing_Status_Borrowed);

                var totalReservedBooks = await _db.Borrowings
                    .AsNoTracking()
                    .CountAsync(b => b.Status == SD.Borrowing_Status_Reserved);

                // Trending books (IDs)
                var trendingBookIds = await _db.Borrowings
                    .AsNoTracking()
                    .Where(b => b.BorrowDate >= lastMonth)
                    .GroupBy(b => b.Copy.BookId)
                    .OrderByDescending(g => g.Count())
                    .Take(15)
                    .Select(g => g.Key)
                    .ToListAsync();

                var trendingBooks = await _db.Book
                    .AsNoTracking()
                    .Where(b => trendingBookIds.Contains(b.BookId))
                    .Select(book => new BorrowableBookDto
                    {
                        BookId = book.BookId,
                        Title = book.Title,
                        Author = book.Author,
                        ISBN = book.ISBN,
                        Publisher = book.Publisher,
                        Category = book.Category,
                        ImageUrl = book.ImageUrl,
                        Description = book.Description,
                        NumPages = book.NumPages,
                        AverageRating = book.AverageRating,
                        RatingsCount = book.RatingsCount,
                        TotalCopies = availableCopies.ContainsKey(book.BookId)
                            ? availableCopies[book.BookId]
                            : 0
                    })
                    .ToListAsync();

                // Book of the day
                var bookOfTheDayEntity = await _db.Book
                    .AsNoTracking()
                    .OrderBy(b => b.BookId * seed)
                    .FirstOrDefaultAsync();

                BorrowableBookDto bookOfTheDay = null;

                if (bookOfTheDayEntity != null)
                {
                    bookOfTheDay = new BorrowableBookDto
                    {
                        BookId = bookOfTheDayEntity.BookId,
                        Title = bookOfTheDayEntity.Title,
                        Author = bookOfTheDayEntity.Author,
                        ISBN = bookOfTheDayEntity.ISBN,
                        Publisher = bookOfTheDayEntity.Publisher,
                        Category = bookOfTheDayEntity.Category,
                        ImageUrl = bookOfTheDayEntity.ImageUrl,
                        Description = bookOfTheDayEntity.Description,
                        NumPages = bookOfTheDayEntity.NumPages,
                        AverageRating = bookOfTheDayEntity.AverageRating,
                        RatingsCount = bookOfTheDayEntity.RatingsCount,
                        TotalCopies = availableCopies.ContainsKey(bookOfTheDayEntity.BookId)
                            ? availableCopies[bookOfTheDayEntity.BookId]
                            : 0
                    };
                }

                // New books
                var newBooks = await _db.Book
                    .AsNoTracking()
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(15)
                    .Select(book => new BorrowableBookDto
                    {
                        BookId = book.BookId,
                        Title = book.Title,
                        Author = book.Author,
                        ISBN = book.ISBN,
                        Publisher = book.Publisher,
                        Category = book.Category,
                        ImageUrl = book.ImageUrl,
                        Description = book.Description,
                        NumPages = book.NumPages,
                        AverageRating = book.AverageRating,
                        RatingsCount = book.RatingsCount,
                        TotalCopies = availableCopies.ContainsKey(book.BookId)
                            ? availableCopies[book.BookId]
                            : 0
                    })
                    .ToListAsync();

                return new DashboardResponseDto
                {
                    BookCount = totalBooks,
                    FavoriteBookCount = totalFavoriteBooks,
                    BorrowedBookCount = totalBorrowedBooks,
                    ReservedBookCount = totalReservedBooks,
                    TrendingBooks = trendingBooks,
                    NewBooks = newBooks,
                    BookOfTheDay = bookOfTheDay
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Dashboard loading failed: {ex.Message}", ex);
            }
        }

    }
}
