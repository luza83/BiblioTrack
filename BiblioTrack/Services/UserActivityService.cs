using Azure.Core;
using BiblioTrack.Data;
using BiblioTrack.Models;
using BiblioTrack.Models.Dto;
using BiblioTrack.Services;
using BiblioTrack.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Reflection.Metadata.BlobBuilder;

namespace BiblioTrack.Services
{
    public class UserActivityService : IUserActivityService
    {
        private readonly ApplicationDbContext _db;

        public UserActivityService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResponse<UserActivityDTO>> GetUsersActivityAsync(GetUserActivityRequest getUserActivityRequest)
        {
            try
            {
                var usersQuery = _db.Users.AsQueryable();

                // Filtering
                if (!string.IsNullOrWhiteSpace(getUserActivityRequest.UserName))
                {
                    usersQuery = usersQuery.Where(u => u.UserName.Contains(getUserActivityRequest.UserName));
                }

                if (!string.IsNullOrWhiteSpace(getUserActivityRequest.Email))
                {
                    usersQuery = usersQuery.Where(u => u.Email.Contains(getUserActivityRequest.Email));
                }
                var totalRecords = await usersQuery.CountAsync();
                // Pagination
                var users = await usersQuery
                    .Skip((getUserActivityRequest.PageNumber - 1) * getUserActivityRequest.PageSize)
                    .Take(getUserActivityRequest.PageSize)
                    .ToListAsync();

                if (users.Count() == 0 )
                    return new PagedResponse<UserActivityDTO>
                    {
                        PageNumber = getUserActivityRequest.PageNumber,
                        PageSize = getUserActivityRequest.PageSize,
                        TotalRecords = 0,
                        Data = new List<UserActivityDTO>()
                    };

                var userIds = users.Select(u => u.Id).ToList();

                var borrowings = await _db.Borrowings
                    .Where(b => userIds.Contains(b.UserId) && b.Status != SD.Borrowing_Status_Returned)
                    .Include(b => b.Copy).ThenInclude(c => c.Book)
                    .Include(b => b.User)
                    .ToListAsync();
                var borrowingsByUser = borrowings
                    .GroupBy(b => b.UserId)
                    .ToDictionary(g => g.Key, g => g.ToList());


                var favorites = await _db.UserFavoriteBook.
                    Where(f => userIds.Contains(f.UserId)).Include(b => b.Book).ToListAsync();

                var favoritesByUser = favorites
                   .GroupBy(f => f.UserId)
                   .ToDictionary(b => b.Key, b => b.ToList());

                var result = new List<UserActivityDTO>();

                foreach (var user in users)
                {
                    borrowingsByUser.TryGetValue(user.Id, out var userBorrowings);
                    userBorrowings ??= new List<Borrowings>();
                    favoritesByUser.TryGetValue(user.Id, out var userFavorites);
                    var fb = new List<Book>();
                    if (userFavorites != null && userFavorites.Count > 0)
                    {
                        fb = [.. userFavorites.Select(f => f.Book)];
                    }
                    var favoriteBooks = new List<UserFavoriteDto>();
                    if (fb != null && fb.Count > 0)
                    {
                        var fb_copies = await _db.BookCopy.Where(c => fb.Select(f => f.BookId).Contains(c.BookId)).ToListAsync();

                        favoriteBooks = userFavorites?.Select(f => new UserFavoriteDto
                        {
                            Id = f.Id,
                            BookId = f.BookId,
                            Book = f.Book,
                            IsBorrowable = fb_copies.Any(c => c.BookId == f.BookId && c.Status == SD.Book_Copy_Status_Available)
                        }).ToList();
                    }

                    var groupedByStatus = userBorrowings
                        .GroupBy(b => b.Status)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    result.Add(new UserActivityDTO
                    {
                        UserId = user.Id,
                        UserName = user.UserName ?? "",
                        BorrowedBooks = MapCopies(groupedByStatus, "Borrowed"),
                        ReservedBooks = MapCopies(groupedByStatus, "Reserved"),
                        FavoriteBooks = favoriteBooks ?? new List<UserFavoriteDto>(),
                    });
                }

                var response = new PagedResponse<UserActivityDTO>
                {
                    PageNumber = getUserActivityRequest.PageNumber,
                    PageSize = getUserActivityRequest.PageSize,
                    TotalRecords = totalRecords,
                    Data = result
                };
                return response;

            }
            catch (Exception ex)
            {
                return new PagedResponse<UserActivityDTO>
                {
                    PageNumber = getUserActivityRequest.PageNumber,
                    PageSize = getUserActivityRequest.PageSize,
                    TotalRecords = 0,
                    Data = new List<UserActivityDTO>()
                };
            }
          
        }
        private List<BorrowingDTO> MapCopies(Dictionary<string, List<Borrowings>> borrowingsByStatus,string status)
        {
            if (!borrowingsByStatus.TryGetValue(status, out var borrowings))
                return new List<BorrowingDTO>();

            return borrowings
                .Select(b => new BorrowingDTO
                {
                    BorrowId = b.BorrowId,
                    Book = b.Copy.Book,
                    CopyId = b.Copy.CopyId,
                    Status = b.Status,
                }).ToList();
        }

    }

}




