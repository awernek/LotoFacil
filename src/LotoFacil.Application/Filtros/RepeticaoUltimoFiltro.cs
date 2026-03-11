using LotoFacil.Domain.Interfaces;
using LotoFacil.Domain.Models;

namespace LotoFacil.Application.Filtros;

public class RepeticaoUltimoFiltro(ConfiguracaoFiltros config) : IFiltro
{
    private HashSet<int> _ultimoResultado = [];

    public string Nome => config.RepeticaoUltimo.Nome;
    public bool Ativo => config.RepeticaoUltimo.Ativo && _ultimoResultado.Count > 0;

    public void CarregarUltimoResultado(IEnumerable<int> numeros)
    {
        _ultimoResultado = numeros.ToHashSet();
    }

    public bool Validar(Jogo jogo)
    {
        if (_ultimoResultado.Count == 0)
            return true;

        var repetidos = jogo.Numeros.Count(n => _ultimoResultado.Contains(n));
        return repetidos >= config.RepeticaoUltimo.Min
            && repetidos <= config.RepeticaoUltimo.Max;
    }
}
