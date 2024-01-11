using System.ComponentModel.DataAnnotations;

namespace BlazorChallengeIBGE.Models;

public class Locality
{
  [Key]
  public Guid Id { get; set; } = Guid.NewGuid();

  [Required(ErrorMessage = "Informe o código IBGE")]
  [StringLength(7, MinimumLength = 7, ErrorMessage = "O código deve conter 7 digitos")]
  [RegularExpression(@"^\d+$", ErrorMessage = "Apenas digitos são permitidos")]
  public string IbgeCode { get; set; } = null!;

  [Required(ErrorMessage = "Informe o Id do Estado")]
  public int StateId { get; set; }
  public State State { get; set; } = null!;

  [Required(ErrorMessage = "Informe a Cidade")]
  [MinLength(3, ErrorMessage = "A cidade deve ter pelo menos 3 caracteres")]
  [MaxLength(100, ErrorMessage = "A cidade deve ter no máximo 100 caracteres")]
  [RegularExpression(@"^[a-zA-ZÀ-ÿ ]*$", ErrorMessage = "Apenas letras e acentuação são permitidos")]
  public string City { get; set; } = null!;
}
