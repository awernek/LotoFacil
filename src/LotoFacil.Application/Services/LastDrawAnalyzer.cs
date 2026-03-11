using LotoFacil.Domain.Models;

namespace LotoFacil.Application.Services;

/// <summary>
/// Modela probabilisticamente a repetição de números do último sorteio.
/// Lotofácil tipicamente repete 8–11 números entre sorteios consecutivos.
/// Gera pesos adicionais para biasing realista na geração.
/// </summary>
public class LastDrawAnalyzer
{
    /// <summary>
    /// Analisa o padrão de repetição consecutiva a partir do histórico.
    /// </summary>
    public LastDrawProfile Analisar(IReadOnlyList<ResultadoHistorico> historico)
    {
        if (historico.Count < 2)
            return LastDrawProfile.Padrao();

        var ordenado = historico.OrderBy(r => r.Concurso).ToList();
        var repeticoes = new List<int>();

        for (int i = 1; i < ordenado.Count; i++)
        {
            var anterior = ordenado[i - 1].Numeros.ToHashSet();
            var repetidos = ordenado[i].Numeros.Count(n => anterior.Contains(n));
            repeticoes.Add(repetidos);
        }

        var media = repeticoes.Average();
        var desvioPadrao = Math.Sqrt(repeticoes.Average(r => Math.Pow(r - media, 2)));

        // distribuição de probabilidade por quantidade de repetições
        var distribuicao = new Dictionary<int, double>();
        var total = (double)repeticoes.Count;
        foreach (var g in repeticoes.GroupBy(r => r))
            distribuicao[g.Key] = g.Count() / total;

        return new LastDrawProfile(media, desvioPadrao, distribuicao);
    }

    /// <summary>
    /// Gera pesos de bias para os 25 números considerando o último sorteio.
    /// Números que estavam no último sorteio recebem peso maior, proporcional
    /// à probabilidade de repetição.
    /// </summary>
    public Dictionary<int, double> GerarPesosBias(
        IReadOnlyList<int> ultimoSorteio,
        LastDrawProfile profile)
    {
        var pesos = new Dictionary<int, double>(25);
        var setUltimo = ultimoSorteio.ToHashSet();

        // Proporção esperada de repetição: média / 15
        var taxaRepeticao = profile.MediaRepeticao / 15.0;

        // Peso relativo: números do último sorteio recebem boost
        // Números novos recebem peso complementar
        double pesoRepetido = 1.0 + taxaRepeticao;    // ~1.6 para média de 9
        double pesoNovo = 1.0 - taxaRepeticao * 0.5;  // ~0.7 para média de 9

        for (int n = 1; n <= 25; n++)
            pesos[n] = setUltimo.Contains(n) ? pesoRepetido : pesoNovo;

        return pesos;
    }
}

public record LastDrawProfile(
    double MediaRepeticao,
    double DesvioPadrao,
    Dictionary<int, double> DistribuicaoRepeticao
)
{
    /// Perfil padrão quando não há dados suficientes (média histórica ≈ 9).
    public static LastDrawProfile Padrao() => new(
        MediaRepeticao: 9.0,
        DesvioPadrao: 1.5,
        DistribuicaoRepeticao: new()
        {
            [7] = 0.08, [8] = 0.18, [9] = 0.28,
            [10] = 0.24, [11] = 0.14, [12] = 0.06
        }
    );
}
