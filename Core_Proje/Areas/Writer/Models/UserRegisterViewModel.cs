using System.ComponentModel.DataAnnotations;

namespace Core_Proje.Areas.Writer.Models
{
    public class UserRegisterViewModel
    {
        [Required(ErrorMessage = "Lütfen adınızı girin")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Lütfen soyadınızı girin")]
        public string Surname { get; set; }
        [Required(ErrorMessage = "Lütfen kullanıcı adını giriniz")]
        public string ImageURL { get; set; }
        [Required(ErrorMessage ="Lütfen bir resim URL değeri giriniz")]
        public string UserName { get; set; }

        [Required(ErrorMessage ="Lütfen şifreyi girin.")]
        public string Password { get; set; }

        [Required(ErrorMessage ="Lütfen şifreyi girin")]
        [Compare("Password", ErrorMessage ="Şifreler uyumlu değil.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage ="Lütfen mail girin")]
        public string Mail { get; set; }
    }
}
