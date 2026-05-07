using CsvHelper;
using CsvHelper.Configuration;
using System;
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


        public List<Book> GetTBRBooks(int? maxPages = null, string? genre = null, string? shelf = null)
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

                if (!string.IsNullOrEmpty(genre))
                    books = books.Where(b => b.Genre != null &&
                            b.Genre.ToLower().Contains(genre.ToLower()));

                if (!string.IsNullOrEmpty(shelf))
                {
                    var selectedShelves = shelf.Split(',').Select(s => s.Trim().ToLower()).ToList();
                    books = books.Where(b => b.Shelf != null &&
                            selectedShelves.Any(s => b.Shelf.ToLower().Contains(s)));
                }

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

        public string ImportBooksFromStream(Stream fileStream)
        {
            using var reader = new StreamReader(fileStream);

            // Peek at the first line to detect delimiter
            var firstLine = reader.ReadLine() ?? "";
            var delimiter = firstLine.Contains(';') ? ";" : ",";

            // Reset to beginning
            fileStream.Position = 0;
            using var reader2 = new StreamReader(fileStream);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = delimiter
            };

            using var csv = new CsvReader(reader2, config);

            var tbrBooks = csv.GetRecords<GoodreadsBook>()
                  .ToList();

            _context.Books.RemoveRange(_context.Books);

            foreach (var b in tbrBooks)
            {
                _context.Books.Add(new Book
                {
                    Title = b.Title,
                    Author = b.Author,
                    PageCount = b.NumberOfPages,
                    Shelf = b.ExclusiveShelf,
                    Genre = null
                });
            }

            _context.SaveChanges();
            return $"{tbrBooks.Count} books imported successfully!";
        }

        public List<string> GetShelves()
        {
            return _context.Books
                           .Where(b => b.Shelf != null)
                           .Select(b => b.Shelf!)
                           .ToList()
                           .SelectMany(s => s.Split(',').Select(x => x.Trim()))
                           .Distinct()
                           .OrderBy(s => s)
                           .ToList();
        }
    }
}