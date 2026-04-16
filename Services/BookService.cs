using CsvHelper;
using System.Globalization;
using TBRPicker.Models;
using CsvHelper.Configuration;
using System.IO;

namespace TBRPicker.Services
{
    public class BookService
    {
        private readonly string _csvPath = @"C:\Users\damla\Desktop\Projects\_TBRPicker\goodreads_library_export.csv";

        public List<GoodreadsBook> GetTBRBooks()
        {
            if (!File.Exists(_csvPath))
            {
                throw new FileNotFoundException($"CSV file not found at path '{_csvPath}'.", _csvPath);
            }

            try
            {
                using var reader = new StreamReader(_csvPath);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                var records = csv.GetRecords<GoodreadsBook>()
                                 .Where(b => b?.ExclusiveShelf?.Contains("to-read") == true)
                                 .ToList();

                return records;
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
    }
}