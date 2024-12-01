let todos = [];
let nombreColumna;
let nombreTabla;
let nombreTabla2;
let relacion;

function cargarListas() {
    listaTablas1();
    listaTablas2();
}

function listaTablas1() {
    const isSql = (localStorage.getItem('isSql') === 'true');
    if (isSql) {
        fetch('/api/Tabla/getTablasSql')
            .then(response => response.json())
            .then(data => {
                const dropdown = document.getElementById('listaTablas1');
                data.forEach(doc => {
                    const option = document.createElement('option');
                    option.value = doc.nombre;
                    option.textContent = doc.nombre;
                    dropdown.appendChild(option);
                });
            })
            .then(() => listaCampos1())
            .then(() => listadoRelaciones())
            .catch(error => console.error('Error al obtener las tablas.', error));
    }
    else {
        //getTablasMySQL();
    }
}

function listaTablas2() {
    const isSql = (localStorage.getItem('isSql') === 'true');
    if (isSql) {
        fetch('/api/Tabla/getTablasSql')
            .then(response => response.json())
            .then(data => {
                const dropdown = document.getElementById('listaTablas2');
                data.forEach(doc => {
                    const option = document.createElement('option');
                    option.value = doc.nombre;
                    option.textContent = doc.nombre;
                    dropdown.appendChild(option);
                });
            })
            .then(() => listaCampos2())
            .catch(error => console.error('Error al obtener las tablas.', error));
    }
    else {
        //getTablasMySQL();
    }
}

function verInput() {
    if (document.getElementById('muchosMuchos').value == "true") {
        document.getElementById('tablaIntermedia').hidden = false;
    }
    else {
        document.getElementById('tablaIntermedia').hidden = true;
    }
}

function listaCampos1() {
    nombreTabla = document.getElementById('listaTablas1').value;
    const isSql = (localStorage.getItem('isSql') === 'true');
    if (isSql) {
        fetch(`/api/listadoCampos/${nombreTabla}`)
            .then(response => response.json())
            .then(data => {
                const dropdown = document.getElementById('listaCampos1');
                dropdown.innerHTML = '';
                data.forEach(doc => {
                    const option = document.createElement('option');
                    option.value = doc.nombre;
                    option.textContent = doc.nombre;
                    dropdown.appendChild(option);
                });
            })
            .then(() => listadoRelaciones())
            .catch(error => console.error('Error al obtener las tablas.', error));
    }
    else {
        //getTablasMySQL();
    }
}

function listaCampos2() {
    nombreTabla2 = document.getElementById('listaTablas2').value;
    const isSql = (localStorage.getItem('isSql') === 'true');
    if (isSql) {
        fetch(`/api/listadoCampos/${nombreTabla2}`)
            .then(response => response.json())
            .then(data => {
                const dropdown = document.getElementById('listaCampos2');
                dropdown.innerHTML = '';
                data.forEach(doc => {
                    const option = document.createElement('option');
                    option.value = doc.nombre;
                    option.textContent = doc.nombre;
                    dropdown.appendChild(option);
                });
            })
            .catch(error => console.error('Error al obtener las tablas.', error));
    }
    else {
        //getTablasMySQL();
    }
}

function listadoRelaciones() {
    nombreTabla = document.getElementById('listaTablas1').value;
    const isSql = (localStorage.getItem('isSql') === 'true');
    if (isSql) {
        getRelacionesSql();
    }
    else {
        //getCamposTablaMySQL();
    }
}

function getRelacionesSql() {
    fetch(`/api/getRelacionesSql/${nombreTabla}`)
        .then(response => response.json())
        .then(data => mostrarCampos(data))
        .catch(error => console.error("No Se Logro Cargar Datos", error));

}

function mostrarCampos(data) {
    const tBody = document.getElementById('detalleTabla');
    tBody.innerHTML = '';

    const button = document.createElement('button');
    data.forEach(item => {
        let deleteButton = button.cloneNode(false);
        deleteButton.innerText = 'Eliminar';
        deleteButton.setAttribute('onclick', `eliminarModal('${item.NombreRelacion}')`);

        let tr = tBody.insertRow();

        let td0 = tr.insertCell(0);
        let txtNombre = document.createTextNode(item.NombreRelacion);
        td0.appendChild(txtNombre);

        let td1 = tr.insertCell(1);
        let txtTabla1 = document.createTextNode(item.Tabla1);
        td1.appendChild(txtTabla1);

        let td2 = tr.insertCell(2);
        let txtCampo1 = document.createTextNode(item.Campo1);
        td2.appendChild(txtCampo1);

        let td3 = tr.insertCell(3);
        let txtTabla2 = document.createTextNode(item.Tabla2);
        td3.appendChild(txtTabla2);

        let td4 = tr.insertCell(4);
        let txtCampo2 = document.createTextNode(item.Campo2);
        td4.appendChild(txtCampo2);

        let td5 = tr.insertCell(5);
        td5.appendChild(deleteButton);
    });
    todos = data;
}

function eliminarModal(nombre) {
    relacion = nombre;
    const modal = document.getElementById('confirmacionModal')
    var mensajeModal = document.getElementById("msjModalConfirm");
    mensajeModal.textContent = `Se llevará a cabo un proceso de eliminación de la relación '${nombre}'. Esta acción es irreversible ¿Desea continuar?`;
    $(modal).modal('show');
}

function eliminarRelacion() {
    const isSql = (localStorage.getItem('isSql') === 'true');

    if (isSql) {
        eliminarRelacionSql();
    }
    else {
        //getCamposTablaMySQL();
    }
}

function eliminarRelacionSql() {
    fetch(`/api/eliminarRelacion/${nombreTabla}/${relacion}`, {
        method: 'PUT',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    }).then(response => {
        return response.json().then(data => {
            if (!response.ok) {
                mostrarModalError(data.message || 'Error al eliminar la relación.');
            }
            else {
                mostrarModalExito(data.message || 'Se elimino la relación correctamente.');
            }
            return data;
        });
    })
        .then(() => listadoRelaciones())
        .catch(error => mostrarModalError(error));
    closeInput();
    return false;
}

