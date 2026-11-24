using Microsoft.EntityFrameworkCore;
using Reservas.Core.Abstractions;
using Reservas.Core.Domain;

namespace Reservas.Infrastructure.Persistencia;

/// <summary>
/// Datos de demostración. Solo se ejecuta si la base está vacía, de modo que
/// dos arranques seguidos no duplican información. Las credenciales son de
/// desarrollo y están documentadas en el README.
/// </summary>
public static class SemillaDesarrollo
{
    public const string EmailAdmin = "admin@universidad.edu";
    public const string ContrasenaAdmin = "Admin123!";

    public const string EmailDocente = "docente@universidad.edu";
    public const string ContrasenaDocente = "Docente123!";

    public const string EmailEstudiante = "estudiante@universidad.edu";
    public const string ContrasenaEstudiante = "Estudiante123!";

    public static async Task SembrarAsync(
        ReservasDbContext db,
        IHashContrasena hash,
        CancellationToken ct = default)
    {
        if (!await db.Usuarios.AnyAsync(ct))
        {
            db.Usuarios.AddRange(
                new Usuario("Administradora del Sistema", EmailAdmin, hash.Hashear(ContrasenaAdmin), Rol.Administrador),
                new Usuario("Ana Torres", EmailDocente, hash.Hashear(ContrasenaDocente), Rol.Docente),
                new Usuario("Luis Pérez", EmailEstudiante, hash.Hashear(ContrasenaEstudiante), Rol.Estudiante));
        }

        if (!await db.Espacios.AnyAsync(ct))
        {
            db.Espacios.AddRange(
                new Espacio("Laboratorio de Redes", TipoEspacio.Laboratorio, 30, "Bloque B, piso 2"),
                new Espacio("Laboratorio de Física", TipoEspacio.Laboratorio, 24, "Bloque C, piso 1"),
                new Espacio("Auditorio Principal", TipoEspacio.Auditorio, 200, "Bloque A, planta baja"),
                new Espacio("Sala de Cómputo 1", TipoEspacio.SalaDeComputo, 40, "Bloque B, piso 1"),
                new Espacio("Sala de Cómputo 2", TipoEspacio.SalaDeComputo, 35, "Bloque B, piso 1"),
                new Espacio("Sala de Cómputo 3", TipoEspacio.SalaDeComputo, 25, "Bloque D, piso 3"));
        }

        await db.SaveChangesAsync(ct);
    }
}
