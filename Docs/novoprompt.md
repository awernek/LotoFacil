You are a senior .NET engineer, statistician and algorithm designer.

You are reviewing and improving a lottery generation engine for the Brazilian game Lotofácil.

The project already contains:

* a game generator (GeradorDeJogos)
* filtering system (FiltroService)
* scoring logic (GameScorer)
* diversity selection
* weighted number generation
* historical data stored in the project

Your goal is to evolve this project from a simple generator into a **statistically-driven lottery engine**.

Focus on:

* statistical modeling
* algorithm quality
* architecture
* maintainability

Do NOT overcomplicate the code. Keep it clean and idiomatic C#.

---

# 1 — Pattern Modeling from Historical Draws

Create a new service:

PatternStatisticsService

This service should analyze historical Lotofácil draws and compute distributions for:

• numbers per row:
1–5
6–10
11–15
16–20
21–25

• parity patterns (even/odd)

• sequence length frequency

• number repetition patterns from previous draw

Example pattern:

3-3-3-3-3
4-3-3-3-2
3-4-3-3-2

Compute probability of each pattern.

Extend GameScorer so that games closer to real historical patterns receive higher scores.

Penalize unrealistic patterns.

---

# 2 — Number Correlation Matrix

Implement a new service:

NumberCorrelationService

This service should build a 25x25 matrix from historical draws representing how frequently numbers appear together.

Example:

correlation[13,14] = strong
correlation[2,25] = weak

Normalize correlations.

Modify GameScorer so that:

games containing historically correlated numbers receive a score bonus.

---

# 3 — Genetic Optimization

Implement a new optimization strategy:

GeneticGameOptimizer

Workflow:

1. generate a large population of candidate games
2. score them
3. select the best ones
4. perform crossover between games
5. apply random mutation
6. iterate for several generations

Mutation examples:

• replace one number
• swap numbers
• adjust parity

This optimizer should evolve games toward higher statistical scores.

---

# 4 — Smart Last Draw Modeling

Lotofácil typically repeats 8–11 numbers from the previous draw.

Create:

LastDrawAnalyzer

This service should:

• detect numbers from the last draw
• model typical repetition patterns
• slightly bias generation toward realistic repetition

Avoid hard rules; use probabilistic weighting.

---

# 5 — Backtesting Engine

Create:

BacktestService

This service should simulate historical draws.

Process:

for each past draw:

1. generate candidate games using only past data
2. select the best games
3. compare with the actual result

Report:

• average hits
• distribution of 11/12/13/14 hits
• statistical performance

Output summary metrics.

---

# 6 — Diversity Metric Improvement

Replace the current diversity logic with:

Jaccard distance

distance = 1 - (intersection / union)

Use this metric when selecting final games to ensure diversity.

---

# 7 — Generator Architecture Refactor

Refactor generation pipeline:

GameGenerator
→ CandidateGeneration
→ Filtering
→ Scoring
→ Ranking
→ DiversitySelection
→ Optimization (genetic)

Ensure clear separation of responsibilities.

---

# 8 — Performance Improvements

Optimize weighted selection.

Avoid recalculating sums repeatedly.

Ensure generator can handle:

10,000+ candidate games efficiently.

---

# 9 — Diagnostics

Extend ResultadoGeracao with:

• rejection rate
• duplicate rate
• average score
• diversity index

---

# 10 — Documentation

Update README explaining:

• architecture
• statistical model
• scoring logic
• backtesting
• tuning parameters

---

Goal: turn the project into a statistically aware, high-quality Lotofácil game generation engine while keeping the code clean and maintainable.


Continue...

You are a senior .NET engineer, statistician and algorithm designer.

You are improving a lottery generation engine for the Brazilian game Lotofácil.

The project already contains:

* GeradorDeJogos
* FiltroService
* GameScorer
* weighted number generation
* diversity logic
* historical draw data

Your task is to evolve the project into a **statistically driven lottery generation engine**.

Focus on:

• statistical modeling
• algorithm quality
• architecture
• performance
• maintainability

Do not overengineer. Keep the code clean and idiomatic C#.

---

# 1 — Pattern Modeling from Historical Draws

Create:

PatternStatisticsService

Analyze historical draws and compute:

• distribution of numbers per row:

1–5
6–10
11–15
16–20
21–25

• parity distributions
• sequence length distributions
• repetition patterns from previous draw

Typical patterns:

3-3-3-3-3
4-3-3-3-2
3-4-3-3-2

Compute probability of each pattern.

Update GameScorer to reward games that resemble historically common patterns.

Penalize unrealistic patterns.

---

# 2 — Number Correlation Matrix

Create:

NumberCorrelationService

Build a 25x25 matrix showing how often numbers appear together in historical draws.

Example:

correlation[13,14] = strong
correlation[2,25] = weak

Normalize the matrix.

Modify GameScorer so that games containing historically correlated numbers gain a score bonus.

---

# 3 — Smart Last Draw Modeling

Create:

LastDrawAnalyzer

Lotofácil typically repeats 8–11 numbers from the previous draw.

Implement probabilistic modeling of this behavior.

When generating games:

slightly bias generation toward realistic repetition counts.

Avoid rigid rules.

---

# 4 — Genetic Optimization Engine

Create:

GeneticGameOptimizer

Workflow:

1 generate large candidate population
2 score candidates
3 select top games
4 crossover between games
5 random mutation
6 repeat for multiple generations

Mutation examples:

• replace a number
• swap numbers
• adjust parity balance

Use the optimizer to evolve games with higher statistical scores.

---

# 5 — Entropy Filtering

Create:

EntropyAnalyzer

Measure entropy of a generated game using factors such as:

• distribution across rows
• sequence structure
• parity balance
• clustering of numbers

Games with very low entropy (too ordered) or very high entropy (too chaotic) should receive a score penalty.

Goal:

favor statistically natural distributions.

---

# 6 — Backtesting Engine

Create:

BacktestService

Simulate historical draws.

Process:

for each past draw:

1 generate candidate games using only historical data available at that moment
2 select best games
3 compare with real result

Report:

• average hits
• distribution of 11/12/13/14 hits
• long term statistical performance

---

# 7 — Diversity Metric Improvement

Replace diversity logic with:

Jaccard distance

distance = 1 - (intersection / union)

Use this metric when selecting final games to maximize diversity.

---

# 8 — Generator Architecture

Refactor generation pipeline:

GameGenerator
→ CandidateGeneration
→ Filtering
→ Scoring
→ Ranking
→ DiversitySelection
→ GeneticOptimization

Ensure single responsibility per component.

---

# 9 — Performance Improvements

Optimize weighted number generation.

Avoid recalculating sums repeatedly.

Ensure generation of 10,000+ candidates is efficient.

---

# 10 — Diagnostics

Extend ResultadoGeracao to include:

• rejection rate
• duplicate rate
• average score
• entropy score
• diversity index

---

# 11 — Documentation

Update README explaining:

• architecture
• statistical modeling
• scoring system
• entropy filtering
• backtesting
• tuning parameters

---

Goal:

transform this project into a robust statistical Lotofácil game generation engine with clean architecture and maintainable code.
