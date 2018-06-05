namespace Server.Rest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using ionix.Data;
    using ionix.Rest;
    using ionix.Utils.Extensions;
    using Microsoft.AspNetCore.Mvc;
    using Models;

    [TokenTableAuth]
    public class UtilsController : ApiControllerBase
    {
        //Örneğin İki Tarih Arası İStenirse bunu bir attribute ile meta dataya göm ve sayı tarih vb tüm alanlar için Kullan.
        [HttpPost]
        public Result<object> Search([FromBody] SearchParams searchParams)
        {
            return this.ResultList(() =>
            {
                var r = new SearchCriteriaResolver().Search(searchParams);
                Datas<object> ret = new Datas<object>();
                ret.EntityList = r.EntityList;
                ret.Total = r.Total;

                return ret;
            });
        }


        [HttpPost]
        public IActionResult GetMetaData([FromBody] HashSet<string> typeFullNameList)
        {
            return this.ResultList(() =>
            {
                return Metadata.Get(typeFullNameList);
            });
        }


        [HttpGet]
        public IActionResult QueryLog(string query)
        {
            var plainTextBytes = Convert.FromBase64String(query);
            query = Encoding.UTF8.GetString(plainTextBytes);

            return this.ResultList(() =>
            {
                List<dynamic> ret = new List<dynamic>();
                if (!String.IsNullOrEmpty(query))
                {
                    ret.AddRange(SQLog.Logger.Logs.Query(query.ToQuery()));
                }
                return ret;
            });
        }

        [HttpGet]// only admin can acces this method.
        public IActionResult ResetServerApp()
        {
            return this.ResultSingle(() =>
            {
                //Başka bir dış process bunu yapacak ve tekrar açacak.
                //var process = new Process
                //{
                //    StartInfo =
                //    {
                //        Verb = "runas",
                //        WorkingDirectory = @"C:\Windows\System32\",
                //        FileName = @"issreset.exe"
                //    }
                //};
                //process.Start();

                return 1;
            });
        }

        [HttpGet]
        public IActionResult GetConnectedUsers()
        {
            return this.ResultList(() =>
            {
                ICollection<V_AppUser> ret = new List<V_AppUser>();
                var users = TokenTable.Instance.GetCurrentUserList();
                if (!users.IsEmptyList())
                {
                    foreach (User user in users)
                    {
                        V_AppUser entity = this.Db.AppUser.QueryViewBy(user.Name);

                        if (null != entity)
                        {
                            ret.Add(entity);
                        }
                    }
                }

                return ret;
            });
        }

        //
    }
}