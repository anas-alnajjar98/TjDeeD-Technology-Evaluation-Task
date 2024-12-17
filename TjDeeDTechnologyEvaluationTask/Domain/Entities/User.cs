namespace Domain.Entities
{
    namespace Domain.Entities
    {
        public class User
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string PasswordHash { get; set; }
            public string PasswordSalt { get; set; }
            public string FullName { get; set; }
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public bool IsDeleted { get; set; } = false;

            public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();

            public ICollection<RefreshToken> RefreshTokens { get; set; }
        }
    }

}
