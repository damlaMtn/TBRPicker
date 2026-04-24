using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;
using TBRPicker.Data;
using TBRPicker.Models;
using static System.Reflection.Metadata.BlobBuilder;

namespace TBRPicker.Services
{
    public class BookService
    {
        private readonly string _csvPath = @"C:\Users\damla\Desktop\Projects\_TBRPicker\goodreads_library_export.csv";

        private readonly AppDbContext _context;

        public BookService(AppDbContext context)
        {
            _context = context;
        }


        public List<Book> GetTBRBooks(int? maxPages = null)
        {
            if (!File.Exists(_csvPath))
            {
                throw new FileNotFoundException($"CSV file not found at path '{_csvPath}'.", _csvPath);
            }

            try
            {
                var books = _context.Books.AsQueryable();

                if (maxPages.HasValue)
                    books = books.Where(b => b.PageCount <= maxPages.Value);

                return books.ToList();
            }
            catch (FileNotFoundException)
            {
                // Re-throw to preserve original exception type and stack.
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new InvalidOperationException($"Access denied to CSV file at '{_csvPath}'.", ex);
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException($"I/O error while reading CSV file at '{_csvPath}'.", ex);
            }
            catch (CsvHelperException ex)
            {
                // CsvHelperException covers parsing/formatting related errors from CsvHelper.
                throw new InvalidDataException($"Failed to parse CSV file at '{_csvPath}'. The file may be malformed: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                // Fallback for any other unexpected error.
                throw new Exception($"Unexpected error while reading CSV file at '{_csvPath}': {ex.Message}", ex);
            }
        }

        public void ImportBooks()
        {
            var tbrBooks = GetTBRBooks();

            foreach (var b in tbrBooks)
            {
                var book = new Book
                {
                    Title = b.Title,
                    Author = b.Author,
                    Shelf = b.Shelf,
                    PageCount = b.PageCount
                };

                _context.Books.Add(book);
            }

            _context.SaveChanges();
        }
    }
}