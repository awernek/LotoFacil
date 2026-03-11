using LotoFacil.Domain.Interfaces;
using LotoFacil.Domain.Models;

namespace LotoFacil.Application.Filtros;

public class HistoricoFiltro(ConfiguracaoFiltros config) : IFiltro
{
    private readonly HashSet<string> _jogosSorteados = [];

    public string Nome => config.Historico.Nome;
    public bool Ativo => config.Historico.Ativo;

    public void CarregarHistorico(IEnumerable<ResultadoHistorico> resultados)
    {
        _jogosSorteados.Clear();
        foreach (var resultado in resultados)
        {
            var chave = string.Join("-", resultado.Numeros.OrderBy(n => n));
            _jogosSorteados.Add(chave);
        }
    }

    public bool Validar(Jogo jogo) => !_jogosSorteados.Contains(jogo.Chave);
}
