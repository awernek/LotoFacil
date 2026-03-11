using LotoFacil.Domain.Models;

namespace LotoFacil.Application.Services;

public class MonteCarloService
{
    private const int TotalNumeros = 25;
    private const int NumerosPorJogo = 15;

    public MonteCarloResultado Simular(
        int totalSimulacoes,
        FiltroService filtroService,
        List<int>? baseInteligente = null)
    {
        int aprovados = 0;
        int reprovados = 0;
        var acertosDistribuicao = new Dictionary<int, int>();

        for (int pontos = 11; pontos <= 15; pontos++)
            acertosDistribuicao[pontos] = 0;

        var pool = baseInteligente is { Count: >= NumerosPorJogo }
            ? baseInteligente
            : Enumerable.Range(1, TotalNumeros).ToList();

        var sorteioSimulado = GerarSorteioReferencia(pool);

        for (int i = 0; i < totalSimulacoes; i++)
        {
            var numeros = pool
                .OrderBy(_ => Random.Shared.Next())
                .Take(NumerosPorJogo)
                .ToList();

            var jogo = Jogo.Criar(numeros);

            if (!filtroService.Validar(jogo))
            {
                reprovados++;
                continue;
            }

            aprovados++;

            var acertos = jogo.Numeros.Count(n => sorteioSimulado.Contains(n));
            if (acertos >= 11 && acertos <= 15)
                acertosDistribuicao[acertos]++;
        }

        return new MonteCarloResultado(
            TotalSimulacoes: totalSimulacoes,
            Aprovados: aprovados,
            Reprovados: reprovados,
            TaxaAprovacao: totalSimulacoes > 0 ? (double)aprovados / totalSimulacoes * 100 : 0,
            AcertosDistribuicao: acertosDistribuicao
        );
    }

    private static HashSet<int> GerarSorteioReferencia(List<int> pool)
    {
        return pool
            .OrderBy(_ => Random.Shared.Next())
            .Take(NumerosPorJogo)
            .ToHashSet();
    }
}

public record MonteCarloResultado(
    int TotalSimulacoes,
    int Aprovados,
    int Reprovados,
    double TaxaAprovacao,
    Dictionary<int, int> AcertosDistribuicao
)
{
    public double PercentualAcertos(int pontos) =>
        Aprovados > 0 && AcertosDistribuicao.TryGetValue(pontos, out var count)
            ? (double)count / Aprovados * 100
            : 0;
}
