using LotoFacil.Domain.Models;

namespace LotoFacil.Application.Services;

public class EstatisticasService
{
    public EstatisticasResultado Analisar(IReadOnlyList<ResultadoHistorico> resultados)
    {
        if (resultados.Count == 0)
            return EstatisticasResultado.Vazio();

        var frequencia = CalcularFrequencia(resultados);
        var delay = CalcularDelay(resultados);
        var repeticoes = CalcularRepeticaoConsecutiva(resultados);

        var ordenadoPorFreq = frequencia.OrderByDescending(kv => kv.Value).ToList();
        var quentes = ordenadoPorFreq.Take(10).Select(kv => kv.Key).ToList();
        var frios = ordenadoPorFreq.TakeLast(10).Select(kv => kv.Key).ToList();

        var mediaSoma = resultados.Average(r => r.Numeros.Sum());
        var mediaPares = resultados.Average(r => r.Numeros.Count(n => n % 2 == 0));
        var mediaRepeticao = repeticoes.Count > 0 ? repeticoes.Average() : 0;

        var distribuicaoPares = resultados
            .GroupBy(r => r.Numeros.Count(n => n % 2 == 0))
            .ToDictionary(g => g.Key, g => g.Count());

        var distribuicaoSoma = CalcularDistribuicaoSoma(resultados);

        var distribuicaoLinhas = CalcularDistribuicaoLinhas(resultados);

        return new EstatisticasResultado(
            Frequencia: frequencia,
            Delay: delay,
            NumerosQuentes: quentes,
            NumerosFrios: frios,
            MediaSoma: mediaSoma,
            MediaPares: mediaPares,
            MediaRepeticaoConsecutiva: mediaRepeticao,
            DistribuicaoPares: distribuicaoPares,
            DistribuicaoSoma: distribuicaoSoma,
            DistribuicaoLinhas: distribuicaoLinhas,
            TotalConcursos: resultados.Count
        );
    }

    public Dictionary<int, double> ObterPesosInteligentes(EstatisticasResultado stats)
    {
        if (stats.TotalConcursos == 0)
            return Enumerable.Range(1, 25).ToDictionary(n => n, _ => 1.0);

        var maxFreq = stats.Frequencia.Values.Max();
        var minFreq = stats.Frequencia.Values.Min();
        var range = maxFreq - minFreq;

        return stats.Frequencia.ToDictionary(
            kv => kv.Key,
            kv => range == 0 ? 1.0 : (kv.Value - minFreq) / (double)range * 0.14 + 0.93
        );
    }

    public List<int> SelecionarBaseInteligente(EstatisticasResultado stats)
    {
        var ordenado = stats.Frequencia
            .OrderByDescending(kv => kv.Value)
            .Select(kv => kv.Key)
            .ToList();

        var hot = ordenado.Take(9).ToList();
        var mid = ordenado.Skip(9).Take(6).ToList();
        var cold = ordenado.Skip(15).Take(3).ToList();

        var baseNumeros = hot.Concat(mid).Concat(cold).Distinct().OrderBy(n => n).ToList();

        if (baseNumeros.Count < 18)
        {
            var faltam = Enumerable.Range(1, 25)
                .Where(n => !baseNumeros.Contains(n))
                .Take(18 - baseNumeros.Count);
            baseNumeros.AddRange(faltam);
            baseNumeros.Sort();
        }

        return baseNumeros;
    }

    private static Dictionary<int, int> CalcularFrequencia(IReadOnlyList<ResultadoHistorico> resultados)
    {
        var freq = Enumerable.Range(1, 25).ToDictionary(n => n, _ => 0);
        foreach (var r in resultados)
            foreach (var n in r.Numeros)
                freq[n]++;
        return freq;
    }

    private static Dictionary<int, int> CalcularDelay(IReadOnlyList<ResultadoHistorico> resultados)
    {
        var delay = Enumerable.Range(1, 25).ToDictionary(n => n, _ => -1);
        var ordenados = resultados.OrderByDescending(r => r.Concurso).ToList();

        foreach (var numero in Enumerable.Range(1, 25))
        {
            for (int i = 0; i < ordenados.Count; i++)
            {
                if (ordenados[i].Numeros.Contains(numero))
                {
                    delay[numero] = i;
                    break;
                }
            }
        }

        return delay;
    }

    private static List<double> CalcularRepeticaoConsecutiva(IReadOnlyList<ResultadoHistorico> resultados)
    {
        var ordenados = resultados.OrderBy(r => r.Concurso).ToList();
        var repeticoes = new List<double>();

        for (int i = 1; i < ordenados.Count; i++)
        {
            var anterior = ordenados[i - 1].Numeros.ToHashSet();
            var repetidos = ordenados[i].Numeros.Count(n => anterior.Contains(n));
            repeticoes.Add(repetidos);
        }

        return repeticoes;
    }

    private static Dictionary<string, int> CalcularDistribuicaoSoma(IReadOnlyList<ResultadoHistorico> resultados)
    {
        return resultados
            .Select(r => r.Numeros.Sum())
            .GroupBy(s => s switch
            {
                < 170 => "< 170",
                <= 185 => "170-185",
                <= 200 => "186-200",
                <= 210 => "201-210",
                <= 220 => "211-220",
                _ => "> 220"
            })
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private static Dictionary<int, Dictionary<int, int>> CalcularDistribuicaoLinhas(
        IReadOnlyList<ResultadoHistorico> resultados)
    {
        var dist = new Dictionary<int, Dictionary<int, int>>();
        for (int linha = 0; linha < 5; linha++)
        {
            var inicio = linha * 5 + 1;
            var fim = inicio + 4;
            var contagens = new Dictionary<int, int>();

            foreach (var r in resultados)
            {
                var count = r.Numeros.Count(n => n >= inicio && n <= fim);
                contagens[count] = contagens.GetValueOrDefault(count) + 1;
            }

            dist[linha] = contagens;
        }
        return dist;
    }
}

public record EstatisticasResultado(
    Dictionary<int, int> Frequencia,
    Dictionary<int, int> Delay,
    List<int> NumerosQuentes,
    List<int> NumerosFrios,
    double MediaSoma,
    double MediaPares,
    double MediaRepeticaoConsecutiva,
    Dictionary<int, int> DistribuicaoPares,
    Dictionary<string, int> DistribuicaoSoma,
    Dictionary<int, Dictionary<int, int>> DistribuicaoLinhas,
    int TotalConcursos
)
{
    public static EstatisticasResultado Vazio() => new(
        Frequencia: Enumerable.Range(1, 25).ToDictionary(n => n, _ => 0),
        Delay: Enumerable.Range(1, 25).ToDictionary(n => n, _ => -1),
        NumerosQuentes: [],
        NumerosFrios: [],
        MediaSoma: 0,
        MediaPares: 0,
        MediaRepeticaoConsecutiva: 0,
        DistribuicaoPares: new(),
        DistribuicaoSoma: new(),
        DistribuicaoLinhas: new(),
        TotalConcursos: 0
    );
}
