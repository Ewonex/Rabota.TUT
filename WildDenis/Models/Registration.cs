using System.ComponentModel.DataAnnotations;

namespace WildDenis.Models
{
    public class Registration
    {
        [Required(ErrorMessage = "Укажите логин")]
        [MaxLength(15, ErrorMessage = "Длина логина не должна превышать 15 символов")]
        [MinLength(5, ErrorMessage = "Длина логина не должна ,быть меньше 5 символов")]
        public string nickName { get; set; }

        [Required(ErrorMessage = "Укажите почту")]
        [MaxLength(30, ErrorMessage = "Длина почты не должна превышать 30 символов")]
        [MinLength(5, ErrorMessage = "Длина почты не должна ,быть меньше 5 символов")]
        public string email { get; set; }

        [Required(ErrorMessage = "Укажите фамилию")]
        [MaxLength(15, ErrorMessage = "Длина фамилии не должна превышать 15 символов")]
        [MinLength(2, ErrorMessage = "Длина фамилии не должна ,быть меньше 2 символов")]
        public string surname { get; set; }

        [Required(ErrorMessage = "Укажите имя")]
        [MaxLength(15, ErrorMessage = "Длина имени не должна превышать 15 символов")]
        [MinLength(2, ErrorMessage = "Длина имени не должна ,быть меньше 2 символов")]
        public string nameOfUser { get; set; }

        [Required(ErrorMessage = "Укажите отчество")]
        [MaxLength(15, ErrorMessage = "Длина отчества не должна превышать 15 символов")]
        [MinLength(2, ErrorMessage = "Длина отчества не должна ,быть меньше 2 символов")]
        public string secName { get; set; }

        [Required(ErrorMessage = "Введите номер телефона")]
        public string phoneNumber { get; set; }

        [Required(ErrorMessage = "Укажите роль")]
        public string roleOfUser { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [StringLength(15, ErrorMessage = "Длина пароля не должна превышать 15 символов")]
        [MinLength(5, ErrorMessage = "Длина пароля не должна ,быть меньше 5 символов")]
        public string pass { get; set; }

        [Required(ErrorMessage = "Повторите пароль")]
        [StringLength(15, ErrorMessage = "Длина пароля не должна превышать 15 символов")]
        [MinLength(5, ErrorMessage = "Длина пароля не должна ,быть меньше 5 символов")]
        public string passRepeat { get; set; }

    }
}
