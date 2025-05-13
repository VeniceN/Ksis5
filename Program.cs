var builder = WebApplication.CreateBuilder(args);

// Устанавливаем явный URL
builder.WebHost.UseUrls("http://127.0.0.2:5020");

builder.Services.AddControllers();

// Логирование
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Обработка HTTP-запросов
app.UseAuthorization();
app.MapControllers();

app.Run();

