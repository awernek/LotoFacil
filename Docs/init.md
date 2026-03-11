# Init - Gerador de Jogos Lotofácil

Contexto técnico do projeto para onboarding de agentes de IA.

## Visão geral

Aplicação Blazor Server (.NET 10) que gera jogos da Lotofácil aplicando filtros estatísticos configuráveis e amostragem ponderada por frequência histórica. A Lotofácil sorteia 15 números de 1 a 25. **Nenhuma estratégia melhora as chances de ganhar** — a probabilidade é sempre 1/3.268.760.

## Solution

```
LotoFacil.slnx
├── src/LotoFacil.Domain         (classlib, sem dependências externas)
├── src/LotoFacil.Application    (classlib, referencia Domain)
├── src/LotoFacil.Infrastructure (classlib, referencia Domain)
└── src/LotoFacil.Web            (Blazor Server, referencia todos)
```

## Dependências

- **Target**: `net10.0`
- **MudBlazor** `9.1.0` — componentes UI (Cards, DataGrid, Switches, Snackbar, etc.)
- Sem banco de dados, sem EF Core, sem autenticação

## Convenções do código

- Idioma: **C#** para tipos e membros; **português** para textos de UI e labels
- Models usam `record` (imutáveis) exceto `ConfiguracaoFiltros` e `FiltroFaixas` que são `class` (mutáveis pela UI)
- Filtros seguem **Strategy Pattern**: todos implementam `IFiltro` com `Nome`, `Ativo` e `Validar(Jogo)`
- DI: `ConfiguracaoFiltros` é **Scoped** via `ConfiguracaoStorage` (config lida/gravada no localStorage); `ICaixaApiClient` via `AddHttpClient`
- `HistoricoStore` é **estático** (compartilhado entre sessões, thread-safe com `Lock`)
- Render mode: `<Routes @rendermode="InteractiveServer">` em `App.razor` — NÃO há `@rendermode` nas páginas individuais nem no layout

## Histórico base

`Program.cs` lê `Docs/últimosjogos.md` no startup via `HistoricoSeeder.Parsear()` e popula `HistoricoStore`. Formato do arquivo: uma linha por concurso, `"3632 - 01 02 03 05 06 09 10 15 17 19 20 21 22 23 25"`. Contém 367 concursos (3266–3632).

## Algoritmo de geração — detalhado

### Rejection sampling com filtros

```csharp
// GeradorDeJogos.Gerar(int quantidade)
while (jogos.Count < quantidade && tentativas < quantidade * 10_000):
    numeros = _pesosInteligentes is not null
        ? SortearPonderado(_pesosInteligentes, 15)   // modo inteligente
        : Enumerable.Range(1,25).OrderBy(_=>Random.Shared.Next()).Take(15) // uniforme
    jogo = Jogo.Criar(numeros)
    if duplicatas.Contains(jogo.Chave): continue
    if !filtroService.Validar(jogo): continue
    duplicatas.Add(jogo.Chave)
    jogos.Add(jogo)
```

### Amostragem ponderada sem reposição (Modo Inteligente)

```csharp
// GeradorDeJogos.SortearPonderado(pesos, 15)
pool = cópia do dicionário {numero -> peso}
para i in 1..15:
    totalPeso = pool.Values.Sum()
    corte = Random.Shared.NextDouble() * totalPeso
    acumulado = 0
    para (numero, peso) in pool:
        acumulado += peso
        escolhido = numero
        if acumulado >= corte: break
    resultado.Add(escolhido)
    pool.Remove(escolhido)
```

**Fórmula dos pesos** (`EstatisticasService.ObterPesosInteligentes`):
```
peso[n] = (freq[n] - minFreq) / range × 0.14 + 0.93
```
Resulta em faixa `[0.93, 1.07]` — razão máx/mín ≈ 1.15×. Com 15 sorteios de 25 números, o número mais frequente tem ~64% de chance de aparecer num jogo vs ~58% do menos frequente.

### Modo Ranqueado

Pipeline em 5 etapas (GeradorDeJogos.GerarRanqueado):

