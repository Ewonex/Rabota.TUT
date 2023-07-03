namespace WildDenis.Models
{
    public class Question
    {
        public int idOfQuestion { get; set; }
        public int idOfTest { get; set; }
        public string textOfQuestion { get; set; }
        public List<string> answers { get; set; }
        public int correctAnswer { get; set; }
    }
}
