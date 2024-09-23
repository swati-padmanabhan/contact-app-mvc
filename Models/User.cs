using System;
using System.Collections.Generic;

namespace ContactAppProject.Models
{
    public class User
    {
        public virtual Guid Id { get; set; }

        public virtual string UserName { get; set; }

        public virtual string Password { get; set; }

        public virtual string FirstName { get; set; }

        public virtual string LastName { get; set; }

        public virtual string Email { get; set; }

        public virtual bool IsAdmin { get; set; }

        public virtual bool IsActive { get; set; } = true;

        public virtual IList<Contact> Contacts { get; set; }

        public virtual Role Role { get; set; } = new Role();
    }
}