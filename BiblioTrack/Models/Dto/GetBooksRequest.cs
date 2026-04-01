using System.ComponentModel.DataAnnotations;

namespace BiblioTrack.Models.Dto
{
    public class GetBooksRequest
    {
        public string? Title { get; set; } = string.Empty;
        public string? Author { get; set; } = string.Empty;
        public string? ISBN { get; set; } = string.Empty;
        public string? Publisher { get; set; } = string.Empty;
        public string? Category { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public double? AverageRating { get; set; } = double.MaxValue;
        public int? RatingsCount { get; set; } = int.MaxValue;
        public bool IncludeUserFavorites { get; set; } = false;
        public bool GetAvailableOnly { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
