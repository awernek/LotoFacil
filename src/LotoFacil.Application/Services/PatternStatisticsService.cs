using LotoFacil.Domain.Models;

namespace LotoFacil.Application.Services;

/// <summary>
/// Analisa distribuições de padrões reais do histórico da Lotofácil:
/// distribuição por faixa (linhas), paridade, sequências e repetição do sorteio anterior.
/// </summary>
public class PatternStatisticsService
{
    private static readonly (int Inicio, int Fim)[] Faixas =
        [(1, 5), (6, 10), (11, 15), (16, 20), (21, 25)];

    /// <summary>
    /// Computa todas as distribuições de padrões a partir do histórico.
    /// </summary>
    private const int MaxTrincas = 500;

    public PatternStatistics Analisar(IReadOnlyList<ResultadoHistorico> historico)
    {
        if (historico.Count == 0)
            return PatternStatistics.Vazio();

        var ordenado = historico.OrderBy(r => r.Concurso).ToList();

        var distribuicaoFaixas = CalcularDistribuicaoFaixas(ordenado);
        var distribuicaoParidade = CalcularDistribuicaoParidade(ordenado);
        var distribuicaoSequencias = CalcularDistribuicaoSequencias(ordenado);
        var distribuicaoRepeticao = CalcularDistribuicaoRepeticao(ordenado);
        var frequenciaIndividual = CalcularFrequenciaIndividual(ordenado);
        var frequenciaPares = CalcularFrequenciaPares(ordenado);
        var frequenciaTrincas = CalcularFrequenciaTrincas(ordenado);

        return new PatternStatistics(
            DistribuicaoFaixas: distribuicaoFaixas,
            DistribuicaoParidade: distribuicaoParidade,
            DistribuicaoSequencias: distribuicaoSequencias,
            DistribuicaoRepeticao: distribuicaoRepeticao,
            FrequenciaIndividual: frequenciaIndividual,
            FrequenciaPares: frequenciaPares,
            FrequenciaTrincas: frequenciaTrincas,
            TotalConcursos: ordenado.Count
        );
    }

    /// <summary>
    /// Calcula o score de aderência de um jogo aos padrões históricos (0 a 1).
    /// </summary>
    public double ScoreAderencia(Jogo jogo, PatternStatistics stats, IReadOnlyList<int>? ultimoSorteio = null)
    {
        if (stats.TotalConcursos == 0) return 0.5;

        double scoreFaixa = ScoreFaixa(jogo, stats);
        double scoreParidade = ScoreParidade(jogo, stats);
        double scoreSequencia = ScoreSequencia(jogo, stats);
        double scoreRepeticao = ultimoSorteio is { Count: > 0 }
            ? ScoreRepeticao(jogo, ultimoSorteio, stats)
            : 0.5;

        // pesos: faixa e paridade são mais discriminantes
        return scoreFaixa * 0.35 + scoreParidade * 0.25 + scoreSequencia * 0.20 + scoreRepeticao * 0.20;
    }

    // ── Distribuição por faixa (padrão tipo "3-3-3-3-3") ─────────────────

    private static Dictionary<string, double> CalcularDistribuicaoFaixas(
        IReadOnlyList<ResultadoHistorico> historico)
    {
        var contagem = new Dictionary<string, int>();
        foreach (var r in historico)
        {
            var padrao = ObterPadraoFaixa(r.Numeros);
            contagem[padrao] = contagem.GetValueOrDefault(padrao) + 1;
        }

        var total = (double)historico.Count;
        return contagem.ToDictionary(kv => kv.Key, kv => kv.Value / total);
    }

    private static string ObterPadraoFaixa(IReadOnlyList<int> numeros)
    {
        var counts = new int[5];
        foreach (var n in numeros)
            counts[(n - 1) / 5]++;
        return string.Join("-", counts);
    }

    private double ScoreFaixa(Jogo jogo, PatternStatistics stats)
    {
        var padrao = ObterPadraoFaixa(jogo.Numeros);
        return stats.DistribuicaoFaixas.GetValueOrDefault(padrao);
    }

    // ── Distribuição de paridade ─────────────────────────────────────────

    private static Dictionary<int, double> CalcularDistribuicaoParidade(
        IReadOnlyList<ResultadoHistorico> historico)
    {
        var contagem = new Dictionary<int, int>();
        foreach (var r in historico)
        {
            var pares = r.Numeros.Count(n => n % 2 == 0);
            contagem[pares] = contagem.GetValueOrDefault(pares) + 1;
        }

        var total = (double)historico.Count;
        return contagem.ToDictionary(kv => kv.Key, kv => kv.Value / total);
    }

    private static double ScoreParidade(Jogo jogo, PatternStatistics stats)
    {
        return stats.DistribuicaoParidade.GetValueOrDefault(jogo.Pares);
    }

    // ── Distribuição de sequências consecutivas ──────────────────────────

