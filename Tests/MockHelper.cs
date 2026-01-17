using Microsoft.EntityFrameworkCore;
using Moq;
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
        internal const string RoleName = "Author";
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
                Name = RoleName, 
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
                CreatedAt = DateTime.UtcNow,
                PartyRoles = new List<PartyRole> { GetMockPartyRole() }
            };
        }

        internal static Role GetMockRole()
        {
            return new Role 
            { 
                Id = RoleId, 
                Name = RoleName, 
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

        internal static Mock<DbSet<T>> GetQueryableMockDbSet<T>(params T[] sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();

            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            return dbSet;
        }
    }
}

// internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
// {
//     private readonly IQueryProvider _inner;

//     internal TestAsyncQueryProvider(IQueryProvider inner)
//     {
//         _inner = inner;
//     }

//     public IQueryable CreateQuery(Expression expression)
//     {
//         return new TestAsyncEnumerable<TEntity>(expression);
//     }

//     public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
//     {
//         return new TestAsyncEnumerable<TElement>(expression);
//     }

//     public object Execute(Expression expression)
//     {
//         return _inner.Execute(expression);
//     }

//     public TResult Execute<TResult>(Expression expression)
//     {
//         return _inner.Execute<TResult>(expression);
//     }

//     public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
//     {
//         return new TestAsyncEnumerable<TResult>(expression);
//     }

//     public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
//     {
//         return Task.FromResult(Execute<TResult>(expression));
//     }
// }

// // Async enumerable for unit testing
// internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
// {
//     public TestAsyncEnumerable(IEnumerable<T> enumerable)
//         : base(enumerable)
//     { }

//     public TestAsyncEnumerable(Expression expression)
//         : base(expression)
//     { }

//     public IAsyncEnumerator<T> GetEnumerator()
//     {
//         return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
//     }

//     IQueryProvider IQueryable.Provider
//     {
//         get { return new TestAsyncQueryProvider<T>(this); }
//     }
// }

// // Async enumerator for unit testing
// internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
// {
//     private readonly IEnumerator<T> _inner;

//     public TestAsyncEnumerator(IEnumerator<T> inner)
//     {
//         _inner = inner;
//     }

//     public void Dispose()
//     {
//         _inner.Dispose();
//     }

//     public T Current
//     {
//         get
//         {
//             return _inner.Current;
//         }
//     }

//     public Task<bool> MoveNext(CancellationToken cancellationToken)
//     {
//         return Task.FromResult(_inner.MoveNext());
//     }
// }