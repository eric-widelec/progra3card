<?php
// --- INICIO Y VALIDACIÓN DE SESIÓN ---
session_start();

// --- CIERRE DE SESIÓN ---
if (isset($_GET['logout']) && $_GET['logout'] == 'true') {
    session_unset();
    session_destroy();
    echo "<script>window.location.href = 'ingreso.html';</script>";
    exit;
}

// --- RESTRICCIÓN DE ACCESO ---
if (!isset($_SESSION['usuario']) || empty($_SESSION['usuario'])) {
    echo "<script>window.location.href = 'ingreso.html';</script>";
    exit;
}

$tiempo_inactividad = 15; // segundos => 2 minutos
if ((!isset($_SESSION['ultima_actividad']) || empty($_SESSION['ultima_actividad'])) || (time() - $_SESSION['ultima_actividad'] > $tiempo_inactividad)) {    
    session_unset();
    session_destroy();
    echo "<script>window.location.href = 'ingreso.html';</script>";
    exit;
}
$_SESSION['ultima_actividad'] = time();

// --- CONFIGURACIÓN DE CONEXIÓN ---
$servername = "localhost";
$username = "root";
$password = "root";
$dbname = "mi_banco_db";

$conn = new mysqli($servername, $username, $password, $dbname);

if ($conn->connect_error) {
    die("Error en la conexión: " . $conn->connect_error);
}

$usuario_actual = $_SESSION['usuario'];

// --- CONSULTA DE DATOS DEL TITULAR Y SU TARJETA ---
$sql_usuario = "SELECT u.nombre, u.apellido, t.num_cuenta, t.numero_tarjeta, t.saldo 
                FROM usuarios u 
                INNER JOIN tarjetas t ON u.documento = t.dni_titular 
                WHERE u.usuario = '".$usuario_actual."'";

$result_usuario = $conn->query($sql_usuario);

$datos_usuario = null;
$liquidaciones = [];

if ($result_usuario && $result_usuario->num_rows > 0) {
    $datos_usuario = $result_usuario->fetch_assoc();
    $num_cuenta = $datos_usuario['num_cuenta'];
    
    // --- CONSULTA DE LIQUIDACIONES ORDENADAS DESCENDENTEMENTE ---
    $sql_liq = "SELECT periodo, fecha_vencimiento, total_a_pagar, pago_minimo 
                FROM liquidaciones 
                WHERE num_cuenta = ".$num_cuenta." 
                ORDER BY periodo DESC";
                
    $result_liq = $conn->query($sql_liq);
    
    if ($result_liq && $result_liq->num_rows > 0) {
        while ($row = $result_liq->fetch_assoc()) {
            $liquidaciones[] = $row;
        }
    }
}

// --- CIERRE DE CONEXIÓN ---
$conn->close();

// --- SEPARACIÓN DE LIQUIDACIONES ---
$ultima_liquidacion = count($liquidaciones) > 0 ? $liquidaciones[0] : null;
$liquidaciones_anteriores = count($liquidaciones) > 1 ? array_slice($liquidaciones, 1) : [];

$num_tarjeta = $datos_usuario ? $datos_usuario['numero_tarjeta'] : "Sin Tarjeta";
?>

<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Mis Tarjetas - Resumen de Cuenta</title>
    <script src="https://cdn.tailwindcss.com"></script>
