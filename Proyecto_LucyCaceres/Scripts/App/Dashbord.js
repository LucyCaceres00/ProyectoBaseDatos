function ConexionSeleccionada() {
    var conexion = document.getElementById("conexion");
    const isSql = (localStorage.getItem('isSql') === 'true');
    if (isSql) {
        conexion.textContent = "Esta conectado al servidor de SQL.";
    }
    else {
        conexion.textContent = "Esta conectado al servidor de MySQL.";
    }
}