using Microsoft.EntityFrameworkCore;
using LibraryCoreApi.Database;
using LibraryCoreApi.Entities;
using LibraryCoreApi.DTOs;
using LibraryCoreApi.Errors;

namespace LibraryCoreApi.Services.Books;

public class BooksService : IBooksService
{
    private readonly DataContext _context;

    public BooksService(DataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BookDto>> GetBooks()
    {
        var books = await _context.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .Include(b => b.Copies)
            .Select(b => new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                ISBN = b.ISBN,
                Description = b.Description,
                AuthorId = b.AuthorId,
                AuthorName = b.Author.Name,
                CategoryId = b.CategoryId,
                CategoryName = b.Category.Name,
                PublishedDate = b.PublishedDate,
                TotalCopies = b.Copies.Count,
                AvailableCopies = b.Copies.Count(c => c.IsAvailable),
                CreatedAt = b.CreatedAt
            })
            .ToListAsync();

        return books;
    }

    public async Task<BookDto> GetBook(int id)
    {
        var book = await _context.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .Include(b => b.Copies)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
        {
            throw new ApiException("Book not found");
        }

        var bookDto = new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            ISBN = book.ISBN,
            Description = book.Description,
            AuthorId = book.AuthorId,
            AuthorName = book.Author.Name,
            CategoryId = book.CategoryId,
            CategoryName = book.Category.Name,
            PublishedDate = book.PublishedDate,
            TotalCopies = book.Copies.Count,
            AvailableCopies = book.Copies.Count(c => c.IsAvailable),
            CreatedAt = book.CreatedAt
        };

