using BiblioTrack.Data;
using BiblioTrack.Models;
using BiblioTrack.Models.Dto;
using BiblioTrack.Services;
using BiblioTrack.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace BiblioTrack.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ApiResponse _response;
        private readonly UserManager<ApplicationUser> _userManager;


        public UserController(ApplicationDbContext db,  UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _response = new ApiResponse();
            _userManager = userManager;
        }
        [Authorize]
        [HttpGet("{userId}", Name = "GetUserById")]

        public async Task<IActionResult> GetUserById(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return Ok(_response);
            }
            var user = await _userManager.FindByIdAsync(userId);
            if(user == null)
            {
                _response.IsSuccess = false;
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return Ok(_response);
            }

            UserDto result = new UserDto()
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName
            };

            if (user == null)
            {
                _response.StatusCode = System.Net.HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                return Ok(_response);
            }

            _response.Result = result;
            _response.IsSuccess = true;
            _response.StatusCode = System.Net.HttpStatusCode.OK;
            return Ok(_response);
        }

        [Authorize]
        [HttpPut("{userId}", Name = "UpdateUser")]
        public async Task<IActionResult> UpdateUser(string userId, [FromForm] UpdateUserRequest updateUserRequest)
        {
            if (!ModelState.IsValid)
            {
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }

            if (userId == null)
            {
                _response.StatusCode = System.Net.HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            var isAdmin = User.IsInRole(SD.Role_Admin);
            if (!isAdmin)
            {
                _response.StatusCode = System.Net.HttpStatusCode.Forbidden;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            var exisitingUser = _userManager.Users.FirstOrDefault(u => u.Id == userId);
            bool isUpdate = false;

            if (exisitingUser == null)
            {
                _response.StatusCode = System.Net.HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }

            if (updateUserRequest.UserName != null && updateUserRequest.UserName != exisitingUser.UserName)
            {
                exisitingUser.UserName = updateUserRequest.UserName;
                isUpdate = true;
            }

            if (updateUserRequest.Email != null && updateUserRequest.Email != exisitingUser.Email)
            {
                exisitingUser.Email = updateUserRequest.Email;
                isUpdate = true;
            }

            if (updateUserRequest.ResetPassword && updateUserRequest.Password != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(exisitingUser);
                var passwordResult = await _userManager.ResetPasswordAsync(exisitingUser, token, updateUserRequest.Password);
                if (!passwordResult.Succeeded)
                {
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = passwordResult.Errors.Select(e => e.Description).ToList();
                    return BadRequest(_response);
                }
                _response.IsSuccess = true;
                _response.StatusCode = System.Net.HttpStatusCode.NoContent;
                return Ok(_response);
            }

            if (!isUpdate)
            {
                _response.IsSuccess = false;
                _response.StatusCode = System.Net.HttpStatusCode.NoContent;
                return Ok(_response);
            }
            var result = await _userManager.UpdateAsync(exisitingUser);
            if (!result.Succeeded)
            {
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(_response);
            }

            _response.IsSuccess = true;
            _response.StatusCode = System.Net.HttpStatusCode.Created;
            return Ok(_response);
        }

        [Authorize]
        [HttpDelete("{userId}", Name = "DeleteUser")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            var isAdmin = User.IsInRole(SD.Role_Admin);
            if (!isAdmin)
            {
                _response.StatusCode = System.Net.HttpStatusCode.Forbidden;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            var existingUser = await _userManager.FindByIdAsync(userId);
            if (existingUser == null)
            {
                _response.StatusCode = System.Net.HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            var userBorrowings =  _db.Borrowings.Where(u => u.UserId == userId);
            var userFavorites = _db.UserFavoriteBook.Where(u => u.UserId == userId).Count();
            if (userBorrowings.Any(b => b.Status == SD.Borrowing_Status_Borrowed || b.Status == SD.Borrowing_Status_Reserved) ||
                userFavorites > 0)
           {
                _response.StatusCode = System.Net.HttpStatusCode.NoContent;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { "User cannot be deleted while they have active borrowings or favorite books registered." };
                return Ok(_response);
            }
            var result = await _userManager.DeleteAsync(existingUser);
            if (!result.Succeeded)
            {
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(_response);
            }
            _response.IsSuccess = true;
            _response.StatusCode = System.Net.HttpStatusCode.OK;
            return Ok(_response);
        }


    }
}
