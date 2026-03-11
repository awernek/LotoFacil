# Plano de Execução — Sequência de Prompts

> Gerado em: 11/03/2026
> Base: `Docs/sequencia-de-prompts.md`
> Estado: análise cruzada com o código existente

---

## Visão Geral

| # | Prompt | Status Atual | Ação Necessária |
|---|--------|-------------|-----------------|
| 1 | PatternStatisticsService | ⚠️ Parcial | Adicionar frequência de pares e trincas |
| 2 | Sistema Modular de Scoring | ❌ Monolítico | Refatorar para componentes com `IScoreComponent` |
| 3 | Simulador Monte Carlo | ✅ Completo | Nenhuma (já existe e funciona) |
| 4 | Algoritmo Genético | ✅ Completo | Ajustar defaults (200 gerações, 500 pop) |
| 5 | GameDiversityService | ⚠️ Embutido | Extrair para serviço dedicado + limite de overlap |
| 6 | Sistema de Backtest | ✅ Completo | Adicionar comparação explícita contra aleatoriedade |
| 7 | Simulador de Cobertura | ❌ Não existe | Criar do zero |

---

## Prompt 1 — PatternStatisticsService (Evolução)

**Arquivo:** `src/LotoFacil.Application/Services/PatternStatisticsService.cs`

### O que já existe
- ✅ Distribuição por faixas (1-5, 6-10, 11-15, 16-20, 21-25)
- ✅ Distribuição de paridade (pares/ímpares)
- ✅ Frequência de sequências consecutivas
- ✅ Distribuição de repetição do último sorteio
- ✅ Score de aderência [0,1]

### O que falta
- ❌ **Frequência individual de cada número (1–25)** — já existe em `EstatisticasService`, mas não em `PatternStatisticsService`
- ❌ **Frequência de pares de números** — quais duplas aparecem juntas com mais frequência
- ❌ **Frequência de trincas de números** — quais trios aparecem juntos com mais frequência

### Tarefas
| ID | Tarefa | Complexidade |
|----|--------|-------------|
| 1.1 | Adicionar `Dictionary<int, int> FrequenciaIndividual` ao `PatternStatistics` | Baixa |
| 1.2 | Adicionar `Dictionary<(int,int), int> FrequenciaPares` — contagem de co-ocorrência de cada par | Média |
| 1.3 | Adicionar `Dictionary<(int,int,int), int> FrequenciaTrincas` — contagem de cada trinca | Média |
| 1.4 | Retornar estruturas ordenadas por frequência para consulta rápida | Baixa |

### Notas
- `NumberCorrelationService` já tem a **matriz 25×25** de co-ocorrência normalizada. A frequência de pares pode ser adicionada ao `PatternStatistics` como dados brutos (contagem absoluta), diferente da matriz normalizada.
- Trincas: com 15 números por jogo, temos C(15,3) = 455 trincas por sorteio. Para ~3500 concursos isso gera ~1.6M entradas — viável, mas guardar apenas top N (ex: top 500).

---

## Prompt 2 — Sistema Modular de Scoring

**Arquivo atual:** `src/LotoFacil.Application/Services/GameScorer.cs` (classe estática monolítica)

### O que já existe
- ✅ Penalidade de paridade, soma, faixas, altos, sequências
- ✅ Bônus de frequência, overlap do último, padrão, correlação
- ✅ Total: ~9 componentes de scoring embutidos inline

### O que falta
- ❌ **Interface `IScoreComponent`** no Domain
- ❌ **Classes separadas** para cada componente de scoring
- ❌ **Pesos configuráveis** por componente
- ❌ **GameScorer composto** que soma componentes ponderados

### Tarefas
| ID | Tarefa | Complexidade |
|----|--------|-------------|
| 2.1 | Criar `IScoreComponent` com método `double Calcular(Jogo, ScoreContext)` em `Domain/Interfaces` | Baixa |
| 2.2 | Criar record `ScoreContext` com stats, ultimoResultado, patternStats, correlationMatrix | Baixa |
| 2.3 | Extrair `FrequencyScoreComponent` | Baixa |
| 2.4 | Extrair `PairCorrelationScoreComponent` | Baixa |
| 2.5 | Extrair `SequencePenaltyComponent` | Baixa |
| 2.6 | Extrair `RangeDistributionScoreComponent` | Baixa |
| 2.7 | Extrair `LastDrawDistanceComponent` | Baixa |
| 2.8 | Extrair `ParityPenaltyComponent` | Baixa |
| 2.9 | Extrair `SumPenaltyComponent` | Baixa |
| 2.10 | Extrair `PatternAdherenceComponent` | Baixa |
| 2.11 | Extrair `HighNumbersPenaltyComponent` | Baixa |
| 2.12 | Refatorar `GameScorer` para usar lista de `IScoreComponent` com pesos | Média |
| 2.13 | Manter compatibilidade com chamadas existentes (método estático wrapper) | Baixa |

