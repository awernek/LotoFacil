using LotoFacil.Domain.Interfaces;
using LotoFacil.Domain.Models;

namespace LotoFacil.Application.Services;

public class FiltroService
{
    private readonly List<IFiltro> _filtros = [];

    public IReadOnlyList<IFiltro> Filtros => _filtros;

    public void Registrar(IFiltro filtro) => _filtros.Add(filtro);

    public void LimparFiltros() => _filtros.Clear();

    public bool Validar(Jogo jogo)
    {
        return _filtros
            .Where(f => f.Ativo)
            .All(f => f.Validar(jogo));
    }

    public int FiltrosAtivos => _filtros.Count(f => f.Ativo);
}
