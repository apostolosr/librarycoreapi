using Microsoft.AspNetCore.Mvc;
using LibraryCoreApi.DTOs;
using LibraryCoreApi.Services.Reservations;

namespace LibraryCoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationsService _reservationsService;

    public ReservationsController(IReservationsService reservationsService)
    {
        _reservationsService = reservationsService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetReservations()
    {
        var reservations = await _reservationsService.GetReservations();
        return Ok(reservations);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReservationDto>> GetReservation(int id)
    {
        var reservation = await _reservationsService.GetReservation(id);
        return Ok(reservation);
    }

    [HttpPost]
    public async Task<ActionResult<ReservationDto>> CreateReservation(CreateReservationDto createDto)
    {
        var reservation = await _reservationsService.CreateReservation(createDto);
        return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
    }

    [HttpPost("borrow")]
    public async Task<ActionResult<ReservationDto>> BorrowBook(BorrowBookDto borrowDto)
    {
        var reservation = await _reservationsService.BorrowBook(borrowDto);
        return Ok(reservation);
    }

    [HttpPost("return")]
    public async Task<ActionResult<ReservationDto>> ReturnBook(ReturnBookDto returnDto)
    {
        var reservation = await _reservationsService.ReturnBook(returnDto);
        return Ok(reservation);
    }

    [HttpGet("borrowing-visibility")]
    public async Task<ActionResult<IEnumerable<BorrowingVisibilityDto>>> GetBorrowingVisibility()
    {
        var visibility = await _reservationsService.GetBorrowingVisibility();
        return Ok(visibility);
    }
}
