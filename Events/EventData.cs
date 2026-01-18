namespace LibraryCoreApi.Events;

public class BookCreatedEvent
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public int CategoryId { get; set; }
    public string? Publisher { get; set; }
    public int TotalCopies { get; set; }
}

public class BookEvent
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int CategoryId { get; set; }
}

public class CategoryEvent
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class PartyEvent
{
    public int PartyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

public class ReservationEvent
{
    public int ReservationId { get; set; }
    public int BookCopyId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string CopyNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
}

public class RoleEvent
{
    public int RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}