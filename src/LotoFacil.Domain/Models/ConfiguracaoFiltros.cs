namespace LotoFacil.Domain.Models;

public class ConfiguracaoFiltros
{
    public FiltroRange Paridade { get; set; } = new("Paridade (Pares)", true, 6, 9, 0, 15);
    public FiltroRange Soma { get; set; } = new("Soma Total", true, 185, 210, 100, 275);
    public FiltroRange Primos { get; set; } = new("Números Primos", false, 4, 7, 0, 9);
    public FiltroRange Fibonacci { get; set; } = new("Números Fibonacci", false, 3, 6, 0, 7);
    public FiltroRange Sequencias { get; set; } = new("Sequências Consecutivas", false, 4, 10, 0, 14);
    public FiltroFaixas Faixas { get; set; } = new();
    public FiltroSimples Historico { get; set; } = new("Evitar Jogos Já Sorteados", true);
    public FiltroRange RepeticaoUltimo { get; set; } = new("Repetição do Último Concurso", true, 5, 11, 0, 15);
    public FiltroRange NumerosAltos { get; set; } = new("Números Altos (22-25)", false, 0, 3, 0, 4);
    public bool ModoInteligente { get; set; } = true;
    public bool ModoRanqueado { get; set; } = false;
    public int TamanhoPoolRanqueado { get; set; } = 5000;

    public ConfiguracaoFiltros Clonar() => new()
    {
        Paridade = Paridade with { },
        Soma = Soma with { },
        Primos = Primos with { },
        Fibonacci = Fibonacci with { },
        Sequencias = Sequencias with { },
        Faixas = Faixas.Clonar(),
        Historico = Historico with { },
        RepeticaoUltimo = RepeticaoUltimo with { },
        NumerosAltos = NumerosAltos with { },
        ModoInteligente = ModoInteligente,
        ModoRanqueado = ModoRanqueado,
        TamanhoPoolRanqueado = TamanhoPoolRanqueado
    };

    public static ConfiguracaoFiltros Padrao() => new();
}

public record FiltroRange(string Nome, bool Ativo, int Min, int Max, int LimiteMin, int LimiteMax);

public record FiltroSimples(string Nome, bool Ativo);

public class FiltroFaixas
{
    public string Nome { get; init; } = "Distribuição por Faixas";
    public bool Ativo { get; set; } = true;
    public int MinPorFaixa { get; set; } = 2;
    public int MaxPorFaixa { get; set; } = 4;

    public static readonly (int Inicio, int Fim)[] Faixas =
    [
        (1, 5), (6, 10), (11, 15), (16, 20), (21, 25)
    ];

    public FiltroFaixas Clonar() => new()
    {
        Ativo = Ativo,
        MinPorFaixa = MinPorFaixa,
        MaxPorFaixa = MaxPorFaixa
    };
}
