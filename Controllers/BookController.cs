using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using TBRPicker.DTOs;
using TBRPicker.Services;

namespace TBRPicker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : ControllerBase
    {
        private readonly BookService _bookService;
        private readonly ILogger<BookController> _logger;
        private readonly AiRecommendationService _aiService;

        public BookController(BookService bookService, AiRecommendationService aiService, ILogger<BookController> logger)
        {
            _bookService = bookService;
            _aiService = aiService;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadCSV(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a valid CSV file.");

            using var stream = file.OpenReadStream();
            var result = _bookService.ImportBooksFromStream(stream);
            return Ok(result);
        }


        [HttpGet("tbr")]
        public IActionResult GetTBRBooks()
        {
            try
            {
                var books = _bookService.GetTBRBooks();
                return Ok(books);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, "CSV file not found.");
                var pd = new ProblemDetails
                {
                    Title = "Data file not found",
                    Detail = "The book export file could not be found. Please upload it or check configuration.",
                    Status = StatusCodes.Status404NotFound
                };
                return NotFound(pd);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Access denied reading CSV.");
                var pd = new ProblemDetails
                {
                    Title = "Access denied",
                    Detail = "The server process does not have permission to read the data file.",
                    Status = StatusCodes.Status403Forbidden
                };
                return StatusCode(StatusCodes.Status403Forbidden, pd);
            }
            catch (InvalidDataException ex)
            {
                _logger.LogWarning(ex, "Malformed CSV.");
                var pd = new ProblemDetails
                {
                    Title = "Malformed data file",
                    Detail = "The CSV file appears to be malformed or unparsable.",
                    Status = StatusCodes.Status400BadRequest
                };
                return BadRequest(pd);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while reading books.");
                var pd = new ProblemDetails
                {
                    Title = "Unexpected error",
                    Detail = "An unexpected error occurred while processing the request.",
                    Status = StatusCodes.Status500InternalServerError
                };
                return StatusCode(StatusCodes.Status500InternalServerError, pd);
            }
        }

        [HttpGet("random")]
        public IActionResult GetRandomBook([FromQuery] int? maxPages = null, [FromQuery] string? genre = null, [FromQuery] string? shelf = null)
        {
            try
            {
                var books = _bookService.GetTBRBooks(maxPages, genre, shelf);

                if (!books.Any())
                {
                    var pd = new ProblemDetails
                    {
                        Title = "No items",
                        Detail = "No books were found on your 'to-read' shelf.",
                        Status = StatusCodes.Status404NotFound
                    };
                    return NotFound(pd);
                }

                var random = new Random();
                var randomBook = books[random.Next(books.Count)];
                return Ok(randomBook);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while selecting random book.");
                return Problem(title: "Unexpected error", detail: "An error occurred while processing the request.");
            }
        }

        [HttpGet("shelves")]
        public IActionResult GetShelves()
        {
            var shelves = _bookService.GetShelves();
            return Ok(shelves);
        }

        [HttpPost("sync")]
        public async Task<IActionResult> SyncCSV(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a valid CSV file.");

            using var stream = file.OpenReadStream();
            var result = _bookService.SyncBooksFromStream(stream);
            return Ok(result);
        }

        [HttpGet("list")]
        public IActionResult GetBooks(
    [FromQuery] string? search = null,
    [FromQuery] string? shelf = null,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
        {
            try
            {
                var books = _bookService.GetFilteredBooks(search, shelf);
                var total = books.Count;
                var paged = books
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Ok(new
                {
                    total,
                    page,
                    pageSize,
                    books = paged
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching book list.");
                return Problem(title: "Unexpected error", detail: "An error occurred while fetching the book list.");
            }
        }

        [HttpPatch("{id}/genre")]
        public async Task<IActionResult> UpdateGenre(int id, [FromBody] UpdateGenreDto dto)
        {
            var result = await _bookService.UpdateGenreAsync(id, dto.Genre);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpGet("genres")]
        public async Task<IActionResult> GetGenres()
        {
            var genres = await _bookService.GetAllGenresAsync();
            return Ok(genres);
        }

        [HttpPost("recommend")]
        public async Task<IActionResult> Recommend([FromBody] AiRecommendationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Mood))
                return BadRequest("Mood description is required");

            // Reuse existing filter logic
            var filtered = _bookService.GetTBRBooks(request.MaxPages, request.Genre, request.Shelf);

            if (!filtered.Any())
                return NotFound("No books found matching your filters");

            // Sample up to 50 randomly
            var sample = filtered
                .OrderBy(_ => Guid.NewGuid())
                .Take(50)
                .ToList();

            var result = await _aiService.RecommendAsync(request.Mood, sample);

            if (result is null)
                return StatusCode(500, "AI recommendation failed");

            // Find the actual book in DB to return full details
            var book = filtered.FirstOrDefault(b =>
                b.Title.Equals(result.Title, StringComparison.OrdinalIgnoreCase));

            return Ok(new
            {
                book,
                reason = result.Reason
            });
        }
    }
}