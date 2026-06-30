<?php
// --- INICIO DE SESIÓN ---
session_start();

// --- CONFIGURACIÓN DE CONEXIÓN ---
$servername = "localhost";
$username = "root";
$password = "root";
$dbname = "mi_banco_db";

// --- DATOS RECIBIDOS DEL FORMULARIO INGRESO.HTML ---
$tipo_doc = $_POST['tipo_doc'];
$documento = $_POST['documento'];
$usuario = $_POST['usuario'];
$password_input = $_POST['password'];

// --- INSTANCIA UN OBJETO PERTENECIENTE A LA CLASE MYSQLI ---
$conn = new mysqli($servername, $username, $password, $dbname);

// --- CHECK CONNECTION ---
if ($conn->connect_error) {
  die("Error en la conexión: " . $conn->connect_error);
}

// --- CONSULTA PARA VALIDAR CREDENCIALES ---
$sql = "SELECT usuario FROM usuarios 
        WHERE documento = '".$documento."' 
        AND tipo_doc = '".$tipo_doc."' 
        AND usuario = '".$usuario."' 
        AND password = '".$password_input."'";

$result = $conn->query($sql);

// --- VERIFICACIÓN Y ALMACENAMIENTO EN SESIÓN ---
if ($result && $result->num_rows > 0) {
    // Si la validación es exitosa, se obtienen los datos
    $row = $result->fetch_assoc();
    
    // --- PERSISTENCIA EN SESIÓN ESTRICTA ---
    $_SESSION['usuario'] = $row['usuario'];
    
    // --- REDIRECCIÓN AL RESUMEN ---
    echo "<script>
            window.location.href = 'resumen.php';
          </script>";
} else {
    // Si los datos son incorrectos
    echo "<script>
            alert('Error: Credenciales incorrectas o usuario no registrado.');
            window.location.href = 'ingreso.html';
          </script>";
}

// --- CIERRE DE CONEXIÓN ---
$conn->close();
?>