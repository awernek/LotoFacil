using LotoFacil.Application.Services;
using LotoFacil.Domain.Models;

namespace LotoFacil.Application.Scoring;

public class ParityPenaltyComponent : IScoreComponent
{
    public string Nome => "Paridade";
    public double Peso { get; init; } = 1.0;

    public double Calcular(Jogo jogo, ScoreContext context)
    {
        const double ideal = 7.5;
        return -Math.Min(15, Math.Abs(jogo.Pares - ideal) * 3);
    }
}

public class SumPenaltyComponent : IScoreComponent
{
    public string Nome => "Soma";
    public double Peso { get; init; } = 1.0;

    public double Calcular(Jogo jogo, ScoreContext context)
    {
        const double ideal = 197;
        return -Math.Min(20, Math.Abs(jogo.Soma - ideal) * 0.3);
    }
}

public class RangeDistributionPenaltyComponent : IScoreComponent
{
    public string Nome => "Distribuição por Faixas";
    public double Peso { get; init; } = 1.0;

    public double Calcular(Jogo jogo, ScoreContext context)
    {
        const int idealPorFaixa = 3;
        double penalidade = 0;
        foreach (var (inicio, fim) in FiltroFaixas.Faixas)
        {
            var count = jogo.Numeros.Count(n => n >= inicio && n <= fim);
            penalidade += Math.Abs(count - idealPorFaixa) * 1.5;
        }
        return -Math.Min(15, penalidade);
    }
}

public class HighNumbersPenaltyComponent : IScoreComponent
{
    public string Nome => "Números Altos";
    public double Peso { get; init; } = 1.0;

    public double Calcular(Jogo jogo, ScoreContext context)
    {
        var altos = jogo.Numeros.Count(n => n >= 22);
        return altos > 3 ? -Math.Min(10, (altos - 3) * 5.0) : 0;
    }
}

public class SequencePenaltyComponent : IScoreComponent
{
    public string Nome => "Sequências";
    public double Peso { get; init; } = 1.0;

    public double Calcular(Jogo jogo, ScoreContext context)
    {
        int seq = 0;
        for (int i = 0; i < jogo.Numeros.Count - 1; i++)
            if (jogo.Numeros[i] + 1 == jogo.Numeros[i + 1]) seq++;

        return seq > 11 ? -Math.Min(10, (seq - 11) * 3.0) : 0;
    }
}

public class FrequencyScoreComponent : IScoreComponent
{
    public string Nome => "Frequência";
    public double Peso { get; init; } = 1.0;

    public double Calcular(Jogo jogo, ScoreContext context)
    {
        if (context.Stats is not { TotalConcursos: > 0 }) return 0;

        var stats = context.Stats;
        var maxFreq = stats.Frequencia.Values.Max();
        var minFreq = stats.Frequencia.Values.Min();
        var range = maxFreq - minFreq;
        if (range == 0) return 7.5;

        var freqNorm = jogo.Numeros.Average(n =>
            (double)(stats.Frequencia.GetValueOrDefault(n) - minFreq) / range);

        return freqNorm * 15;
    }
}

public class LastDrawDistanceComponent : IScoreComponent
{
    public string Nome => "Distância Último Sorteio";
    public double Peso { get; init; } = 1.0;

    public double Calcular(Jogo jogo, ScoreContext context)
    {
        if (context.UltimoResultado is not { Count: > 0 }) return 0;

        var setUltimo = context.UltimoResultado.ToHashSet();
        var overlap = jogo.Numeros.Count(n => setUltimo.Contains(n));
        const int idealOverlap = 9;
        var distancia = Math.Abs(overlap - idealOverlap);
        return Math.Max(0, 10 - distancia * 2.0);
    }
}

public class PatternAdherenceComponent : IScoreComponent
{
    public string Nome => "Aderência a Padrões";
    public double Peso { get; init; } = 1.0;

    public double Calcular(Jogo jogo, ScoreContext context)
    {
        if (context.PatternStats is not { TotalConcursos: > 0 }) return 0;

        var service = new PatternStatisticsService();
        var aderencia = service.ScoreAderencia(jogo, context.PatternStats, context.UltimoResultado);
        return aderencia * 12;
    }
}

public class PairCorrelationScoreComponent : IScoreComponent
{
    public string Nome => "Correlação de Pares";
    public double Peso { get; init; } = 1.0;

    public double Calcular(Jogo jogo, ScoreContext context)
    {
        if (context.CorrelationMatrix is not { TotalConcursos: > 0 }) return 0;

        var score = context.CorrelationMatrix.ScoreCorrelacao(jogo);
        return score * 8;
    }
}
