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

                            newCopyStatus = SD.Book_Copy_Status_Available;
                            updateBookCopy = true;

                            updateBorrowing = false;
                            break;
                    }

                }

                if (updateBookCopy && !string.IsNullOrEmpty(newCopyStatus))
                {
                    var copyUpdated = await _bookCopyService.UpdateBookCopy(existingBorrowing.CopyId, newCopyStatus, commitChanges:false);
                    if (!copyUpdated.Success)
                    {
                        return false;
                    }
                    if (!updateBorrowing)
                    {
                        await _db.SaveChangesAsync();
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

        public async Task<BorrowingDTO> AddBorrowing(AddBorrowingRequest addBorrowingRequest)
        {
            try
            {
                var firstAvailableCopy = _db.BookCopy
                                .Where(bc => addBorrowingRequest.BookId == bc.BookId && bc.Status == SD.Book_Copy_Status_Available)
                                .FirstOrDefault();

                if (firstAvailableCopy == null)
                {
                    throw new Exception("No book copies available for this book");
                }
                Borrowings borrowing = new()
                {
                    UserId = addBorrowingRequest.UserId,
                    CopyId = firstAvailableCopy.CopyId,
                    BorrowDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(15),
                    Status = SD.Borrowing_Status_Reserved
                };


                var bookCopyUpdated = await _bookCopyService.UpdateBookCopy(copyId: firstAvailableCopy.CopyId,
                                                                            copyStatus: SD.Book_Copy_Status_Reserved,
                                                                            commitChanges: false);

                if (!bookCopyUpdated.Success)
                {

                    throw new Exception("Failed to update book copy status");
                }
                _db.Borrowings.Add(borrowing);
                await _db.SaveChangesAsync();

                var response = new BorrowingDTO()
                {
                    BorrowId = borrowing.BorrowId,
                    CopyId = borrowing.CopyId,
                    Copy = borrowing.Copy,
                    BorrowDate = borrowing.BorrowDate,
                    DueDate = borrowing.DueDate,
                    ReturnDate = borrowing.ReturnDate,
                    Status = borrowing.Status
                };

                return response;
            }
            catch (Exception ex)
            {

                 throw;
            }

        }
    }
}
