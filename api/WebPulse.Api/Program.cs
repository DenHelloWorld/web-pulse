using WebPulse.Api.Hubs;
using WebPulse.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Стандартная диагностика (оставляем OpenAPI, если нужно)
builder.Services.AddOpenApi();

// 2. РЕГИСТРАЦИЯ ИНФРАСТРУКТУРЫ WEB PULSE
builder.Services.AddSignalR();
builder.Services.AddHostedService<DataHarvester>();

// 3. НАСТРОЙКА CORS
// Это критично: без этого Angular (порт 4200) не сможет подключиться к API (порт 5xxx)
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAngular", policy => policy
        .WithOrigins("http://localhost:4200") 
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

var app = builder.Build();

// Настройка конвейера (Pipeline)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Включаем CORS ПЕРЕД маппингом хабов
app.UseCors("AllowAngular");

// На реальном сервере можно оставить, но для локальной разработки с самоподписанными 
// сертификатами иногда проще закомментировать UseHttpsRedirection для тестов с SignalR.
// app.UseHttpsRedirection();

// 4. МАППИНГ ХАБА
app.MapHub<PulseHub>("/pulseHub");

// Старую погоду можно оставить для тестов или удалить
app.MapGet("/health", () => "Server is Pulse-ready!");

app.Run();