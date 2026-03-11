using LotoFacil.Application.Services;
using LotoFacil.Domain.Models;

namespace LotoFacil.Application.Scoring;

/// <summary>
/// Contexto compartilhado entre todos os componentes de scoring.
/// Contém dados históricos e estatísticos necessários para avaliar jogos.
/// </summary>
public record ScoreContext(
    EstatisticasResultado? Stats = null,
    IReadOnlyList<int>? UltimoResultado = null,
    PatternStatistics? PatternStats = null,
    CorrelationMatrix? CorrelationMatrix = null
);

/// <summary>
/// Componente individual de scoring. Retorna valor positivo (bônus) ou negativo (penalidade).
/// </summary>
public interface IScoreComponent
{
    string Nome { get; }
    double Peso { get; }
    double Calcular(Jogo jogo, ScoreContext context);
}
