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

// --- CONEXIÓN ---
$conn = new mysqli($servername, $username, $password, $dbname);

// --- CHECK CONEXIÓN ---
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
    // Se obtienen los datos
    $row = $result->fetch_assoc();
    
    // --- PERSISTENCIA EN SESIÓN  ---
    $_SESSION['usuario'] = $row['usuario'];
    $_SESSION['ultima_actividad'] = time();
    
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