using LotoFacil.Domain.Models;

namespace LotoFacil.Application.Services;

public static class HistoricoSeeder
{
    /// <summary>
    /// Parseia linhas no formato: "3632 - 01 02 03 05 06 09 10 15 17 19 20 21 22 23 25"
    /// </summary>
    public static IReadOnlyList<ResultadoHistorico> Parsear(IEnumerable<string> linhas)
    {
        var resultados = new List<ResultadoHistorico>();

        foreach (var linha in linhas)
        {
            var texto = linha.Trim();
            if (string.IsNullOrWhiteSpace(texto)) continue;

            var partes = texto.Split(" - ", 2);
            if (partes.Length != 2) continue;

            if (!int.TryParse(partes[0].Trim(), out var concurso)) continue;

            var numeros = partes[1]
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(n => int.TryParse(n, out var num) ? num : -1)
                .Where(n => n >= 1 && n <= 25)
                .OrderBy(n => n)
                .ToList();

            if (numeros.Count != 15) continue;

            resultados.Add(new ResultadoHistorico(concurso, DateTime.MinValue, numeros));
        }

        return resultados.OrderByDescending(r => r.Concurso).ToList();
    }
}
