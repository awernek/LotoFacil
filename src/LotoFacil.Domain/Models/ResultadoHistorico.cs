namespace LotoFacil.Domain.Models;

public record ResultadoHistorico(
    int Concurso,
    DateTime Data,
    IReadOnlyList<int> Numeros
);
