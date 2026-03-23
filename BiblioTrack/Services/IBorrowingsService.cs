using BiblioTrack.Models.Dto;

namespace BiblioTrack.Services
{
    public interface IBorrowingsService
    {
        Task<bool> UpdateBorrowing(string userId, UpdateBorrowingDTO updateBorrowingDTO, bool isAdmin = false);

    }
}
