namespace Server.Models
{
    using System;
    using System.Dynamic;

    public sealed class UserLocal
    {
        public string Name { get; set; }

        public int RoleId { get; set; }

        public Role Role { get; set; }

        public ExpandoObject Person { get; set; }

        public Guid? Token { get; set; }
    }
}
