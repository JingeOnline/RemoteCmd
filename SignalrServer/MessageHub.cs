using Microsoft.AspNetCore.SignalR;

namespace SignalrServer
{
    public class MessageHub:Hub
    {
        /// <summary>
        /// 建立连接
        /// </summary>
        /// <returns></returns>
        public override Task OnConnectedAsync()
        {
            Console.WriteLine("客户端连接成功");
            return base.OnConnectedAsync();
        }

        /// <summary>
        /// 当客户端断开连接的时候
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"客户端断开连接: {exception?.Message}");
            return base.OnDisconnectedAsync(exception);
        }

        public async Task ServerReceiveMessage(string s)
        {
            Console.WriteLine(s);
        }

        //public async Task ServerSendMessage(string s)
        //{
        //    await Clients.All.SendAsync("ClientReceiveMessage", s);
        //}
    }
}
