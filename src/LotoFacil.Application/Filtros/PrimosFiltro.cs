using LotoFacil.Domain.Interfaces;
using LotoFacil.Domain.Models;

namespace LotoFacil.Application.Filtros;

public class PrimosFiltro(ConfiguracaoFiltros config) : IFiltro
{
    private static readonly HashSet<int> Primos = [2, 3, 5, 7, 11, 13, 17, 19, 23];

    public string Nome => config.Primos.Nome;
    public bool Ativo => config.Primos.Ativo;

    public bool Validar(Jogo jogo)
    {
        var count = jogo.Numeros.Count(n => Primos.Contains(n));
        return count >= config.Primos.Min && count <= config.Primos.Max;
    }
}
