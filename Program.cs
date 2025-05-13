var builder = WebApplication.CreateBuilder(args);

// Óñòàíàâëèâàåì ÿâíûé URL
builder.WebHost.UseUrls("http://0.0.0.0:5020");

builder.Services.AddControllers();

// Ëîãèðîâàíèå
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Îáðàáîòêà HTTP-çàïðîñîâ
app.UseAuthorization();
app.MapControllers();

app.Run();
