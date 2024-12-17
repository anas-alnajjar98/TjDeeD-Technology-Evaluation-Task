using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Domain.Entities;

namespace Domain.Entities
{
    public class UserRoleUser
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int RoleId { get; set; }
        public UserRole Role { get; set; }
    }
}
