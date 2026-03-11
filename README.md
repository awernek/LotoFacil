# Gerador de Jogos Lotofácil

Aplicação web Blazor Server para gerar jogos da Lotofácil com filtros estatísticos configuráveis e geração ponderada por frequência histórica.

> **Aviso**: nenhuma estratégia de geração aumenta a probabilidade de ganhar. Cada sorteio é independente. A probabilidade de acertar os 15 números é sempre 1 em 3.268.760.

## Stack

- .NET 10 / Blazor Server (Interactive Server via `<Routes @rendermode="InteractiveServer">`)
- MudBlazor 9.1.0 (UI components)
- API pública da Caixa para resultados recentes (sujeita a bloqueio 403)
- Histórico base seedado de `Docs/últimosjogos.md` no startup (367 concursos)
- Sem banco de dados (estado em memória)

## Como rodar

```bash
cd src/LotoFacil.Web
dotnet run
```

Acesse `http://localhost:5252`.

## Algoritmo de geração

### Modo padrão (aleatório com filtros)

```
para cada jogo até atingir quantidade:
  1. sorteia 15 números de 1–25 com distribuição uniforme
  2. verifica duplicata (HashSet<string> pela chave ordenada)
  3. aplica cadeia de filtros ativos (Strategy Pattern)
     — se reprova em qualquer filtro → descarta e tenta de novo
  4. limite: quantidade × 10.000 tentativas
```

### Modo Inteligente (ponderado por frequência histórica)

Mesma lógica, mas o passo 1 usa **amostragem ponderada sem reposição**. Os pesos podem ser combinados com **bias do último concurso** (LastDrawAnalyzer) para suavizar repetição/variação:

```
pesos base: EstatisticasService.ObterPesosInteligentes()
  peso[n] = (freq[n] - minFreq) / range × 0.14 + 0.93  → faixa [0.93, 1.07]

opcional: CombinarPesos() multiplica pelos pesos do LastDrawAnalyzer (perfil do último sorteio)

SortearPonderado(pesos, 15): pool de 25 pesos; a cada sorteio: corte = random × soma; acumula até corte; remove escolhido do pool
```

O viés é **intencional e leve**: o número mais frequente historicamente tem ~64%
de chance de aparecer num jogo vs ~58% do menos frequente. Não prevê sorteios —
é apenas uma influência estatística suave sobre a distribuição gerada.

### Modo Ranqueado

Pipeline em 5 etapas: **candidatos → score → ranking → diversidade → otimização genética**:

```
1. Candidatos: gerar pool (padrão 5.000) que passam todos os filtros
2. Score: GameScorer.CalcularScore() com frequência, padrões, correlação e último concurso
3. Ranking: ordenar por score decrescente
4. Diversidade: selecionar iterativamente
   — 1º: sorteio entre os top-5 do ranking
   — cada seguinte: maximiza diversidade mínima (Jaccard) contra os já selecionados
5. Otimização: se ≥ 4 jogos, GeneticGameOptimizer refina a população e nova seleção por diversidade
```

> O modo ranqueado produz jogos estatisticamente parecidos com os históricos,
> mas **não aumenta chances de ganhar**. Qualquer combinação específica tem a
> mesma probabilidade: 1/3.268.760.

## Estrutura do projeto

