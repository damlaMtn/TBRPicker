using CsvHelper.Configuration.Attributes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TBRPicker.Models
{
    public class GoodreadsBook
    {
        [Name("Book Id")]
        public string BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }

        [Name("Bookshelves")]
        public string ExclusiveShelf { get; set; }

        [Name("Number of Pages")]
        public double? NumberOfPages { get; set; }

        [Name("Genres")]
        public string? Genres { get; set; }
    }
}