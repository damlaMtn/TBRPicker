# 📚 TBRPicker

> **Work in progress** — actively being developed.

A random book picker that reads your Goodreads TBR (To Be Read) list and suggests what to read next. Built with ASP.NET Core.

---

## What it does

If you're anything like me, your TBR list is out of control and choosing the next book is somehow harder than reading it. TBRPicker solves that by picking one for you — randomly, or filtered by page count, mood, or genre.

---

## Features

- ✅ Imports your Goodreads library export (CSV)
- ✅ Filters for your TBR shelf automatically
- ✅ Returns a random book from your list
- 🔜 Filter by page count
- 🔜 Filter by mood / genre
- 🔜 Simple web frontend

---

## Tech Stack

- [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/) — backend API
- [CsvHelper](https://joshclose.github.io/CsvHelper/) — Goodreads CSV parsing
- Entity Framework Core + SQLite — data persistence *(coming soon)*

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022 or VS Code

### Run locally

1. Clone the repository
   ```bash
   git clone https://github.com/yourusername/TBRPicker.git
   cd TBRPicker
   ```

2. Export your Goodreads library
   - Go to Goodreads → Account → Settings → scroll down to **Export Library**
   - Download the CSV file

3. Update the CSV path in `Program.cs`
   ```csharp
   var csvPath = @"C:\your\path\to\goodreads_library_export.csv";
   ```

4. Run the project
   ```bash
   dotnet run
   ```

5. Open Swagger UI at `https://localhost:{port}/swagger`

---

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/books/tbr` | Returns your full TBR list |
| GET | `/api/books/random` | Returns a random book from your TBR |

*More endpoints coming as the project develops.*

---

## Project Status

This project is being built as a learning exercise in ASP.NET Core backend development. It is a work in progress and will be updated regularly.

---

## License

[MIT](LICENSE) — feel free to use, fork, or adapt.
