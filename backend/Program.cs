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

            // --- 1. DATOS DEL CLIENTE ---
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

            // Campos ahora obligatorios según la estructura de la base de datos
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

            // Validación proactiva y estricta del formato de fecha
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

            // Validación de obligatoriedad y estructura básica de email
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

            // --- 2. SELECCIÓN DEL BANCO EMISOR ---
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

            // --- 3. VALIDACIONES Y PERSISTENCIA (USUARIO + TARJETA) ---
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
            throw new NotImplementedException();
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
            Console.WriteLine("--- ELIMINAR TARJETA DEL SISTEMA ---");
            Console.Write("Ingrese el Número de Cuenta de la tarjeta a dar de baja: ");
            int numCuenta = Convert.ToInt32(Console.ReadLine());

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n⚠️ ADVERTENCIA: Se eliminará la tarjeta, sus liquidaciones y los datos de acceso web vinculados.");
            Console.ResetColor();
            Console.Write("¿Está seguro de continuar? (S/N): ");

            if (Console.ReadLine().ToUpper() == "S")
            {
                // === A realizar ===
                // Aquí deben ejecutar un DELETE sobre la tabla 'tarjetas' donde num_cuenta = numCuenta.
                // Como definimos ON DELETE CASCADE en la base de datos, las liquidaciones se borrarán solas.
                // Opcional: Evaluar si también eliminan al usuario de la tabla 'usuarios' o si lo mantienen.

                bool exito = DarDeBajaTarjeta(numCuenta);

                if (exito)
                    Console.WriteLine("\nTarjeta eliminada correctamente del sistema.");
                else
                    Console.WriteLine("\nError al intentar eliminar la tarjeta. Verifique el número de cuenta.");
            }
            else
            {
                Console.WriteLine("\nOperación cancelada.");
            }

            Console.WriteLine("\nPresione una tecla para volver al menú...");
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