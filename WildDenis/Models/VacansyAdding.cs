using System.ComponentModel.DataAnnotations;
using Xunit.Sdk;

namespace WildDenis.Models
{
    public class VacansyAdding
    {
        [Required(ErrorMessage = "Укажите название должности")]
        [MaxLength(30, ErrorMessage = "Длина не должна превышать 30 символов")]
        [MinLength(2, ErrorMessage = "Длина не должна ,быть меньше 2 символов")]
        public string vacansyName { get; set; }

        [Required(ErrorMessage = "опишите себя")]
        [MaxLength(299, ErrorMessage = "Длина не должна превышать 299 символов")]
        [MinLength(2, ErrorMessage = "Длина не должна ,быть меньше 2 символов")]
        public string aboutVacansy { get; set; }

        [Required(ErrorMessage = "Укажите требуемые навыки")]
        [MaxLength(299, ErrorMessage = "Длина не должна превышать 299 символов")]
        [MinLength(2, ErrorMessage = "Длина не должна ,быть меньше 2 символов")]
        public string skills { get; set; }

        [Required(ErrorMessage = "Укажите з.п.")]
        public int salary { get; set; }


    }
}
