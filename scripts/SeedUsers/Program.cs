/*
 * ============================================================
 *  SEED DE USUARIOS - Brittany Salon Backend
 *  Genera Empleados y Clientes con contraseñas BCrypt
 *  Compatible con BCrypt.Net-Next (mismo que usa el backend)
 *
 *  USO:
 *    cd scripts/SeedUsers
 *    dotnet run
 * ============================================================
 */

using BCrypt.Net;
using Microsoft.Data.SqlClient;

const string CONNECTION_STRING =
    @"Server=localhost\SQLEXPRESS;Database=BRITTANYSALON;" +
    "Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True;";

// ----------------------------------------------------------------
//  DATOS DE EMPLEADOS
// ----------------------------------------------------------------
var employees = new List<(string Name, string Email, string Phone, string Password, string Specialty)>
{
    (
        Name:      "Antony Morales López",
        Email:     "antony.empleado@ucr.ac.cr",
        Phone:     "88001122",
        Password:  "TonyML2025",
        Specialty: "Corte y Peinado"
    ),
    (
        Name:      "Sofía Rodríguez Vargas",
        Email:     "sofia.rodriguez@brittanysalon.com",
        Phone:     "87654321",
        Password:  "SofiR2025!",
        Specialty: "Colorimetría y Tinte"
    ),
};

// ----------------------------------------------------------------
//  DATOS DE CLIENTES
// ----------------------------------------------------------------
var clients = new List<(string Name, string Email, string Phone, string Password)>
{
    (
        Name:     "Laura Pérez González",
        Email:    "laura.perez@gmail.com",
        Phone:    "70011234",
        Password: "LauraP2025"
    ),
    (
        Name:     "Carlos Méndez Vega",
        Email:    "carlos.mendez@hotmail.com",
        Phone:    "71225566",
        Password: "CarlosM2025"
    ),
};

// ----------------------------------------------------------------
//  EJECUCIÓN
// ----------------------------------------------------------------
Console.WriteLine("=== Brittany Salon - Seed de Usuarios ===\n");

await using var connection = new SqlConnection(CONNECTION_STRING);

try
{
    await connection.OpenAsync();
    Console.WriteLine("✅ Conexión a SQL Server establecida.\n");
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"❌ No se pudo conectar a la BD: {ex.Message}");
    Console.ResetColor();
    return;
}

// --- Insertar Empleados ---
Console.WriteLine("--- Insertando Empleados ---");
foreach (var emp in employees)
{
    // Verificar si ya existe
    var checkCmd = connection.CreateCommand();
    checkCmd.CommandText = "SELECT COUNT(1) FROM Employee WHERE email = @email";
    checkCmd.Parameters.AddWithValue("@email", emp.Email.ToLower().Trim());
    var exists = (int)(await checkCmd.ExecuteScalarAsync() ?? 0);

    if (exists > 0)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  ⚠️  Empleado ya existe (omitido): {emp.Email}");
        Console.ResetColor();
        continue;
    }

    // Hashear contraseña con workFactor 10 (igual que BCrypt por defecto)
    var hash = BCrypt.Net.BCrypt.HashPassword(emp.Password, workFactor: 10);

    var cmd = connection.CreateCommand();
    cmd.CommandText = @"
        INSERT INTO Employee (name, email, phone, passwordHash, imageUrl, specialty, isActive, createdAt)
        VALUES (@name, @email, @phone, @passwordHash, NULL, @specialty, 1, @createdAt)";

    cmd.Parameters.AddWithValue("@name",         emp.Name);
    cmd.Parameters.AddWithValue("@email",        emp.Email.ToLower().Trim());
    cmd.Parameters.AddWithValue("@phone",        emp.Phone);
    cmd.Parameters.AddWithValue("@passwordHash", hash);
    cmd.Parameters.AddWithValue("@specialty",    emp.Specialty);
    cmd.Parameters.AddWithValue("@createdAt",    DateTime.Now);

    await cmd.ExecuteNonQueryAsync();

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"  ✅ Empleado creado: {emp.Name} ({emp.Email})");
    Console.ResetColor();
    Console.WriteLine($"     Contraseña: {emp.Password}");
    Console.WriteLine($"     Hash:       {hash[..30]}...");
}

Console.WriteLine();

// --- Insertar Clientes ---
Console.WriteLine("--- Insertando Clientes ---");
foreach (var cli in clients)
{
    // Verificar si ya existe
    var checkCmd = connection.CreateCommand();
    checkCmd.CommandText = "SELECT COUNT(1) FROM Client WHERE email = @email";
    checkCmd.Parameters.AddWithValue("@email", cli.Email.ToLower().Trim());
    var exists = (int)(await checkCmd.ExecuteScalarAsync() ?? 0);

    if (exists > 0)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  ⚠️  Cliente ya existe (omitido): {cli.Email}");
        Console.ResetColor();
        continue;
    }

    var hash = BCrypt.Net.BCrypt.HashPassword(cli.Password, workFactor: 10);

    var cmd = connection.CreateCommand();
    cmd.CommandText = @"
        INSERT INTO Client (name, email, phone, passwordHash, pendingBalance, imageUrl, isActive, createdAt)
        VALUES (@name, @email, @phone, @passwordHash, 0, NULL, 1, @createdAt)";

    cmd.Parameters.AddWithValue("@name",         cli.Name);
    cmd.Parameters.AddWithValue("@email",        cli.Email.ToLower().Trim());
    cmd.Parameters.AddWithValue("@phone",        cli.Phone);
    cmd.Parameters.AddWithValue("@passwordHash", hash);
    cmd.Parameters.AddWithValue("@createdAt",    DateTime.Now);

    await cmd.ExecuteNonQueryAsync();

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"  ✅ Cliente creado: {cli.Name} ({cli.Email})");
    Console.ResetColor();
    Console.WriteLine($"     Contraseña: {cli.Password}");
    Console.WriteLine($"     Hash:       {hash[..30]}...");
}

Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("=== Seed completado exitosamente ===");
Console.ResetColor();
Console.WriteLine();
Console.WriteLine("Credenciales insertadas:");
Console.WriteLine("┌─────────────────────────────────────────────────────────────────────┐");
Console.WriteLine("│ ROL      │ EMAIL                            │ CONTRASEÑA             │");
Console.WriteLine("├─────────────────────────────────────────────────────────────────────┤");
Console.WriteLine("│ EMPLOYEE │ antony.empleado@ucr.ac.cr        │ TonyML2025             │");
Console.WriteLine("│ EMPLOYEE │ sofia.rodriguez@brittanysalon.com│ SofiR2025!             │");
Console.WriteLine("│ CLIENT   │ laura.perez@gmail.com            │ LauraP2025             │");
Console.WriteLine("│ CLIENT   │ carlos.mendez@hotmail.com        │ CarlosM2025            │");
Console.WriteLine("└─────────────────────────────────────────────────────────────────────┘");
