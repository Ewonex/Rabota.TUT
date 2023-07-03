namespace WildDenis.Models
{
    public class PickedTest
    {
        public int idOfTest { get; set; }
        public string NameOfSkill { get; set; }
        public string description { get; set; }
        public string nameOfTest { get; set; }
        public List<Question> questions { get; set; }

    }
}
