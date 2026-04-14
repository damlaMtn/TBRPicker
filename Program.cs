using CsvHelper;
using System.Globalization;
using TBRPicker.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//*******//

var csvPath = @"PATH";

using var reader = new StreamReader(csvPath);
using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

var books = csv.GetRecords<GoodreadsBook>()
               .Where(b => b.Bookshelf == "to-read")
               .ToList();

int count = 0;

foreach (var book in books)
{
    Console.WriteLine(book.Title);
    count++;
}

Console.WriteLine(count);

//*******//



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
