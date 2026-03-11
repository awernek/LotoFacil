Você é um cientista de dados especializado em análise estatística de loterias.

Tenho um histórico completo de concursos da Lotofácil.

Crie um módulo em C# chamado `PatternStatisticsService` que calcule:

1. Frequência de cada número (1–25)
2. Frequência de pares de números
3. Frequência de trincas de números
4. Frequência de números consecutivos
5. Distribuição por faixas:

   * 1–5
   * 6–10
   * 11–15
   * 16–20
   * 21–25

O serviço deve retornar estruturas de dados otimizadas para consulta rápida durante a geração de jogos.

Inclua:

* classes de modelo
* métodos de cálculo
* exemplos de uso

O objetivo é alimentar um sistema de geração inteligente de jogos.

Você é um engenheiro de software especialista em algoritmos probabilísticos.

Crie um sistema modular de scoring para jogos da Lotofácil.

Estrutura desejada:

Interface:
IScoreComponent

Componentes:
FrequencyScoreComponent
PairCorrelationScoreComponent
SequencePenaltyComponent
RangeDistributionScoreComponent
LastDrawDistanceComponent

Cada componente deve retornar um score parcial.

O GameScorer final deve somar os componentes ponderados.

Objetivo:
avaliar a qualidade estatística de um jogo com 15 números entre 1 e 25.

Forneça implementação completa em C#.

Implemente um simulador Monte Carlo para validar estratégias da Lotofácil.

Requisitos:

Entrada:

* lista de jogos gerados
* quantidade de sorteios simulados (ex: 100000)

Processo:

* simular sorteios aleatórios de 15 números entre 1 e 25
* comparar cada sorteio com todos os jogos

Calcular estatísticas:

* média de acertos
* distribuição de acertos (10,11,12,13,14,15)
* probabilidade de pelo menos 11 pontos
* probabilidade de pelo menos 12 pontos

Retornar relatório completo.

Linguagem: C#
Priorizar performance.

Implemente um algoritmo genético para otimizar jogos da Lotofácil.

Representação:
cada indivíduo = um jogo de 15 números

Fitness:
usar GameScorer existente

Operações:

Mutação:
substituir 1 ou 2 números aleatórios

Crossover:
combinar números de dois jogos

Seleção:
top 20% mais bem avaliados

Processo:

* gerar população inicial (500 jogos)
* rodar 200 gerações
* manter diversidade entre jogos

Objetivo:
evoluir jogos com score estatístico mais alto.

Forneça implementação completa em C#.

Crie um serviço chamado `GameDiversityService`.

Objetivo:
garantir diversidade entre jogos gerados.

Regras:

* calcular similaridade entre dois jogos usando índice de Jaccard
* impedir que dois jogos compartilhem mais que 11 números

Funções:

CalculateSimilarity(gameA, gameB)

FilterGamesByDiversity(List<Game> games)

Linguagem: C#

Implemente um sistema de backtest para Lotofácil.

Entrada:

* histórico real de concursos
* conjunto de jogos gerados pelo sistema

Para cada concurso:

comparar todos os jogos com o resultado real.

Calcular:

* média de acertos
* maior acerto obtido
* quantidade de vezes com 11,12,13,14,15 pontos

Gerar relatório final comparando:

estratégia do sistema vs geração aleatória.

Crie um simulador que determine quantos jogos são necessários
para cobrir combinações da Lotofácil com alta probabilidade.

Entrada:
quantidade de jogos (ex: 50, 100, 500)

Processo:
simular 1 milhão de sorteios.

Calcular:

probabilidade de obter:

> =11 pontos
> =12 pontos
> =13 pontos
> =14 pontos

Retornar tabela comparativa.
