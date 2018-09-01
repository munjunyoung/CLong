using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CLongLib;

namespace CLongServer
{
    internal class Program
    {
        private TcpListener tcpServer;
        public Int32 portNumber = 23000;
        
        //Thread signal
        private ManualResetEvent tcpClientConnected = new ManualResetEvent(false);
        
        private static void Main()
        {
            Program newProgram = new Program();
            newProgram.TcpStart();
        }
        
        /// <summary>
        /// Start Tcp Socket
        /// </summary>
        private void TcpStart()
        {
            var ep = new IPEndPoint(IPAddress.Any, portNumber);
            tcpServer = new TcpListener(ep);
            
            tcpServer.Start();
            Console.WriteLine("[TCPSERVER] : TCP Server Start! \n");
            //begin accept
            BeginAccept();
        }

        /// <summary>
        /// beginAccept Func
        /// </summary>
        private void BeginAccept()
        {
            tcpServer.BeginAcceptTcpClient(ConnectCb, tcpServer);
            Console.WriteLine("[TCPSERVER] : Waiting for a Connection....");
            tcpClientConnected.WaitOne();
        }

        /// <summary>
        /// acceptCallBack
        /// </summary>
        /// <param name="ar"></param>
        private void ConnectCb(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;
            ClientTCP newClient= new ClientTCP(listener.EndAcceptTcpClient(ar));
            Console.WriteLine("[TCPSERVER] : New Client Connected Completed");
            tcpServer.BeginAcceptTcpClient(ConnectCb, tcpServer);
        }
    }
}
