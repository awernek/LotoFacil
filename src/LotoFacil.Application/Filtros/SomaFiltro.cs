using LotoFacil.Domain.Interfaces;
using LotoFacil.Domain.Models;

namespace LotoFacil.Application.Filtros;

public class SomaFiltro(ConfiguracaoFiltros config) : IFiltro
{
    public string Nome => config.Soma.Nome;
    public bool Ativo => config.Soma.Ativo;

    public bool Validar(Jogo jogo)
    {
        var soma = jogo.Soma;
        return soma >= config.Soma.Min && soma <= config.Soma.Max;
    }
}
