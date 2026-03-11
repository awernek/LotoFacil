using LotoFacil.Domain.Models;

namespace LotoFacil.Application.Services;

public static class HistoricoStore
{
    private static readonly List<ResultadoHistorico> _resultados = [];
    private static readonly Lock _lock = new();

    public static IReadOnlyList<ResultadoHistorico> Resultados
    {
        get
        {
            lock (_lock)
                return _resultados.ToList();
        }
    }

    public static void Atualizar(IEnumerable<ResultadoHistorico> resultados)
    {
        lock (_lock)
        {
            _resultados.Clear();
            _resultados.AddRange(resultados);
        }
    }

    public static int Quantidade
    {
        get
        {
            lock (_lock)
                return _resultados.Count;
        }
    }
}
