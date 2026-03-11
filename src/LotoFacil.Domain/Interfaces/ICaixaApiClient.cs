using LotoFacil.Domain.Models;

namespace LotoFacil.Domain.Interfaces;

public interface ICaixaApiClient
{
    Task<IReadOnlyList<ResultadoHistorico>> ObterResultadosAsync(int quantidade);
    Task<ResultadoHistorico?> ObterUltimoResultadoAsync();
}
