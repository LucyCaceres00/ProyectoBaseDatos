let todos = [];
let idTabla;
let nombreTabla;

function getListado() {
    const isSql = (localStorage.getItem('isSql') === 'true');
    if (isSql) {
        getTablasSQL();
    }
    else {
        getTablasMySQL();
    }
}

function getTablasSQL() {
    fetch(`/api/Tabla/getTablasSql`)
        .then(response => response.json())
        .then(data => mostrarTablas(data))
        .catch(error => console.error("No Se Logro Cargar Datos", error));
}

function getTablasMySQL() {
    fetch(`/api/Tabla/getTablasMySql`)
        .then(response => response.json())
        .then(data => mostrarTablas(data))
        .catch(error => console.error("No Se Logro Cargar Datos", error));
}

function mostrarTablas(data) {
    const tBody = document.getElementById('listadoTablas');
    tBody.innerHTML = '';

    const button = document.createElement('button');
    data.forEach(item => {

        let editButton = button.cloneNode(false);
        editButton.innerText = 'Editar';
        editButton.setAttribute('onclick', `editTabla('${item.nombre}')`);

        let vaciarButton = button.cloneNode(false);
        vaciarButton.innerText = 'Vaciar';
        vaciarButton.setAttribute('onclick', `vaciarModal('${item.nombre}')`);

        let deleteButton = button.cloneNode(false);
        deleteButton.innerText = 'Eliminar';
        deleteButton.setAttribute('onclick', `eliminarModal('${item.nombre}')`);

        let tr = tBody.insertRow();

        let td5 = tr.insertCell(0);
        let txtId = document.createTextNode(item.Id);
        td5.appendChild(txtId);

        let td0 = tr.insertCell(1);
        let txtNombre = document.createTextNode(item.nombre);
        td0.appendChild(txtNombre);

        let td1 = tr.insertCell(2);
        let txtFechaCrea = document.createTextNode(new Date(item.fechaCreacion).toLocaleDateString('es-ES', {
            day: '2-digit',
            month: '2-digit',
            year: '2-digit'
        }));
        td1.appendChild(txtFechaCrea);

        let td2 = tr.insertCell(3);
        td2.appendChild(editButton);

        let td3 = tr.insertCell(4);
        td3.appendChild(vaciarButton);

        let td4 = tr.insertCell(5);
        td4.appendChild(deleteButton);
    });
    todos = data;
}

function registrarTabla() {
    const isSql = (localStorage.getItem('isSql') === 'true');
    if (isSql) {
        registrarTablaSQL();
    }
    else {
        registrarTablaMySQL();
    }
}

function registrarTablaMySQL() {
    const nombre = document.getElementById('tableName').value.trim();

    // Validar que el nombre no esté vacío
    if (!nombre) {
        mostrarModalError('El nombre de la tabla es requerido.');
        return;
    }

    fetch(`/api/Tabla/createTableSql/${nombre}`, {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    })
        .then(response => {
            return response.json().then(data => {
                if (!response.ok) {
                    mostrarModalError(data.message || 'Error al crear la tabla');
                } else {
                    mostrarModalExito(data.message || 'La tabla se creó correctamente.');
                }
                return data;
            });
        })
        .then(() => getListado())
        .then(() => limpiarCampo())
        .catch(error => mostrarModalError(error));
}


function registrarTablaSQL() {
    const nombre = document.getElementById('tableName').value.trim();

    fetch(`/api/Tabla/createTableSql/${nombre}`, {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    }).then(response => {
        return response.json().then(data => {
            if (!response.ok) {
                mostrarModalError(data.message || 'Error al crear la tabla');
            }
            else {
                mostrarModalExito(data.message || 'La tabla se creó correctamente.');
            }
            return data;
        });
    })
        .then(() => getListado())
        .then(() => limpiarCampo())
        .catch(error => mostrarModalError(error));
}

function limpiarCampo() {
    document.getElementById('tableName').value = "";
    document.getElementById('btnVaciar').hidden = true;
    document.getElementById('btnElimar').hidden = true;
}

function editarTabla() {
    const isSql = (localStorage.getItem('isSql') === 'true');
    if (isSql) {
        updateTablaSQL();
    }
    else {
        updateTablaMySQL();
    }
}


function updateTablaSQL() {
    const nombre = document.getElementById('tableName').value.trim();

    fetch(`/api/Tabla/createTableSql/${idTabla}/${nombre}`, {
        method: 'PUT',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    }).then(response => {
        return response.json().then(data => {
            if (!response.ok) {
                mostrarModalError(data.message || 'Error al cambiar el nombre de la tabla');
            }
            else {
                mostrarModalExito(data.message || 'Se actualizo  el nombre de la tabla correctamente.');
            }
            return data;
        });
    })
        .then(() => getListado())
        .then(() => limpiarCampo())
        .catch(error => mostrarModalError(error));
    closeInput();
    return false;
}

