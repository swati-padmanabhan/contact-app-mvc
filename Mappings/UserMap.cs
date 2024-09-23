using ContactAppProject.Models;
using FluentNHibernate.Mapping;

namespace ContactAppProject.Mappings
{
    public class UserMap : ClassMap<User>
    {
        public UserMap()
        {
            Table("Users");
            Id(u => u.Id).GeneratedBy.GuidComb();
            Map(u => u.UserName);
            Map(u => u.Password);
            Map(u => u.FirstName);
            Map(u => u.LastName);
            Map(u => u.Email);
            Map(u => u.IsAdmin);
            Map(u => u.IsActive);
            HasOne(r => r.Role).Cascade.All().PropertyRef(r => r.User).Constrained();
            HasMany(c => c.Contacts).Inverse().Cascade.All();
        }
    }
}