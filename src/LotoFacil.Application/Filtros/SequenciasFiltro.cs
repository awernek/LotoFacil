using LotoFacil.Domain.Interfaces;
using LotoFacil.Domain.Models;

namespace LotoFacil.Application.Filtros;

public class SequenciasFiltro(ConfiguracaoFiltros config) : IFiltro
{
    public string Nome => config.Sequencias.Nome;
    public bool Ativo => config.Sequencias.Ativo;

    public bool Validar(Jogo jogo)
    {
        int sequencias = 0;
        for (int i = 0; i < jogo.Numeros.Count - 1; i++)
        {
            if (jogo.Numeros[i] + 1 == jogo.Numeros[i + 1])
                sequencias++;
        }

        return sequencias >= config.Sequencias.Min && sequencias <= config.Sequencias.Max;
    }
}
