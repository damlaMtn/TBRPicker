# 📚 TBRPicker

> **Work in progress** — actively being developed.

A random book picker that reads your Goodreads TBR (To Be Read) list and suggests what to read next. Built with ASP.NET Core.

---

## What it does

If you're anything like me, your TBR list is out of control and choosing the next book is somehow harder than reading it. TBRPicker solves that by picking one for you. Randomly, or filtered by page count or genre.

---

## Features

- ✅ Imports your Goodreads library export (CSV)
- ✅ Filters for your TBR shelf automatically
- ✅ Stores your books in a local SQLite database
- ✅ Returns a random book from your list
- ✅ Filter by maximum page count
- ✅ Filter by genre
- ✅ Simple web frontend

---

## Tech Stack

- [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/) — backend API
- [CsvHelper](https://joshclose.github.io/CsvHelper/) — Goodreads CSV parsing
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/) + SQLite — data persistence

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022 or VS Code

### Run locally

1. Clone the repository
   ```bash
   git clone https://github.com/damlaMtn/TBRPicker.git
   cd TBRPicker
   ```

2. Export your Goodreads library
   - Go to Goodreads → Account → Settings → scroll down to **Export Library**
   - Download the CSV file

3. Update the CSV path in `BookService.cs`
   ```csharp
   private readonly string _csvPath = @"C:\your\path\to\goodreads_library_export.csv";
   ```

4. Run the project
   ```bash
   dotnet run
   ```

5. Open Swagger UI at `https://localhost:{port}/swagger`

6. Call `POST /api/book/import` once to import your books into the database

---

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/book/tbr` | Returns your full TBR list |
| GET | `/api/book/random` | Returns a random book from your TBR |
| GET | `/api/book/random?maxPages=300` | Random book under 300 pages |
| GET | `/api/book/random?genre=fantasy` | Random book by genre |
| GET | `/api/book/random?maxPages=300&genre=fantasy` | Combine filters |
| POST | `/api/book/import` | Imports your Goodreads CSV into the database |

---

## Project Status

This project is being built as a learning exercise in ASP.NET Core backend development. It is a work in progress and will be updated regularly.

---

## License

[MIT](LICENSE) — feel free to use, fork, or adapt.
