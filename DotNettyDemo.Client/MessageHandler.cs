using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;

namespace DotNettyDemo.Client
{
    class MessageHandler : DotNetty.Transport.Channels.ChannelHandlerAdapter
    {
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var byteBuffer = message as DotNetty.Buffers.IByteBuffer;
            if (byteBuffer != null)
            {
                var msg = byteBuffer.ToString(Encoding.UTF8);
                Console.WriteLine("Recive from Server[" + context.Channel.RemoteAddress + "]：" + msg);
            }

        }
        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            context.Flush();
        }
        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            base.ExceptionCaught(context, exception);
            context.CloseAsync();
        }
    }
}
