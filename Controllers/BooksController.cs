using Microsoft.AspNetCore.Mvc;
using LibraryCoreApi.DTOs;
using LibraryCoreApi.Services.Books;

namespace LibraryCoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBooksService _booksService;

    public BooksController(IBooksService booksService)
    {
        _booksService = booksService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks()
    {
        var books = await _booksService.GetBooks();

        return Ok(books);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookDto>> GetBook(int id)
    {
        var book = await _booksService.GetBook(id);

        if (book == null)
        {
            return NotFound();
        }

        return Ok(book);
    }

    [HttpGet("availability/{id}")]
    public async Task<ActionResult<BookAvailabilityDto>> GetBookAvailability(int id)
    {
        var book = await _booksService.GetBookAvailability(id);

        if (book == null)
        {
            return NotFound();
        }

        return Ok(book);
    }

    [HttpGet("availability/title/{title}")]
    public async Task<ActionResult<BookAvailabilityDto>> GetBookAvailabilityByTitle(string title)
    {
        var book = await _booksService.GetBookAvailabilityByTitle(title);

        if (book == null)
        {
            return NotFound();
        }

        return Ok(book);
    }

    [HttpPost]
    public async Task<ActionResult<BookDto>> CreateBook(CreateBookDto createDto)
    {
        var book = await _booksService.CreateBook(createDto);

        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BookDto>> UpdateBook(int id, UpdateBookDto updateDto)
    {
        var book = await _booksService.UpdateBook(id, updateDto);

        return Ok(book);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        await _booksService.DeleteBook(id);
        return NoContent();
    }
}
