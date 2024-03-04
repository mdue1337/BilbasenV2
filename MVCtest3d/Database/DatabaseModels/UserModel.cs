namespace MVCtest3d.Database.DatabaseModels
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Password {  get; set; }
        public string Email { get; set; }
        public bool Activated { get; set; }
    }
}