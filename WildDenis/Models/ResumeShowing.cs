namespace WildDenis.Models
{
    public class ResumeShowing
    {
        public int idOfResume { get; set; }
        public string NameOfUser { get; set; }
        public string Surname { get; set; }
        public string SecName { get; set; }
        public string Phone { get; set; }
        public string email { get; set; }
        public string Photo { get; set; }
        public string vacansyName { get; set; }
        public string aboutMe { get; set; }
        public List<string> defaultSkills { get; set; }
        public List<string> proovedSkills { get; set; }
    }
}
