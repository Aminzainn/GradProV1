namespace GP.Models.DTO
{
    public class RegisterDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string LastName { get; set; }
        public bool Gender { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
