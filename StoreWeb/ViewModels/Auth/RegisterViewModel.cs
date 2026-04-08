using System.ComponentModel.DataAnnotations;

namespace StoreWeb.ViewModels.Auth;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Ad Soyad alani zorunludur.")]
    [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir.")]
    [Display(Name = "Ad Soyad")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta alani zorunludur.")]
    [EmailAddress(ErrorMessage = "Gecerli bir e-posta adresi giriniz.")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sifre alani zorunludur.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Sifre en az 6 karakter olmalidir.")]
    [DataType(DataType.Password)]
    [Display(Name = "Sifre")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sifre tekrar alani zorunludur.")]
    [DataType(DataType.Password)]
    [Display(Name = "Sifre Tekrar")]
    [Compare(nameof(Password), ErrorMessage = "Sifre alanlari eslesmiyor.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