    private static Dictionary<int, double> CalcularDistribuicaoSequencias(
        IReadOnlyList<ResultadoHistorico> historico)
    {
        var contagem = new Dictionary<int, int>();
        foreach (var r in historico)
        {
            var seq = ContarSequencias(r.Numeros);
            contagem[seq] = contagem.GetValueOrDefault(seq) + 1;
        }

        var total = (double)historico.Count;
        return contagem.ToDictionary(kv => kv.Key, kv => kv.Value / total);
    }

    private static int ContarSequencias(IReadOnlyList<int> numeros)
    {
        int seq = 0;
        var sorted = numeros.OrderBy(n => n).ToList();
        for (int i = 0; i < sorted.Count - 1; i++)
            if (sorted[i] + 1 == sorted[i + 1]) seq++;
        return seq;
    }

    private static double ScoreSequencia(Jogo jogo, PatternStatistics stats)
    {
        var seq = ContarSequencias(jogo.Numeros);
        return stats.DistribuicaoSequencias.GetValueOrDefault(seq);
    }

    // ── Distribuição de repetição do último sorteio ──────────────────────

    private static Dictionary<int, double> CalcularDistribuicaoRepeticao(
        IReadOnlyList<ResultadoHistorico> historico)
    {
        var contagem = new Dictionary<int, int>();
        for (int i = 1; i < historico.Count; i++)
        {
            var anterior = historico[i - 1].Numeros.ToHashSet();
            var repetidos = historico[i].Numeros.Count(n => anterior.Contains(n));
            contagem[repetidos] = contagem.GetValueOrDefault(repetidos) + 1;
        }

        var total = (double)(historico.Count - 1);
        if (total <= 0) return new();
        return contagem.ToDictionary(kv => kv.Key, kv => kv.Value / total);
    }

    private static double ScoreRepeticao(Jogo jogo, IReadOnlyList<int> ultimoSorteio, PatternStatistics stats)
    {
        var setUltimo = ultimoSorteio.ToHashSet();
        var repetidos = jogo.Numeros.Count(n => setUltimo.Contains(n));
        return stats.DistribuicaoRepeticao.GetValueOrDefault(repetidos);
    }

    // ── Frequência individual de cada número (1–25) ──────────────────────

    private static Dictionary<int, int> CalcularFrequenciaIndividual(
        IReadOnlyList<ResultadoHistorico> historico)
    {
        var freq = Enumerable.Range(1, 25).ToDictionary(n => n, _ => 0);
        foreach (var r in historico)
            foreach (var n in r.Numeros)
                freq[n]++;
        return freq;
    }

    // ── Frequência de pares ──────────────────────────────────────────────

    private static Dictionary<(int, int), int> CalcularFrequenciaPares(
        IReadOnlyList<ResultadoHistorico> historico)
    {
        var freq = new Dictionary<(int, int), int>();
        foreach (var r in historico)
        {
            var nums = r.Numeros;
            for (int i = 0; i < nums.Count; i++)
                for (int j = i + 1; j < nums.Count; j++)
                {
                    var par = (nums[i], nums[j]);
                    freq[par] = freq.GetValueOrDefault(par) + 1;
                }
        }
        return freq;
    }

    // ── Frequência de trincas (top N por performance) ────────────────────

    private static Dictionary<(int, int, int), int> CalcularFrequenciaTrincas(
        IReadOnlyList<ResultadoHistorico> historico)
    {
        var freq = new Dictionary<(int, int, int), int>();
        foreach (var r in historico)
        {
            var nums = r.Numeros;
            for (int i = 0; i < nums.Count; i++)
                for (int j = i + 1; j < nums.Count; j++)
                    for (int k = j + 1; k < nums.Count; k++)
                    {
                        var trinca = (nums[i], nums[j], nums[k]);
                        freq[trinca] = freq.GetValueOrDefault(trinca) + 1;
                    }
        }

        // Retorna apenas top N para manter memória controlada
        return freq
            .OrderByDescending(kv => kv.Value)
            .Take(MaxTrincas)
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }
}

public record PatternStatistics(
    Dictionary<string, double> DistribuicaoFaixas,
    Dictionary<int, double> DistribuicaoParidade,
    Dictionary<int, double> DistribuicaoSequencias,
    Dictionary<int, double> DistribuicaoRepeticao,
    Dictionary<int, int> FrequenciaIndividual,
    Dictionary<(int, int), int> FrequenciaPares,
    Dictionary<(int, int, int), int> FrequenciaTrincas,
    int TotalConcursos
)
{
    public static PatternStatistics Vazio() => new(
        DistribuicaoFaixas: new(),
        DistribuicaoParidade: new(),
        DistribuicaoSequencias: new(),
        DistribuicaoRepeticao: new(),
        FrequenciaIndividual: Enumerable.Range(1, 25).ToDictionary(n => n, _ => 0),
        FrequenciaPares: new(),
        FrequenciaTrincas: new(),
        TotalConcursos: 0
    );
}
