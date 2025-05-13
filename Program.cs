var builder = WebApplication.CreateBuilder(args);

// ������������� ����� URL
builder.WebHost.UseUrls("http://0.0.0.0:5020");

builder.Services.AddControllers();

// �����������
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// ��������� HTTP-��������
app.UseAuthorization();
app.MapControllers();

app.Run();
