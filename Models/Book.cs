namespace TBRPicker.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string? GoodreadsId { get; set; }  // for sync later
        public string Title { get; set; }
        public string Author { get; set; }
        public int? PageCount { get; set; }
        public string Shelf { get; set; }
        public string? Genre { get; set; }
    }
}