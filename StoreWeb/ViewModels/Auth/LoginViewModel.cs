using System.ComponentModel.DataAnnotations;

namespace StoreWeb.ViewModels.Auth;

public class LoginViewModel
{
    [Required(ErrorMessage = "E-posta alani zorunludur.")]
    [EmailAddress(ErrorMessage = "Gecerli bir e-posta adresi giriniz.")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sifre alani zorunludur.")]
    [DataType(DataType.Password)]
    [Display(Name = "Sifre")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Beni Hatirla")]
    public bool RememberMe { get; set; }
}
