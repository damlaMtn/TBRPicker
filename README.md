# 📚 TBRPicker

> **Work in progress** — actively being developed.

A random book picker that reads your Goodreads TBR list and suggests what to read next. Upload your Goodreads export, pick your shelves, and let the app decide for you.

---

## What it does

If you're anything like me, your TBR list is out of control and choosing the next book is somehow harder than reading it. TBRPicker solves that by picking one for you — randomly, or filtered by page count, genre, or shelf.

---

## Features

- ✅ Upload your Goodreads library export (CSV) directly from the browser
- ✅ Automatically detects your shelves from your own data
- ✅ Stores your books in a local SQLite database
- ✅ Returns a random book from your selected shelves
- ✅ Filter by maximum page count
- ✅ Filter by genre
- ✅ Filter by one or multiple shelves
- 🔜 Edit book genres from the frontend
- 🔜 Show full book list on the page
- 🔜 AI-powered mood-based picking

---

## Tech Stack

- [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/) — backend API
- [CsvHelper](https://joshclose.github.io/CsvHelper/) — Goodreads CSV parsing
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/) + SQLite — data persistence
- Bootstrap 5 — frontend styling

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

2. Run the project
   ```bash
   dotnet run
   ```

3. Open your browser at `https://localhost:{port}/`

4. Export your Goodreads library
   - Go to Goodreads → Account → Settings → scroll down to **Export Library**
   - Download the CSV file

5. Upload the CSV file from the app's homepage and click **Import**

6. Your shelves will appear automatically — select the ones you want and click **Pick a book for me**!

---

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/book/tbr` | Returns your full book list |
| GET | `/api/book/random` | Returns a random book |
| GET | `/api/book/random?maxPages=300` | Random book under 300 pages |
| GET | `/api/book/random?genre=fantasy` | Random book by genre |
| GET | `/api/book/random?shelf=to-read` | Random book from a specific shelf |
| GET | `/api/book/random?shelf=to-read,books-i-own` | Random book from multiple shelves |
| GET | `/api/book/shelves` | Returns all available shelves |
| POST | `/api/book/upload` | Uploads and imports a Goodreads CSV file |

---

## Project Status

This project is being built as a learning exercise in ASP.NET Core backend development. It is a work in progress and will be updated regularly.

---

## License

[MIT](LICENSE) — feel free to use, fork, or adapt.
