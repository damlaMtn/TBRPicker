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
        private readonly AppDbContext _context;

        public BookService(AppDbContext context)
        {
            _context = context;
        }


        public List<Book> GetTBRBooks(int? maxPages = null, string? genre = null, string? shelf = null)
        {
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
                            selectedShelves.All(s => b.Shelf.ToLower().Contains(s)));
                }

                return books.ToList();
            }
            catch (FileNotFoundException)
            {
                // Re-throw to preserve original exception type and stack.
                throw;
            }
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
                    GoodreadsId = b.BookId,
                    Title = b.Title,
                    Author = b.Author,
                    PageCount = (int?)b.NumberOfPages,
                    Shelf = b.ExclusiveShelf,
                    Genre = b.Genres
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

        public string SyncBooksFromStream(Stream fileStream)
        {
            using var reader = new StreamReader(fileStream);
            var firstLine = reader.ReadLine() ?? "";
            var delimiter = firstLine.Contains(';') ? ";" : ",";

            fileStream.Position = 0;
            using var reader2 = new StreamReader(fileStream);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = delimiter
            };

            using var csv = new CsvReader(reader2, config);

            var csvBooks = csv.GetRecords<GoodreadsBook>().ToList();

            var existingIds = _context.Books
                .Where(b => b.GoodreadsId != null)
                .Select(b => b.GoodreadsId!)
                .ToHashSet();

            var newBooks = csvBooks
                .Where(b => !existingIds.Contains(b.BookId))
                .ToList();

            foreach (var b in newBooks)
            {
                _context.Books.Add(new Book
                {
                    GoodreadsId = b.BookId,
                    Title = b.Title,
                    Author = b.Author,
                    PageCount = (int?)b.NumberOfPages,
                    Shelf = b.ExclusiveShelf,
                    Genre = b.Genres
                });
            }

            _context.SaveChanges();
            return $"{newBooks.Count} new books added. {csvBooks.Count - newBooks.Count} already existed, skipped.";
        }

        public List<Book> GetFilteredBooks(string? search = null, string? shelf = null)
        {
            var books = _context.Books.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var q = search.ToLower();
                books = books.Where(b =>
                    b.Title.ToLower().Contains(q) ||
                    b.Author.ToLower().Contains(q));
            }

            if (!string.IsNullOrEmpty(shelf))
            {
                var selectedShelves = shelf.Split(',').Select(s => s.Trim().ToLower()).ToList();
                books = books.Where(b => b.Shelf != null &&
                        selectedShelves.All(s => b.Shelf.ToLower().Contains(s)));
            }

            return books.OrderBy(b => b.Title).ToList();
        }

        public async Task<bool> UpdateGenreAsync(int id, string? genre)
        {
            var book = await _context.Books.FindAsync(id);
            if (book is null) return false;

            book.Genre = string.IsNullOrWhiteSpace(genre) ? null : genre.Trim();
            await _context.SaveChangesAsync();
            return true;
        }
    }
}