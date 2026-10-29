using System.ComponentModel.DataAnnotations;
using CozinhaApp.Models;

namespace CozinhaApp.ViewModels;

public class ReceitaResumo
{
    public Receita Receita { get; set; } = null!;
    public int Porcoes { get; set; }
}

public class EstoqueFormViewModel
{
    [Required(ErrorMessage = "Informe o nome do ingrediente.")]
    [StringLength(100)]
    public string Nome { get; set; } = "";

    [Required(ErrorMessage = "Informe a quantidade.")]
    [Range(0.001, 100000000, ErrorMessage = "A quantidade deve ser maior que zero.")]
    public decimal? Quantidade { get; set; }

    public UnidadeMedida Unidade { get; set; } = UnidadeMedida.Grama;

    [Required(ErrorMessage = "Informe a validade.")]
    [DataType(DataType.Date)]
    public DateTime? Validade { get; set; }
}

public class ReceitaFormViewModel
{
    [Required(ErrorMessage = "Dê um nome ao prato.")]
    [StringLength(120)]
    public string Nome { get; set; } = "";

    [StringLength(500)]
    public string? Descricao { get; set; }

    public List<EtapaFormViewModel> Etapas { get; set; } = new();
}

public class EtapaFormViewModel
{
    [Required(ErrorMessage = "Descreva todas as etapas.")]
    public string Descricao { get; set; } = "";

    public List<EtapaIngredienteFormViewModel> Ingredientes { get; set; } = new();
}

public class EtapaIngredienteFormViewModel
{
    public int IngredienteId { get; set; }
    public decimal Quantidade { get; set; }
    public UnidadeMedida Unidade { get; set; } = UnidadeMedida.Grama;
}
