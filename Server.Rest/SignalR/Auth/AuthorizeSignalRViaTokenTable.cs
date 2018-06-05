//namespace Server.Rest
//{
//    using System;
//    using Microsoft.AspNet.SignalR;
//    using Microsoft.AspNet.SignalR.Hubs;
//    using Rest;

//    [AttributeUsage(AttributeTargets.Class)]
//    public class AuthorizeSignalRViaTokenTableAttribute : AuthorizeAttribute
//    {
//        public AuthorizeSignalRViaTokenTableAttribute()
//        {
//            this.RequireOutgoing = false;
//        }

//        public override bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request)
//        {
//            if (!Config.WebApiAuthEnabled)
//                return true;
//            string hubToken = request.QueryString[nameof(hubToken)];
//            if (!String.IsNullOrEmpty(hubToken))
//            {
//                Guid token;
//                if (Guid.TryParse(hubToken, out token))
//                {
//                    ionix.Rest.User user;
//                    return TokenTable.Instance.TryAuthenticateToken(token, out user);
//                }

//            }
//            return false;
//            // return base.AuthorizeHubConnection(hubDescriptor, request);
//        }

//        protected override bool UserAuthorized(System.Security.Principal.IPrincipal user)
//        {
//            return true;
//        }

//        //protected override bool UserAuthorized(System.Security.Principal.IPrincipal user)
//        //{
//        //    if (user == null)
//        //    {
//        //        throw new ArgumentNullException(nameof(user));
//        //    }



//        //    var principal = user as ClaimsPrincipal;

//        //    if (principal != null)
//        //    {
//        //        Claim authenticated = principal.FindFirst(ClaimTypes.Authentication);
//        //        if (authenticated != null && authenticated.Value == "true")
//        //        {
//        //            return true;
//        //        }
//        //        else
//        //        {
//        //            return false;
//        //        }
//        //    }
//        //    else
//        //    {
//        //        return false;
//        //    }
//        //}
//    }
//}