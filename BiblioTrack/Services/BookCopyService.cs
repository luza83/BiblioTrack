using BiblioTrack.Data;
using BiblioTrack.Utility;
using Microsoft.EntityFrameworkCore;

namespace BiblioTrack.Services
{
    public class BookCopyService
    {
        private readonly ApplicationDbContext _db;
 
        public BookCopyService(ApplicationDbContext db)
        {
            _db = db;

        }

        public async Task<(bool Success, string Message)> UpdateBookCopy(int copyId, string copyStatus)
        {
            var existingBookCopy = await _db.BookCopy
                .FirstOrDefaultAsync(u => u.CopyId == copyId);

            if (existingBookCopy == null)
                return (false, "Book Copy not found");

            try
            {
                if(existingBookCopy.Status != SD.Book_Copy_Status_Available && copyStatus == SD.Book_Copy_Status_Borrowed)
                {
                    return (false, "Book is not available");
                }
                existingBookCopy.Status = copyStatus;
                _db.BookCopy.Update(existingBookCopy);
                await _db.SaveChangesAsync();
                return (true, "Book copy successfully updated");
            }
            catch (Exception)
            {
                return (false, "Failed to update book copy" );
            }
        }

    }
}
