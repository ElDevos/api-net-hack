// Ejemplo 1: API mínima en memoria (sin base de datos)
// Opción del portal: "Aplicación web"

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

// Expone el documento OpenAPI en /openapi/v1.json
app.MapOpenApi();

// Almacenamiento en memoria: se pierde al reiniciar la aplicación
var tareas = new List<Tarea>
{
    new(1, "Crear la VM en Azure", true),
    new(2, "Desplegar la API en App Service by David Zhang", false)
};
var siguienteId = 3;

app.MapGet("/", () => Results.Ok(new
{
    servicio = "TareasApi",
    entorno = app.Environment.EnvironmentName,
    servidor = Environment.MachineName   // permite ver qué instancia responde al escalar
}));

// Endpoint de salud, útil para la opción "Health check" de App Service
app.MapGet("/health", () => Results.Ok("OK"));

app.MapGet("/api/tareas", () => tareas);

app.MapGet("/api/tareas/{id:int}", (int id) =>
    tareas.FirstOrDefault(t => t.Id == id) is { } tarea
        ? Results.Ok(tarea)
        : Results.NotFound());

app.MapPost("/api/tareas", (NuevaTarea entrada) =>
{
    if (string.IsNullOrWhiteSpace(entrada.Titulo))
        return Results.BadRequest("El título es obligatorio.");

    var tarea = new Tarea(siguienteId++, entrada.Titulo, false);
    tareas.Add(tarea);
    return Results.Created($"/api/tareas/{tarea.Id}", tarea);
});

app.MapPut("/api/tareas/{id:int}/completar", (int id) =>
{
    var indice = tareas.FindIndex(t => t.Id == id);
    if (indice < 0) return Results.NotFound();

    tareas[indice] = tareas[indice] with { Completada = true };
    return Results.Ok(tareas[indice]);
});

app.MapDelete("/api/tareas/{id:int}", (int id) =>
    tareas.RemoveAll(t => t.Id == id) > 0 ? Results.NoContent() : Results.NotFound());

app.Run();

record Tarea(int Id, string Titulo, bool Completada);
record NuevaTarea(string Titulo);
