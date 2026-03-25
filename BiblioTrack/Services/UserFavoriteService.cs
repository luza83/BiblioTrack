using Azure;
using BiblioTrack.Data;
using BiblioTrack.Models;
using BiblioTrack.Models.Dto;
using BiblioTrack.Utility;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BiblioTrack.Services
{
    public class UserFavoriteService: IUserFavoriteService
    {
        private readonly ApplicationDbContext _db;

        public UserFavoriteService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> AddToFavorites(UserFavoriteBooksRequest userFavoriteBooksRequest)
        {
            try
            {
                UserFavoriteBookModel newFavorite = new UserFavoriteBookModel();
                newFavorite.UserId = userFavoriteBooksRequest.UserId;
                newFavorite.BookId = userFavoriteBooksRequest.BookId;

                _db.UserFavoriteBook.Add(newFavorite);
                await _db.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<bool> RemoveFromFavorites(UserFavoriteBooksRequest userFavoriteBooksRequest)
        {
            try
            {

                UserFavoriteBookModel? existingFavoriteBook = await _db.UserFavoriteBook.FirstOrDefaultAsync(u => u.UserId == userFavoriteBooksRequest.UserId && u.BookId == userFavoriteBooksRequest.BookId);

                if (existingFavoriteBook == null)
                {
                    return false;
                }


                _db.UserFavoriteBook.Remove(existingFavoriteBook);
                await _db.SaveChangesAsync();

                return true;


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

    }
}
