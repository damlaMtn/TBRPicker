namespace TBRPicker.Models
{
    public class Book
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public int PageCount { get; set; }
        public string Shelf { get; set; }
        public double AverageRating { get; set; }
        public double MyRating { get; set; }
    }
}