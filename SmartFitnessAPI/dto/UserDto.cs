namespace SmartFitnessApi.Models
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
    }
    public class UserQueueDto
    {
        public int Id { get; set; }          // e.g. "u123"
        public string Name { get; set; }        // DisplayName
        public string AvatarUrl { get; set; }   // ProfilePictureUrl
        public int Age { get; set; }            // computed from DateOfBirth
        public string City { get; set; }        // City
        public int FitnessLevel { get; set; }   // FitnessLevel
    }
}