        return bookDto;
    }

    public async Task<BookAvailabilityDto> GetBookAvailability(int id)
    {
        var book = await _context.Books
            .Include(b => b.Copies)
                .ThenInclude(c => c.CurrentReservation!)
                    .ThenInclude(r => r.Customer)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
        {
            throw new ApiException("Book not found");
        }

        var availabilityDto = new BookAvailabilityDto
        {
            BookId = book.Id,
            Title = book.Title,
            ISBN = book.ISBN,
            TotalCopies = book.Copies.Count,
            AvailableCopies = book.Copies.Count(c => c.IsAvailable),
            Copies = book.Copies.Select(c => new BookCopyInfoDto
            {
                CopyId = c.Id,
                CopyNumber = c.CopyNumber,
                IsAvailable = c.IsAvailable,
                CurrentBorrower = c.CurrentReservation != null && 
                                 c.CurrentReservation.Status == ReservationStatus.Borrowed
                    ? c.CurrentReservation.Customer.Name
                    : null
            }).ToList()
        };

        return availabilityDto;
    }

    public async Task<BookAvailabilityDto> GetBookAvailabilityByTitle(string title)
    {
        var book = await _context.Books
            .Include(b => b.Copies)
                .ThenInclude(c => c.CurrentReservation!)
                    .ThenInclude(r => r.Customer)
            .FirstOrDefaultAsync(b => b.Title.ToLower() == title.ToLower());

        if (book == null)
        {
            throw new ApiException("Book not found");
        }

        var availabilityDto = new BookAvailabilityDto
        {
            BookId = book.Id,
            Title = book.Title,
            ISBN = book.ISBN,
            TotalCopies = book.Copies.Count,
            AvailableCopies = book.Copies.Count(c => c.IsAvailable),
            Copies = book.Copies.Select(c => new BookCopyInfoDto
            {
                CopyId = c.Id,
                CopyNumber = c.CopyNumber,
                IsAvailable = c.IsAvailable,
                CurrentBorrower = c.CurrentReservation != null && 
                                 c.CurrentReservation.Status == ReservationStatus.Borrowed
                    ? c.CurrentReservation.Customer.Name
                    : null
            }).ToList()
        };

        return availabilityDto;
    }

    public async Task<BookDto> CreateBook(CreateBookDto createDto)
    {
        // Validate author exists and has Author role
        var author = await _context.Parties
            .Include(p => p.PartyRoles)
                .ThenInclude(pr => pr.Role)
            .FirstOrDefaultAsync(p => p.Id == createDto.AuthorId);

        if (author == null)
        {
            throw new ApiException("Author not found");
        }

        var hasAuthorRole = author.PartyRoles.Any(pr => pr.Role.Name.Equals("Author", StringComparison.OrdinalIgnoreCase));
        if (!hasAuthorRole)
        {
            throw new ApiException("Party does not have Author role");
        }

        // Validate category exists
        var category = await _context.Categories.FindAsync(createDto.CategoryId);
        if (category == null)
        {
            throw new ApiException("Category not found");
        }

        // Check ISBN uniqueness
        var existingBook = await _context.Books.FirstOrDefaultAsync(b => b.ISBN == createDto.ISBN);
        if (existingBook != null)
        {
            throw new ApiException("Book with this ISBN already exists");
        }

        var book = new Book
        {
            Title = createDto.Title,
            ISBN = createDto.ISBN,
            Description = createDto.Description,
            AuthorId = createDto.AuthorId,
            CategoryId = createDto.CategoryId,
            PublishedDate = createDto.PublishedDate,
            CreatedAt = DateTime.UtcNow
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        // Create book copies
        for (int i = 1; i <= createDto.NumberOfCopies; i++)
        {
            var copy = new BookCopy
            {
                BookId = book.Id,
                CopyNumber = $"{book.Id}-{i}",
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.BookCopies.Add(copy);
        }

        await _context.SaveChangesAsync();

        // Reload with relationships
        await _context.Entry(book)
            .Reference(b => b.Author)
            .LoadAsync();
        await _context.Entry(book)
            .Reference(b => b.Category)
            .LoadAsync();
        await _context.Entry(book)
            .Collection(b => b.Copies)
            .LoadAsync();

        var bookDto = new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            ISBN = book.ISBN,
            Description = book.Description,
            AuthorId = book.AuthorId,
            AuthorName = book.Author.Name,
            CategoryId = book.CategoryId,
            CategoryName = book.Category.Name,
            PublishedDate = book.PublishedDate,
            TotalCopies = book.Copies.Count,
            AvailableCopies = book.Copies.Count(c => c.IsAvailable),
            CreatedAt = book.CreatedAt
        };

        return bookDto;
    }

    public async Task<BookDto> UpdateBook(int id, UpdateBookDto updateDto)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
        {
            throw new ApiException("Book not found");
        }

        // Check ISBN uniqueness if changed
        if (book.ISBN != updateDto.ISBN)
        {
            var existingBook = await _context.Books.FirstOrDefaultAsync(b => b.ISBN == updateDto.ISBN);
            if (existingBook != null)
            {
                throw new ApiException("Book with this ISBN already exists");
            }
        }

        // Validate category exists
        var category = await _context.Categories.FindAsync(updateDto.CategoryId);
        if (category == null)
        {
            throw new ApiException("Category not found");
        }

        book.Title = updateDto.Title;
        book.ISBN = updateDto.ISBN;
        book.Description = updateDto.Description;
        book.CategoryId = updateDto.CategoryId;
        book.PublishedDate = updateDto.PublishedDate;
        book.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Reload with relationships
        await _context.Entry(book)
            .Reference(b => b.Author)
            .LoadAsync();
        await _context.Entry(book)
            .Reference(b => b.Category)
            .LoadAsync();
        await _context.Entry(book)
            .Collection(b => b.Copies)
            .LoadAsync();

        return new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            ISBN = book.ISBN,
            Description = book.Description,
            AuthorId = book.AuthorId,
            AuthorName = book.Author.Name,
            CategoryId = book.CategoryId,
            CategoryName = book.Category.Name,
            PublishedDate = book.PublishedDate,
            TotalCopies = book.Copies.Count,
            AvailableCopies = book.Copies.Count(c => c.IsAvailable),
            CreatedAt = book.CreatedAt
        };
    }

    public async Task<BookDto> DeleteBook(int id)
    {
        var book = await _context.Books
            .Include(b => b.Copies)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
        {
            throw new ApiException("Book not found");
        }

        // Check if any copies are currently borrowed
        var borrowedCopies = book.Copies.Any(c => !c.IsAvailable);
        if (borrowedCopies)
        {
            throw new ApiException("Cannot delete book with borrowed copies");
        }

        // Reload with relationships before deletion
        await _context.Entry(book)
            .Reference(b => b.Author)
            .LoadAsync();
        await _context.Entry(book)
            .Reference(b => b.Category)
            .LoadAsync();

        var bookDto = new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            ISBN = book.ISBN,
            Description = book.Description,
            AuthorId = book.AuthorId,
            AuthorName = book.Author.Name,
            CategoryId = book.CategoryId,
            CategoryName = book.Category.Name,
            PublishedDate = book.PublishedDate,
            TotalCopies = book.Copies.Count,
            AvailableCopies = book.Copies.Count(c => c.IsAvailable),
            CreatedAt = book.CreatedAt
        };

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        return bookDto;
    }
}
