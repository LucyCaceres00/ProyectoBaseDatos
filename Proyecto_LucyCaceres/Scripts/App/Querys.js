document.getElementById('executeQuery').addEventListener('click', () => {
    const query = document.getElementById('sqlQuery').value;

    // Validate query input
    if (!query.trim()) {
        alert('Please enter a query before executing.');
        return;
    }

    // Execute query via API
    fetch('/api/executeQuery', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ query })
    })
        .then(response => response.json())
        .then(data => {
            const resultsContainer = document.getElementById('queryResults');
            resultsContainer.innerHTML = '';

            // Check if the response contains error
            if (data.error) {
                resultsContainer.innerHTML = `<div class="text-danger">Error: ${data.error}</div>`;
                return;
            }

            // Display results
            if (Array.isArray(data.results)) {
                // Create table for SELECT query results
                const table = document.createElement('table');
                table.classList.add('table', 'table-bordered', 'table-striped');

                // Table headers
                const headers = Object.keys(data.results[0] || {});
                const thead = document.createElement('thead');
                const headerRow = document.createElement('tr');
                headers.forEach(header => {
                    const th = document.createElement('th');
                    th.textContent = header;
                    headerRow.appendChild(th);
                });
                thead.appendChild(headerRow);
                table.appendChild(thead);

                // Table body
                const tbody = document.createElement('tbody');
                data.results.forEach(row => {
                    const tr = document.createElement('tr');
                    headers.forEach(header => {
                        const td = document.createElement('td');
                        td.textContent = row[header] || '';
                        tr.appendChild(td);
                    });
                    tbody.appendChild(tr);
                });
                table.appendChild(tbody);

                resultsContainer.appendChild(table);
            } else {
                // Display simple message for non-SELECT queries
                resultsContainer.innerHTML = `<div class="text-success">${data.message}</div>`;
            }
        })
        .catch(error => {
            document.getElementById('queryResults').innerHTML = `<div class="text-danger">Error: ${error.message}</div>`;
        });
});