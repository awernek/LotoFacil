using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static Random rnd = new Random();

    static void Main()
    {
        int quantidadeJogos = 100;

        for (int i = 0; i < quantidadeJogos; i++)
        {
            var jogo = GerarJogo();
            Console.WriteLine(string.Join(" ", jogo.Select(n => n.ToString("D2"))));
        }
    }

    static List<int> GerarJogo()
    {
        while (true)
        {
            var numeros = Enumerable.Range(1, 25)
                .OrderBy(x => rnd.Next())
                .Take(15)
                .OrderBy(x => x)
                .ToList();

            if (ValidarJogo(numeros))
                return numeros;
        }
    }

    static bool ValidarJogo(List<int> jogo)
    {
        int pares = jogo.Count(n => n % 2 == 0);
        int impares = 15 - pares;

        if (pares < 6 || pares > 9)
            return false;

        int altos = jogo.Count(n => n >= 22);
        if (altos > 3)
            return false;

        int sequencias = 0;

        for (int i = 0; i < jogo.Count - 1; i++)
        {
            if (jogo[i] + 1 == jogo[i + 1])
                sequencias++;
        }

        if (sequencias > 4)
            return false;

        return true;
    }
}