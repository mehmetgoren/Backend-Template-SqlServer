namespace Server.Rest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using ionix.Data;
    using ionix.Rest;
    using Microsoft.AspNetCore.Mvc;
    using Models;

    //Bu Controller Authorize Olmuyor.
    public class UnauthorizedController : ApiControllerBase
    {
        //[HttpPost]
        //public Result<string> SignInByCredentials([FromBody] Credentials credentials)
        //{
        //    return this.ResultSingle(() =>
        //    {
        //        Guid? token = TokenTable.Instance.SignIn(credentials);
        //        if (null != token)
        //        {
        //            return token.Value.ToString();
        //        }

        //        return String.Empty;
        //    });
        //}

        [HttpPost]
        public Result<UserLocal> Login([FromBody] Credentials credentials)
        {
            return this.ResultSingle(() =>
            {
                StackTrace stackTrace = new StackTrace();
                UserLocal ret = null;
                if (null != credentials && !String.IsNullOrEmpty(credentials.Username) &&
                    !String.IsNullOrEmpty(credentials.Password))
                {
                        //AppUser Tablosunda Var mı?
                        AppUser appUser = this.Db.AppUser.QuerySingle("select * from AppUser a where lower(a.Username)=@0".ToQuery(credentials.Username.ToLower()));
                        if (null != appUser)
                        {
                            if (credentials.Password.ToLower() == appUser.Password?.ToLower())
                            {
                                ret = new UserLocal();
                                ret.Name = credentials.Username;
                                ret.RoleId = appUser.RoleId;
                                ret.Token = TokenTable.Instance.Login(credentials);

                                ret.Role = this.Db.Role.SelectById(appUser.RoleId);

                                if (appUser.LoginCount == null)
                                    appUser.LoginCount = 0L;
                                appUser.LoginCount++;
                                this.Db.AppUser.Update(appUser, p => p.LoginCount);
                            }
                            else
                            {
                                SQLog.Logger.Create(stackTrace)
                                    .Code(313)
                                    .Message(
                                        $"Unsuccessful login attempt. UserNname: '{credentials.Username}'. Password: '{credentials.Password}'")
                                    .SaveAsync();
                            }
                        }
                }
                if (null != ret && ret.Token != null)
                {
                    SQLog.Logger.Create(stackTrace)
                        .Code(313)
                        .Message(
                            $"Unsuccessful Login: '{credentials?.Username}. Password: '{credentials?.Password}'") //.Object(ret.Kisi)
                        .SaveAsync();
                }
                else
                {
                    SQLog.Logger.Create(stackTrace)
                        .Code(313)
                        .Message(
                            $"Successful Login: '{credentials?.Username}.") //.Object(ret.Kisi)
                        .SaveAsync();
                }
                return ret;
            });
        }

        [HttpPost]
        public IActionResult Logout(Guid token)
        {
            return this.ResultSingle(() => TokenTable.Instance.Logout(token));
        }

        [HttpGet]
        public IActionResult GetAppSettingList()
        {
            var ret = this.Db.AppSetting.Select();
            return Json(ret);
        }

        [HttpGet]
        public IActionResult Test()
        {
            var list = this.Db.Menu.Select().ToList();


            var ret = MakeTree(list);
            return Json(ret);
        }


        private static Menu FindInTree(IEnumerable<Menu> list, int id)
        {
            if (list != null &&  0 < id)
            foreach (var item in list)
            {
                if (item.Id == id)
                    return item;
                var child = FindInTree(item.Childs, id);
                if (null != child)
                    return child;
            }

            return null;
        }

        private static List<Menu> MakeTree(List<Menu> list)
        {
            var copyList = new List<Menu>(list);
            foreach (var item in copyList)
            {
                if (item.ParentId != null)
                {
                    var parent = FindInTree(list, item.ParentId.Value);
                    if (null == parent.Childs)
                        parent.Childs = new List<Menu>();
                    parent.Childs.Add(item);

                    list.Remove(item);
                }
                if (null == item.Childs)
                    item.Childs = new List<Menu>();
                item.Childs = MakeTree(item.Childs);
            }
            return list;
        }
    }
}