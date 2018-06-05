namespace Server.Rest
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using ionix.Rest;
    using ionix.Utils.Extensions;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using Newtonsoft.Json.Linq;
    using ionix.Data;

    //i believe service layer should be thin as mush as posible. Therefore i implement this controller as a proxy.
    [TokenTableAuth]
    public partial class AdminPanelController : ApiControllerBase
    {
        [HttpGet]
        public Result<Role> GetRoles()
        {
            return this.ResultList(this.Db.Role.Select);
        }

        [HttpGet]
        public Result<Role> GetRolesNoAdmin()
        {
            return this.ResultList(this.Db.Role.SelectAdminsOnly);
        }

        [HttpPost]
        public Result<int> SaveRole([FromBody] Role role)
        {
            return this.ResultSingle(() => this.Db.Role.Upsert(role));
        }

        public Result<V_Menu> GetMenus()
        {
            return this.ResultList(this.Db.GetV_MenuList);
        }

        [HttpPost]
        public Result<int> SaveMenu([FromBody] Menu menu)
        {
            if (!this.IsModelValid(menu))
                return this.ResultValidationFail<int>();


            return this.ResultSingle(() => this.Db.Menu.Upsert(menu));
        }


        [HttpGet]
        public Result<Menu> CreateMenu(int roleId)
        {
            return this.ResultList(() =>
            {
                List<Menu> list = new List<Menu>();

                Role role = this.Db.Role.SelectById(roleId);
                if (null != role)
                {
                    IEnumerable<Menu> menus;
                    if (role.IsAdmin)
                    {
                        menus = this.Db.Menu.Select().OrderBy(m => m.OrderNum);
                    }
                    else
                    {
                        menus =
                            this.Db.Menu.Query(
                                "select * from Menu where Id in (select MenuId from RoleMenu where RoleId=@0 and HasAccess=1) and Visible=1 order by OrderNum"
                                    .ToQuery(roleId));
                    }

                    list.AddRange(TreeView(menus, null));

                    List<Menu> tempList = new List<Menu>(list);
                    foreach (var parent in tempList)
                    {
                        if (parent.Childs.Count == 0)
                            list.Remove(parent);
                    }
                }

                return list;
            });
        }
        private static IEnumerable<Menu> TreeView(IEnumerable<Menu> pureList, int? parentId)
        {
            List<Menu> list = pureList.Where(i => i.ParentId == parentId).ToList();
            foreach (var menuItem in list)
            {
                var childs = TreeView(pureList, menuItem.Id);
                menuItem.Childs.AddRange(childs);
            }
            return list;
        }


        [HttpGet]
        public Result<V_RoleMenu> GetRoleMenuList(int roleId)
        {
            return this.ResultList(() => this.Db.GetV_RoleMenuList(roleId));
        }

        [HttpPost]
        public Result<int> SaveRoleMenu([FromBody] ApiParameter ap)
        {
            return this.ResultSingle(() =>
            {
                int roleId = ap[nameof(roleId)].ConvertTo<int>();
                JArray vRoleMenus = (JArray) ap[nameof(vRoleMenus)];

                IEnumerable<V_RoleMenu> list = vRoleMenus.ToTypedList<V_RoleMenu>();

                int ret = 0;
                if (!list.IsEmptyList())
                {
                    list.ForEach(i =>
                    {
                        i.RoleId = roleId;
                    });

                    using (var tran = this.CreateTransactionalDbContext())
                    {
                        ret += tran.RoleMenu.DeleteByRoleId(roleId);

                        List<RoleMenu> entityList = new List<RoleMenu>();

                        list.ForEach(i =>
                        {
                            RoleMenu rm = new RoleMenu();
                            rm.HasAccess = i.HasAccess ?? false;
                            rm.MenuId = i.Id;
                            rm.RoleId = roleId;

                            entityList.Add(rm);
                        });

                        ret += tran.RoleMenu.BatchInsert(entityList);

                        tran.Commit();
                    }
                }

                return ret;
            });
        }
        

        [HttpPost]
        public Result<int> SaveAppUser([FromBody] AppUser model)
        {
            if (!this.IsModelValid(model))
                return this.ResultValidationFail<int>();

            return this.ResultSingle(() =>
            {
                model.Username = model.Username?.Trim();
                model.Password = model.Password?.Trim();

                int ret = this.Db.AppUser.Upsert(model);

                return ret;
            });
        }
        //


        [HttpGet]
        public IActionResult GetAppSettingList()
        {
            return this.ResultList(() =>
            {
                List<AppSetting> ret = new List<AppSetting>();

                HashSet<string> hash = new HashSet<string>();
                var props = typeof(Config).GetProperties(BindingFlags.Public | BindingFlags.Static);

                props.ForEach(p => hash.Add(p.Name));

                IList<AppSetting> dbList = this.Db.AppSetting.Select().OrderBy(p => p.Name).ToList();

                foreach (string name in hash)
                {
                    var setting = dbList.FirstOrDefault(p => p.Name == name);
                    if (null == setting)
                    {
                        setting = new AppSetting() {Name = name};
                    }

                    ret.Add(setting);
                }

                return ret;
            });
        }

        [HttpPost]
        public IActionResult UpdateAllAppSetting([FromBody] IEnumerable<AppSetting> appSettingList)
        {
            return this.ResultSingle(() =>
            {
                int ret = 0;
                if (this.IsModelValid(appSettingList))
                {
                    using (var tran = this.CreateTransactionalDbContext())
                    {
                        ret += tran.AppSetting.DeleteAll();

                        ret += tran.AppSetting.BatchInsert(appSettingList);

                        tran.Commit();
                    }
                }

                return ret;
            });
        }
    }
} 