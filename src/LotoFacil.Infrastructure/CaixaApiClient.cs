using System.Text.Json;
using LotoFacil.Domain.Interfaces;
using LotoFacil.Domain.Models;

namespace LotoFacil.Infrastructure;

public class CaixaApiClient(HttpClient httpClient) : ICaixaApiClient
{
    private const string BaseUrl = "https://servicebus2.caixa.gov.br/portaldeloterias/api/lotofacil";
    private const int TamanhoBatch = 5;

    public async Task<ResultadoHistorico?> ObterUltimoResultadoAsync()
    {
        try
        {
            var json = await httpClient.GetStringAsync(BaseUrl);
            return ParseResultado(json);
        }
        catch
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<ResultadoHistorico>> ObterResultadosAsync(int quantidade)
    {
        var resultados = new List<ResultadoHistorico>();

        var ultimo = await ObterUltimoResultadoAsync();
        if (ultimo is null) return resultados;

        resultados.Add(ultimo);

        var concursoAtual = ultimo.Concurso - 1;
        var restantes = Math.Min(quantidade - 1, concursoAtual);

        for (int offset = 0; offset < restantes; offset += TamanhoBatch)
        {
            var tamanhoAtual = Math.Min(TamanhoBatch, restantes - offset);
            var batch = Enumerable.Range(0, tamanhoAtual)
                .Select(i => ObterResultadoPorConcursoAsync(concursoAtual - offset - i))
                .ToList();

            var resultadosBatch = await Task.WhenAll(batch);
            resultados.AddRange(resultadosBatch.Where(r => r is not null)!);
        }

        return resultados.OrderByDescending(r => r.Concurso).ToList();
    }

    private async Task<ResultadoHistorico?> ObterResultadoPorConcursoAsync(int concurso)
    {
        try
        {
            var json = await httpClient.GetStringAsync($"{BaseUrl}/{concurso}");
            return ParseResultado(json);
        }
        catch
        {
            return null;
        }
    }

    private static ResultadoHistorico? ParseResultado(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var concurso = root.GetProperty("numero").GetInt32();

            var dataStr = root.GetProperty("dataApuracao").GetString();
            var data = DateTime.TryParse(dataStr, out var dt) ? dt : DateTime.MinValue;

            var dezenas = root.GetProperty("listaDezenas")
                .EnumerateArray()
                .Select(d => int.Parse(d.GetString()!))
                .OrderBy(n => n)
                .ToList();

            return new ResultadoHistorico(concurso, data, dezenas);
        }
        catch
        {
            return null;
        }
    }
}
