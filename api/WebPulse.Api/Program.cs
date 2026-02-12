using WebPulse.Api.Hubs;
using WebPulse.Api.Services;
using WebPulse.Api.Constants;

var builder = WebApplication.CreateBuilder(args);

// 1. Стандартная диагностика (оставляем OpenAPI, если нужно)
builder.Services.AddOpenApi();

// 2. РЕГИСТРАЦИЯ ИНФРАСТРУКТУРЫ WEB PULSE
builder.Services.AddSignalR();

// Регистрация наших сервисов
builder.Services.AddSingleton<ISentimentAnalysisService, SentimentAnalysisService>();
builder.Services.AddHostedService<PulseGenerationService>();

// Регистрация провайдеров данных
builder.Services.AddHttpClient<RedditProvider>();
builder.Services.AddSingleton<ICommentProvider, RedditProvider>();

// Временно оставляем старый DataHarvester для совместимости
builder.Services.AddHostedService<DataHarvester>();

// 3. НАСТРОЙКА CORS
// Это критично: без этого Angular (порт 4200) не сможет подключиться к API (порт 5xxx)
builder.Services.AddCors(options => {
    options.AddPolicy(ProviderConstants.CORS.PolicyName, policy => policy
        .WithOrigins(ProviderConstants.CORS.AngularOrigin)
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
app.UseCors(ProviderConstants.CORS.PolicyName);

// На реальном сервере можно оставить, но для локальной разработки с самоподписанными
// сертификатами иногда проще закомментировать UseHttpsRedirection для тестов с SignalR.
// app.UseHttpsRedirection();

// 4. МАППИНГ ХАБА
app.MapHub<PulseHub>(ProviderConstants.SignalR.HubPath);

// Старую погоду можно оставить для тестов или удалить
app.MapGet("/health", () => "Server is Pulse-ready!");

app.Run();
