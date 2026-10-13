using System;
using System.Net;
using System.Net.Sockets;

namespace RfidMes
{

    /// <summary>
    /// 将TCP客户端和缓冲区连接在一起，以便在服务器中进行管理
    /// </summary>
    public class TcpClientState
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tcpClient">The TCP client</param>
        /// <param name="buffer">The byte array buffer</param>
        public TcpClientState(TcpClient tcpClient, byte[] buffer)
        {
            if (tcpClient == null)
                throw new ArgumentNullException("tcpClient");
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            tcpClient.ReceiveBufferSize = 65365;
            this.TcpClient = tcpClient;
            this.Buffer = buffer;
        }


        //public int MsgConn { get; set; }



        /// <summary>
        /// 获取 TcpClient
        /// </summary>
        public TcpClient TcpClient { get; private set; }

        /// <summary>
        /// 获取缓冲区字节
        /// </summary>
        public byte[] Buffer { get; private set; }

        /// <summary>
        /// 获取网络流
        /// </summary>
        public NetworkStream NetworkStream
        {
            get
            {
                if (TcpClient.Connected)
                {
                    return TcpClient.GetStream();
                }
                else
                {
                    System.Net.Sockets.NetworkStream netstream = null;
                    return netstream;
                }
            }
        }

    }

}
