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

        // Funciones a completar:

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

            // Validación del formato de fecha
            string fechaNac = "";
            while (true)
            {
                Console.Write("Fecha de Nacimiento (YYYY-MM-DD): ");
                fechaNac = Console.ReadLine().Trim();

                // Verifica el formato exacto y que la fecha sea válida en el calendario
                if (DateTime.TryParseExact(fechaNac, "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out _))
                {
                    break; // Es una fecha válida, salimos del bucle
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠️ Error: Formato incorrecto o fecha inexistente. Ingrese respetando la estructura YYYY-MM-DD.");
                    Console.ResetColor();
                }
            }

            // Validación de obligatoriedad y estructura de email
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

            // --- VALIDACIONES Y PERSISTENCIA (USUARIO + TARJETA) ---
            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                try
                {
                    conexion.Open();

                    // Validación de Cliente (Documento + Tipo)
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

                    // Validación de Email
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

                    // Generación y Validación de Tarjeta
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

                    // Inserción del Cliente
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

                            // Inserción de la Tarjeta vinculada al Cliente
                            string queryTarjeta = @"INSERT INTO tarjetas (numero_tarjeta, banco_emisor, dni_titular) 
                                            VALUES (@numTarjeta, @banco, @dniTitular)";

                            using (MySqlCommand comandoTarjeta = new MySqlCommand(queryTarjeta, conexion))
                            {
                                comandoTarjeta.Parameters.AddWithValue("@numTarjeta", numeroTarjeta);
                                comandoTarjeta.Parameters.AddWithValue("@banco", bancoEmisor);
                                comandoTarjeta.Parameters.AddWithValue("@dniTitular", documento);

                                int filasTarjeta = comandoTarjeta.ExecuteNonQuery();

                                if (filasTarjeta > 0)
                                {
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.WriteLine($"\n💳 Tarjeta Creada y Asignada Exitosamente:");
                                    Console.WriteLine($"Número: {numeroTarjeta.Substring(0, 4)} {numeroTarjeta.Substring(4, 4)} {numeroTarjeta.Substring(8, 4)} {numeroTarjeta.Substring(12, 4)}");
                                    Console.WriteLine($"Banco: {bancoEmisor}");
                                    Console.WriteLine($"Titular DNI: {documento}");
                                    Console.WriteLine($"Estado: Activa (Saldo inicial: $0.00)");
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

                    // Validación de número de cuenta
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
                            return; // vuelve al menú principal
                        }
                    }

                    // Ingreso y Validación de Periodo
                    string periodo = "";
                    while (true)
                    {
                        Console.Write("Periodo a liquidar (YYYY-MM): ");
                        periodo = Console.ReadLine().Trim();

                        // Validación estructural
                        if (periodo.Length == 7 && periodo[4] == '-' &&
                            int.TryParse(periodo.Substring(0, 4), out _) &&
                            int.TryParse(periodo.Substring(5, 2), out int mes) && mes >= 1 && mes <= 12)
                        {
                            // Validación en base de datos
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
                                    break; // Formato correcto y sin duplicados, salimos del bucle
                                }
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("⚠️ Error: Formato de periodo incorrecto. Use estrictamente YYYY-MM (ej: 2026-05).");
                            Console.ResetColor();
                        }
                    }

                    // Validación de fecha
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
                            Console.WriteLine("⚠️ Error: Formato incorrecto o fecha inexistente. Ingrese respetando la estructura YYYY-MM-DD.");
                            Console.ResetColor();
                        }
                    }

                    // Validacion de ingreso superior/igual a 0
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

                    // Persistencia
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
            Console.WriteLine("--- LISTADO GENERAL DE TARJETAS ---");
            Console.WriteLine("{0,-12} {1,-18} {2,-20} {3,-15}", "Nro Cuenta", "Nro Tarjeta", "Banco Emisor", "DNI Titular");
            Console.WriteLine("----------------------------------------------------------------------");

            // === A realizar ===
            // Aquí deben implementar un SELECT sobre la tabla 'tarjetas'
            // para recorrer las filas e imprimirlas en la consola.

            ObtenerYMostrarTarjetas();

            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }

        static void MenuVerDetalleTarjeta()
        {
            Console.Clear();
            Console.WriteLine("--- DETALLE DE TARJETA Y CLIENTE ---");
            Console.Write("Ingrese el Número de Cuenta a consultar: ");
            int numCuenta = Convert.ToInt32(Console.ReadLine());

            // === A realizar ===
            // Aquí deben realizar un SELECT con un JOIN entre 'tarjetas' y 'usuarios' 
            // filtrando por el numCuenta para traer todos los campos (Nombre, Apellido, Email, Saldo, etc.)

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

            // 1. Validación de ingreso numérico para la cuenta
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

            using (MySqlConnection conexion = new MySqlConnection(connectionString))
            {
                try
                {
                    conexion.Open();

                    // Validacion de existencia de número de cuenta y obtener el DNI del titular
                    string dniTitular = "";
                    string queryCheck = "SELECT dni_titular FROM tarjetas WHERE num_cuenta = @numCuenta";

                    using (MySqlCommand cmdCheck = new MySqlCommand(queryCheck, conexion))
                    {
                        cmdCheck.Parameters.AddWithValue("@numCuenta", numCuenta);
                        object result = cmdCheck.ExecuteScalar();

                        // Si result es null, significa que no existe ninguna fila con ese número de cuenta
                        if (result == null)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"\n❌ Operación cancelada: No se encontró ninguna tarjeta registrada bajo la cuenta {numCuenta}.");
                            Console.ResetColor();
                            Console.WriteLine("\nPresione cualquier tecla para volver al menú...");
                            Console.ReadKey();
                            return; // Aborta la operación y vuelve al menú
                        }

                        dniTitular = result.ToString();
                    }

                    // Confirmación de seguridad
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n⚠️ ADVERTENCIA: Se eliminará la cuenta {numCuenta}, sus liquidaciones y TODOS los datos del cliente (DNI {dniTitular}).");
                    Console.ResetColor();
                    Console.Write("¿Está seguro de continuar? (S/N): ");

                    if (Console.ReadLine().Trim().ToUpper() == "S")
                    {
                        // Ejecucion de baja (Tarjeta y Usuario)
                        string queryDelete = @"
                    DELETE FROM tarjetas WHERE num_cuenta = @numCuenta;
                    DELETE FROM usuarios WHERE documento = @dniTitular;";

                        using (MySqlCommand cmdDelete = new MySqlCommand(queryDelete, conexion))
                        {
                            cmdDelete.Parameters.AddWithValue("@numCuenta", numCuenta);
                            cmdDelete.Parameters.AddWithValue("@dniTitular", dniTitular);

                            int filasAfectadas = cmdDelete.ExecuteNonQuery();

                            if (filasAfectadas > 0)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"\n✅ ¡Baja exitosa! La tarjeta y el cliente (DNI {dniTitular}) han sido eliminados del sistema.");
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("\n⚠️ No se afectó ningún registro. Verifique el estado de la base de datos.");
                                Console.ResetColor();
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("\nOperación cancelada por el usuario.");
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


        // =========================================================================
        // MÉTODOS BASE QUE DEBEN COMPLETAR CON LA LÓGICA 
        // =========================================================================

        static void ObtenerYMostrarTarjetas()
        {
            // Completar 
            // Ejemplo de impresión dentro del bucle: 
            // Console.WriteLine("{0,-12} {1,-18} {2,-20} {3,-15}", reader["num_cuenta"], reader["numero_tarjeta"], ...);
        }

        static void MostrarDetalleCompleto(int cuenta)
        {
            // Completar haciendo un SELECT con JOIN de usuarios y tarjetas WHERE num_cuenta = @cuenta
        }

        static bool DarDeBajaTarjeta(int cuenta)
        {
            // Completar usando un DELETE FROM tarjetas WHERE num_cuenta = @cuenta
            return false;
        }
    }
}