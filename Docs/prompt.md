You are a senior .NET architect and statistics-oriented engineer.

I have a .NET project that generates Lotofácil lottery games using filters and statistical heuristics.

The project already has:

- Clean separation of layers (Domain, Application, Infrastructure, Web)
- A Strategy Pattern using IFiltro to validate generated games
- Filters such as:
  - Parity
  - Sum range
  - Numerical ranges
  - Fibonacci numbers
  - Prime numbers
  - Sequences
  - Historical validation

Your task is to improve this system and evolve it into a more advanced lottery analysis and generation engine.

Goals:

1. Improve the statistical validity of generated games.
2. Add new filters used by experienced Lotofácil players.
3. Add a statistics engine that analyzes historical results.
4. Make the generator smarter instead of purely random.
5. Keep the architecture clean and modular.

Implement the following improvements:

1) Last draw repetition filter

Lotofácil usually repeats between 8 and 11 numbers from the previous draw.

Create a filter:

FiltroRepeticaoUltimoConcurso

Rules:
- Count how many numbers from the last draw exist in the generated game
- Accept only games with 8–11 repeated numbers

2) Cartela row distribution filter

The card layout is:

1  2  3  4  5
6  7  8  9 10
11 12 13 14 15
16 17 18 19 20
21 22 23 24 25

Most real draws contain between:

2 and 4 numbers per row.

Create:

FiltroDistribuicaoLinhas

3) Improve sum filter

Instead of allowing 170–220, narrow the statistically common range to:

185–210.

4) Create a StatisticsService

This service should:

- Analyze historical draws
- Calculate frequency of each number
- Detect hot numbers
- Detect cold numbers
- Calculate average parity
- Calculate average sum
- Calculate average repetition from previous draw

Return structured statistical insights.

5) Intelligent number selection

Instead of selecting numbers completely randomly:

Select a base of 18 numbers composed of:

- 9 hot numbers
- 6 medium frequency numbers
- 3 cold numbers

Then generate games of 15 numbers from this base.

6) Monte Carlo simulation module

Create a module capable of simulating:

100,000 or more random draws

Use this to evaluate filter performance.

Metrics to collect:

- % of generated games reaching 11 points
- % reaching 12 points
- % reaching 13 points

7) Improve generator performance

Avoid brute force loops when generating games.

Instead:

- Generate candidate sets intelligently
- Apply filters efficiently
- Ensure generation of N valid games quickly

8) Add a statistical dashboard (optional if Web layer exists)

Display:

- Number frequency heatmap
- Delay (how many draws since last appearance)
- Distribution of numbers per row
- Parity distribution
- Historical sum distribution

General constraints:

- Maintain clean architecture
- Write readable, maintainable code
- Use dependency injection
- Avoid monolithic services
- Keep filters modular

Explain your reasoning when adding statistical heuristics.

The final result should behave like a small "Lotofácil analysis lab".

Additional UI/UX requirement for generated games visualization.

The application currently displays generated games in a card/grid layout that resembles the Lotofácil ticket (5x5 grid with selected numbers highlighted).

Add a second visualization mode called "List Mode" that allows users to easily copy generated games and paste them into browser plugins used to auto-fill bets on the official Caixa Lotofácil website.

Requirements:

1) Add a toggle between two visualization modes:

- Card Layout (existing visual representation)
- List Mode (copy-friendly format)

Example UI toggle:

[ Card View ] [ List View ]

2) List Mode format

Each generated game must appear as a single line of numbers separated by spaces.

Numbers must always be formatted with two digits (01–25).

Example output:

01 02 03 05 06 07 08 10 11 12 13 14 15 18 19
01 02 03 04 06 07 08 09 10 11 13 14 15 18 21
01 02 03 05 06 07 08 09 10 11 13 14 16 19 23
01 02 04 05 06 07 08 10 11 12 13 14 15 19 21

3) Add a "Copy All Games" button

This button should copy the entire list of generated games to the clipboard in the format above.

Implementation suggestions:

- Use a textarea or preformatted block for easy copy
- Add a clipboard helper
- Ensure newline-separated output

4) Add export options

Allow exporting generated games as:

- TXT file
- CSV file

TXT format example:

01 02 03 05 06 07 08 10 11 12 13 14 15 18 19
01 02 03 04 06 07 08 09 10 11 13 14 15 18 21

CSV format example:

01,02,03,05,06,07,08,10,11,12,13,14,15,18,19

5) Ensure both visualization modes use the same underlying generated data.

The goal is to allow users to either:

- visually inspect the game patterns in card format
- quickly copy/paste games into browser automation plugins for the official Caixa Lotofácil betting page.

Focus on clean UI/UX and maintain the existing architecture.