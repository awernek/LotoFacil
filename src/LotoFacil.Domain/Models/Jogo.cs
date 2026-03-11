namespace LotoFacil.Domain.Models;

public record Jogo
{
    public IReadOnlyList<int> Numeros { get; init; } = [];

    public int Soma => Numeros.Sum();
    public int Pares => Numeros.Count(n => n % 2 == 0);
    public int Impares => 15 - Pares;

    public string Chave => string.Join("-", Numeros);

    public static Jogo Criar(IEnumerable<int> numeros)
    {
        var lista = numeros.OrderBy(n => n).ToList();
        return new Jogo { Numeros = lista };
    }
}
