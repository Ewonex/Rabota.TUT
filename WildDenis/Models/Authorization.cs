using System.ComponentModel.DataAnnotations;

namespace WildDenis.Models
{
    public class Authorization
    {
        [Required(ErrorMessage = "Введите логин")]
        public string nickName { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [StringLength(15, ErrorMessage = "Длина пароля не должна превышать 15 символов")]
        [MinLength(5, ErrorMessage = "Длина пароля не должна ,быть меньше 5 символов")]
        public string pass { get; set; }

        public bool keepLoggedIn { get; set; }

    }
}
