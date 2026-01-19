using LibraryCoreApi.Entities;
using LibraryCoreApi.DTOs;

namespace LibraryCoreApi.Tests
{
	internal static class MockHelper
	{
        internal const int BookId = 1;
        internal const string Title = "The Great Gatsby";
        internal const string AuthorName = "F. Scott Fitzgerald";
        internal const string CategoryName = "Fiction"; 
        internal const string ISBN = "9780743273565";
        internal const string Description = "A classic novel about the Jazz Age";
        internal const int AuthorId = 1;
        internal const int CategoryId = 1;
        internal const int TotalCopies = 10;
        internal const int AvailableCopies = 10;
        internal static DateTime PublishedDate = new(1925, 4, 10);

        internal const int PartyId = 1;
        internal const string Name = "John Doe";
        internal const string Email = "john.doe@example.com";
        internal const string Phone = "1234567890";
        internal const string Address = "123 Main St, Anytown, USA";

        internal const int RoleId = 1;
        internal const string AuthorRoleName = "Author";
        internal const string CustomerRoleName = "Customer";
        internal const string RoleDescription = "A person who writes books";

        internal const int ReservationId = 1;
        internal const int BookCopyId = 1;
        internal const string BookTitle = "The Great Gatsby";
        internal const string CopyNumber = "1234567890";
        internal const int CustomerId = 1;
        internal const string CustomerName = "John Doe";
        internal static DateTime ReservedAt = new(2026, 1, 1, 12, 0, 0);
        internal static DateTime BorrowedAt = new(2026, 1, 2, 12, 0, 0);
        internal static DateTime ReturnedAt = new(2026, 1, 3, 12, 0, 0);
        internal static DateTime DueDate = new(2026, 1, 4, 12, 0, 0);
        internal static string Status = "Reserved";

        internal const string CustomerEmail = "john.doe@example.com";

        internal static BookDto GetMockBookDto()
        {
            return new BookDto 
            { 
                Id = BookId, 
                Title = Title, 
                AuthorId = AuthorId, 
                CategoryId = CategoryId, 
                ISBN = ISBN, 
                Description = Description, 
                PublishedDate = PublishedDate,
                AuthorName = AuthorName,
                CategoryName = CategoryName,
                TotalCopies = TotalCopies,
                AvailableCopies = AvailableCopies,
                CreatedAt = DateTime.UtcNow
            };
        }

        internal static PartyDto GetMockPartyDto()
        {
            return new PartyDto 
            { 
                Id = PartyId, 
                Name = Name,
                Email = Email,
                Phone = Phone,
                Address = Address,
                Roles = new List<string> { "Author", "Customer" },
                CreatedAt = DateTime.UtcNow
            };
        }

        internal static RoleDto GetMockRoleDto()
        {
            return new RoleDto 
            { 
                Id = RoleId, 
                Name = AuthorRoleName, 
                Description = RoleDescription, 
                CreatedAt = DateTime.UtcNow
            };
        }

        internal static CategoryDto GetMockCategoryDto()
        {
            return new CategoryDto 
            { 
                Id = CategoryId, 
                Name = CategoryName, 
                CreatedAt = DateTime.UtcNow
            };
        }

        internal static ReservationDto GetMockReservationDto()
        {
            return new ReservationDto 
            { 
                Id = ReservationId, 
                BookCopyId = BookCopyId, 
                BookTitle = BookTitle, 
                CopyNumber = CopyNumber, 
                CustomerId = CustomerId, 
                CustomerName = CustomerName, 
                ReservedAt = ReservedAt, 
                BorrowedAt = BorrowedAt, 
                ReturnedAt = ReturnedAt, 
                DueDate = DueDate, 
                Status = Status, 
            };
        }

        internal static BorrowingVisibilityDto GetMockBorrowingVisibilityDto()
        {
            return new BorrowingVisibilityDto 
            { 
                BookId = BookId, 
                BookTitle = BookTitle, 
                CurrentBorrowers = new List<CurrentBorrowerDto> { GetMockCurrentBorrowerDto() }
            };
        }

        internal static CurrentBorrowerDto GetMockCurrentBorrowerDto()
        {
            return new CurrentBorrowerDto 
            { 
                CustomerId = CustomerId, 
                CustomerName = CustomerName, 
                CustomerEmail = CustomerEmail, 
                CopyNumber = CopyNumber, 
                BorrowedAt = BorrowedAt, 
                DueDate = DueDate
            };
        }

        internal static Party GetMockParty()
        {
            return new Party 
            { 
                Id = PartyId, 
                Name = Name, 
                Email = Email, 
                Phone = Phone, 
                Address = Address, 
                CreatedAt = DateTime.UtcNow
            };
        }

        internal static Role GetMockRole()
        {
            return new Role 
            { 
                Id = RoleId, 
                Name = AuthorRoleName, 
                Description = RoleDescription, 
                CreatedAt = DateTime.UtcNow
            };
        }

        internal static PartyRole GetMockPartyRole()
        {
            return new PartyRole 
            { 
                PartyId = PartyId, 
                RoleId = RoleId, 
                AssignedAt = DateTime.UtcNow
            };
        }

        internal static Category GetMockCategory()
        {
            return new Category 
            { 
                Id = CategoryId, 
                Name = CategoryName, 
                CreatedAt = DateTime.UtcNow
            };
        }

        internal static Book GetMockBook()
        {
            return new Book 
            { 
                Id = BookId, 
                Title = Title, 
                AuthorId = AuthorId, 
                CategoryId = CategoryId, 
                ISBN = ISBN, 
                Description = Description, 
                PublishedDate = PublishedDate, 
                CreatedAt = DateTime.UtcNow
            };
        }

        internal static BookCopy GetMockBookCopy()
        {
            return new BookCopy 
            { 
                Id = BookCopyId, 
                BookId = BookId, 
                CopyNumber = CopyNumber, 
                IsAvailable = true, 
                CreatedAt = DateTime.UtcNow
            };
        }

        internal static Reservation GetMockReservation()
        {
            return new Reservation 
            { 
                Id = ReservationId, 
                BookCopyId = BookCopyId, 
                CustomerId = CustomerId, 
                ReservedAt = ReservedAt, 
                BorrowedAt = BorrowedAt, 
                ReturnedAt = ReturnedAt, 
                DueDate = DueDate, 
                Status = ReservationStatus.Reserved,
            };
        }
    }
}

