using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNettyDemo.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(() => StartClient()).Wait();
        }
        static async Task StartClient()
        {
            var bossGroup = new DotNetty.Transport.Channels.MultithreadEventLoopGroup();
            try
            {
                var bootstarp = new DotNetty.Transport.Bootstrapping.Bootstrap();
                bootstarp.Group(bossGroup);
                bootstarp.Channel<DotNetty.Transport.Channels.Sockets.TcpSocketChannel>();
                bootstarp.Option(DotNetty.Transport.Channels.ChannelOption.TcpNodelay, true);
                bootstarp.Option(DotNetty.Transport.Channels.ChannelOption.SoKeepalive, true);
                bootstarp.Handler(new DotNetty.Transport.Channels.ActionChannelInitializer<DotNetty.Transport.Channels.Sockets.ISocketChannel>((channel) =>
                {
                    channel.Pipeline.AddLast(new MessageHandler());
                }));
                bootstarp.RemoteAddress("127.0.0.1", 8007);
                var ichannel = await bootstarp.ConnectAsync();
                Console.WriteLine("successfull connection to " + ichannel.RemoteAddress);
                while (true)
                {
                    var line = Console.ReadLine();
                    if (line == "exit") { await ichannel.CloseAsync(); break; }
                    await ichannel.WriteAndFlushAsync(DotNetty.Buffers.Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes(line)));
                }

            }
            catch (Exception ex) { Console.WriteLine(ex); }
            finally
            {
                Task.WaitAll(bossGroup.ShutdownGracefullyAsync());
            }
        }
    }
}
