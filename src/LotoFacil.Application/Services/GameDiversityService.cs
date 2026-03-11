using LotoFacil.Domain.Models;

namespace LotoFacil.Application.Services;

/// <summary>
/// Serviço dedicado de diversidade entre jogos.
/// Usa distância de Jaccard e impede overlap excessivo.
/// </summary>
public class GameDiversityService
{
    /// <summary>
    /// Similaridade de Jaccard entre dois jogos (0 = totalmente diferentes, 1 = idênticos).
    /// </summary>
    public double CalcularSimilaridade(Jogo a, Jogo b)
    {
        var setA = a.Numeros.ToHashSet();
        var setB = b.Numeros.ToHashSet();
        var intersecao = setA.Count(setB.Contains);
        var uniao = setA.Count + setB.Count - intersecao;
        return uniao > 0 ? (double)intersecao / uniao : 1.0;
    }

    /// <summary>
    /// Distância de Jaccard (1 - similaridade). Quanto maior, mais diversos.
    /// </summary>
    public double CalcularDistancia(Jogo a, Jogo b) => 1.0 - CalcularSimilaridade(a, b);

    /// <summary>
    /// Conta quantos números dois jogos compartilham.
    /// </summary>
    public int ContarOverlap(Jogo a, Jogo b)
    {
        var setB = b.Numeros.ToHashSet();
        return a.Numeros.Count(setB.Contains);
    }

    /// <summary>
    /// Filtra jogos para garantir diversidade mínima.
    /// Remove jogos que compartilhem mais que <paramref name="maxOverlap"/> números
    /// com qualquer jogo já aceito.
    /// </summary>
    /// <param name="jogos">Jogos candidatos, preferencialmente já ordenados por score.</param>
    /// <param name="maxOverlap">Máximo de números compartilhados permitido (padrão: 11).</param>
    public List<Jogo> FiltrarPorDiversidade(IReadOnlyList<Jogo> jogos, int maxOverlap = 11)
    {
        var aceitos = new List<Jogo>();

        foreach (var jogo in jogos)
        {
            var tooSimilar = aceitos.Any(a => ContarOverlap(a, jogo) > maxOverlap);
            if (!tooSimilar)
                aceitos.Add(jogo);
        }

        return aceitos;
    }

    /// <summary>
    /// Calcula o índice de diversidade médio (Jaccard distance) de um conjunto de jogos.
    /// Retorna valor entre 0 (todos iguais) e 1 (todos totalmente diferentes).
    /// </summary>
    public double CalcularIndiceDiversidade(IReadOnlyList<Jogo> jogos)
    {
        if (jogos.Count < 2) return 0;

        double soma = 0;
        int pares = 0;
        for (int i = 0; i < jogos.Count; i++)
            for (int j = i + 1; j < jogos.Count; j++)
            {
                soma += CalcularDistancia(jogos[i], jogos[j]);
                pares++;
            }

        return soma / pares;
    }
}
