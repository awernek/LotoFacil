using LotoFacil.Domain.Interfaces;
using LotoFacil.Domain.Models;

namespace LotoFacil.Application.Filtros;

public class FibonacciFiltro(ConfiguracaoFiltros config) : IFiltro
{
    private static readonly HashSet<int> Fibonacci = [1, 2, 3, 5, 8, 13, 21];

    public string Nome => config.Fibonacci.Nome;
    public bool Ativo => config.Fibonacci.Ativo;

    public bool Validar(Jogo jogo)
    {
        var count = jogo.Numeros.Count(n => Fibonacci.Contains(n));
        return count >= config.Fibonacci.Min && count <= config.Fibonacci.Max;
    }
}
