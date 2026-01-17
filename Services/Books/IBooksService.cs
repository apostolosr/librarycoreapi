using LibraryCoreApi.DTOs;

namespace LibraryCoreApi.Services.Books;



public interface IBooksService
{
    Task<IEnumerable<BookDto>> GetBooks();
    Task<BookDto> GetBook(int id);
    Task<BookAvailabilityDto> GetBookAvailability(int id);
    Task<BookAvailabilityDto> GetBookAvailabilityByTitle(string title);
    Task<BookDto> CreateBook(CreateBookDto createDto);
    Task<BookDto> UpdateBook(int id, UpdateBookDto updateDto);
    Task<BookDto> DeleteBook(int id);
}