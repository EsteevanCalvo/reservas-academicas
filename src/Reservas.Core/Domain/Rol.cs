namespace Reservas.Core.Domain;

/// <summary>
/// Roles fijos del sistema. No existe un CRUD de roles: agregar uno nuevo
/// es una decisión de permisos y, por lo tanto, un cambio de código revisado.
/// </summary>
public enum Rol
{
    Administrador,
    Docente,
    Estudiante
}
