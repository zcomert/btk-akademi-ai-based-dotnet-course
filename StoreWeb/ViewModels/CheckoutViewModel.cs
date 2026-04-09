using System.ComponentModel.DataAnnotations;

namespace StoreWeb.ViewModels;

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Ad Soyad zorunludur.")]
    [Display(Name = "Ad Soyad")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Teslimat adresi zorunludur.")]
    [Display(Name = "Teslimat Adresi")]
    public string Address { get; set; } = string.Empty;
}
