using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Reservas.Api.Endpoints;
using Reservas.Api.Errores;
using Reservas.Api.Inicio;
using Reservas.Api.Seguridad;
using Reservas.Core.Domain;
using Reservas.Core.Servicios;
using Reservas.Infrastructure;
using Reservas.Infrastructure.Seguridad;

var builder = WebApplication.CreateBuilder(args);

// Los enums viajan como texto ("Laboratorio") y no como números, para que el
// contrato sea legible en Swagger y en los registros.
builder.Services.ConfigureHttpJsonOptions(opciones =>
{
    opciones.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

string cadenaConexion = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'Default' en la configuración.");

builder.Services.AgregarInfraestructura(cadenaConexion);

builder.Services.AddScoped<ServicioEspacios>();
builder.Services.AddScoped<ServicioUsuarios>();
builder.Services.AddScoped<ServicioAutenticacion>();

OpcionesJwt opcionesJwt = builder.Configuration
    .GetSection(OpcionesJwt.Seccion)
    .Get<OpcionesJwt>() ?? new OpcionesJwt();

opcionesJwt.Validar();
builder.Services.Configure<OpcionesJwt>(builder.Configuration.GetSection(OpcionesJwt.Seccion));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        // Sin el mapeo automático de claims, el identificador del usuario se lee
        // explícitamente del claim "sub" que emite el sistema.
        opciones.MapInboundClaims = false;
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = opcionesJwt.Emisor,
            ValidateAudience = true,
            ValidAudience = opcionesJwt.Audiencia,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opcionesJwt.Clave)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization(opciones =>
{
    // Política de caída: un endpoint sin política explícita exige autenticación.
    // Así, olvidar un RequireAuthorization no deja un endpoint abierto.
    opciones.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    opciones.AddPolicy(Politicas.Administrador, politica =>
        politica.RequireRole(nameof(Rol.Administrador)));
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorDeExcepciones>();
builder.Services.AddHostedService<InicializadorDeBaseDeDatos>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Sistema de Reservas de Espacios Académicos",
        Version = "v1",
        Description = "API REST para gestionar espacios académicos, usuarios y reservas. " +
                      "Inicie sesión en /api/auth/login y use el botón Authorize con el token obtenido."
    });

    opciones.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Token JWT. Escriba: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    });

    opciones.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI(opciones =>
{
    opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "Reservas académicas v1");
    opciones.RoutePrefix = "swagger";
});

app.UseAuthentication();
app.UseMiddleware<VerificarUsuarioActivoMiddleware>();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/swagger"))
    .AllowAnonymous()
    .ExcludeFromDescription();

EndpointsAutenticacion.Mapear(app);
EndpointsEspacios.Mapear(app);
EndpointsUsuarios.Mapear(app);

app.Run();

/// <summary>Punto de entrada expuesto para las pruebas de integración.</summary>
public partial class Program
{
}
