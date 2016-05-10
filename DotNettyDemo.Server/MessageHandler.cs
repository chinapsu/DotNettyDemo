using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;

namespace DotNettyDemo.Server
{
    public class MessageHandler : DotNetty.Transport.Channels.ChannelHandlerAdapter
    {
        public static List<IChannelHandlerContext> ClientList = new List<IChannelHandlerContext>();
        public override void ChannelRegistered(IChannelHandlerContext context)
        {
            base.ChannelRegistered(context);
            ClientList.RemoveAll(h => h.Channel.Id.AsLongText() == context.Channel.Id.AsLongText());
            ClientList.Add(context);
            Console.WriteLine("ChannelRegistered|" + ClientList.Count);
        }
        public override void ChannelUnregistered(IChannelHandlerContext context)
        {
            Console.WriteLine("ChannelUnregistered|" + ClientList.Count);
            ClientList.RemoveAll(h => h.Channel.Id.AsLongText() == context.Channel.Id.AsLongText());
            base.ChannelUnregistered(context);
        }
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            DotNetty.Buffers.IByteBuffer buffer = message as DotNetty.Buffers.IByteBuffer;
            if (buffer != null)
            {
                Console.WriteLine("Received from client[" + context.Channel.RemoteAddress + "]: " + buffer.ToString(Encoding.UTF8));
            }
            var newBuffer = DotNetty.Buffers.Unpooled.WrappedBuffer(buffer.ToArray());
            System.Threading.Thread th = new System.Threading.Thread((msg) =>//使用新线程处理业务，避免阻塞ChannelRead方法处理复杂业务时，导致不同次收到的消息连成一片。
            {
                //这里是业务逻辑代码。
                //System.Threading.Thread.Sleep(2000);
                //下发过程。
                context.WriteAndFlushAsync(msg);
            });
            th.IsBackground = true;
            th.Start(newBuffer);
        }
        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            context.Flush();
            //context.WriteAndFlushAsync(DotNetty.Buffers.Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes("宋兴柱")));
        }
        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            ClientList.RemoveAll(h => h.Channel.Id.AsLongText() == context.Channel.Id.AsLongText());
            context.CloseAsync();
            Console.WriteLine("ExceptionCaught|" + ClientList.Count + "|" + exception.Message);
        }
    }
}