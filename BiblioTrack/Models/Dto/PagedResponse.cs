namespace BiblioTrack.Models.Dto
{
    public class PagedResponse<T>
    {
        public List<T> Data { get; set; } = new List<T>();

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalRecords { get; set; }

        public int TotalPages =>
            (int)Math.Ceiling(TotalRecords / (double)PageSize);

    }
}
