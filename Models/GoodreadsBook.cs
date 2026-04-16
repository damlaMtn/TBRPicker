using CsvHelper.Configuration.Attributes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TBRPicker.Models
{
    public class GoodreadsBook
    {
        public string Title { get; set; }
        public string Author { get; set; }

        [Name("Bookshelves")]
        public string ExclusiveShelf { get; set; }

        [Name("Number of Pages")]
        public int? NumberOfPages { get; set; }
    }
}