```csharp
// 1. Candidatos: GerarCandidatos(tamanhoPool) — jogos que passam nos filtros
// 2. Score: CalcularScoreCompleto(jogo) → GameScorer.CalcularScore (freq, padrões, correlação)
// 3. Ranking: ordenar por score desc
// 4. Diversidade: SelecionarComDiversidade — 1º entre top-5; depois maximiza Jaccard mínimo
// 5. Otimização: se ≥ 4 jogos, GeneticGameOptimizer.Otimizar() e nova SelecionarComDiversidade
```

## Filtros padrão

| Filtro | Classe | Min | Max | Ativo |
|--------|--------|-----|-----|-------|
| Paridade | `ParidadeFiltro` | 6 | 9 | Sim |
| Soma | `SomaFiltro` | 185 | 210 | Sim |
| Faixas | `FaixasFiltro` | 2/faixa | 4/faixa | Sim |
| Histórico | `HistoricoFiltro` | — | — | Sim |
| Repetição último | `RepeticaoUltimoFiltro` | 5 | 11 | Sim |
| Sequências | `SequenciasFiltro` | 4 | 10 | **Não** |
| Primos | `PrimosFiltro` | 4 | 7 | **Não** |
| Fibonacci | `FibonacciFiltro` | 3 | 6 | **Não** |
| Números Altos | `NumerosAltosFiltro` | 0 | 3 | **Não** |

> **Sequências desativada por padrão**: a média histórica de pares consecutivos em
> jogos de 15/25 é ≈ 8,4. Com max=4 (valor anterior), ~95% dos jogos eram
> descartados e os aprovados sempre incluíam 01 e 25 (números de borda com
> apenas um vizinho possível, que reduzem a contagem de pares consecutivos).

## Arquitetura por camada

### Domain

- `Jogo` — record com `Numeros` (IReadOnlyList<int>), props computadas `Soma`, `Pares`, `Impares`, `Chave`
- `ConfiguracaoFiltros` — classe mutável com propriedade por filtro + `ModoInteligente`, `ModoRanqueado`, `TamanhoPoolRanqueado`. Filtros de range usam `FiltroRange` (record); Histórico usa `FiltroSimples` (record: Nome, Ativo); Faixas usa `FiltroFaixas` (class com MinPorFaixa, MaxPorFaixa)
- `ResultadoHistorico` — record (Concurso, Data, Numeros)
- `IFiltro` — `string Nome`, `bool Ativo`, `bool Validar(Jogo jogo)`
- `ICaixaApiClient` — `ObterResultadosAsync(int quantidade)`, `ObterUltimoResultadoAsync()`

### Application

**Filtros** — todos recebem `ConfiguracaoFiltros` via construtor e leem limiares dele.

**Scoring:** `IScoreComponent` (Nome, Peso, Calcular(Jogo, ScoreContext)); `ScoreContext` (Stats, UltimoResultado, PatternStats, CorrelationMatrix). `ScoreComponents.cs`: ParityPenalty, SumPenalty, RangeDistributionPenalty, HighNumbersPenalty, SequencePenalty, FrequencyScore, LastDrawDistance, PatternAdherence, PairCorrelationScore.

**Services:**
- `FiltroService` — mantém `List<IFiltro>` via `Registrar(IFiltro)`; `Validar(Jogo)` aplica todos os ativos
- `GeradorDeJogos` — rejection sampling com `SortearPonderado` ou uniforme; `DefinirPesosInteligentes`, `DefinirContextoHistorico(stats, ultimoResultado)`, `DefinirContextoEstatistico(patternStats, correlationMatrix, lastDrawProfile)`; `CombinarPesos()` mistura pesos inteligentes com bias do LastDrawAnalyzer; `GerarRanqueado` usa GameScorer + diversidade Jaccard + GeneticGameOptimizer; `ResultadoGeracao` inclui ScoreMedio, DiversidadeMedia, Duplicatas
- `GameScorer` — estático; `CalcularScore(jogo, stats, ultimoResultado, patternStats, correlationMatrix)`, `Decompor()` por componente, `Jaccard(a,b)`, `Overlap`, `Diversidade`
- `GeneticGameOptimizer` — recebe `Func<Jogo, double>`; `Otimizar(populacaoInicial, geracoes, tamanhoPopulacao)`
- `PatternStatisticsService` — `Analisar(historico)` → `PatternStatistics`; `ScoreAderencia(jogo, stats, ultimoSorteio)`
- `NumberCorrelationService` — `Construir(historico)` → `CorrelationMatrix`; matriz usa `ScoreCorrelacao(jogo)`
- `LastDrawAnalyzer` — `Analisar(historico)` → `LastDrawProfile`; `GerarPesosBias(ultimoResultado, profile)` para CombinarPesos
- `HistoricoStore` — cache estático thread-safe; `Atualizar(IReadOnlyList<ResultadoHistorico>)`, `Resultados`, `Quantidade`
- `HistoricoSeeder` — `Parsear(IEnumerable<string>)` para formato `"NNNN - 01 02 ... 25"`
- `EstatisticasService` — `Analisar()` → `EstatisticasResultado`; `ObterPesosInteligentes(stats)` → pesos [0.93, 1.07]
- `MonteCarloService` — simula N jogos, conta aprovados e distribui acertos (11-15 pontos)
- `BacktestService`, `CoverageSimulatorService` — backtest e simulação de cobertura