### Arquitetura Proposta
```
IScoreComponent
├── FrequencyScoreComponent        (+15 pts)
├── PairCorrelationScoreComponent  (+8 pts)
├── PatternAdherenceComponent      (+12 pts)
├── LastDrawDistanceComponent      (+10 pts)
├── ParityPenaltyComponent         (-15 pts)
├── SumPenaltyComponent            (-20 pts)
├── RangeDistributionComponent     (-15 pts)
├── HighNumbersPenaltyComponent    (-10 pts)
└── SequencePenaltyComponent       (-10 pts)

GameScorer
 └── List<(IScoreComponent, double Peso)>
      └── CalcularScore() = 100 + Σ(componente.Calcular() × peso)
```

---

## Prompt 3 — Simulador Monte Carlo

**Arquivo:** `src/LotoFacil.Application/Services/MonteCarloService.cs`

### Status: ✅ COMPLETO

- ✅ Simula sorteios aleatórios de 15 entre 25
- ✅ Compara com jogos gerados
- ✅ Calcula distribuição de acertos (11-15)
- ✅ Calcula probabilidade de ≥11, ≥12, etc.
- ✅ Retorna `MonteCarloResultado` com métricas completas

### Tarefas
| ID | Tarefa | Complexidade |
|----|--------|-------------|
| 3.1 | _(nenhuma ação necessária)_ | — |

---

## Prompt 4 — Algoritmo Genético

**Arquivo:** `src/LotoFacil.Application/Services/GeneticGameOptimizer.cs`

### O que já existe
- ✅ Seleção por torneio (k=3)
- ✅ Crossover por interseção + exclusivos
- ✅ Mutação (substituir 1-2 números)
- ✅ Elitismo (top 10%)
- ✅ Prevenção de duplicatas

### O que falta ajustar
- ❌ Defaults pedem **200 gerações** (atual: 50)
- ❌ Defaults pedem **população de 500** (atual: 200)
- ❌ Seleção pede **top 20%** (atual: elitismo 10%)

### Tarefas
| ID | Tarefa | Complexidade |
|----|--------|-------------|
| 4.1 | Alterar default de `geracoes` de 50 → 200 | Trivial |
| 4.2 | Alterar default de `tamanhoPopulacao` de 200 → 500 | Trivial |
| 4.3 | Alterar elitismo de 10% → 20% | Trivial |
| 4.4 | Ajustar chamada em `GerarRanqueado` (atualmente usa 30 gerações) | Baixa |

---

## Prompt 5 — GameDiversityService

**Estado atual:** lógica embutida em `GeradorDeJogos.SelecionarComDiversidade()` e `GameScorer.Jaccard()`

### O que já existe
- ✅ `GameScorer.Jaccard(a, b)` — distância de Jaccard
- ✅ `GameScorer.Overlap(a, b)` — contagem de números compartilhados
- ✅ Seleção por diversidade no pipeline ranqueado

### O que falta
- ❌ **Classe dedicada `GameDiversityService`**
- ❌ **`CalculateSimilarity(gameA, gameB)`** — método explícito
- ❌ **`FilterGamesByDiversity(List<Jogo>)`** — filtro com limite configurável
- ❌ **Limite de overlap máximo** (proposto: 11 números)

### Tarefas
| ID | Tarefa | Complexidade |
|----|--------|-------------|
| 5.1 | Criar `GameDiversityService` em `Application/Services` | Baixa |
| 5.2 | Método `CalcularSimilaridade(Jogo a, Jogo b)` retornando Jaccard | Trivial |
| 5.3 | Método `FiltrarPorDiversidade(List<Jogo>, int maxOverlap = 11)` | Média |
| 5.4 | Método `CalcularIndiceDiversidade(List<Jogo>)` retornando Jaccard médio | Baixa |
| 5.5 | Integrar no pipeline do `GeradorDeJogos` substituindo lógica inline | Média |

