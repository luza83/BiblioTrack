using System.ComponentModel.DataAnnotations;

namespace BiblioTrack.Models.Dto
{
    public class UpdateUserRequest
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public bool ResetPassword { get; set; } = false;
        public string? Email { get; set; }
        public string? Role { get; set; }
        
    }
}
