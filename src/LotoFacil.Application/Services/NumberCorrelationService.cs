using LotoFacil.Domain.Models;

namespace LotoFacil.Application.Services;

/// <summary>
/// Constrói uma matriz 25×25 de co-ocorrência normalizada a partir do histórico.
/// Permite avaliar quão frequentemente pares de números aparecem juntos.
/// </summary>
public class NumberCorrelationService
{
    private const int TotalNumeros = 25;

    /// <summary>
    /// Constrói a matriz de correlação normalizada [0,1] a partir do histórico.
    /// </summary>
    public CorrelationMatrix Construir(IReadOnlyList<ResultadoHistorico> historico)
    {
        if (historico.Count == 0)
            return CorrelationMatrix.Vazio();

        // Matriz de co-ocorrência bruta (simétrica)
        var coOcorrencia = new int[TotalNumeros + 1, TotalNumeros + 1];

        foreach (var r in historico)
        {
            var nums = r.Numeros;
            for (int i = 0; i < nums.Count; i++)
                for (int j = i + 1; j < nums.Count; j++)
                {
                    coOcorrencia[nums[i], nums[j]]++;
                    coOcorrencia[nums[j], nums[i]]++;
                }
        }

        // Normalizar para [0,1]
        int max = 0;
        int min = int.MaxValue;
        for (int i = 1; i <= TotalNumeros; i++)
            for (int j = i + 1; j <= TotalNumeros; j++)
            {
                var v = coOcorrencia[i, j];
                if (v > max) max = v;
                if (v < min) min = v;
            }

        var range = max - min;
        var normalizada = new double[TotalNumeros + 1, TotalNumeros + 1];

        if (range > 0)
        {
            for (int i = 1; i <= TotalNumeros; i++)
                for (int j = 1; j <= TotalNumeros; j++)
                {
                    if (i == j)
                    {
                        normalizada[i, j] = 1.0;
                        continue;
                    }
                    normalizada[i, j] = (double)(coOcorrencia[i, j] - min) / range;
                }
        }

        return new CorrelationMatrix(normalizada, historico.Count);
    }
}

public class CorrelationMatrix
{
    private readonly double[,] _matrix;
    public int TotalConcursos { get; }

    public CorrelationMatrix(double[,] matrix, int totalConcursos)
    {
        _matrix = matrix;
        TotalConcursos = totalConcursos;
    }

    public double this[int a, int b] => _matrix[a, b];

    /// <summary>
    /// Calcula o score médio de correlação entre todos os pares de números do jogo.
    /// Retorna valor entre 0 e 1.
    /// </summary>
    public double ScoreCorrelacao(Jogo jogo)
    {
        if (TotalConcursos == 0) return 0.5;

        var nums = jogo.Numeros;
        double soma = 0;
        int pares = 0;

        for (int i = 0; i < nums.Count; i++)
            for (int j = i + 1; j < nums.Count; j++)
            {
                soma += _matrix[nums[i], nums[j]];
                pares++;
            }

        return pares > 0 ? soma / pares : 0.5;
    }

    public static CorrelationMatrix Vazio() =>
        new(new double[26, 26], 0);
}
