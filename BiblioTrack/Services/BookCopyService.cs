using BiblioTrack.Data;
using BiblioTrack.Models;
using BiblioTrack.Models.Dto;
using BiblioTrack.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BiblioTrack.Services
{
    public class BookCopyService : IBookCopyService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookCopyService(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<(bool Success, string Message)> UpdateBookCopy(int copyId, string copyStatus, bool commitChanges)
        {
            var existingBookCopy = await _db.BookCopy
                .FirstOrDefaultAsync(u => u.CopyId == copyId);

            if (existingBookCopy == null)
                return (false, "Book Copy not found");

            try
            {
                var isBorrowable = existingBookCopy.Status == SD.Book_Copy_Status_Available || existingBookCopy.Status == SD.Book_Copy_Status_Reserved;
                if (!isBorrowable && copyStatus == SD.Book_Copy_Status_Borrowed)
                {
                    return (false, "Book is not available");
                }
                existingBookCopy.Status = copyStatus;
                _db.BookCopy.Update(existingBookCopy);
                if (commitChanges)
                {
                    await _db.SaveChangesAsync();
                }
                return (true, "Book copy successfully updated");
            }
            catch (Exception)
            {
                return (false, "Failed to update book copy" );
            }
        }
    }
}
