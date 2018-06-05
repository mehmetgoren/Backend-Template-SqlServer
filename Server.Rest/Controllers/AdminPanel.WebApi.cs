namespace Server.Rest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using ionix.Rest;
    using ionix.Utils.Extensions;
    using Microsoft.AspNetCore.Mvc;
    using Models;

    partial class AdminPanelController
    {
        [HttpGet]//Reflected Kısımdan veriler. Yani Assembly ile olanlar.
        public Result<TreeNode> GetApiActionsHierarchical(string role)
        {
            return this.ResultList(() => GetApiActionsHierarchical_Internal(role));
        }

        private static IEnumerable<TreeNode> GetApiActionsHierarchical_Internal(string role)
        {
            if (String.IsNullOrEmpty(role))
                throw new ArgumentNullException(nameof(role));

            var roleView = SqlRoleStorageProvider.Instance.GetAll();
            var indexed = IndexedRoles.Create(roleView);

            var asms = AppDomain.CurrentDomain.GetAssemblies();
            List<TreeNode> ret = new List<TreeNode>();
            ControllerActionsList reflectedList = ControllerActionsList.Create<ReflectController>(asms);

            foreach (ControllerActions reflectedCA in reflectedList)
            {
                string controllerName = reflectedCA.ControllerType.Name.Replace("Controller", "");
                TreeNode parent = new TreeNode();
                parent.data = reflectedCA.ControllerType.FullName;
                parent.label = controllerName;
                parent.children = new List<TreeNode>();

                bool hasParentCheck = false;
                foreach (MethodInfo mi in reflectedCA)
                {
                    TreeNode child = new TreeNode() { label = mi.Name };
                    var entity = indexed.Find(role, controllerName, mi.Name);
                    child.@checked = entity != null && entity.Enabled;
                    hasParentCheck = hasParentCheck || child.@checked;

                    parent.children.Add(child);
                }
                parent.@checked = hasParentCheck;
                ret.Add(parent);
            }
            return ret;
        }


        [HttpPost]//Reflection verileri ile oluşan ekrandan db aktarılacak veriler.
        public Result<int> SaveActionRoles([FromBody]SaveRoleActionsModel par)
        {
            return this.ResultSingle(() =>
            {
                List<RoleControllerActionEntity> list = new List<RoleControllerActionEntity>(par.Data.Count());
                foreach (var node in par.Data)
                {
                    foreach (var childNode in node.children)//Çünkü child note ile action lar kayıt edilmeli.
                    {
                        RoleControllerActionEntity entity = new RoleControllerActionEntity(par.RoleName, node.label, childNode.label);//role/controller/action
                        entity.Enabled = childNode.@checked;

                        list.Add(entity);
                    }
                }

                return SaveActionRoles(SqlRoleStorageProvider.Instance, list, AuthorizationValidator.Instance);
            });
        }

        ////Veriler Silinmeyecek sadece upsert edilecek.Ek olarak kullanılmayan Kısımlar elbete IRoleStorage ile ueniden silinebilir. Yani mesela fk ile refere edilmeyenler.
        private static int SaveActionRoles(IRoleStorageProvider provider, IEnumerable<RoleControllerActionEntity> uiEntityList, IAuthorizationValidator validator)//validator for refresh
        {
            int ret = 0;
            if (null != provider && !uiEntityList.IsEmptyList())
            {
                ret = provider.Save(uiEntityList);
                validator.RefreshStorageAndCachedData();
            }
            return ret;
        }

        [HttpPost]
        public Result<int> ClearUnusedRoleActions()
        {
            return this.ResultSingle(() => ClearUnusedRoleActions(SqlRoleStorageProvider.Instance, AuthorizationValidator.Instance));
        }

        private static int ClearUnusedRoleActions(IRoleStorageProvider provider, IAuthorizationValidator validator)
        {
            int ret = 0;
            if (null != provider && null != validator)
            {
                ret += provider.ClearNonExistRecords();
                validator.RefreshStorageAndCachedData();
            }
            return ret;
        }
    }
}
