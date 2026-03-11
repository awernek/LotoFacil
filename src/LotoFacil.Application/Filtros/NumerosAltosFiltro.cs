using LotoFacil.Domain.Interfaces;
using LotoFacil.Domain.Models;

namespace LotoFacil.Application.Filtros;

public class NumerosAltosFiltro(ConfiguracaoFiltros config) : IFiltro
{
    private const int LimiteAlto = 22;

    public string Nome => config.NumerosAltos.Nome;
    public bool Ativo => config.NumerosAltos.Ativo;

    public bool Validar(Jogo jogo)
    {
        var altos = jogo.Numeros.Count(n => n >= LimiteAlto);
        return altos >= config.NumerosAltos.Min && altos <= config.NumerosAltos.Max;
    }
}
