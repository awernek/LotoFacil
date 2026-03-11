using LotoFacil.Domain.Models;

namespace LotoFacil.Application.Services;

public class GeradorDeJogos(FiltroService filtroService)
{
    private const int TotalNumeros = 25;
    private const int NumerosPorJogo = 15;
    private const int MaxTentativasPorJogo = 10_000;

    private Dictionary<int, double>? _pesosInteligentes;
    private EstatisticasResultado? _statsHistoricas;
    private IReadOnlyList<int>? _ultimoResultado;
    private PatternStatistics? _patternStats;
    private CorrelationMatrix? _correlationMatrix;
    private LastDrawProfile? _lastDrawProfile;

    public void DefinirPesosInteligentes(Dictionary<int, double> pesos)
    {
        _pesosInteligentes = pesos;
    }

    public void DefinirContextoHistorico(EstatisticasResultado stats, IReadOnlyList<int> ultimoResultado)
    {
        _statsHistoricas = stats;
        _ultimoResultado = ultimoResultado;
    }

    public void DefinirContextoEstatistico(
        PatternStatistics? patternStats,
        CorrelationMatrix? correlationMatrix,
        LastDrawProfile? lastDrawProfile)
    {
        _patternStats = patternStats;
        _correlationMatrix = correlationMatrix;
        _lastDrawProfile = lastDrawProfile;
    }

    public ResultadoGeracao Gerar(int quantidade)
    {
        var jogos = new List<Jogo>();
        var duplicatas = new HashSet<string>();
        int tentativasTotal = 0;
        int descartados = 0;
        int numDuplicatas = 0;
        var pesosCombinados = CombinarPesos();

        while (jogos.Count < quantidade)
        {
            tentativasTotal++;

            if (tentativasTotal > quantidade * MaxTentativasPorJogo)
                break;

            var numeros = pesosCombinados is not null
                ? SortearPonderado(pesosCombinados, NumerosPorJogo)
                : Enumerable.Range(1, TotalNumeros).OrderBy(_ => Random.Shared.Next()).Take(NumerosPorJogo).ToList();

            var jogo = Jogo.Criar(numeros);

            if (duplicatas.Contains(jogo.Chave))
            {
                descartados++;
                numDuplicatas++;
                continue;
            }

            if (!filtroService.Validar(jogo))
            {
                descartados++;
                continue;
            }

            duplicatas.Add(jogo.Chave);
            jogos.Add(jogo);
        }

        for (int i = jogos.Count - 1; i > 0; i--)
        {
            var j = Random.Shared.Next(i + 1);
            (jogos[i], jogos[j]) = (jogos[j], jogos[i]);
        }

        var scoreMedio = jogos.Count > 0
            ? jogos.Average(j => CalcularScoreCompleto(j))
            : 0;

        return new ResultadoGeracao(jogos, tentativasTotal, descartados, scoreMedio,
            CalcularDiversidadeMedia(jogos), numDuplicatas);
    }

    /// <summary>
    /// Pipeline completo: CandidateGeneration → Filtering → Scoring → Ranking → DiversitySelection → Optimization
    /// </summary>
    public ResultadoGeracao GerarRanqueado(int quantidade, int tamanhoPool)
    {
        // ── 1. Candidate Generation + Filtering ──────────────────────────
        var candidatos = GerarCandidatos(tamanhoPool);
        int tentativasTotal = candidatos.TentativasTotal;

        if (candidatos.Jogos.Count == 0)
            return new ResultadoGeracao([], tentativasTotal, 0);

        // ── 2. Scoring ───────────────────────────────────────────────────
        var scored = candidatos.Jogos
            .Select(j => (Jogo: j, Score: CalcularScoreCompleto(j)))
            .ToList();

        // ── 3. Ranking ───────────────────────────────────────────────────
        var ordenados = scored.OrderByDescending(x => x.Score).ToList();

        // ── 4. Diversity Selection (Jaccard) ─────────────────────────────
        var selecionados = SelecionarComDiversidade(ordenados, quantidade);

        // ── 5. Optimization (genetic) ────────────────────────────────────
        if (selecionados.Count >= 4)
        {
            var optimizer = new GeneticGameOptimizer(CalcularScoreCompleto);
            var otimizados = optimizer.Otimizar(
                populacaoInicial: selecionados,
                geracoes: 200,
                tamanhoPopulacao: Math.Max(selecionados.Count * 4, 500));

            selecionados = SelecionarComDiversidade(otimizados, quantidade);
        }

        var scoreMedioR = selecionados.Count > 0
            ? selecionados.Average(j => CalcularScoreCompleto(j))
            : 0;

        return new ResultadoGeracao(selecionados, tentativasTotal,
            tentativasTotal - candidatos.Jogos.Count,
            scoreMedioR, CalcularDiversidadeMedia(selecionados));
    }

