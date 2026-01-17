using Microsoft.EntityFrameworkCore;
using LibraryCoreApi.Database;
using LibraryCoreApi.Entities;
using LibraryCoreApi.DTOs;
using LibraryCoreApi.Errors;

namespace LibraryCoreApi.Services.Reservations;

public class ReservationsService : IReservationsService
{
    private readonly DataContext _context;

    public ReservationsService(DataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ReservationDto>> GetReservations()
    {
        var reservations = await _context.Reservations
            .Include(r => r.BookCopy)
                .ThenInclude(bc => bc.Book)
            .Include(r => r.Customer)
            .Select(r => new ReservationDto
            {
                Id = r.Id,
                BookCopyId = r.BookCopyId,
                BookTitle = r.BookCopy.Book.Title,
                CopyNumber = r.BookCopy.CopyNumber,
                CustomerId = r.CustomerId,
                CustomerName = r.Customer.Name,
                ReservedAt = r.ReservedAt,
                BorrowedAt = r.BorrowedAt,
                ReturnedAt = r.ReturnedAt,
                DueDate = r.DueDate,
                Status = r.Status.ToString()
            })
            .ToListAsync();

        return reservations;
    }

    public async Task<ReservationDto> GetReservation(int id)
    {
        var reservation = await _context.Reservations
            .Include(r => r.BookCopy)
                .ThenInclude(bc => bc.Book)
            .Include(r => r.Customer)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation == null)
        {
            throw new KeyNotFoundException("Reservation not found");
        }

        var reservationDto = new ReservationDto
        {
            Id = reservation.Id,
            BookCopyId = reservation.BookCopyId,
            BookTitle = reservation.BookCopy.Book.Title,
            CopyNumber = reservation.BookCopy.CopyNumber,
            CustomerId = reservation.CustomerId,
            CustomerName = reservation.Customer.Name,
            ReservedAt = reservation.ReservedAt,
            BorrowedAt = reservation.BorrowedAt,
            ReturnedAt = reservation.ReturnedAt,
            DueDate = reservation.DueDate,
            Status = reservation.Status.ToString()
        };

        return reservationDto;
    }

    public async Task<ReservationDto> CreateReservation(CreateReservationDto createDto)
    {
        // Validate customer exists and has Customer role
        var customer = await _context.Parties
            .Include(p => p.PartyRoles)
                .ThenInclude(pr => pr.Role)
            .FirstOrDefaultAsync(p => p.Id == createDto.CustomerId);

        if (customer == null)
        {
            throw new KeyNotFoundException("Customer not found");
        }

        var hasCustomerRole = customer.PartyRoles.Any(pr => pr.Role.Name.Equals("Customer", StringComparison.OrdinalIgnoreCase));
        if (!hasCustomerRole)
        {
            throw new ApiException("Party does not have Customer role");
        }

        // Find an available copy of the book
        var availableCopy = await _context.BookCopies
            .Include(bc => bc.Book)
            .FirstOrDefaultAsync(bc => bc.BookId == createDto.BookId && bc.IsAvailable);

        if (availableCopy == null)
        {
            throw new ApiException("No available copies of this book");
        }

        // Create reservation
        var reservation = new Reservation
        {
            BookCopyId = availableCopy.Id,
            CustomerId = createDto.CustomerId,
            ReservedAt = DateTime.UtcNow,
            Status = ReservationStatus.Reserved
        };

        _context.Reservations.Add(reservation);
        
        // Mark copy as unavailable
        availableCopy.IsAvailable = false;
        availableCopy.CurrentReservation = reservation;

        await _context.SaveChangesAsync();

        // Reload with relationships
        await _context.Entry(reservation)
            .Reference(r => r.BookCopy)
            .LoadAsync();
        await _context.Entry(reservation.BookCopy)
            .Reference(bc => bc.Book)
            .LoadAsync();
        await _context.Entry(reservation)
            .Reference(r => r.Customer)
            .LoadAsync();

        var reservationDto = new ReservationDto
        {
            Id = reservation.Id,
            BookCopyId = reservation.BookCopyId,
            BookTitle = reservation.BookCopy.Book.Title,
            CopyNumber = reservation.BookCopy.CopyNumber,
            CustomerId = reservation.CustomerId,
            CustomerName = reservation.Customer.Name,
            ReservedAt = reservation.ReservedAt,
            BorrowedAt = reservation.BorrowedAt,
            ReturnedAt = reservation.ReturnedAt,
            DueDate = reservation.DueDate,
            Status = reservation.Status.ToString()
        };

        return reservationDto;
    }