```
src/
  LotoFacil.Domain/
    Models/
      Jogo.cs                      # Record: 15 números, soma, pares, chave
      ConfiguracaoFiltros.cs       # Configuração de todos os filtros
      ResultadoHistorico.cs        # Resultado de concurso (número, data, dezenas)
    Interfaces/
      IFiltro.cs                   # Interface Strategy para filtros
      ICaixaApiClient.cs           # Contrato para buscar resultados da Caixa

  LotoFacil.Application/
    Filtros/
      ParidadeFiltro.cs            # Pares: 6–9 (ativo por padrão)
      SomaFiltro.cs                # Soma: 185–210 (ativo por padrão)
      FaixasFiltro.cs              # Distribuição: 2–4 por faixa (ativo por padrão)
      PrimosFiltro.cs              # Primos: 4–7 (desativado por padrão)
      FibonacciFiltro.cs           # Fibonacci: 3–6 (desativado por padrão)
      SequenciasFiltro.cs          # Pares consecutivos: 4–10 (desativado por padrão)
      HistoricoFiltro.cs           # Evita jogos já sorteados (ativo por padrão)
      RepeticaoUltimoFiltro.cs     # Repetição do último concurso: 5–11 (ativo por padrão)
      NumerosAltosFiltro.cs        # Números 22–25: 0–3 (desativado por padrão)
    Scoring/
      IScoreComponent.cs           # Contexto + interface dos componentes de score
      ScoreComponents.cs           # Parity, Sum, Range, Frequency, LastDraw, Pattern, Correlation
    Services/
      GeradorDeJogos.cs            # Rejeição + ponderado + ranqueado (diversidade + genético)
      FiltroService.cs             # Cadeia de filtros ativos (Strategy)
      HistoricoStore.cs            # Cache estático dos resultados (thread-safe com Lock)
      HistoricoSeeder.cs           # Parser de Docs/últimosjogos.md para seed inicial
      EstatisticasService.cs       # Frequência, delay, pesos inteligentes
      GameScorer.cs                # Score por componentes (freq, padrões, correlação, Jaccard)
      GameDiversityService.cs      # Diversidade entre jogos
      GeneticGameOptimizer.cs      # Otimização genética no modo ranqueado
      PatternStatisticsService.cs  # Estatísticas de padrões (faixas, paridade, sequências)
      NumberCorrelationService.cs  # Matriz de correlação entre números
      LastDrawAnalyzer.cs          # Perfil do último concurso e bias para pesos
      MonteCarloService.cs         # Simulação Monte Carlo
      BacktestService.cs           # Backtest de estratégias
      CoverageSimulatorService.cs  # Simulador de cobertura

  LotoFacil.Infrastructure/
    CaixaApiClient.cs              # HttpClient para API da Caixa (batches de 5 req.)

  LotoFacil.Web/
    Components/
      Pages/
        Home.razor                 # Gerador + cards + lista + export TXT/CSV
        Configuracao.razor         # Configuração dos filtros
        Historico.razor            # Importação de resultados da Caixa
        Dashboard.razor            # Heatmaps + Monte Carlo
      Layout/
        MainLayout.razor           # Drawer, AppBar, dark mode, MudBlazor providers
    Program.cs                     # DI + seed do histórico no startup
    Components/App.razor           # <Routes @rendermode="InteractiveServer">
```

## Filtros disponíveis

| Filtro | Descrição | Range padrão | Ativo |
|--------|-----------|-------------|-------|
| Paridade | Quantidade de números pares | 6–9 | Sim |
| Soma | Soma dos 15 números | 185–210 | Sim |
| Faixas | Números por faixa (1-5, 6-10, 11-15, 16-20, 21-25) | 2–4 por faixa | Sim |
| Histórico | Evita repetir jogos já sorteados | — | Sim |
| Repetição | Sobreposição com o último concurso | 5–11 | Sim |
| Sequências | Pares adjacentes consecutivos | 4–10 | Não |
| Primos | Números primos (2,3,5,7,11,13,17,19,23) | 4–7 | Não |
| Fibonacci | Números Fibonacci (1,2,3,5,8,13,21) | 3–6 | Não |
| Números Altos | Números na faixa 22–25 | 0–3 | Não |

> **Nota sobre Sequências**: a média histórica de pares consecutivos em jogos
> de 15/25 números é ≈ 8,4. Usar max < 5 descarta ~95% dos jogos válidos e
> força artificialmente a presença dos números de borda (01 e 25).

## Arquitetura

- **Clean Architecture** (Domain → Application → Infrastructure → Web)
- **Strategy Pattern** nos filtros: cada filtro implementa `IFiltro`, simples de estender
- **Render mode unificado**: `<Routes @rendermode="InteractiveServer">` em `App.razor`
  garante que layout e providers do MudBlazor estão no mesmo circuito interativo
- **Configuração**: `ConfiguracaoFiltros` é Scoped via `ConfiguracaoStorage` (localStorage no cliente)
- **Seed no startup**: `Program.cs` lê `Docs/últimosjogos.md` via `HistoricoSeeder`
  e popula `HistoricoStore` antes de aceitar requisições
- **API da Caixa**: requisições em batches de 5 para evitar rate-limit
