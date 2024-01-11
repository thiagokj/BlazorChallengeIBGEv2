using System.ComponentModel.DataAnnotations;

namespace BlazorChallengeIBGE.Models;
public class State
{
    [Key]
    [Required(ErrorMessage = "Informe o código IBGE")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "O código deve conter 2 digitos")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Apenas digitos são permitidos")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe a sigla do Estado")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "A sigla deve conter 2 caracteres")]
    [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Apenas letras são permitidas")]
    public string Uf { get; set; } = null!;

    [Required(ErrorMessage = "Informe o nome do Estado")]
    [MinLength(3, ErrorMessage = "O Estado deve ter pelo menos 4 caracteres")]
    [MaxLength(100, ErrorMessage = "O Estado deve ter no máximo 100 caracteres")]
    [RegularExpression(@"^[a-zA-ZÀ-ÿ ]*$", ErrorMessage = "Apenas letras e acentuação são permitidos")]
    public string FullName { get; set; } = null!;
}