using LotoFacil.Domain.Models;

namespace LotoFacil.Application.Services;

/// <summary>
/// Otimizador genético que evolui jogos candidatos via seleção, crossover e mutação
/// para maximizar o score estatístico.
/// </summary>
public class GeneticGameOptimizer
{
    private const int TotalNumeros = 25;
    private const int NumerosPorJogo = 15;

    private readonly Func<Jogo, double> _scoreFn;

    public GeneticGameOptimizer(Func<Jogo, double> scoreFn)
    {
        _scoreFn = scoreFn;
    }

    /// <summary>
    /// Executa o algoritmo genético para otimizar uma população de jogos.
    /// </summary>
    /// <param name="populacaoInicial">Jogos candidatos iniciais.</param>
    /// <param name="geracoes">Número de gerações de evolução.</param>
    /// <param name="tamanhoPopulacao">Tamanho da população por geração.</param>
    /// <param name="taxaMutacao">Probabilidade de mutação por indivíduo (0-1).</param>
    public List<(Jogo Jogo, double Score)> Otimizar(
        IReadOnlyList<Jogo> populacaoInicial,
        int geracoes = 200,
        int tamanhoPopulacao = 500,
        double taxaMutacao = 0.15)
    {
        // Scorer + rankear população inicial
        var populacao = populacaoInicial
            .Select(j => (Jogo: j, Score: _scoreFn(j)))
            .OrderByDescending(x => x.Score)
            .Take(tamanhoPopulacao)
            .ToList();

        if (populacao.Count < 4) return populacao;

        for (int gen = 0; gen < geracoes; gen++)
        {
            var novaGeracao = new List<(Jogo Jogo, double Score)>();
            var chaves = new HashSet<string>(populacao.Select(p => p.Jogo.Chave));

            // Elitismo: manter top 20%
            int elite = Math.Max(2, tamanhoPopulacao / 5);
            novaGeracao.AddRange(populacao.Take(elite));

            // Preencher restante com crossover + mutação
            while (novaGeracao.Count < tamanhoPopulacao)
            {
                var pai1 = SelecionarPorTorneio(populacao);
                var pai2 = SelecionarPorTorneio(populacao);

                var filho = Crossover(pai1.Jogo, pai2.Jogo);

                if (Random.Shared.NextDouble() < taxaMutacao)
                    filho = Mutar(filho);

                if (!chaves.Contains(filho.Chave))
                {
                    chaves.Add(filho.Chave);
                    novaGeracao.Add((filho, _scoreFn(filho)));
                }
            }

            populacao = novaGeracao
                .OrderByDescending(x => x.Score)
                .Take(tamanhoPopulacao)
                .ToList();
        }

        return populacao;
    }

    /// <summary>
    /// Seleção por torneio: pega 3 aleatórios e retorna o melhor.
    /// </summary>
    private static (Jogo Jogo, double Score) SelecionarPorTorneio(
        List<(Jogo Jogo, double Score)> populacao)
    {
        const int tamanhoTorneio = 3;
        (Jogo Jogo, double Score) melhor = populacao[Random.Shared.Next(populacao.Count)];

        for (int i = 1; i < tamanhoTorneio; i++)
        {
            var candidato = populacao[Random.Shared.Next(populacao.Count)];
            if (candidato.Score > melhor.Score)
                melhor = candidato;
        }

        return melhor;
    }

    /// <summary>
    /// Crossover: combina números de dois pais, mantendo 15 números válidos.
    /// Pega a interseção de ambos e completa com exclusivos de cada pai aleatoriamente.
    /// </summary>
    private static Jogo Crossover(Jogo pai1, Jogo pai2)
    {
        var set1 = pai1.Numeros.ToHashSet();
        var set2 = pai2.Numeros.ToHashSet();

        // Números que ambos têm
        var comuns = set1.Intersect(set2).ToList();

        // Números exclusivos de cada pai
        var exclusivos1 = set1.Except(set2).ToList();
        var exclusivos2 = set2.Except(set1).ToList();

        var todosExclusivos = exclusivos1.Concat(exclusivos2)
            .OrderBy(_ => Random.Shared.Next())
            .ToList();

        var resultado = new List<int>(comuns);
        int faltam = NumerosPorJogo - resultado.Count;

        // Preenche com exclusivos aleatorizados
        resultado.AddRange(todosExclusivos.Take(faltam));

        // Se ainda faltam (improvável), completa com números disponíveis
        if (resultado.Count < NumerosPorJogo)
        {
            var disponiveis = Enumerable.Range(1, TotalNumeros)
                .Where(n => !resultado.Contains(n))
                .OrderBy(_ => Random.Shared.Next())
                .Take(NumerosPorJogo - resultado.Count);
            resultado.AddRange(disponiveis);
        }

        return Jogo.Criar(resultado);
    }

    /// <summary>
    /// Mutação: substitui 1 ou 2 números por números não presentes no jogo.
    /// </summary>
    private static Jogo Mutar(Jogo jogo)
    {
        var numeros = jogo.Numeros.ToList();
        var presentes = numeros.ToHashSet();
        var ausentes = Enumerable.Range(1, TotalNumeros)
            .Where(n => !presentes.Contains(n))
            .OrderBy(_ => Random.Shared.Next())
            .ToList();

        if (ausentes.Count == 0) return jogo;

        // 1 ou 2 mutações
        int mutacoes = Random.Shared.Next(1, Math.Min(3, ausentes.Count + 1));
        for (int i = 0; i < mutacoes; i++)
        {
            var idxRemover = Random.Shared.Next(numeros.Count);
            numeros[idxRemover] = ausentes[i];
        }

        return Jogo.Criar(numeros);
    }
}
