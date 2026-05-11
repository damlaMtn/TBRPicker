async function pickBook() {
    const maxPages = document.getElementById('maxPages').value;
    const genre = document.getElementById('genre').value;

    // Get checked shelves
    const checkedShelves = [...document.querySelectorAll('#shelfCheckboxes input:checked')]
        .map(cb => cb.value);

    let url = '/api/book/random';
    const params = new URLSearchParams();
    if (maxPages) params.append('maxPages', maxPages);
    if (genre) params.append('genre', genre);
    if (checkedShelves.length > 0) params.append('shelf', checkedShelves.join(','));
    if ([...params].length > 0) url += '?' + params.toString();

    try {
        const response = await fetch(url);

        if (response.status === 404) {
            document.getElementById('result').style.display = 'none';
            document.getElementById('noResult').style.display = 'block';
            return;
        }

        const book = await response.json();
        document.getElementById('bookTitle').textContent = book.title;
        document.getElementById('bookAuthor').textContent = book.author;
        document.getElementById('bookPages').textContent = book.pageCount ? `${book.pageCount} pages` : '';
        document.getElementById('result').style.display = 'block';
        document.getElementById('noResult').style.display = 'none';

    } catch (error) {
        console.error('Error:', error);
    }
}

async function uploadCSV() {
    const fileInput = document.getElementById('csvFile');
    const message = document.getElementById('importMessage');

    if (!fileInput.files[0]) {
        message.style.display = 'block';
        message.style.color = '#888';
        message.textContent = 'Please select a CSV file first.';
        return;
    }

    const formData = new FormData();
    formData.append('file', fileInput.files[0]);

    try {
        message.style.display = 'block';
        message.style.color = '#888';
        message.textContent = 'Importing...';

        const response = await fetch('/api/book/upload', {
            method: 'POST',
            body: formData
        });

        const text = await response.text();
        message.style.color = response.ok ? '#2E75B6' : '#dc3545';
        message.textContent = text;

        if (response.ok) await loadShelves();

    } catch (error) {
        message.style.color = '#dc3545';
        message.textContent = 'Something went wrong. Please try again.';
    }
}

async function loadShelves() {
    const response = await fetch('/api/book/shelves');
    const shelves = await response.json();

    const container = document.getElementById('shelfCheckboxes');
    container.innerHTML = '';

    shelves.forEach(shelf => {
        const label = document.createElement('label');
        label.className = 'badge border';
        label.style.cssText = 'cursor:pointer; font-size:0.85rem; padding: 8px 12px; color: #2E75B6; border-color: #2E75B6 !important; font-weight: normal;';

        const checkbox = document.createElement('input');
        checkbox.type = 'checkbox';
        checkbox.value = shelf;
        checkbox.style.marginRight = '6px';

        checkbox.addEventListener('change', () => {
            updateBookCount();
            if (bookListOpen) {
                currentPage = 1;
                loadBookList();
            }
        });

        // Check to-read by default if it exists
        if (shelf === 'to-read') checkbox.checked = true;

        label.appendChild(checkbox);
        label.appendChild(document.createTextNode(shelf));
        container.appendChild(label);
    });

    document.getElementById('shelfSection').style.display = 'block';

    updateBookCount();
}

async function syncCSV() {
    const fileInput = document.getElementById('csvFile');
    const message = document.getElementById('importMessage');

    if (!fileInput.files[0]) {
        message.style.display = 'block';
        message.style.color = '#888';
        message.textContent = 'Please select a CSV file first.';
        return;
    }

    const formData = new FormData();
    formData.append('file', fileInput.files[0]);

    try {
        message.style.display = 'block';
        message.style.color = '#888';
        message.textContent = 'Syncing...';

        const response = await fetch('/api/book/sync', {
            method: 'POST',
            body: formData
        });

        const text = await response.text();
        message.style.color = response.ok ? '#2E75B6' : '#dc3545';
        message.textContent = text;

        if (response.ok) await loadShelves();

    } catch (error) {
        message.style.color = '#dc3545';
        message.textContent = 'Something went wrong. Please try again.';
    }
}

let currentPage = 1;
let totalBooks = 0;
const pageSize = 20;
let bookListOpen = false;
let searchTimeout = null;

function toggleBookList() {
    bookListOpen = !bookListOpen;
    document.getElementById('bookListSection').style.display = bookListOpen ? 'block' : 'none';
    document.getElementById('toggleIcon').textContent = bookListOpen ? '▲' : '▼';
    if (bookListOpen) loadBookList();
}

function searchBooks() {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
        currentPage = 1;
        loadBookList();
    }, 300);
}

function changePage(direction) {
    const maxPage = Math.ceil(totalBooks / pageSize);
    currentPage = Math.max(1, Math.min(currentPage + direction, maxPage));
    loadBookList();
}

async function loadBookList() {
    const search = document.getElementById('bookSearch')?.value || '';
    const checkedShelves = [...document.querySelectorAll('#shelfCheckboxes input:checked')]
        .map(cb => cb.value);

    const params = new URLSearchParams();
    params.append('page', currentPage);
    params.append('pageSize', pageSize);
    if (search) params.append('search', search);
    if (checkedShelves.length > 0) params.append('shelf', checkedShelves.join(','));

    const response = await fetch('/api/book/list?' + params.toString());
    const data = await response.json();

    totalBooks = data.total;

    document.getElementById('bookCount').textContent = `(${totalBooks})`;

    const tbody = document.getElementById('bookTableBody');
    tbody.innerHTML = '';

    if (data.books.length === 0) {
        tbody.innerHTML = '<tr><td colspan="4" class="text-center text-muted">No books found.</td></tr>';
    } else {
        data.books.forEach(book => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>${book.title}</td>
                <td>${book.author}</td>
                <td>${book.pageCount ?? '—'}</td>
                <td>${book.genre ?? '—'}</td>
            `;
            tbody.appendChild(tr);
        });
    }

    document.getElementById('pageInfo').textContent =
        `Page ${currentPage} of ${Math.ceil(totalBooks / pageSize)}`;
    document.getElementById('prevBtn').disabled = currentPage === 1;
    document.getElementById('nextBtn').disabled = currentPage >= Math.ceil(totalBooks / pageSize);
}

async function updateBookCount() {
    const checkedShelves = [...document.querySelectorAll('#shelfCheckboxes input:checked')]
        .map(cb => cb.value);

    const params = new URLSearchParams();
    params.append('page', 1);
    params.append('pageSize', 1);
    if (checkedShelves.length > 0) params.append('shelf', checkedShelves.join(','));

    const response = await fetch('/api/book/list?' + params.toString());
    const data = await response.json();

    document.getElementById('bookCount').textContent = `(${data.total})`;
}