using BiblioTrack.Data;
using BiblioTrack.Models;
using BiblioTrack.Models.Dto;
using BiblioTrack.Services;
using BiblioTrack.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BiblioTrack.Services
{
    public class UserActivityService : IUserActivityService
    {
        private readonly ApplicationDbContext _db;

        public UserActivityService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<UserActivityModel>> GetUsersActivityAsync()
        {
            var borrowings = await _db.Borrowings
                .Where(b =>  b.Status != SD.Borrowing_Status_Returned)
                .Include(b => b.Copy).Include(b => b.User)
                .ToListAsync();

            if (!borrowings.Any())
                return new List<UserActivityModel>();

         
            var usersWithBorrowings = new HashSet<string>(borrowings.Select(b => b.UserId).ToList());

            var borrowingsByUser = borrowings.GroupBy(b => b.UserId).ToDictionary(g => g.Key, g => g.ToList());

            // Build a view model per user
            var result = new List<UserActivityModel>(); 

            foreach( string id  in usersWithBorrowings)
            {
                borrowingsByUser.TryGetValue(id, out var userBorrowings);
                userBorrowings ??= new List<Borrowings>();

                var groupedByStatus = userBorrowings
                    .GroupBy(b => b.Status)
                    .ToDictionary(g => g.Key, g => g.ToList());

                result.Add(new UserActivityModel
                {
                    UserId = id,
                    UserName = userBorrowings.First().User?.UserName ?? "",
                    BorrowedBooks = MapCopies(groupedByStatus, "Borrowed"),
                    ReservedBooks = MapCopies(groupedByStatus, "Reserved"),
                    FavoriteBooks = new List<BookCopy>()
                });
            }
            
            return result;
        }
        private List<BookCopy> MapCopies(Dictionary<string, List<Borrowings>> borrowingsByStatus,string status)
        {
            if (!borrowingsByStatus.TryGetValue(status, out var borrowings))
                return new List<BookCopy>();

            return borrowings
                .Select(b => new BookCopy
                {
                    BookId = b.Copy!.BookId,
                    CopyId = b.Copy.CopyId,
                    Status = b.Copy.Status,
                }).ToList();
        }

    }

}