### Infrastructure

- `CaixaApiClient` — `GET https://servicebus2.caixa.gov.br/portaldeloterias/api/lotofacil/{concurso?}`. Batches de 5 requisições simultâneas. Headers de browser para tentar contornar 403. Parse manual via `System.Text.Json` (campos: `numero`, `dataApuracao`, `listaDezenas`)

### Web

- `App.razor` — `<Routes @rendermode="InteractiveServer">` define o modo interativo para toda a árvore
- `MainLayout.razor` — providers do MudBlazor (`MudPopoverProvider`, `MudSnackbarProvider`, etc.) ficam acessíveis porque estão no mesmo circuito interativo do Routes
- `Home.razor` — gerador principal; quando ModoInteligente e histórico disponível: EstatisticasService (pesos + DefinirContextoHistorico), PatternStatisticsService, NumberCorrelationService, LastDrawAnalyzer e DefinirContextoEstatistico; CriarFiltroService() com 9 filtros; Gerar ou GerarRanqueado conforme Config; try/catch/finally em GerarJogos()
- `Configuracao.razor` — persiste via `ConfiguracaoStorage` (localStorage)
- `Historico.razor` — importa da Caixa, exibe tabela com ErrorBoundary em volta do DataGrid
- `Dashboard.razor` — heatmaps de frequência e delay, Monte Carlo

## Fluxo de geração

```
Home.GerarJogos()
  → CriarFiltroService() com 9 filtros (Paridade, Soma, Faixas, Primos, Fibonacci, Sequencias, NumerosAltos, Historico, RepeticaoUltimo) lendo Config
  → new GeradorDeJogos(filtroService)
  → se ModoInteligente && histórico disponível:
      → EstatisticasService.Analisar() → stats
      → EstatisticasService.ObterPesosInteligentes(stats) → DefinirPesosInteligentes
      → GeradorDeJogos.DefinirContextoHistorico(stats, resultados[0].Numeros)
      → PatternStatisticsService.Analisar() → patternStats
      → NumberCorrelationService.Construir() → correlationMatrix
      → LastDrawAnalyzer.Analisar() → lastDrawProfile
      → GeradorDeJogos.DefinirContextoEstatistico(patternStats, correlationMatrix, lastDrawProfile)
  → se ModoRanqueado:
      → GeradorDeJogos.GerarRanqueado(quantidade, TamanhoPoolRanqueado)
  → senão:
      → GeradorDeJogos.Gerar(quantidade)
  → retorna ResultadoGeracao (jogos, tentativas, descartados, scoreMedio, diversidadeMedia, duplicatas)
```

## Para adicionar um novo filtro

1. Criar classe em `Application/Filtros/` implementando `IFiltro`
2. Adicionar propriedade em `ConfiguracaoFiltros` (Domain) com default sensato — usar `FiltroRange` ou `FiltroSimples` conforme o caso
3. Registrar no `CriarFiltroService()` de `Home.razor` (e em `Dashboard.razor` se usar filtros lá)
4. Adicionar card de configuração em `Configuracao.razor`

## Comandos úteis

```bash
dotnet build                              # compilar tudo
dotnet run --project src/LotoFacil.Web    # rodar (http://localhost:5252)
dotnet watch --project src/LotoFacil.Web  # hot reload
```
