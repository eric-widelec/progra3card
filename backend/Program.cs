using System;
using MySql.Data.MySqlClient;

namespace Progra3Card.Administrativo
{
    class Program
    {
        private static string connectionString = "Server=localhost;Port=3306;Database=mi_banco_db;Uid=root;Pwd=root;";

        static void Main(string[] args)
        {
            bool salir = false;
            while (!salir)
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("    SISTEMA ADMINISTRATIVO PROGRA3CARD   ");
                Console.WriteLine("========================================");
                Console.WriteLine("1. Emitir Nueva Tarjeta (Alta de Cliente)");
                Console.WriteLine("2. Listar Tarjetas");
                Console.WriteLine("3. Ver Detalle de una Tarjeta / Cliente");
                Console.WriteLine("4. Eliminar Tarjeta (Baja de Sistema)");
                Console.WriteLine("5. Emitir Nueva Liquidación Mensual");
                Console.WriteLine("6. Salir");
                Console.WriteLine("========================================");
                Console.Write("Seleccione una opción: ");

                switch (Console.ReadLine())
                {
                    case "1": MenuEmitirTarjeta(); break;
                    case "2": MenuListarTarjetas(); break;
                    case "3": MenuVerDetalleTarjeta(); break;
                    case "4": MenuEliminarTarjeta(); break;
                    case "5": MenuEmitirLiquidacion(); break;
                    case "6": salir = true; break;
                    default:
                        Console.WriteLine("Opción no válida. Presione una tecla para continuar...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        // --- MÉTODOS DEL MENÚ ---

        static void MenuEmitirTarjeta()
        {
            Console.Clear();
            Console.WriteLine("==================================================");
            Console.WriteLine("           EMITIR NUEVA TARJETA (ALTA)            ");
            Console.WriteLine("==================================================");

            // --- DATOS DEL CLIENTE ---
            Console.WriteLine("\n--- Datos del Cliente ---");

            string tipoDoc = "";
            while (tipoDoc != "DNI" && tipoDoc != "PASAPORTE")
            {
                Console.Write("Tipo de Documento (DNI / PASAPORTE): ");
                tipoDoc = Console.ReadLine().Trim().ToUpper();
            }

            string documento = "";
            while (string.IsNullOrWhiteSpace(documento))
            {
                Console.Write("Número de Documento: ");
                documento = Console.ReadLine().Trim();
            }

            string nombre = "";
            while (string.IsNullOrWhiteSpace(nombre))
            {
                Console.Write("Nombre: ");
                nombre = Console.ReadLine().Trim();
            }

            string apellido = "";
            while (string.IsNullOrWhiteSpace(apellido))
            {
                Console.Write("Apellido: ");
                apellido = Console.ReadLine().Trim();
            }

            // --- VALIDACIÓN DE FECHA ---
            string fechaNac = "";
            while (true)
            {
                Console.Write("Fecha de Nacimiento (YYYY-MM-DD): ");
                fechaNac = Console.ReadLine().Trim();

                if (DateTime.TryParseExact(fechaNac, "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out _))
                {
                    break;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠️ Error: Fecha inexistente o estructura incorrecta. Ingrese respetando YYYY-MM-DD.");
                    Console.ResetColor();
                }
            }

            // --- VALIDACIÓN DE EMAIL ---
            string email = "";
            while (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                Console.Write("Email: ");
                email = Console.ReadLine().Trim();

                if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠️ Error: Ingrese una dirección de correo válida.");
                    Console.ResetColor();
                }
            }

            // --- SELECCIÓN DEL BANCO EMISOR ---
            string bancoEmisor = "";
            bool bancoValido = false;

            while (!bancoValido)
            {
                Console.WriteLine("\nSeleccione el Banco Emisor:");
                Console.WriteLine("1. Banco Nación");
                Console.WriteLine("2. Banco Provincia");
                Console.WriteLine("3. Banco Galicia");
                Console.WriteLine("4. Banco Santander");
                Console.WriteLine("5. Banco BBVA");
                Console.WriteLine("6. Banco Macro");
                Console.Write("Opción: ");

                switch (Console.ReadLine().Trim())
                {
                    case "1": bancoEmisor = "Banco Nación"; bancoValido = true; break;
                    case "2": bancoEmisor = "Banco Provincia"; bancoValido = true; break;
                    case "3": bancoEmisor = "Banco Galicia"; bancoValido = true; break;
                    case "4": bancoEmisor = "Banco Santander"; bancoValido = true; break;
                    case "5": bancoEmisor = "Banco BBVA"; bancoValido = true; break;
                    case "6": bancoEmisor = "Banco Macro"; bancoValido = true; break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("\nOpción no válida. Ingrese un número del 1 al 6.");
                        Console.ResetColor();
                        break;
                }
            }

            // --- INGRESO DE SALDO ---
            decimal saldoInicial = 0;
            while (true)
            {
                Console.Write("\nSaldo Inicial ($) [Opcional - Presione Enter para $0.00]: ");
                string inputSaldo = Console.ReadLine().Trim();

                if (string.IsNullOrWhiteSpace(inputSaldo))
                {
                    saldoInicial = 0;
                    break;
                }

                if (decimal.TryParse(inputSaldo, out saldoInicial) && saldoInicial > 0)
                {
                    break;
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠️ Error: Si ingresa un saldo, debe ser un valor numérico mayor a 0.");
                Console.ResetColor();
            }

            // --- PERSISTENCIA EN BASE DE DATOS ---
            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                try
                {
                    conexion.Open();

                    // --- VALIDACIÓN DE CLIENTE ---
                    string queryCheckUsuario = "SELECT COUNT(*) FROM usuarios WHERE documento = @documento AND tipo_doc = @tipoDoc";
                    using (MySqlCommand cmdCheckUsuario = new MySqlCommand(queryCheckUsuario, conexion))
                    {
                        cmdCheckUsuario.Parameters.AddWithValue("@documento", documento);
                        cmdCheckUsuario.Parameters.AddWithValue("@tipoDoc", tipoDoc);

                        int usuarioExiste = Convert.ToInt32(cmdCheckUsuario.ExecuteScalar());

                        if (usuarioExiste > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"\n❌ Operación cancelada: Ya existe un cliente registrado con el {tipoDoc} {documento}.");
                            Console.ResetColor();
                            Console.WriteLine("\nPresione cualquier tecla para volver al menú...");
                            Console.ReadKey();
                            return;
                        }
                    }

                    // --- VALIDACIÓN DE EMAIL ---
                    string queryCheckEmail = "SELECT COUNT(*) FROM usuarios WHERE email = @email";
                    using (MySqlCommand cmdCheckEmail = new MySqlCommand(queryCheckEmail, conexion))
                    {
                        cmdCheckEmail.Parameters.AddWithValue("@email", email);
                        int emailExiste = Convert.ToInt32(cmdCheckEmail.ExecuteScalar());

                        if (emailExiste > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"\n❌ Operación cancelada: El email {email} ya se encuentra registrado en el sistema.");
                            Console.ResetColor();
                            Console.WriteLine("\nPresione cualquier tecla para volver al menú...");
                            Console.ReadKey();
                            return;
                        }
                    }

                    // --- GENERACIÓN DE TARJETA ---
                    Random rnd = new Random();
                    string numeroTarjeta = "";
                    bool tarjetaLibre = false;

                    while (!tarjetaLibre)
                    {
                        long bloque1 = rnd.Next(1000, 10000);
                        long bloque2 = rnd.Next(1000, 10000);
                        long bloque3 = rnd.Next(1000, 10000);
                        long bloque4 = rnd.Next(1000, 10000);

                        numeroTarjeta = $"{bloque1}{bloque2}{bloque3}{bloque4}";

                        string queryVerificacionTarjeta = "SELECT COUNT(*) FROM tarjetas WHERE numero_tarjeta = @numTarjeta";
                        using (MySqlCommand cmdVerificarTarjeta = new MySqlCommand(queryVerificacionTarjeta, conexion))
                        {
                            cmdVerificarTarjeta.Parameters.AddWithValue("@numTarjeta", numeroTarjeta);
                            int cantidad = Convert.ToInt32(cmdVerificarTarjeta.ExecuteScalar());

                            if (cantidad == 0)
                            {
                                tarjetaLibre = true;
                            }
                        }
                    }

                    // --- INSERCIÓN DE CLIENTE ---
                    string queryUsuario = @"INSERT INTO usuarios (documento, tipo_doc, nombre, apellido, fecha_nacimiento, email) 
                            VALUES (@documento, @tipoDoc, @nombre, @apellido, @fechaNacimiento, @email)";

                    using (MySqlCommand comandoUsuario = new MySqlCommand(queryUsuario, conexion))
                    {
                        comandoUsuario.Parameters.AddWithValue("@documento", documento);
                        comandoUsuario.Parameters.AddWithValue("@tipoDoc", tipoDoc);
                        comandoUsuario.Parameters.AddWithValue("@nombre", nombre);
                        comandoUsuario.Parameters.AddWithValue("@apellido", apellido);
                        comandoUsuario.Parameters.AddWithValue("@fechaNacimiento", fechaNac);
                        comandoUsuario.Parameters.AddWithValue("@email", email);

                        int filasUsuario = comandoUsuario.ExecuteNonQuery();

                        if (filasUsuario > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("\n✅ ¡Usuario cliente registrado exitosamente en el sistema!");

                            // --- INSERCIÓN DE TARJETA ---
                            string queryTarjeta = @"INSERT INTO tarjetas (numero_tarjeta, banco_emisor, dni_titular, saldo) 
                                    VALUES (@numTarjeta, @banco, @dniTitular, @saldo)";

                            using (MySqlCommand comandoTarjeta = new MySqlCommand(queryTarjeta, conexion))
                            {
                                comandoTarjeta.Parameters.AddWithValue("@numTarjeta", numeroTarjeta);
                                comandoTarjeta.Parameters.AddWithValue("@banco", bancoEmisor);
                                comandoTarjeta.Parameters.AddWithValue("@dniTitular", documento);
                                comandoTarjeta.Parameters.AddWithValue("@saldo", saldoInicial);

                                int filasTarjeta = comandoTarjeta.ExecuteNonQuery();

                                if (filasTarjeta > 0)
                                {
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.WriteLine($"\n💳 Tarjeta Creada y Asignada Exitosamente:");
                                    Console.WriteLine($"Número: {numeroTarjeta.Substring(0, 4)} {numeroTarjeta.Substring(4, 4)} {numeroTarjeta.Substring(8, 4)} {numeroTarjeta.Substring(12, 4)}");
                                    Console.WriteLine($"Banco: {bancoEmisor}");
                                    Console.WriteLine($"Titular DNI: {documento}");
                                    Console.WriteLine($"Estado: Activa (Saldo inicial: ${saldoInicial:F2})");
                                }
                            }
                            Console.ResetColor();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n❌ Ocurrió un error inesperado:");
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                }
            }

            Console.WriteLine("\nPresione cualquier tecla para volver al menú...");
            Console.ReadKey();
        }

        static void MenuEmitirLiquidacion()
        {
            Console.Clear();
            Console.WriteLine("==================================================");
            Console.WriteLine("           EMITIR NUEVA LIQUIDACIÓN               ");
            Console.WriteLine("==================================================");

            int numCuenta = 0;
            while (true)
            {
                Console.Write("\nIngrese el Número de Cuenta: ");
                if (int.TryParse(Console.ReadLine().Trim(), out numCuenta) && numCuenta > 0)
                {
                    break;
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠️ Error: Ingrese un número de cuenta válido (mayor a 0).");
                Console.ResetColor();
            }

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                try
                {
                    conexion.Open();

                    // --- VALIDACIÓN DE CUENTA ---
                    string queryCheckCuenta = "SELECT COUNT(*) FROM tarjetas WHERE num_cuenta = @numCuenta";
                    using (MySqlCommand cmdCheckCuenta = new MySqlCommand(queryCheckCuenta, conexion))
                    {
                        cmdCheckCuenta.Parameters.AddWithValue("@numCuenta", numCuenta);
                        int cuentaExiste = Convert.ToInt32(cmdCheckCuenta.ExecuteScalar());

                        if (cuentaExiste == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"\n❌ Operación cancelada: No existe ninguna tarjeta registrada bajo la cuenta {numCuenta}.");
                            Console.ResetColor();
                            Console.WriteLine("\nPresione cualquier tecla para volver al menú...");
                            Console.ReadKey();
                            return;
                        }
                    }

                    // --- VALIDACIÓN DE PERIODO ---
                    string periodo = "";
                    while (true)
                    {
                        Console.Write("Periodo a liquidar (YYYY-MM): ");
                        periodo = Console.ReadLine().Trim();

                        if (periodo.Length == 7 && periodo[4] == '-' &&
                            int.TryParse(periodo.Substring(0, 4), out _) &&
                            int.TryParse(periodo.Substring(5, 2), out int mes) && mes >= 1 && mes <= 12)
                        {
                            string queryCheckPeriodo = "SELECT COUNT(*) FROM liquidaciones WHERE num_cuenta = @numCuenta AND periodo = @periodo";
                            using (MySqlCommand cmdCheckPeriodo = new MySqlCommand(queryCheckPeriodo, conexion))
                            {
                                cmdCheckPeriodo.Parameters.AddWithValue("@numCuenta", numCuenta);
                                cmdCheckPeriodo.Parameters.AddWithValue("@periodo", periodo);
                                int periodoExiste = Convert.ToInt32(cmdCheckPeriodo.ExecuteScalar());

                                if (periodoExiste > 0)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"\n❌ Error: Ya existe una liquidación generada para el periodo {periodo} en la cuenta {numCuenta}.");
                                    Console.ResetColor();
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("⚠️ Error: Estructura de periodo incorrecta. Use estrictamente YYYY-MM (ej: 2026-05).");
                            Console.ResetColor();
                        }
                    }

                    // --- VALIDACIÓN DE FECHA ---
                    string fechaVencimiento = "";
                    while (true)
                    {
                        Console.Write("Fecha de Vencimiento (YYYY-MM-DD): ");
                        fechaVencimiento = Console.ReadLine().Trim();

                        if (DateTime.TryParseExact(fechaVencimiento, "yyyy-MM-dd",
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out _))
                        {
                            break;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("⚠️ Error: Fecha inexistente o estructura incorrecta. Ingrese respetando YYYY-MM-DD.");
                            Console.ResetColor();
                        }
                    }

                    // --- VALIDACIÓN DE MONTOS ---
                    decimal totalPagar = 0;
                    while (true)
                    {
                        Console.Write("Total a Pagar ($): ");
                        if (decimal.TryParse(Console.ReadLine().Trim(), out totalPagar) && totalPagar >= 0)
                        {
                            break;
                        }

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("⚠️ Error: El total a pagar debe ser un valor numérico superior o igual a 0.");
                        Console.ResetColor();
                    }

                    decimal pagoMinimo = 0;
                    while (true)
                    {
                        Console.Write("Pago Mínimo ($): ");
                        if (decimal.TryParse(Console.ReadLine().Trim(), out pagoMinimo) && pagoMinimo >= 0)
                        {
                            if (pagoMinimo > totalPagar)
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("⚠️ Error: El pago mínimo no puede ser mayor al total a pagar.");
                                Console.ResetColor();
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("⚠️ Error: El pago mínimo debe ser un valor numérico superior o igual a 0.");
                            Console.ResetColor();
                        }
                    }

                    // --- PERSISTENCIA EN BASE DE DATOS ---
                    string queryInsert = @"INSERT INTO liquidaciones (num_cuenta, periodo, fecha_vencimiento, total_a_pagar, pago_minimo) 
                                   VALUES (@numCuenta, @periodo, @fechaVencimiento, @totalPagar, @pagoMinimo)";

                    using (MySqlCommand cmdInsert = new MySqlCommand(queryInsert, conexion))
                    {
                        cmdInsert.Parameters.AddWithValue("@numCuenta", numCuenta);
                        cmdInsert.Parameters.AddWithValue("@periodo", periodo);
                        cmdInsert.Parameters.AddWithValue("@fechaVencimiento", fechaVencimiento);
                        cmdInsert.Parameters.AddWithValue("@totalPagar", totalPagar);
                        cmdInsert.Parameters.AddWithValue("@pagoMinimo", pagoMinimo);

                        int filasAfectadas = cmdInsert.ExecuteNonQuery();

                        if (filasAfectadas > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"\n✅ ¡Liquidación del periodo {periodo} registrada exitosamente para la cuenta {numCuenta}!");
                            Console.ResetColor();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n❌ Ocurrió un error inesperado de base de datos:");
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                }
            }

            Console.WriteLine("\nPresione cualquier tecla para volver al menú...");
            Console.ReadKey();
        }

        static void MenuListarTarjetas()
        {
            Console.Clear();
            Console.WriteLine("=====================================================================================================");
            Console.WriteLine("                                  LISTADO GENERAL DE TARJETAS                                        ");
            Console.WriteLine("=====================================================================================================");

            Console.WriteLine("{0,-10} | {1,-19} | {2,-18} | {3,-12} | {4,-25}",
                "Nro Cuenta", "Nro Tarjeta", "Banco Emisor", "DNI Titular", "Nombre y Apellido");
            Console.WriteLine("-----------------------------------------------------------------------------------------------------");

            ObtenerYMostrarTarjetas();

            Console.WriteLine("\nPresione cualquier tecla para volver al menú...");
            Console.ReadKey();
        }

        static void MenuVerDetalleTarjeta()
        {
            Console.Clear();
            Console.WriteLine("==================================================");
            Console.WriteLine("       DETALLE DE TARJETA Y CLIENTE               ");
            Console.WriteLine("==================================================");

            // --- VALIDACIÓN DE INGRESO ---
            int numCuenta = 0;
            while (true)
            {
                Console.Write("\nIngrese el Número de Cuenta a consultar: ");
                if (int.TryParse(Console.ReadLine().Trim(), out numCuenta) && numCuenta > 0)
                {
                    break;
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠️ Error: Ingrese un número de cuenta válido (mayor a 0).");
                Console.ResetColor();
            }

            MostrarDetalleCompleto(numCuenta);

            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }

        static void MenuEliminarTarjeta()
        {
            Console.Clear();
            Console.WriteLine("==================================================");
            Console.WriteLine("       ELIMINAR TARJETA Y CLIENTE (BAJA)          ");
            Console.WriteLine("==================================================");

            // --- VALIDACIÓN DE INGRESO ---
            int numCuenta = 0;
            while (true)
            {
                Console.Write("\nIngrese el Número de Cuenta de la tarjeta a dar de baja: ");
                if (int.TryParse(Console.ReadLine().Trim(), out numCuenta) && numCuenta > 0)
                {
                    break;
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠️ Error: Ingrese un número de cuenta válido (mayor a 0).");
                Console.ResetColor();
            }

            // --- CONFIRMACIÓN DE ACCIÓN ---
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n⚠️ ADVERTENCIA: Se eliminará la cuenta, sus liquidaciones y TODOS los datos del cliente titular.");
            Console.ResetColor();
            Console.Write("¿Está seguro de continuar con la baja? (S/N): ");

            if (Console.ReadLine().Trim().ToUpper() == "S")
            {
                bool exito = DarDeBajaTarjeta(numCuenta);

                if (exito)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n✅ ¡Baja exitosa! La tarjeta y el cliente han sido eliminados del sistema.");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"\n⚠️ Operación denegada: No se encontró ninguna tarjeta registrada bajo la cuenta {numCuenta}.");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.WriteLine("\nOperación cancelada por el usuario.");
            }

            Console.WriteLine("\nPresione cualquier tecla para volver al menú...");
            Console.ReadKey();
        }

        // =========================================================================
        // MÉTODOS AUXILIARES Y DE BASE DE DATOS
        // =========================================================================

        static void ObtenerYMostrarTarjetas()
        {
            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                try
                {
                    conexion.Open();

                    string query = @"
                SELECT 
                    t.num_cuenta, t.numero_tarjeta, t.banco_emisor, 
                    u.documento, u.nombre, u.apellido 
                FROM tarjetas t
                INNER JOIN usuarios u ON t.dni_titular = u.documento
                ORDER BY t.num_cuenta ASC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conexion))
                    {
                        using (MySqlDataReader lector = cmd.ExecuteReader())
                        {
                            bool hayRegistros = false;

                            while (lector.Read())
                            {
                                hayRegistros = true;

                                string numCuenta = lector["num_cuenta"].ToString();
                                string numTarjeta = lector["numero_tarjeta"].ToString();
                                string tarjetaFormateada = "";

                                if (numTarjeta.Length == 16)
                                {
                                    tarjetaFormateada = $"{numTarjeta.Substring(0, 4)} {numTarjeta.Substring(4, 4)} {numTarjeta.Substring(8, 4)} {numTarjeta.Substring(12, 4)}";
                                }
                                else
                                {
                                    tarjetaFormateada = numTarjeta;
                                }

                                string banco = lector["banco_emisor"].ToString();
                                string dni = lector["documento"].ToString();
                                string titular = $"{lector["nombre"]} {lector["apellido"]}";

                                Console.WriteLine("{0,-10} | {1,-19} | {2,-18} | {3,-12} | {4,-25}",
                                    numCuenta, tarjetaFormateada, banco, dni, titular);
                            }

                            if (!hayRegistros)
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("⚠️ No hay tarjetas registradas en el sistema en este momento.");
                                Console.ResetColor();
                            }
                        }
                    }
                    Console.WriteLine("=====================================================================================================");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n❌ Ocurrió un error al intentar consultar los registros de tarjetas:");
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                }
            }
        }

        static void MostrarDetalleCompleto(int cuenta)
        {
            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                try
                {
                    conexion.Open();

                    string query = @"
                SELECT 
                    t.num_cuenta, t.numero_tarjeta, t.banco_emisor, t.estado, t.saldo,
                    u.documento, u.tipo_doc, u.nombre, u.apellido, u.email, u.fecha_nacimiento
                FROM tarjetas t
                INNER JOIN usuarios u ON t.dni_titular = u.documento
                WHERE t.num_cuenta = @numCuenta";

                    using (MySqlCommand cmd = new MySqlCommand(query, conexion))
                    {
                        cmd.Parameters.AddWithValue("@numCuenta", cuenta);

                        using (MySqlDataReader lector = cmd.ExecuteReader())
                        {
                            if (lector.Read())
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("\n==================================================");
                                Console.WriteLine("                 DATOS DEL CLIENTE                ");
                                Console.WriteLine("==================================================");
                                Console.WriteLine($"Titular:     {lector["nombre"]} {lector["apellido"]}");
                                Console.WriteLine($"Documento:   {lector["tipo_doc"]} {lector["documento"]}");
                                Console.WriteLine($"Email:       {lector["email"]}");

                                DateTime fechaNac = Convert.ToDateTime(lector["fecha_nacimiento"]);
                                Console.WriteLine($"Nacimiento:  {fechaNac:yyyy-MM-dd}");

                                Console.WriteLine("\n==================================================");
                                Console.WriteLine("                 DATOS DE LA TARJETA              ");
                                Console.WriteLine("==================================================");
                                Console.WriteLine($"Nro Cuenta:  {lector["num_cuenta"]}");

                                string numTarjeta = lector["numero_tarjeta"].ToString();
                                string tarjetaFormateada = $"{numTarjeta.Substring(0, 4)} {numTarjeta.Substring(4, 4)} {numTarjeta.Substring(8, 4)} {numTarjeta.Substring(12, 4)}";

                                Console.WriteLine($"Nro Tarjeta: {tarjetaFormateada}");
                                Console.WriteLine($"Banco:       {lector["banco_emisor"]}");
                                Console.WriteLine($"Estado:      {lector["estado"]}");
                                Console.WriteLine($"Saldo Actual:${lector["saldo"]}");
                                Console.WriteLine("==================================================\n");
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"\n⚠️ Operación denegada: No se encontró ningún cliente asociado a la cuenta Nro: {cuenta}.");
                                Console.ResetColor();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n❌ Ocurrió un error al intentar consultar los registros:");
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                }
            }
        }

        static bool DarDeBajaTarjeta(int cuenta)
        {
            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                try
                {
                    conexion.Open();

                    // --- VALIDACIÓN DE EXISTENCIA ---
                    string dniTitular = "";
                    string queryCheck = "SELECT dni_titular FROM tarjetas WHERE num_cuenta = @numCuenta";

                    using (MySqlCommand cmdCheck = new MySqlCommand(queryCheck, conexion))
                    {
                        cmdCheck.Parameters.AddWithValue("@numCuenta", cuenta);
                        object result = cmdCheck.ExecuteScalar();

                        if (result == null)
                        {
                            return false;
                        }

                        dniTitular = result.ToString();
                    }

                    // --- ELIMINACIÓN DE REGISTROS ---
                    string queryDelete = @"
                DELETE FROM tarjetas WHERE num_cuenta = @numCuenta;
                DELETE FROM usuarios WHERE documento = @dniTitular;";

                    using (MySqlCommand cmdDelete = new MySqlCommand(queryDelete, conexion))
                    {
                        cmdDelete.Parameters.AddWithValue("@numCuenta", cuenta);
                        cmdDelete.Parameters.AddWithValue("@dniTitular", dniTitular);

                        int filasAfectadas = cmdDelete.ExecuteNonQuery();

                        return filasAfectadas > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n❌ Ocurrió un error interno en la base de datos:");
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();

                    return false;
                }
            }
        }
    }
}