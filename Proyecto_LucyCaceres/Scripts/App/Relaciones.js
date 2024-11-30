let todos = [];
let nombreColumna;
let nombreTabla;
let nombreTabla2;

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
    if (document.getElementById('muchosMuchos').value == "true")
    {
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
        deleteButton.setAttribute('onclick', `eliminarModal('${item.nombre}')`);

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
    document.getElementById('addBotton').style.display = 'block';
    limpiarCampo();
}

function verificarTipoDato() {
    var select = document.getElementById('listaTipoDatos');
    var input = document.getElementById('espeDato');
    var valor = select.value;

    input.disabled = true;
    input.placeholder = '';

    if (valor.includes('char')) {
        input.disabled = false;
        input.placeholder = 'Ejemplo: 2';
    }
    if (valor.includes('decimal')) {
        input.disabled = false;
        input.placeholder = 'Ejemplo: 10,2';
    }
}

function llavePrimaria() {
    if (document.getElementById('primaryKey').value == "true") {
        document.getElementById('isNull').value = "false";
        document.getElementById('isNull').disabled = true;
    }
    else
        document.getElementById('isNull').disabled = false;
}

function editTabla(nombre) {
    nombreColumna = nombre;
    document.getElementById('editBotton').style.display = 'block';
    document.getElementById('addBotton').style.display = 'none';

    const item = todos.find(item => item.nombre === nombre);
    document.getElementById('nombreCampo').value = item.nombre;
    document.getElementById('listaTipoDatos').value = item.tipoDato;
    document.getElementById('primaryKey').value = item.isPrimaryKey == 1 ? "true" : "false";
    document.getElementById('isNull').value = item.isNull;
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
        primaryKey: document.getElementById('primaryKey').value
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
                    mostrarModalError(data.message || 'Hubo un error al intentar crear el campo.');
                }
                else {
                    mostrarModalExito(data.message || 'Se agrego la columna correctamente.');
                }
                return data;
            });
        })
        .then(() => getCamposTablaSQL())
        .then(() => limpiarCampo())
        .catch(error => mostrarModalError(error));
}

function limpiarCampo() {
    document.getElementById('nombreCampo').value = "";
    document.getElementById('espeDato').value = "";
    document.getElementById('isNull').value = "";
    document.getElementById('primaryKey').value = "";
}

function editarCampos() {
    const isSql = (localStorage.getItem('isSql') === 'true');
    if (isSql) {
        editarCampoSQL();
    }
    else {
        //getTablasMySQL();
    }
}

function editarCampoSQL() {
    const item = {
        nombre: document.getElementById('nombreCampo').value,
        tipoDato: document.getElementById('listaTipoDatos').value,
        especificacion: document.getElementById('espeDato').value,
        isNull: document.getElementById('isNull').value,
        primaryKey: document.getElementById('primaryKey').value
    };

    fetch(`/api/editarColumna/${nombreTabla}/${nombreColumna}`, {
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
                    mostrarModalError(data.message || 'Hubo un error al intentar crear el campo.');
                }
                else {
                    mostrarModalExito(data.message || 'Se agrego la columna correctamente.');
                }
                return data;
            });
        })
        .then(() => getCamposTablaSQL())
        .then(() => limpiarCampo())
        .catch(error => mostrarModalError(error));
}