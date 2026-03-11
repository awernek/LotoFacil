using LotoFacil.Domain.Models;

namespace LotoFacil.Application.Services;

/// <summary>
/// Simula o gerador em concursos passados usando apenas dados anteriores a cada concurso testado.
/// Útil para validar se a estratégia de geração teria produzido jogos competitivos historicamente.
///
/// Importante: desempenho no backtest NÃO implica desempenho futuro.
/// Cada sorteio é independente — o backtest mede consistência estatística, não poder preditivo.
/// </summary>
public class BacktestService
{
    private readonly EstatisticasService _statsService = new();

    /// <param name="historico">Todos os concursos disponíveis, em qualquer ordem.</param>
    /// <param name="jogosPorConcurso">Jogos gerados para testar contra cada concurso alvo.</param>
    /// <param name="concursosTestados">Quantos dos concursos mais recentes serão usados como alvo.</param>
    public BacktestResultado Executar(
        IReadOnlyList<ResultadoHistorico> historico,
        int jogosPorConcurso = 10,
        int concursosTestados = 30)
    {
        var ordenado = historico.OrderBy(r => r.Concurso).ToList();

        // mínimo de concursos anteriores para calcular estatísticas com sentido
        if (ordenado.Count < 15)
            return BacktestResultado.Insuficiente();

        var inicio = Math.Max(10, ordenado.Count - concursosTestados);
        var resultados = new List<BacktestConcursoResultado>();

        for (int i = inicio; i < ordenado.Count; i++)
        {
            var alvo = ordenado[i];
            var dadosAnteriores = ordenado.Take(i).ToList();

            var stats = _statsService.Analisar(dadosAnteriores);
            var pesos = _statsService.ObterPesosInteligentes(stats);

            var gerador = new GeradorDeJogos(new FiltroService());
            gerador.DefinirPesosInteligentes(pesos);
            gerador.DefinirContextoHistorico(stats, dadosAnteriores[^1].Numeros);

            var geracao = gerador.Gerar(jogosPorConcurso);

            var alvoSet = alvo.Numeros.ToHashSet();
            var hitsPorJogo = geracao.Jogos
                .Select(j => j.Numeros.Count(n => alvoSet.Contains(n)))
                .ToList();

            if (hitsPorJogo.Count > 0)
                resultados.Add(new BacktestConcursoResultado(
                    Concurso: alvo.Concurso,
                    MelhorAcerto: hitsPorJogo.Max(),
                    MediaAcertosPorJogo: hitsPorJogo.Average(),
                    HitsPorJogo: hitsPorJogo
                ));
        }

        return new BacktestResultado(resultados);
    }
    /// <summary>
    /// Backtest comparativo: executa a estratégia do sistema vs geração puramente aleatória.
    /// </summary>
    public BacktestComparativo ExecutarComparativo(
        IReadOnlyList<ResultadoHistorico> historico,
        int jogosPorConcurso = 10,
        int concursosTestados = 30)
    {
        var sistema = Executar(historico, jogosPorConcurso, concursosTestados);
        var aleatorio = ExecutarAleatorio(historico, jogosPorConcurso, concursosTestados);
        return new BacktestComparativo(sistema, aleatorio);
    }

    private BacktestResultado ExecutarAleatorio(
        IReadOnlyList<ResultadoHistorico> historico,
        int jogosPorConcurso,
        int concursosTestados)
    {
        var ordenado = historico.OrderBy(r => r.Concurso).ToList();
        if (ordenado.Count < 15)
            return BacktestResultado.Insuficiente();

        var inicio = Math.Max(10, ordenado.Count - concursosTestados);
        var resultados = new List<BacktestConcursoResultado>();

        for (int i = inicio; i < ordenado.Count; i++)
        {
            var alvo = ordenado[i];
            var jogos = new List<Jogo>();

            for (int j = 0; j < jogosPorConcurso; j++)
            {
                var numeros = Enumerable.Range(1, 25)
                    .OrderBy(_ => Random.Shared.Next())
                    .Take(15)
                    .ToList();
                jogos.Add(Jogo.Criar(numeros));
            }

            var alvoSet = alvo.Numeros.ToHashSet();
            var hitsPorJogo = jogos
                .Select(jg => jg.Numeros.Count(n => alvoSet.Contains(n)))
                .ToList();

            if (hitsPorJogo.Count > 0)
                resultados.Add(new BacktestConcursoResultado(
                    Concurso: alvo.Concurso,
                    MelhorAcerto: hitsPorJogo.Max(),
                    MediaAcertosPorJogo: hitsPorJogo.Average(),
                    HitsPorJogo: hitsPorJogo
                ));
        }

        return new BacktestResultado(resultados);
    }
}

public record BacktestComparativo(BacktestResultado Sistema, BacktestResultado Aleatorio)
{
    public double VantagemMediaMelhorAcerto =>
        Sistema.MediaMelhorAcerto - Aleatorio.MediaMelhorAcerto;

    public double VantagemPercentualPremio =>
        Sistema.PercentualComPremio - Aleatorio.PercentualComPremio;
}

public record BacktestConcursoResultado(
    int Concurso,
    int MelhorAcerto,
    double MediaAcertosPorJogo,
    IReadOnlyList<int> HitsPorJogo
);

public record BacktestResultado(IReadOnlyList<BacktestConcursoResultado> Resultados)
{
    public static BacktestResultado Insuficiente() => new([]);

    public bool TemDados => Resultados.Count > 0;

    /// Média do melhor jogo gerado por concurso testado
    public double MediaMelhorAcerto =>
        TemDados ? Resultados.Average(r => r.MelhorAcerto) : 0;

    /// Média de acertos de todos os jogos gerados em todos os concursos testados
    public double MediaGeralAcertos =>
        TemDados ? Resultados.Average(r => r.MediaAcertosPorJogo) : 0;

    /// Distribuição: quantos concursos tiveram melhor jogo com X acertos
    public Dictionary<int, int> DistribuicaoMelhorAcerto =>
        Resultados
            .GroupBy(r => r.MelhorAcerto)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.Count());

    /// Percentual de concursos em que o melhor jogo acertou >= 11
    public double PercentualComPremio =>
        TemDados
            ? Resultados.Count(r => r.MelhorAcerto >= 11) * 100.0 / Resultados.Count
            : 0;

    /// Distribuição detalhada de acertos por faixa (11, 12, 13, 14, 15) — todos os jogos
    public Dictionary<int, int> DistribuicaoHits
    {
        get
        {
            var dist = new Dictionary<int, int>();
            foreach (var faixa in new[] { 11, 12, 13, 14, 15 })
                dist[faixa] = Resultados.Sum(r => r.HitsPorJogo.Count(h => h == faixa));
            return dist;
        }
    }
}
