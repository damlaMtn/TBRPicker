using CsvHelper;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using TBRPicker.Models;
using System.IO;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<TBRPicker.Services.BookService>();

var app = builder.Build();


app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var ex = feature?.Error;

        var pd = new ProblemDetails
        {
            Title = "An unexpected error occurred",
            Status = StatusCodes.Status500InternalServerError
        };

        context.Response.ContentType = "application/problem+json";

        if (ex is FileNotFoundException)
        {
            pd.Title = "Data file not found";
            pd.Detail = "The book export file could not be found.";
            pd.Status = StatusCodes.Status404NotFound;
            context.Response.StatusCode = StatusCodes.Status404NotFound;
        }
        else if (ex is UnauthorizedAccessException)
        {
            pd.Title = "Access denied";
            pd.Detail = "Permission denied while accessing the data file.";
            pd.Status = StatusCodes.Status403Forbidden;
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
        }
        else if (ex is InvalidDataException)
        {
            pd.Title = "Malformed data";
            pd.Detail = "The CSV file is malformed or could not be parsed.";
            pd.Status = StatusCodes.Status400BadRequest;
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
        else
        {
            // In production do not expose ex.Message or stack trace.
            pd.Detail = app.Environment.IsDevelopment() ? ex?.Message : "Internal server error";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }

        await context.Response.WriteAsJsonAsync(pd);
    });
});

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
