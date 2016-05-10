using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNettyDemo.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(() => { return RunServer(); }).Wait();
        }
        static async Task RunServer()
        {
            //定义一个NIO的Selecter，这里叫住Group。全局控制。
            var bossGroup = new DotNetty.Transport.Channels.MultithreadEventLoopGroup(1);
            var workerGroup = new DotNetty.Transport.Channels.MultithreadEventLoopGroup();
            try
            {
                var bootstrap = new DotNetty.Transport.Bootstrapping.ServerBootstrap();//服务启动器
                bootstrap.Group(bossGroup, workerGroup);
                bootstrap.Channel<DotNetty.Transport.Channels.Sockets.TcpServerSocketChannel>();
                bootstrap.Option(DotNetty.Transport.Channels.ChannelOption.SoBacklog, 100);//接受的客户端最大连接数
                bootstrap.ChildHandler(new DotNetty.Transport.Channels.ActionChannelInitializer<DotNetty.Transport.Channels.Sockets.ISocketChannel>(channel =>
                {
                    DotNetty.Transport.Channels.IChannelPipeline pipeline = channel.Pipeline;
                    pipeline.AddLast(new MessageHandler());
                }));

                var bootstrapChannel = await bootstrap.BindAsync(8007);
                Console.WriteLine("Service has been runing ....  press Enter to stop. enput message boardcast...");
                while (true)
                {
                    var line = Console.ReadLine();
                    if (line == "exit")
                    {
                        await bootstrapChannel.CloseAsync();
                        break;
                    }
                    MessageHandler.ClientList.ForEach(ch => ch.WriteAndFlushAsync(DotNetty.Buffers.Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes(line))));
                }
            }
            finally
            {
                Task.WaitAll(bossGroup.ShutdownGracefullyAsync(), workerGroup.ShutdownGracefullyAsync());
            }


        }
    }
}
