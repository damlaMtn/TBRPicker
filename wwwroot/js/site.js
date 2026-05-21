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

        if (response.ok) {
            await loadShelves();
            await loadBookList();
        }

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
let bookListOpen = true;
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
        <td class="genre-cell" data-id="${book.id}" data-genre="${book.genre ?? ''}">
            <div class="genre-display">
                <span class="genre-text ${!book.genre ? 'text-muted fst-italic' : ''}">
                    ${book.genre || 'No genre'}
                </span>
                <button class="btn btn-link btn-sm p-0 ms-1 edit-genre-btn" title="Edit genre">
                    <i class="bi bi-pencil" style="font-size:11px;"></i>
                </button>
            </div>
            <div class="genre-input-wrap" style="display:none;">
                <input type="text" class="form-control form-control-sm genre-input"
                       value="${book.genre ?? ''}"
                       placeholder="e.g. Fiction, Literary">
                <button class="btn btn-sm btn-success confirm-genre-btn" title="Save">✓</button>
                <button class="btn btn-sm btn-secondary cancel-genre-btn" title="Cancel">✕</button>
            </div>
        </td>
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

// Genre edit handlers
document.addEventListener('click', function (e) {
    if (e.target.closest('.edit-genre-btn')) {
        // Close any other open edit cells first
        document.querySelectorAll('.genre-cell').forEach(c => cancelGenreEdit(c));

        const cell = e.target.closest('.genre-cell');
        cell.querySelector('.genre-display').style.display = 'none';
        const wrap = cell.querySelector('.genre-input-wrap');
        wrap.style.display = 'flex';
        wrap.style.gap = '6px';
        wrap.style.alignItems = 'center';
        wrap.querySelector('.genre-input').focus();
    }

    if (e.target.closest('.confirm-genre-btn')) {
        saveGenre(e.target.closest('.genre-cell'));
    }

    if (e.target.closest('.cancel-genre-btn')) {
        cancelGenreEdit(e.target.closest('.genre-cell'));
    }
});

document.addEventListener('keydown', function (e) {
    if (!e.target.classList.contains('genre-input')) return;
    const cell = e.target.closest('.genre-cell');
    if (e.key === 'Enter') { e.preventDefault(); saveGenre(cell); }
    if (e.key === 'Escape') { cancelGenreEdit(cell); }
});

function cancelGenreEdit(cell) {
    cell.querySelector('.genre-input').value = cell.dataset.genre;
    cell.querySelector('.genre-input-wrap').style.display = 'none';
    cell.querySelector('.genre-display').style.display = 'flex';
    cell.querySelector('.genre-display').style.alignItems = 'center';
    cell.querySelector('.genre-display').style.gap = '6px';
}

async function saveGenre(cell) {
    const id = cell.dataset.id;
    const newGenre = cell.querySelector('.genre-input').value.trim();

    try {
        const response = await fetch(`/api/book/${id}/genre`, {
            method: 'PATCH',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ genre: newGenre })
        });

        if (!response.ok) throw new Error('Save failed');

        cell.dataset.genre = newGenre;
        const textEl = cell.querySelector('.genre-text');
        textEl.textContent = newGenre || 'No genre';
        textEl.className = 'genre-text' + (!newGenre ? ' text-muted fst-italic' : '');

        cancelGenreEdit(cell);
        showToast('Genre saved!', 'success');
    } catch {
        showToast('Failed to save genre.', 'danger');
        cancelGenreEdit(cell);
    }
}

function showToast(message, type) {
    const existing = document.getElementById('grToast');
    if (existing) existing.remove();

    const toast = document.createElement('div');
    toast.id = 'grToast';
    toast.className = `alert alert-${type} position-fixed bottom-0 end-0 m-3 shadow`;
    toast.style.zIndex = '9999';
    toast.style.fontSize = '0.9rem';
    toast.textContent = message;
    document.body.appendChild(toast);
    setTimeout(() => toast.remove(), 2500);
}

// Initialize on page load
(async () => {
    await loadShelves();
    await loadBookList();
    document.getElementById('bookListSection').style.display = 'block';
    document.getElementById('toggleIcon').textContent = '▲';
})();