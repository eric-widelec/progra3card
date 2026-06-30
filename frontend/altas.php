<?php
// --- CONFIGURACIÓN DE CONEXIÓN ---
$servername = "localhost";
$username = "root";
$password = "root";
$dbname = "mi_banco_db";

// --- DATOS RECIBIDOS DEL FORMULARIO REGISTRO.HTML ---
$tipo_doc = $_POST['tipo_doc'];
$documento = $_POST['documento'];
$usuario = $_POST['usuario'];
$passwordA = $_POST['passwordA'];

// --- INSTANCIA UN OBJETO "$conn" PERTENECIENTE A LA CLASE MYSQLI PROPIA DE PHP ---
$conn = new mysqli($servername, $username, $password, $dbname);

// --- CHECK CONNECTION ---
if ($conn->connect_error) {
  die("Error en la conexión: " . $conn->connect_error);
}

// --- ACTUALIZACIÓN EN BASE DE DATOS ---
$sql = "UPDATE usuarios SET usuario = '".$usuario."', password = '".$passwordA."' 
        WHERE documento = '".$documento."' AND tipo_doc = '".$tipo_doc."'";

if ($conn->query($sql) === TRUE) {
    if ($conn->affected_rows > 0) {
        echo "<script>
                alert('¡Usuario web activado con éxito!');
                window.location.href = 'ingreso.html';
              </script>";
    } else {
        echo "<script>
                alert('Operación denegada: No se encontró ningún cliente registrado con ese documento, o los datos ya estaban configurados.');
                window.location.href = 'registro.html';
              </script>";
    }
} else {
    echo "<script>
            alert('Error actualizando en la tabla: " . $conn->error . "');
            window.location.href = 'registro.html';
          </script>";
}

// --- CIERRE DE CONEXIÓN ---
$conn->close();
?>