    public async Task<ReservationDto> BorrowBook(BorrowBookDto borrowDto)
    {
        var reservation = await _context.Reservations
            .Include(r => r.BookCopy)
            .FirstOrDefaultAsync(r => r.Id == borrowDto.ReservationId);

        if (reservation == null)
        {
            throw new KeyNotFoundException("Reservation not found");
        }

        if (reservation.Status != ReservationStatus.Reserved)
        {
            throw new ApiException("Only reserved books can be borrowed");
        }

        reservation.Status = ReservationStatus.Borrowed;
        reservation.BorrowedAt = DateTime.UtcNow;
        reservation.DueDate = borrowDto.DueDate;

        await _context.SaveChangesAsync();

        // Reload with relationships
        await _context.Entry(reservation)
            .Reference(r => r.BookCopy)
            .LoadAsync();
        await _context.Entry(reservation.BookCopy)
            .Reference(bc => bc.Book)
            .LoadAsync();
        await _context.Entry(reservation)
            .Reference(r => r.Customer)
            .LoadAsync();

        var reservationDto = new ReservationDto
        {
            Id = reservation.Id,
            BookCopyId = reservation.BookCopyId,
            BookTitle = reservation.BookCopy.Book.Title,
            CopyNumber = reservation.BookCopy.CopyNumber,
            CustomerId = reservation.CustomerId,
            CustomerName = reservation.Customer.Name,
            ReservedAt = reservation.ReservedAt,
            BorrowedAt = reservation.BorrowedAt,
            ReturnedAt = reservation.ReturnedAt,
            DueDate = reservation.DueDate,
            Status = reservation.Status.ToString()
        };

        return reservationDto;
    }

    public async Task<ReservationDto> ReturnBook(ReturnBookDto returnDto)
    {
        var reservation = await _context.Reservations
            .Include(r => r.BookCopy)
            .FirstOrDefaultAsync(r => r.Id == returnDto.ReservationId);

        if (reservation == null)
        {
            throw new KeyNotFoundException("Reservation not found");
        }

        if (reservation.Status != ReservationStatus.Borrowed)
        {
            throw new ApiException("Only borrowed books can be returned");
        }

        reservation.Status = ReservationStatus.Returned;
        reservation.ReturnedAt = DateTime.UtcNow;

        // Mark copy as available
        reservation.BookCopy.IsAvailable = true;
        reservation.BookCopy.CurrentReservation = null;

        await _context.SaveChangesAsync();

        // Reload with relationships
        await _context.Entry(reservation)
            .Reference(r => r.BookCopy)
            .LoadAsync();
        await _context.Entry(reservation.BookCopy)
            .Reference(bc => bc.Book)
            .LoadAsync();
        await _context.Entry(reservation)
            .Reference(r => r.Customer)
            .LoadAsync();

        var reservationDto = new ReservationDto
        {
            Id = reservation.Id,
            BookCopyId = reservation.BookCopyId,
            BookTitle = reservation.BookCopy.Book.Title,
            CopyNumber = reservation.BookCopy.CopyNumber,
            CustomerId = reservation.CustomerId,
            CustomerName = reservation.Customer.Name,
            ReservedAt = reservation.ReservedAt,
            BorrowedAt = reservation.BorrowedAt,
            ReturnedAt = reservation.ReturnedAt,
            DueDate = reservation.DueDate,
            Status = reservation.Status.ToString()
        };

        return reservationDto;
    }

    public async Task<IEnumerable<BorrowingVisibilityDto>> GetBorrowingVisibility()
    {
        var borrowedReservations = await _context.Reservations
            .Include(r => r.BookCopy)
                .ThenInclude(bc => bc.Book)
            .Include(r => r.Customer)
            .Where(r => r.Status == ReservationStatus.Borrowed)
            .ToListAsync();

        var groupedByBook = borrowedReservations
            .GroupBy(r => new { r.BookCopy.Book.Id, r.BookCopy.Book.Title })
            .Select(g => new BorrowingVisibilityDto
            {
                BookId = g.Key.Id,
                BookTitle = g.Key.Title,
                CurrentBorrowers = g.Select(r => new CurrentBorrowerDto
                {
                    CustomerId = r.CustomerId,
                    CustomerName = r.Customer.Name,
                    CustomerEmail = r.Customer.Email,
                    CopyNumber = r.BookCopy.CopyNumber,
                    BorrowedAt = r.BorrowedAt ?? r.ReservedAt,
                    DueDate = r.DueDate
                }).ToList()
            })
            .ToList();

        return groupedByBook;
    }
}
