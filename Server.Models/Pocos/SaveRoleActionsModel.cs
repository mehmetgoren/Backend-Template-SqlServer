namespace Server.Models
{
    using System.Collections.Generic;

    public class SaveRoleActionsModel
    {
        public string RoleName { get; set; }
        public IEnumerable<TreeNode> Data { get; set; }
    }
}
