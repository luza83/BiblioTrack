using Azure;
using BiblioTrack.Data;
using BiblioTrack.Models;
using BiblioTrack.Models.Dto;
using BiblioTrack.Utility;
using System.Net;

namespace BiblioTrack.Services
{
    public class BorrowingsService : IBorrowingsService
    {
        private readonly ApplicationDbContext _db;
        private readonly IBookCopyService _bookCopyService;
        
    
        public BorrowingsService(ApplicationDbContext db, IBookCopyService bookCopyService)
        {
            _db = db;
            _bookCopyService = bookCopyService;
        }

        public async Task<bool> UpdateBorrowing(string userId, UpdateBorrowingDTO updateBorrowingDTO, bool isAdmin =false)
        {
            try
            {
                Borrowings? existingBorrowing = await _db.Borrowings.FindAsync(updateBorrowingDTO.BorrowId);

               

                if (existingBorrowing == null)
                {          
                   return false;
                }
                var isAuthorized = isAdmin || string.Equals(existingBorrowing.UserId, userId);

                if (existingBorrowing?.BorrowId != updateBorrowingDTO.BorrowId || !isAuthorized)
                {
                    return false;
                }


                bool updateBookCopy = false;
                bool updateBorrowing = !string.IsNullOrEmpty(updateBorrowingDTO.NewBorrowStatus) &&
                                       existingBorrowing.Status != updateBorrowingDTO.NewBorrowStatus;
                string? newCopyStatus = null;

                // Change Due Date
                if (existingBorrowing.Status == SD.Borrowing_Status_Borrowed &&
                    updateBorrowingDTO.DueDate != DateTime.MinValue &&
                    existingBorrowing.DueDate < updateBorrowingDTO.DueDate)
                {
                    existingBorrowing.DueDate = updateBorrowingDTO.DueDate;
                    existingBorrowing.Status = SD.Borrowing_Status_Borrowed;
                }


                if (updateBorrowing)
                {
                    switch (updateBorrowingDTO.NewBorrowStatus)
                    {
                        case SD.Borrowing_Status_Returned:
                            existingBorrowing.Status = updateBorrowingDTO.NewBorrowStatus;
                            existingBorrowing.ReturnDate = DateTime.Now;

                            newCopyStatus = SD.Book_Copy_Status_Available;
                            updateBookCopy = true;
                            break;

                        case SD.Borrowing_Status_Borrowed:
                            existingBorrowing.Status = updateBorrowingDTO.NewBorrowStatus;
                            newCopyStatus = SD.Book_Copy_Status_Borrowed;
                            updateBookCopy = true;
                            break;

                        case SD.Book_Copy_Status_Available:
                            _db.Borrowings.Remove(existingBorrowing);
                            await _db.SaveChangesAsync();

                            newCopyStatus = SD.Book_Copy_Status_Available;
                            updateBookCopy = true;

                            updateBorrowing = false;
                            break;
                    }

                }

                if (updateBookCopy && !string.IsNullOrEmpty(newCopyStatus))
                {
                    var copyUpdated = await _bookCopyService.UpdateBookCopy(existingBorrowing.CopyId, newCopyStatus);
                    if (!copyUpdated.Success)
                    {
                        return false;
                    }
                    if (!updateBorrowing)
                    {
                        return true;
                    }

                }

                if (updateBorrowing)
                {
                    _db.Borrowings.Update(existingBorrowing);
                    await _db.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
