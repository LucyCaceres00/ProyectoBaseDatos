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
                dropdown.innerHTML = '';
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
                dropdown.innerHTML = '';
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

        let td6 = tr.insertCell(0);
        let txtTipoRelacion = document.createTextNode(item.TipoRelacion);
        td6.appendChild(txtTipoRelacion);

        let td0 = tr.insertCell(1);
        let txtNombre = document.createTextNode(item.NombreRelacion);
        td0.appendChild(txtNombre);

        let td1 = tr.insertCell(2);
        let txtTabla1 = document.createTextNode(item.Tabla1);
        td1.appendChild(txtTabla1);

        let td2 = tr.insertCell(3);
        let txtCampo1 = document.createTextNode(item.Campo1);
        td2.appendChild(txtCampo1);

        let td3 = tr.insertCell(4);
        let txtTabla2 = document.createTextNode(item.Tabla2);
        td3.appendChild(txtTabla2);

        let td4 = tr.insertCell(5);
        let txtCampo2 = document.createTextNode(item.Campo2);
        td4.appendChild(txtCampo2);

        let td5 = tr.insertCell(6);
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

function agregarRelaciones() {
    const isSql = (localStorage.getItem('isSql') === 'true');
    if (isSql) {
        agregarRelacionSQL();
    }
    else {
        getTablasMySQL();
    }
}

function agregarRelacionSQL() {
    const item = {
        TablaIntermedia: document.getElementById('nombreIntermedia').value,
        Tabla1: document.getElementById('listaTablas1').value,
        Campo1: document.getElementById('listaCampos1').value,
        Tabla2: document.getElementById('listaTablas2').value,
        Campo2: document.getElementById('listaCampos2').value
    };

    fetch(`/api/agregarRelacion`, {
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
                    mostrarModalError(data.message || 'Hubo un error al intentar crear la relacion.');
                }
                else {
                    mostrarModalExito(data.message || 'Se agrego la relacion correctamente.');
                }
                return data;
            });
        })
        .then(() => listaTablas1())
        .catch(error => mostrarModalError(error));
}