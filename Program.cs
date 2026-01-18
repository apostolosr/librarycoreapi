using LibraryCoreApi.Errors;
using LibraryCoreApi.Database;
using LibraryCoreApi.Services.Books;
using LibraryCoreApi.Services.Parties;
using LibraryCoreApi.Services.Categories;
using LibraryCoreApi.Services.Roles;
using LibraryCoreApi.Services.Reservations;
using LibraryCoreApi.Events;


// Check if we should run the database seeder
if (args.Length > 0 && args[0] == "seed")
{
    var hostBuilder = Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddDbContext<DataContext>();
        });

    var host = hostBuilder.Build();
    
    using (var scope = host.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        await DatabaseSeeder.SeedAsync(context);
        Console.WriteLine("Database seeding completed successfully!");
    }
    
    return;
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<DataContext>();

// Register services
builder.Services.AddSingleton<IEventPublisher, RabbitEventPubliser>();
builder.Services.AddScoped<IBooksService, BooksService>();
builder.Services.AddScoped<IPartiesService, PartiesService>();
builder.Services.AddScoped<ICategoriesService, CategoriesService>();
builder.Services.AddScoped<IRolesService, RolesService>();
builder.Services.AddScoped<IReservationsService, ReservationsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<DefaultErrorHandler>();

// TODO: Add authorization middleware for the API for authorizing based on service key

app.Run();
