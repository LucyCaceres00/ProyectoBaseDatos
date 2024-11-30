﻿let todos = [];
let listadoTablas = [];
let idTabla;
let nombreColumna;
let nombreTabla;

function getListadoCampos() {
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

function listaTablas() {
    const isSql = (localStorage.getItem('isSql') === 'true');
    if (isSql) {
        fetch('/api/Tabla/getTablasSql')
            .then(response => response.json())
            .then(data => {
                const dropdown = document.getElementById('listaTablas');
                data.forEach(doc => {
                    const option = document.createElement('option');
                    option.value = doc.nombre;
                    option.textContent = doc.nombre;
                    dropdown.appendChild(option);
                });
            })
            .then(() => listadoCampos())
            .catch(error => console.error('Error al obtener las tablas.', error));
    }
    else {
        //getTablasMySQL();
    }
}


function listaTipoDatos() {
    const isSql = (localStorage.getItem('isSql') === 'true');
    if (isSql) {
        fetch('/api/campos/getDataType')
            .then(response => response.json())
            .then(data => {
                const dropdown = document.getElementById('listaTipoDatos');
                data.forEach(doc => {
                    const option = document.createElement('option');
                    option.value = doc.tipoDato;
                    option.textContent = doc.tipoDato;
                    dropdown.appendChild(option);
                });
            })
            .catch(error => console.error('Error al obtener los tipos de datos.', error));
    }
    else {
        //getTablasMySQL();
    }
}

function closeInput() {
    document.getElementById('editBotton').style.display = 'none';
    document.getElementById('addBotton').style.display = 'block';
    limpiarCampos();
}

function limpiarCampos() {
    document.getElementById('listaTablas').value = '';
    document.getElementById('nombreCampo').value = '';
    document.getElementById('listaTipoDatos').value = '';
    document.getElementById('isNull').value = '';
    document.getElementById('isPrimaryKey').value = '';
}

function verificarTipoDato() {
    var select = document.getElementById('listaTipoDatos');
    var input = document.getElementById('espeDato');
    var valor = select.value;

    input.readOnly = true;
    input.disabled = true;
    input.placeholder = '';

    if (valor.includes('char')) {
        input.readOnly = false;
        input.placeholder = 'Ejemplo: 2';
    }
    if (valor.includes('decimal')) {
        input.disabled = false;
        input.placeholder = 'Ejemplo: (10,2)';
    }
}

function listadoCampos() {
    nombreTabla = document.getElementById('listaTablas').value;
    const isSql = (localStorage.getItem('isSql') === 'true');
    if (isSql) {
        getCamposTablaSQL();
    }
    else {
        getCamposTablaMySQL();
    }
}

function getCamposTablaSQL() {
    document.getElementById('listaTablas').value = nombreTabla;
    fetch(`/api/listadoCampos/${nombreTabla}`)
        .then(response => response.json())
        .then(data => mostrarCampos(data))
        .catch(error => console.error("No Se Logro Cargar Datos", error));

}

function mostrarCampos(data) {
    const tBody = document.getElementById('detalleTabla');
    tBody.innerHTML = '';

    const button = document.createElement('button');
    data.forEach(item => {

        let editButton = button.cloneNode(false);
        editButton.innerText = 'Editar';
        editButton.setAttribute('onclick', `editTabla('${item.nombre}')`);

        let deleteButton = button.cloneNode(false);
        deleteButton.innerText = 'Eliminar';
        deleteButton.setAttribute('onclick', `eliminarModal('${item.nombre}')`);

        let tr = tBody.insertRow();

        let td0 = tr.insertCell(0);
        let txtnombre = document.createTextNode(item.nombre);
        td0.appendChild(txtnombre);

        let td1 = tr.insertCell(1);
        let txttipoDato = document.createTextNode(item.tipoDato);
        td1.appendChild(txttipoDato);

        let td2 = tr.insertCell(2);
        let txtisNull = document.createTextNode(item.isNull == 1 ? "Sí" : "No");
        td2.appendChild(txtisNull);

        let td3 = tr.insertCell(3);
        let txtisPrimaryKey = document.createTextNode(item.isPrimaryKey == 1 ? "Sí" : "No");
        td3.appendChild(txtisPrimaryKey);

        let td4 = tr.insertCell(4);
        td4.appendChild(editButton);

        let td5 = tr.insertCell(5);
        td5.appendChild(deleteButton);
    });
    todos = data;
}

function eliminarCampo() {
    const isSql = (localStorage.getItem('isSql') === 'true');

    if (isSql) {
        eliminarColumnaSQL();
    }
    else {
        getCamposTablaMySQL();
    }
}

function eliminarColumnaSQL() {
    fetch(`/api/eliminarColumna/${nombreTabla}/${nombreColumna}`, {
        method: 'PUT',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    }).then(response => {
        return response.json().then(data => {
            if (!response.ok) {
                mostrarModalError(data.message || 'Error al eliminar la columna.');
            }
            else {
                mostrarModalExito(data.message || 'Se elimino la columna correctamente.');
            }
            return data;
        });
    })
        .then(() => getCamposTablaSQL())
        .catch(error => mostrarModalError(error));
    closeInput();
    return false;
}

function eliminarModal(nombre) {
    nombreColumna = nombre;
    const modal = document.getElementById('confirmacionModal')
    var mensajeModal = document.getElementById("msjModalConfirm");
    mensajeModal.textContent = `Se llevará a cabo un proceso de eliminación del campo de tabla '${nombre}'. Esta acción es irreversible ¿Desea continuar?`;
    $(modal).modal('show');
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

function agregarCampos() {
    const isSql = (localStorage.getItem('isSql') === 'true');
    if (isSql) {
        registrarCampoSQL();
    }
    else {
        getTablasMySQL();
    }
}

function registrarCampoSQL() {
    const item = {
        nombre: document.getElementById('nombreCampo').value,
        tipoDato: document.getElementById('listaTipoDatos').value,
        especificacion: document.getElementById('espeDato').value,
        isNull: document.getElementById('isNull').value,
        isPrimaryKey: document.getElementById('isPrimaryKey').value
    };

    fetch(`/api/agregarColumna/${nombreTabla}`, {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(item)
    })
        .then(response => {
            return response.json().then(data => {
                if (!response.ok) {
                    mostrarModalError(data.message || 'Error al eliminar la columna.');
                }
                else {
                    mostrarModalExito(data.message || 'Se elimino la columna correctamente.');
                }
                return data;
            });
        })
        .then(() => getCamposTablaSQL())
        .catch(error => mostrarModalError(error));
}