    private (List<Jogo> Jogos, int TentativasTotal) GerarCandidatos(int maxCandidatos)
    {
        var candidatos = new List<Jogo>();
        var duplicatas = new HashSet<string>();
        int tentativasTotal = 0;
        var pesosCombinados = CombinarPesos();

        while (candidatos.Count < maxCandidatos && tentativasTotal < maxCandidatos * 3)
        {
            tentativasTotal++;

            var numeros = pesosCombinados is not null
                ? SortearPonderado(pesosCombinados, NumerosPorJogo)
                : Enumerable.Range(1, TotalNumeros).OrderBy(_ => Random.Shared.Next()).Take(NumerosPorJogo).ToList();

            var jogo = Jogo.Criar(numeros);

            if (duplicatas.Contains(jogo.Chave)) continue;
            if (!filtroService.Validar(jogo)) continue;

            duplicatas.Add(jogo.Chave);
            candidatos.Add(jogo);
        }

        return (candidatos, tentativasTotal);
    }

    private static List<Jogo> SelecionarComDiversidade(
        List<(Jogo Jogo, double Score)> ordenados, int quantidade)
    {
        var selecionados = new List<Jogo>();
        if (ordenados.Count == 0) return selecionados;

        const int topParaSorteio = 5;
        var idxPrimeiro = Random.Shared.Next(Math.Min(topParaSorteio, ordenados.Count));
        selecionados.Add(ordenados[idxPrimeiro].Jogo);
        var restantes = ordenados.Where((c, i) => i != idxPrimeiro).ToList();

        while (selecionados.Count < quantidade && restantes.Count > 0)
        {
            var comDiversidade = restantes
                .Select(c => (Candidato: c, MinJaccard: selecionados.Min(s => GameScorer.Jaccard(c.Jogo, s))))
                .OrderByDescending(x => x.MinJaccard)
                .ThenByDescending(x => x.Candidato.Score)
                .ToList();

            var k = Math.Min(topParaSorteio, comDiversidade.Count);
            var escolhido = comDiversidade[Random.Shared.Next(k)];
            selecionados.Add(escolhido.Candidato.Jogo);
            restantes.Remove(escolhido.Candidato);
        }

        return selecionados;
    }

    private static List<int> SortearPonderado(Dictionary<int, double> pesos, int quantidade)
    {
        var pool = new Dictionary<int, double>(pesos);
        var resultado = new List<int>(quantidade);
        var totalPeso = pool.Values.Sum(); // calculado uma vez; mantido via subtração

        for (int i = 0; i < quantidade && pool.Count > 0; i++)
        {
            var corte = Random.Shared.NextDouble() * totalPeso;
            double acumulado = 0;
            int escolhido = pool.Keys.Last();

            foreach (var (numero, peso) in pool)
            {
                acumulado += peso;
                escolhido = numero;
                if (acumulado >= corte) break;
            }

            resultado.Add(escolhido);
            totalPeso -= pool[escolhido];
            pool.Remove(escolhido);
        }

        return resultado;
    }

    private double CalcularScoreCompleto(Jogo jogo) =>
        GameScorer.CalcularScore(jogo, _statsHistoricas, _ultimoResultado, _patternStats, _correlationMatrix);

    /// <summary>
    /// Combina pesos inteligentes com bias do último sorteio (LastDrawAnalyzer).
    /// </summary>
    private Dictionary<int, double>? CombinarPesos()
    {
        if (_pesosInteligentes is null) return null;

        if (_lastDrawProfile is null || _ultimoResultado is null)
            return _pesosInteligentes;

        var analyzer = new LastDrawAnalyzer();
        var biasPesos = analyzer.GerarPesosBias(_ultimoResultado, _lastDrawProfile);

        // multiplicar pesos inteligentes pelo bias do último sorteio
        var combinados = new Dictionary<int, double>(_pesosInteligentes.Count);
        foreach (var (numero, peso) in _pesosInteligentes)
            combinados[numero] = peso * biasPesos.GetValueOrDefault(numero, 1.0);

        return combinados;
    }

    private static double CalcularDiversidadeMedia(List<Jogo> jogos)
    {
        if (jogos.Count < 2) return 0;
        double soma = 0;
        int pares = 0;
        for (int i = 0; i < jogos.Count; i++)
            for (int j = i + 1; j < jogos.Count; j++)
            {
                soma += GameScorer.Jaccard(jogos[i], jogos[j]);
                pares++;
            }
        return soma / pares;
    }
}

public record ResultadoGeracao(
    IReadOnlyList<Jogo> Jogos,
    int TentativasTotal,
    int Descartados,
    double ScoreMedio = 0,
    double DiversidadeMedia = 0,
    int Duplicatas = 0
)
{
    public double TaxaAprovacao => TentativasTotal > 0
        ? (double)Jogos.Count / TentativasTotal * 100
        : 0;

    public double TaxaDescarte => TentativasTotal > 0
        ? (double)Descartados / TentativasTotal * 100
        : 0;

    public double TaxaDuplicatas => TentativasTotal > 0
        ? (double)Duplicatas / TentativasTotal * 100
        : 0;

    /// Índice de diversidade Jaccard médio entre todos os pares de jogos (0 a 1).
    public double IndiceDiversidade => DiversidadeMedia;
}
