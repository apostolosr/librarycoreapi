using Microsoft.EntityFrameworkCore;
using LibraryCoreApi.Database;
using LibraryCoreApi.Entities;
using LibraryCoreApi.DTOs;
using LibraryCoreApi.Errors;
using LibraryCoreApi.Events;

namespace LibraryCoreApi.Services.Books;

public class BooksService : IBooksService
{
    private readonly DataContext _context;
    private readonly IEventPublisher _eventManager;

    public BooksService(DataContext context, IEventPublisher eventManager)
    {
        _context = context;
        _eventManager = eventManager;
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
                Publisher = b.Publisher,
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
            throw new KeyNotFoundException("Book not found");
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
            Publisher = book.Publisher,
            TotalCopies = book.Copies.Count,
            AvailableCopies = book.Copies.Count(c => c.IsAvailable),
            CreatedAt = book.CreatedAt
        };

        return bookDto;
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
            throw new KeyNotFoundException("Author not found");
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
            throw new KeyNotFoundException("Category not found");
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
            Publisher = book.Publisher,
            TotalCopies = book.Copies.Count,
            AvailableCopies = book.Copies.Count(c => c.IsAvailable),
            CreatedAt = book.CreatedAt
        };

        // Publish book created event
        var bookCreatedEvent = new BookCreatedEvent
        {
            BookId = bookDto.Id,
            Title = bookDto.Title,
            ISBN = bookDto.ISBN,
            AuthorId = bookDto.AuthorId,
            CategoryId = bookDto.CategoryId,
            Publisher = bookDto.Publisher,
            TotalCopies = bookDto.TotalCopies,
        };

        await _eventManager.PublishEvent("book.created", bookCreatedEvent);

        return bookDto;
    }

    public async Task<BookDto> UpdateBook(int id, UpdateBookDto updateDto)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
        {
            throw new KeyNotFoundException("Book not found");
        }

        // Validate category exists
        var category = await _context.Categories.FindAsync(updateDto.CategoryId);
        if (category == null)
        {
            throw new KeyNotFoundException("Category not found");
        }

        book.Title = updateDto.Title;
        book.Description = updateDto.Description;
        book.CategoryId = updateDto.CategoryId;
        book.PublishedDate = updateDto.PublishedDate;
        book.UpdatedAt = DateTime.UtcNow;
        book.Publisher = updateDto.Publisher;

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

        // Publish book updated event
        var bookUpdatedEvent = new BookEvent
        {
            BookId = book.Id,
            Title = book.Title,
            CategoryId = book.CategoryId,
        };

        await _eventManager.PublishEvent("book.updated", bookUpdatedEvent);

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

    public async Task DeleteBook(int id)
    {
        var book = await _context.Books
            .Include(b => b.Copies)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
        {
            throw new KeyNotFoundException("Book not found");
        }

        // Check if any copies are currently borrowed
        var borrowedCopies = book.Copies.Any(c => !c.IsAvailable);
        if (borrowedCopies)
        {
            throw new ApiException("Cannot delete book with borrowed copies");
        }


        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        // Publish book deleted event
        var bookDeletedEvent = new BookEvent
        {
            BookId = book.Id,
            Title = book.Title,
            CategoryId = book.CategoryId,
        };

        await _eventManager.PublishEvent("book.deleted", bookDeletedEvent);
    }
}
