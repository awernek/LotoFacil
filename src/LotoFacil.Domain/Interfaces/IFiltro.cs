using LotoFacil.Domain.Models;

namespace LotoFacil.Domain.Interfaces;

public interface IFiltro
{
    string Nome { get; }
    bool Ativo { get; }
    bool Validar(Jogo jogo);
}
