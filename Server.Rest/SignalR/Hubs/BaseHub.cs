namespace Server.Rest
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.SignalR;

    // [AuthorizeSignalRViaTokenTable]
    public abstract class BaseHub : Hub
    {
        private static StackTrace stOnConnected;
        public override Task OnConnectedAsync()
        {
            // Add your own code here.
            // For example: in a chat application, record the association between
            // the current connection ID and user name, and mark the user as online.
            // After the code in this method completes, the client is informed that
            // the connection is established; for example, in a JavaScript client,
            // the start().done callback is executed.
            if (this.Logging)
            {
                if (null == stOnConnected)
                    stOnConnected = new StackTrace();

                SQLog.Logger.Create(stOnConnected)
                    .Code(1010)
                    .Message($"Client (ClientId: {this.Context.ConnectionId}) was connected to Hub ({this.GetType()}:").SaveAsync();
            }
            return base.OnConnectedAsync();
        }

        private static StackTrace stOnDisconnected;

        public override Task OnDisconnectedAsync(Exception exception)
        {
            // Add your own code here.
            // For example: in a chat application, mark the user as offline, 
            // delete the association between the current connection id and user name.

            if (this.Logging)
            {
                if (null == stOnDisconnected)
                    stOnDisconnected = new StackTrace();

                SQLog.Logger.Create(stOnDisconnected)
                    .Code(1010)
                    .Message(
                        $"Client (ClientId: {this.Context.ConnectionId}) was disconnected to Hub ({this.GetType()}:")
                    .SaveAsync();
            }
            return base.OnDisconnectedAsync(exception);
        }

        protected bool Logging { get; set; } = true;

        public virtual Task JoinGroupAsync(string groupName)
        {
            return this.Groups.AddAsync(this.Context.ConnectionId, groupName);
        }

        public async Task JoinGroup(string groupName)
        {
            await this.JoinGroupAsync(groupName);
        }

        public virtual Task LeaveGroupAsync(string groupName)
        {
            return this.Groups.RemoveAsync(this.Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await this.LeaveGroupAsync(groupName);
        }
    }
}