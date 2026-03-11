using LotoFacil.Application.Scoring;
using LotoFacil.Domain.Models;

namespace LotoFacil.Application.Services;

public static class GameScorer
{
    // Componentes registrados na ordem: penalidades → bônus
    private static readonly IScoreComponent[] DefaultComponents =
    [
        new ParityPenaltyComponent(),
        new SumPenaltyComponent(),
        new RangeDistributionPenaltyComponent(),
        new HighNumbersPenaltyComponent(),
        new SequencePenaltyComponent(),
        new FrequencyScoreComponent(),
        new LastDrawDistanceComponent(),
        new PatternAdherenceComponent(),
        new PairCorrelationScoreComponent(),
    ];

    /// <summary>
    /// Calcula score usando todos os componentes padrão (backward-compatible).
    /// </summary>
    public static double CalcularScore(
        Jogo jogo,
        EstatisticasResultado? stats = null,
        IReadOnlyList<int>? ultimoResultado = null,
        PatternStatistics? patternStats = null,
        CorrelationMatrix? correlationMatrix = null)
    {
        var context = new ScoreContext(stats, ultimoResultado, patternStats, correlationMatrix);
        return CalcularScore(jogo, context, DefaultComponents);
    }

    /// <summary>
    /// Calcula score usando componentes customizados e contexto explícito.
    /// </summary>
    public static double CalcularScore(Jogo jogo, ScoreContext context, IReadOnlyList<IScoreComponent> components)
    {
        double score = 100;
        foreach (var component in components)
            score += component.Calcular(jogo, context) * component.Peso;

        return Math.Clamp(score, 0, 100);
    }

    /// <summary>
    /// Retorna a decomposição detalhada do score por componente.
    /// </summary>
    public static IReadOnlyList<(string Nome, double Contribuicao)> Decompor(
        Jogo jogo,
        ScoreContext context,
        IReadOnlyList<IScoreComponent>? components = null)
    {
        var comps = components ?? DefaultComponents;
        return comps.Select(c => (c.Nome, c.Calcular(jogo, context) * c.Peso)).ToList();
    }

    // ── diversidade ──────────────────────────────────────────────────────────

    public static int Overlap(Jogo a, Jogo b)
    {
        var setB = b.Numeros.ToHashSet();
        return a.Numeros.Count(setB.Contains);
    }

    /// <summary>
    /// Distância de Jaccard: 1 - (interseção / união). Quanto maior, mais diversos.
    /// </summary>
    public static double Jaccard(Jogo a, Jogo b)
    {
        var setA = a.Numeros.ToHashSet();
        var setB = b.Numeros.ToHashSet();
        var intersecao = setA.Count(setB.Contains);
        var uniao = setA.Count + setB.Count - intersecao;
        return uniao > 0 ? 1.0 - (double)intersecao / uniao : 0;
    }

    public static int Diversidade(Jogo a, Jogo b) => 15 - Overlap(a, b);
}
