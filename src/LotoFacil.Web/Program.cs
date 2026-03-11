using LotoFacil.Application.Services;
using LotoFacil.Domain.Interfaces;
using LotoFacil.Domain.Models;
using LotoFacil.Infrastructure;
using LotoFacil.Web.Components;
using LotoFacil.Web.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
    });

builder.Services.AddMudServices();

builder.Services.AddScoped<ConfiguracaoStorage>();
builder.Services.AddScoped(sp => sp.GetRequiredService<ConfiguracaoStorage>().Config);

builder.Services.AddHttpClient<ICaixaApiClient, CaixaApiClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
    client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
    client.DefaultRequestHeaders.Add("Accept-Language", "pt-BR,pt;q=0.9");
    client.DefaultRequestHeaders.Add("Referer", "https://loterias.caixa.gov.br/");
    client.DefaultRequestHeaders.Add("Origin", "https://loterias.caixa.gov.br");
});

var app = builder.Build();

// Carrega histórico base do arquivo local no startup
var historicoPath = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, "..", "..", "Docs", "últimosjogos.md"));

if (File.Exists(historicoPath))
{
    var linhas = File.ReadAllLines(historicoPath);
    var historico = HistoricoSeeder.Parsear(linhas);
    HistoricoStore.Atualizar(historico);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
