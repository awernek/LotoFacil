using System.Text.Json;
using LotoFacil.Domain.Models;
using Microsoft.JSInterop;

namespace LotoFacil.Web.Services;

public class ConfiguracaoStorage(IJSRuntime js)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public ConfiguracaoFiltros Config { get; private set; } = ConfiguracaoFiltros.Padrao();

    public async Task CarregarAsync()
    {
        try
        {
            var json = await js.InvokeAsync<string?>("LotoFacilConfigGet");
            if (string.IsNullOrWhiteSpace(json))
                return;

            var loaded = JsonSerializer.Deserialize<ConfiguracaoFiltrosDto>(json, JsonOptions);
            if (loaded is null) return;

            AplicarDto(loaded);
        }
        catch
        {
            // Ignora erros de localStorage (privado, etc.)
        }
    }

    public async Task SalvarAsync()
    {
        try
        {
            var dto = CriarDto();
            var json = JsonSerializer.Serialize(dto, JsonOptions);
            await js.InvokeVoidAsync("LotoFacilConfigSet", json);
        }
        catch
        {
            // Ignora erros
        }
    }

    private void AplicarDto(ConfiguracaoFiltrosDto dto)
    {
        Config.Paridade = dto.Paridade ?? Config.Paridade;
        Config.Soma = dto.Soma ?? Config.Soma;
        Config.Primos = dto.Primos ?? Config.Primos;
        Config.Fibonacci = dto.Fibonacci ?? Config.Fibonacci;
        Config.Sequencias = dto.Sequencias ?? Config.Sequencias;
        Config.Historico = dto.Historico ?? Config.Historico;
        Config.RepeticaoUltimo = dto.RepeticaoUltimo ?? Config.RepeticaoUltimo;
        Config.NumerosAltos = dto.NumerosAltos ?? Config.NumerosAltos;
        Config.ModoInteligente = dto.ModoInteligente;
        Config.ModoRanqueado = dto.ModoRanqueado;
        Config.TamanhoPoolRanqueado = dto.TamanhoPoolRanqueado > 0 ? dto.TamanhoPoolRanqueado : 5000;
        if (dto.Faixas is not null)
        {
            Config.Faixas.Ativo = dto.Faixas.Ativo;
            Config.Faixas.MinPorFaixa = dto.Faixas.MinPorFaixa;
            Config.Faixas.MaxPorFaixa = dto.Faixas.MaxPorFaixa;
        }
    }

    private ConfiguracaoFiltrosDto CriarDto() => new()
    {
        Paridade = Config.Paridade,
        Soma = Config.Soma,
        Primos = Config.Primos,
        Fibonacci = Config.Fibonacci,
        Sequencias = Config.Sequencias,
        Historico = Config.Historico,
        RepeticaoUltimo = Config.RepeticaoUltimo,
        NumerosAltos = Config.NumerosAltos,
        ModoInteligente = Config.ModoInteligente,
        ModoRanqueado = Config.ModoRanqueado,
        TamanhoPoolRanqueado = Config.TamanhoPoolRanqueado,
        Faixas = new FiltroFaixasDto
        {
            Ativo = Config.Faixas.Ativo,
            MinPorFaixa = Config.Faixas.MinPorFaixa,
            MaxPorFaixa = Config.Faixas.MaxPorFaixa
        }
    };

    private sealed class ConfiguracaoFiltrosDto
    {
        public FiltroRange? Paridade { get; set; }
        public FiltroRange? Soma { get; set; }
        public FiltroRange? Primos { get; set; }
        public FiltroRange? Fibonacci { get; set; }
        public FiltroRange? Sequencias { get; set; }
        public FiltroSimples? Historico { get; set; }
        public FiltroRange? RepeticaoUltimo { get; set; }
        public FiltroRange? NumerosAltos { get; set; }
        public bool ModoInteligente { get; set; }
        public bool ModoRanqueado { get; set; }
        public int TamanhoPoolRanqueado { get; set; }
        public FiltroFaixasDto? Faixas { get; set; }
    }

    private sealed class FiltroFaixasDto
    {
        public bool Ativo { get; set; }
        public int MinPorFaixa { get; set; }
        public int MaxPorFaixa { get; set; }
    }
}
