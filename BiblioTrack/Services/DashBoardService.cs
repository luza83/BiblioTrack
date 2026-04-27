using BiblioTrack.Data;
using BiblioTrack.Models;
using BiblioTrack.Models.Dto;
using BiblioTrack.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics.Metrics;
using System.Linq;

namespace BiblioTrack.Services
{
    public class DashBoardService : IDashboardService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMemoryCache _cache;

        public DashBoardService(ApplicationDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
      
        }
        public async Task<DashboardResponseDto> GetDashboardData()
        {
            try
            {
                return await _cache.GetOrCreateAsync("dashboard", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                    var lastMonth = DateTime.UtcNow.AddMonths(-1);
                    var availableCopies = await _db.BookCopy
                        .AsNoTracking()
                        .Where(c => c.Status == SD.Book_Copy_Status_Available)
                        .GroupBy(c => c.BookId)
                        .Select(g => new { BookId = g.Key, Count = g.Count() })
                        .ToDictionaryAsync(x => x.BookId, x => x.Count);

                    var totalBooks = await _db.Book
                        .AsNoTracking()
                        .CountAsync();

                    var totalFavoriteBooks = await _db.UserFavoriteBook
                        .AsNoTracking()
                        .Select(b => b.BookId)
                        .Distinct()
                        .CountAsync();

                    var activeBorrowings = await _db.Borrowings
                        .AsNoTracking()
                        .Where(b => b.Status == SD.Borrowing_Status_Borrowed ||
                                    b.Status == SD.Borrowing_Status_Reserved)
                        .GroupBy(b => b.Status)
                        .Select(g => new { Status = g.Key, Count = g.Count() })
                        .ToListAsync();

                    var totalBorrowedBooks = activeBorrowings
                        .FirstOrDefault(x => x.Status == SD.Borrowing_Status_Borrowed)?.Count ?? 0;

                    var totalReservedBooks = activeBorrowings
                        .FirstOrDefault(x => x.Status == SD.Borrowing_Status_Reserved)?.Count ?? 0;



                    var bookOfTheDay = await _db.Book
                        .AsNoTracking()
                        .OrderBy(b => b.BookId).Skip(DateTime.UtcNow.DayOfYear)
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
                            RatingsCount = book.RatingsCount
                        })
                        .FirstOrDefaultAsync();

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
                        BookOfTheDay = bookOfTheDay!
                    };
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Dashboard loading failed: {ex.Message}", ex);
            }
        }
    }
}
