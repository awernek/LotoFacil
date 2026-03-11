using LotoFacil.Domain.Interfaces;
using LotoFacil.Domain.Models;

namespace LotoFacil.Application.Filtros;

public class FaixasFiltro(ConfiguracaoFiltros config) : IFiltro
{
    public string Nome => config.Faixas.Nome;
    public bool Ativo => config.Faixas.Ativo;

    public bool Validar(Jogo jogo)
    {
        foreach (var (inicio, fim) in FiltroFaixas.Faixas)
        {
            var count = jogo.Numeros.Count(n => n >= inicio && n <= fim);
            if (count < config.Faixas.MinPorFaixa || count > config.Faixas.MaxPorFaixa)
                return false;
        }

        return true;
    }
}
