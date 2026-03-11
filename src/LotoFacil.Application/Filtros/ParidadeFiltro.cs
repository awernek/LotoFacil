using LotoFacil.Domain.Interfaces;
using LotoFacil.Domain.Models;

namespace LotoFacil.Application.Filtros;

public class ParidadeFiltro(ConfiguracaoFiltros config) : IFiltro
{
    public string Nome => config.Paridade.Nome;
    public bool Ativo => config.Paridade.Ativo;

    public bool Validar(Jogo jogo)
    {
        var pares = jogo.Pares;
        return pares >= config.Paridade.Min && pares <= config.Paridade.Max;
    }
}