</head>
<body class="bg-gray-100 font-sans min-h-screen flex flex-col justify-between">

    <header class="bg-[#004691] text-white shadow-md">
        <div class="max-w-6xl mx-auto px-6 py-4 flex justify-between items-center">
            <h1 class="text-xl font-semibold">Mis <span class="font-bold">Tarjetas</span></h1>
            <div class="flex items-center gap-4">
                <span class="text-sm hidden sm:inline-block">
                    Bienvenido, <strong><?php echo $datos_usuario ? $datos_usuario['nombre'] . ' ' . $datos_usuario['apellido'] : 'Usuario'; ?></strong>
                </span>
                <a href="resumen.php?logout=true" class="bg-blue-800 hover:bg-blue-900 px-4 py-2 rounded text-sm font-medium transition duration-200">
                    Cerrar Sesión
                </a>
            </div>
        </div>
    </header>

    <main class="flex-grow p-6">
        <div class="max-w-6xl mx-auto space-y-6">
            
            <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6 flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
                <div>
                    <h2 class="text-gray-500 text-sm uppercase tracking-wide font-semibold">Tarjeta de Crédito</h2>
                    <p class="text-2xl font-bold text-gray-800"><?php echo $num_tarjeta; ?></p>
                </div>
                <div class="text-left md:text-right">
                    <h2 class="text-gray-500 text-sm uppercase tracking-wide font-semibold">Saldo Actual</h2>
                    <p class="text-2xl font-bold text-[#004691]">$ <?php echo $datos_usuario ? $datos_usuario['saldo'] : '0.00'; ?></p>
                </div>
            </div>

            <div class="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
                <div class="bg-gray-50 border-b border-gray-200 px-6 py-4">
                    <h2 class="text-lg font-bold text-[#004691]">Última Liquidación</h2>
                </div>
                
                <div class="p-6">
                    <?php if ($ultima_liquidacion): ?>
                        <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-6">
                            <div>
                                <p class="text-xs text-gray-500 uppercase font-semibold">Periodo</p>
                                <p class="text-lg font-medium text-gray-800"><?php echo $ultima_liquidacion['periodo']; ?></p>
                            </div>
                            <div>
                                <p class="text-xs text-gray-500 uppercase font-semibold">Vencimiento</p>
                                <p class="text-lg font-medium text-gray-800"><?php echo $ultima_liquidacion['fecha_vencimiento']; ?></p>
                            </div>
                            <div>
                                <p class="text-xs text-gray-500 uppercase font-semibold">Pago Mínimo</p>
                                <p class="text-lg font-medium text-gray-800">$ <?php echo $ultima_liquidacion['pago_minimo']; ?></p>
                            </div>
                            <div>
                                <p class="text-xs text-gray-500 uppercase font-semibold">Total a Pagar</p>
                                <p class="text-2xl font-bold text-red-600">$ <?php echo $ultima_liquidacion['total_a_pagar']; ?></p>
                            </div>
                        </div>
                    <?php else: ?>
                        <p class="text-gray-500 text-center py-4">No hay liquidaciones registradas en esta cuenta.</p>
                    <?php endif; ?>
                </div>
            </div>

            <?php if (count($liquidaciones_anteriores) > 0): ?>
                <div class="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
                    <div class="bg-gray-50 border-b border-gray-200 px-6 py-4">
                        <h2 class="text-lg font-bold text-gray-700">Períodos Anteriores</h2>
                    </div>
                    <div class="overflow-x-auto">
                        <table class="w-full text-left border-collapse">
                            <thead>
                                <tr class="bg-white text-gray-500 text-xs uppercase border-b border-gray-200">
                                    <th class="px-6 py-4 font-semibold">Periodo</th>
                                    <th class="px-6 py-4 font-semibold">Fecha de Vencimiento</th>
                                    <th class="px-6 py-4 font-semibold">Pago Mínimo</th>
                                    <th class="px-6 py-4 font-semibold">Total Pagado</th>
                                </tr>
                            </thead>
                            <tbody class="text-gray-700 text-sm">
                                <?php foreach ($liquidaciones_anteriores as $liq): ?>
                                    <tr class="border-b border-gray-100 hover:bg-gray-50 transition duration-150">
                                        <td class="px-6 py-4 font-medium"><?php echo $liq['periodo']; ?></td>
                                        <td class="px-6 py-4"><?php echo $liq['fecha_vencimiento']; ?></td>
                                        <td class="px-6 py-4">$ <?php echo $liq['pago_minimo']; ?></td>
                                        <td class="px-6 py-4 font-semibold text-gray-800">$ <?php echo $liq['total_a_pagar']; ?></td>
                                    </tr>
                                <?php endforeach; ?>
                            </tbody>
                        </table>
                    </div>
                </div>
            <?php endif; ?>

        </div>
    </main>

    <footer class="bg-gray-50 text-[10px] text-gray-500 text-center p-4 border-t border-gray-200">
        Portal Oficial de Consultas de Liquidaciones Progra3card.
    </footer>
</body>
</html>