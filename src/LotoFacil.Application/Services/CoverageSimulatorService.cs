using LotoFacil.Domain.Models;

namespace LotoFacil.Application.Services;

/// <summary>
/// Simula quantos jogos são necessários para cobrir combinações da Lotofácil
/// com alta probabilidade. Roda simulação de força bruta com sorteios aleatórios.
/// </summary>
public class CoverageSimulatorService
{
    private const int TotalNumeros = 25;
    private const int NumerosPorJogo = 15;

    /// <summary>
    /// Simula cobertura para diferentes quantidades de jogos.
    /// </summary>
    /// <param name="jogos">Jogos gerados pelo sistema para testar.</param>
    /// <param name="sorteiosSimulados">Quantidade de sorteios aleatórios para simular.</param>
    public CoverageResult Simular(IReadOnlyList<Jogo> jogos, int sorteiosSimulados = 100_000)
    {
        var quantidades = new[] { 10, 25, 50, 100, 200, 500 };
        var resultados = new List<CoverageFaixa>();

        foreach (var qtd in quantidades.Where(q => q <= jogos.Count))
        {
            var subset = jogos.Take(qtd).ToList();
            var faixa = SimularParaQuantidade(subset, sorteiosSimulados);
            resultados.Add(faixa);
        }

        // Sempre testar o total de jogos fornecidos se não estiver na lista
        if (!quantidades.Contains(jogos.Count) && jogos.Count > 0)
        {
            var faixaTotal = SimularParaQuantidade(jogos, sorteiosSimulados);
            resultados.Add(faixaTotal);
        }

        resultados.Sort((a, b) => a.QuantidadeJogos.CompareTo(b.QuantidadeJogos));

        return new CoverageResult(resultados, sorteiosSimulados);
    }

    /// <summary>
    /// Simula cobertura para uma única quantidade de jogos.
    /// </summary>
    public CoverageFaixa SimularParaQuantidade(
        IReadOnlyList<Jogo> jogos, int sorteiosSimulados = 100_000)
    {
        // Pré-computar HashSets para performance
        var jogosSets = jogos.Select(j => j.Numeros.ToHashSet()).ToList();
        var pool = Enumerable.Range(1, TotalNumeros).ToArray();

        int acertos11 = 0, acertos12 = 0, acertos13 = 0, acertos14 = 0, acertos15 = 0;

        for (int s = 0; s < sorteiosSimulados; s++)
        {
            // Gerar sorteio aleatório
            Shuffle(pool);
            var sorteio = new HashSet<int>(pool.Take(NumerosPorJogo));

            // Encontrar melhor acerto entre todos os jogos
            int melhorHits = 0;
            foreach (var jogoSet in jogosSets)
            {
                int hits = 0;
                foreach (var n in jogoSet)
                    if (sorteio.Contains(n)) hits++;

                if (hits > melhorHits)
                {
                    melhorHits = hits;
                    if (melhorHits == NumerosPorJogo) break; // Máximo atingido
                }
            }

            if (melhorHits >= 11) acertos11++;
            if (melhorHits >= 12) acertos12++;
            if (melhorHits >= 13) acertos13++;
            if (melhorHits >= 14) acertos14++;
            if (melhorHits >= 15) acertos15++;
        }

        var total = (double)sorteiosSimulados;
        return new CoverageFaixa(
            QuantidadeJogos: jogos.Count,
            ProbabilidadeMinimo11: acertos11 / total * 100,
            ProbabilidadeMinimo12: acertos12 / total * 100,
            ProbabilidadeMinimo13: acertos13 / total * 100,
            ProbabilidadeMinimo14: acertos14 / total * 100,
            ProbabilidadeMinimo15: acertos15 / total * 100
        );
    }

    private static void Shuffle(int[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = Random.Shared.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}

public record CoverageFaixa(
    int QuantidadeJogos,
    double ProbabilidadeMinimo11,
    double ProbabilidadeMinimo12,
    double ProbabilidadeMinimo13,
    double ProbabilidadeMinimo14,
    double ProbabilidadeMinimo15
);

public record CoverageResult(
    IReadOnlyList<CoverageFaixa> Faixas,
    int SorteiosSimulados
);
