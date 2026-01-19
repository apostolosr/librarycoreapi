# Library Management System API

A comprehensive Library Management System API built with ASP.NET Core 10.0 and PostgreSQL.

## Features

### Parties & Role Management
- **CRUD Operations** for Parties
- **CRUD Operations** for Roles (Author, Customer)
- A party can belong to multiple roles (e.g., both Author and Customer)

### Book & Category Management
- **CRUD Operations** for Books
- **CRUD Operations** for Categories (Fiction, Mystery, etc.)
- Track book availability by ID or Title
- Support for multiple copies of the same book
- Each book copy can be borrowed by only one Customer at a time

### Reservation & Borrowing Management
- Create reservations for books
- Borrow books (convert reservation to borrowing)
- Return books
- Track borrowing history

### Borrowing Visibility
- Get a list of all book titles with currently borrowed copies
- View which customers have borrowed which books

## Events - Auditing

All actions are published as events to a message broker (RabbitMQ) and a background worker (`RabbitMQEventConsumer`) stores them in MongoDB (event store).

### Event Storage
- Events are automatically consumed from RabbitMQ and persisted to MongoDB
- Events can be retrieved via the Events API endpoints for auditing purposes

### Event Cleanup
- A background worker (`EventCleaner`) periodically cleans old events from the event store
- By default, events older than 1 year are deleted
- Cleanup runs every 24 hours

## API Endpoints

### Parties
- `GET /api/parties` - Get all parties
- `GET /api/parties/{id}` - Get party by ID
- `POST /api/parties` - Create a new party
- `PUT /api/parties/{id}` - Update a party
- `DELETE /api/parties/{id}` - Delete a party

### Roles
- `GET /api/roles` - Get all roles
- `GET /api/roles/{id}` - Get role by ID
- `POST /api/roles` - Create a new role
- `PUT /api/roles/{id}` - Update a role
- `DELETE /api/roles/{id}` - Delete a role

### Categories
- `GET /api/categories` - Get all categories
- `GET /api/categories/{id}` - Get category by ID
- `POST /api/categories` - Create a new category
- `PUT /api/categories/{id}` - Update a category
- `DELETE /api/categories/{id}` - Delete a category

### Books
- `GET /api/books` - Get all books
- `GET /api/books/{id}` - Get book by ID
- `POST /api/books` - Create a new book (on behalf of Author)
- `PUT /api/books/{id}` - Update a book
- `DELETE /api/books/{id}` - Delete a book

### Reservations
- `GET /api/reservations` - Get all reservations
- `GET /api/reservations/{id}` - Get reservation by ID
- `POST /api/reservations` - Create a reservation (on behalf of Customer)
- `POST /api/reservations/borrow` - Borrow a book (convert reservation to borrowing)
- `POST /api/reservations/return` - Return a borrowed book
- `GET /api/reservations/borrowing-visibility` - Get list of books with current borrowers

### Events
- `GET /api/events/book?lastIndex=0&pageSize=100` - Get book (book, category) - related events (with pagination support)
- `GET /api/events/user?lastIndex=0&pageSize=100` - Get user (party / reservation/ role) - related events  (with pagination support)


## Setup Instructions

### Prerequisites
- .NET SDK 10.0
- Docker and Docker Compose

### Local Development with Docker

1. **Start the services:**
   ```bash
    make dev
   ```

2a. **Create and apply database migrations:**
   # In a new terminal
   ```bash
    make migrate 
   ```

2b (optional). **Seed database:**
   # In a new terminal
   ```bash
    make seed 
   ```

3. **Access the API:**
   - API: http://localhost:8080
   - Swagger UI: http://localhost:8080/swagger (Development only)
   - PostgreSQL: localhost:5432 (from host machine, if needed)

## Running Tests

Run tests:

```bash
 make test
```

Tests are located in the `Tests/` directory.


## Database Schema

### Entities
- **Party**: Represents a person (can be Author, Customer, or both)
- **Role**: Defines roles (Author, Customer, but not limited to these only)
- **PartyRole**: Many-to-many relationship between Parties and Roles
- **Category**: Book categories (Fiction, Mystery, etc.)
- **Book**: Book information
- **BookCopy**: Individual copies of a book
- **Reservation**: Tracks book reservations and borrowings

