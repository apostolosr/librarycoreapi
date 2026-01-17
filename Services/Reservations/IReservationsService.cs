using LibraryCoreApi.DTOs;

namespace LibraryCoreApi.Services.Reservations;

public interface IReservationsService
{
    Task<IEnumerable<ReservationDto>> GetReservations();
    Task<ReservationDto> GetReservation(int id);
    Task<ReservationDto> CreateReservation(CreateReservationDto createDto);
    Task<ReservationDto> BorrowBook(BorrowBookDto borrowDto);
    Task<ReservationDto> ReturnBook(ReturnBookDto returnDto);
    Task<IEnumerable<BorrowingVisibilityDto>> GetBorrowingVisibility();
}
