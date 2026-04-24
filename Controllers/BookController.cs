using Microsoft.AspNetCore.Mvc;
using System.IO;
using TBRPicker.Services;

namespace TBRPicker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : ControllerBase
    {
        private readonly BookService _bookService;
        private readonly ILogger<BookController> _logger;

        public BookController(BookService bookService, ILogger<BookController> logger)
        {
            _bookService = bookService;
            _logger = logger;
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
        public IActionResult GetRandomBook([FromQuery] int? maxPages = null)
        {
            try
            {
                var books = _bookService.GetTBRBooks(maxPages);

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

        [HttpPost("import")]
        public IActionResult ImportBooks()
        {
            _bookService.ImportBooks();
            return Ok("Books imported successfully!");
        }
    }
}