### Key Relationships
- Party - Role: Many-to-Many (via PartyRole)
- Book - Author: Many-to-One (Party with Author role)
- Book - Category: Many-to-One
- Book - BookCopy: One-to-Many
- BookCopy - Reservation: One-to-One (optional)
- Reservation - Customer: Many-to-One (Party with Customer role)

## Example Usage

### 1. Create Roles
```bash
POST /api/roles
{
  "name": "Author",
  "description": "Book author"
}

POST /api/roles
{
  "name": "Customer",
  "description": "Library customer"
}
```

### 2. Create a Party with Roles
```bash
POST /api/parties
{
  "name": "John Doe",
  "email": "john@example.com",
  "roleIds": [1, 2]  // Both Author and Customer
}
```

### 3. Create Categories
```bash
POST /api/categories
{
  "name": "Fiction",
  "description": "Fictional books"
}

POST /api/categories
{
  "name": "Mystery",
  "description": "Mystery novels"
}
```

### 4. Add a Book (on behalf of Author)
```bash
POST /api/books
{
  "title": "The Great Novel",
  "isbn": "978-1234567890",
  "description": "A great novel",
  "authorId": 1,
  "categoryId": 1,
  "publishedDate": "2024-01-01T00:00:00Z",
  "numberOfCopies": 3
}
```

### 5. Create a Reservation (on behalf of Customer)
```bash
POST /api/reservations
{
  "bookId": 1,
  "customerId": 1
}
```

### 6. Borrow a Book
```bash
POST /api/reservations/borrow
{
  "reservationId": 1,
  "dueDate": "2024-12-31T00:00:00Z"
}
```

### 7. Return a Book
```bash
POST /api/reservations/return
{
  "reservationId": 1
}
```

### 8. Get Borrowing Visibility
```bash
GET /api/reservations/borrowing-visibility
```

## Production Deployment (not tested, merely serving as a template)

Use the production Dockerfile and docker-compose file:

```bash
make publish
```

## Background Workers

### RabbitMQEventConsumer
- Consumes events from RabbitMQ message broker
- Automatically stores all events to MongoDB event store
- Runs continuously as a hosted background service
- Processes events asynchronously with manual acknowledgment for reliability

#### Current Consumer Policy
- **QoS Settings**: `prefetchCount: 1` - Processes one message at a time
- **Routing**: Wildcard pattern `#` - Consumes all events from the exchange
- **Acknowledgment**: Manual acknowledgment (`autoAck: false`) - Messages are only removed from queue after successful MongoDB storage
- **Error Handling**: Failed messages are requeued (`requeue: true`) for retry
- **Connection Recovery**: Automatic reconnection enabled with 10-second retry interval

#### Tradeoffs
**Current Approach (Sequential Processing):**
- **Reliability**: High - One message at a time ensures no message loss on failure
- **Data Integrity**: Guaranteed - Each event is persisted before processing the next
- **Error Recovery**: Robust - Failed messages are automatically requeued
- **Memory Efficiency**: Low memory footprint - Only one message in memory at a time
- **Throughput**: Lower - Cannot process multiple events concurrently
- **Latency**: Higher - Events must wait in queue for sequential processing
- **Scalability**: Limited - Single consumer handles all event types

**Performance vs Reliability:**
The current implementation prioritizes **reliability over performance**, which is appropriate for an event store where data integrity is critical. However, this may become a bottleneck under high event volume.

#### Future Performance Optimizations
1. **Parallel Processing**
   - Increase `prefetchCount` to allow multiple unacknowledged messages (e.g., 10-50)
   - Process multiple events concurrently
   - Tradeoff: Higher memory usage, but significantly improved throughput

2. **Multiple Consumers with Routing**
   - Deploy separate consumers for different event types, ex : 
     - `book.*` → BookEventConsumer
     - `party.*` → PartyEventConsumer
     - `reservation.*` → ReservationEventConsumer
   - Each consumer can have optimized QoS settings per event type
   - Enables horizontal scaling and independent scaling per event type

3. **Monitoring & Metrics**
   - Track processing rate, queue depth, and error rates
   - Enable proactive scaling based on metrics

### EventCleaner
- Periodically cleans old events from the MongoDB event store
- Deletes events older than 1 year (365 days)
- Runs cleanup every 24 hours
- Runs as a hosted background service

## Notes

- All timestamps are stored in UTC
- A book copy can only be borrowed by one customer at a time
- Books can only be added for parties with the "Author" role
- Reservations can only be created for parties with the "Customer" role
- The system tracks the full lifecycle: Reserved → Borrowed → Returned