function updateTablaSQL() {
    const idTabla = document.getElementById('tableId').value.trim(); 
    const nombre = document.getElementById('tableName').value.trim();

    if (!idTabla || !nombre) {
        mostrarModalError('El ID de la tabla y el nombre son requeridos.');
        return;
    }

    fetch(`/api/Tabla/updateTableSql/${idTabla}/${nombre}`, { 
        method: 'PUT',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    })
        .then(response => {
            return response.json().then(data => {
                if (!response.ok) {
                    mostrarModalError(data.message || 'Error al cambiar el nombre de la tabla.');
                } else {
                    mostrarModalExito(data.message || 'Se actualizó el nombre de la tabla correctamente.');
                }
                return data;
            });
        })
        .then(() => getListado()) // Actualizar el listado de tablas
        .then(() => limpiarCampo()) // Limpiar el campo de entrada
        .catch(error => mostrarModalError(`Error: ${error.message}`)); // Manejar errores

    closeInput(); // Cerrar el formulario de entrada (si aplica)
    return false; // Evitar recargar la página
}


function editTabla(nombre) {
    document.getElementById('editBotton').style.display = 'block';
    document.getElementById('addBotton').style.display = 'none';

    const item = todos.find(item => item.nombre === nombre);
    document.getElementById('tableName').value = item.nombre;
    idTabla = item.Id;
}

function vaciarTabla() {
    const isSql = (localStorage.getItem('isSql') === 'true');
    if (isSql) {
        vaciarTablaSQL();
    }
    else {
        vaciarTablaMySQL();
    }
}

function vaciarTablaSQL() {
    fetch(`/api/Tabla/vaciarTablaSQL/${nombreTabla}`, {
        method: 'PUT',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    }).then(response => {
        return response.json().then(data => {
            if (!response.ok) {
                mostrarModalError(data.message || 'Error al vaciar los datos de la tabla.');
            }
            else {
                mostrarModalExito(data.message || 'Se eliminaron los datos de la tabla correctamente.');
            }
            return data;
        });
    })
        .then(() => getListado())
        .then(() => limpiarCampo())
        .catch(error => mostrarModalError(error));
    closeInput();
    return false;
}

function vaciarTablaMySQL(nombre) {
    fetch(`/api/Tabla/vaciarTablaSQL/${nombre}`, {
        method: 'PUT',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    }).then(response => {
        return response.json().then(data => {
            if (!response.ok) {
                mostrarModalError(data.message || 'Error al cambiar el nombre de la tabla');
            }
            mostrarModalExito(data.message || 'La tabla se creó correctamente.');
            return data;
        });
    })
        .then(() => getListado())
        .then(() => limpiarCampo())
        .catch(error => mostrarModalError(error));
    closeInput();
    return false;
}

function mostrarModalError(mensaje) {
    const errorModal = document.getElementById('errorModal');
    var mensajeModal = document.getElementById("mensajeModal");
    mensajeModal.textContent = mensaje;
    $(errorModal).modal('show');
}

function mostrarModalExito(mensaje) {
    const errorModal = document.getElementById('modalExito')
    var mensajeModal = document.getElementById("modalExitoMessage");
    mensajeModal.textContent = mensaje;
    $(errorModal).modal('show');
}

function vaciarModal(nombre) {
    nombreTabla = nombre;
    document.getElementById('btnElimar').hidden = true;
    document.getElementById('btnVaciar').hidden = false;
    const modal = document.getElementById('confirmacionModal')
    var mensajeModal = document.getElementById("msjModalConfirm");
    mensajeModal.textContent = `Se llevará a cabo un proceso de eliminación de todos los registros de la tabla '${nombre}'. Esta acción es irreversible ¿Desea continuar?`;
    $(modal).modal('show');
}

function eliminarModal(nombre) {
    nombreTabla = nombre;
    document.getElementById('btnElimar').hidden = false;
    document.getElementById('btnVaciar').hidden = true;
    const modal = document.getElementById('confirmacionModal')
    var mensajeModal = document.getElementById("msjModalConfirm");
    mensajeModal.textContent = `Se llevará a cabo un proceso de eliminación de la tabla '${nombre}'. Esta acción es irreversible ¿Desea continuar?`;
    $(modal).modal('show');
}

function closeInput() {
    limpiarCampo();
    document.getElementById('editBotton').style.display = 'none';
    document.getElementById('addBotton').style.display = 'block';
}

function eliminarTabla() {
    const isSql = (localStorage.getItem('isSql') === 'true');
    if (isSql) {
        eliminarTablaSQL();
    }
    else {
        eliminarTablaMySQL();
    }
}

function eliminarTablaSQL() {
    fetch(`/api/Tabla/eliminarTableSql/${nombreTabla}`, {
        method: 'PUT',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    }).then(response => {
        return response.json().then(data => {
            if (!response.ok) {
                mostrarModalError(data.message || 'Error al eliminar la tabla.');
            }
            else {
                mostrarModalExito(data.message || 'Se elimino la tabla correctamente.');
            }
            return data;
        });
    })
        .then(() => getListado())
        .then(() => limpiarCampo())
        .catch(error => mostrarModalError(error));
    closeInput();
    return false;
}

function eliminarTablaMySQL() {
    fetch(`/api/Tabla/vaciarTablaSQL/${nombreTabla}`, {
        method: 'PUT',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    }).then(response => {
        return response.json().then(data => {
            if (!response.ok) {
                mostrarModalError(data.message || 'Error al vaciar los datos de la tabla.');
            }
            mostrarModalExito(data.message || 'Se vacio la tabla correctamente.');
            return data;
        });
    })
        .then(() => getListado())
        .then(() => limpiarCampo())
        .catch(error => mostrarModalError(error));
    closeInput();
    return false;
}
