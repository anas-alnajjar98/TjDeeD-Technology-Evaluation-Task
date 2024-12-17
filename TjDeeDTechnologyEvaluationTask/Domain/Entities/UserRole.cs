using Domain.Entities.Domain.Entities;

namespace Domain.Entities
{
    public class UserRole
    {
        public int Id { get; set; }
        public string RoleName { get; set; }

        
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