---

## Prompt 6 — Sistema de Backtest

**Arquivo:** `src/LotoFacil.Application/Services/BacktestService.cs`

### O que já existe
- ✅ Teste contra concursos históricos (forward-testing correto)
- ✅ Métricas: melhor acerto, média, percentual com prêmio
- ✅ Distribuição de acertos por concurso
- ✅ `BacktestResultado` completo

### O que falta
- ❌ **Comparação explícita: sistema vs geração aleatória**
- ❌ **Relatório com distribuição detalhada (11/12/13/14/15)**

### Tarefas
| ID | Tarefa | Complexidade |
|----|--------|-------------|
| 6.1 | Adicionar método `ExecutarComparativo()` que roda backtest com e sem pesos inteligentes | Média |
| 6.2 | Adicionar `DistribuicaoAcertos` (Dict 11→N, 12→N, etc.) ao `BacktestResultado` | Baixa |
| 6.3 | Calcular distribuição total de acertos de todos os jogos em todos os concursos | Baixa |

---

## Prompt 7 — Simulador de Cobertura

**Estado:** ❌ NÃO EXISTE

### Descrição
Serviço que calcula quantos jogos são necessários para atingir uma probabilidade alvo de cobertura.

### Tarefas
| ID | Tarefa | Complexidade |
|----|--------|-------------|
| 7.1 | Criar `CoverageSimulatorService` em `Application/Services` | Alta |
| 7.2 | Método `Simular(int quantidadeJogos, int sorteiosSimulados = 1_000_000)` | Alta |
| 7.3 | Para cada quantidade de jogos (ex: 50, 100, 500), simular sorteios e calcular P(≥11), P(≥12), P(≥13), P(≥14) | Média |
| 7.4 | Retornar `CoverageResult` com tabela comparativa | Baixa |
| 7.5 | Otimizar para performance (sorteios em paralelo se necessário) | Média |

### Modelo de Saída
```
| Jogos |  P(≥11) |  P(≥12) |  P(≥13) |  P(≥14) |
|-------|---------|---------|---------|---------|
|    50 |  98.5%  |  62.3%  |  15.7%  |   1.2%  |
|   100 |  99.9%  |  85.1%  |  29.4%  |   2.3%  |
|   500 | 100.0%  |  99.8%  |  76.5%  |  11.2%  |
```

---

## Ordem de Execução Recomendada

```
Fase A — Fundação (independentes)
├── Prompt 1: PatternStatisticsService (pares + trincas)
├── Prompt 5: GameDiversityService (extrair)
└── Prompt 7: CoverageSimulatorService (criar)

Fase B — Refatoração de Scoring (depende da Fase A para testes)
└── Prompt 2: Sistema modular IScoreComponent

Fase C — Ajustes finais
├── Prompt 4: Ajustar defaults do GA
├── Prompt 6: Backtest comparativo
└── Prompt 3: (nada a fazer)

Fase D — Integração e validação
├── Atualizar páginas Blazor (se necessário)
├── Build + teste manual
└── Rodar backtest para validar melhorias
```

---

## Estimativa de Impacto

| Fase | Arquivos Novos | Arquivos Modificados | Risco |
|------|---------------|---------------------|-------|
| A | 2 (DiversityService, CoverageSimulator) | 1 (PatternStatisticsService) | Baixo |
| B | ~9 (componentes de score) + 1 (interface) | 2 (GameScorer, GeradorDeJogos) | Médio |
| C | 0 | 2 (GeneticGameOptimizer, BacktestService) | Baixo |
| D | 0 | ~2 (páginas Blazor) | Baixo |

---

## Checklist Final

- [x] Prompt 1: Frequência individual, pares, trincas no PatternStatisticsService
- [x] Prompt 2: IScoreComponent + 9 componentes + GameScorer composto
- [x] Prompt 3: _(nada)_
- [x] Prompt 4: GA defaults ajustados (200 gen, 500 pop, 20% elite)
- [x] Prompt 5: GameDiversityService dedicado com limite de overlap
- [x] Prompt 6: Backtest comparativo (sistema vs aleatório)
- [x] Prompt 7: CoverageSimulatorService completo
- [x] Build sem erros
- [ ] Teste manual via UI
