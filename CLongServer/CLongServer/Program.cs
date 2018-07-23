using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CLongServer
{
    internal class Program
    {
        private TcpListener tcpServer;
        private Int32 portNumber = 23000;
        public static IPEndPoint ep;
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
            ep = new IPEndPoint(IPAddress.Any, portNumber);
            tcpServer = new TcpListener(ep);
            tcpServer.Start();
            Console.WriteLine("[TCPSERVER] : TCP Server Start! \n");
            
            //begin accept
            DoBeginAcceptTcpClient();
        }

        /// <summary>
        /// beginAccept Func
        /// </summary>
        private void DoBeginAcceptTcpClient()
        {
            tcpServer.BeginAcceptTcpClient(OnConnectTcpCallBack, tcpServer);
            Console.WriteLine("[TCPSERVER] : Waiting for a Connection....");
            tcpClientConnected.WaitOne();
        }

        /// <summary>
        /// acceptCallBack
        /// </summary>
        /// <param name="ar"></param>
        private void OnConnectTcpCallBack(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;
            ClientTCP newClient= new ClientTCP(listener.EndAcceptTcpClient(ar));
            newClient.ipePoint = (IPEndPoint)listener.LocalEndpoint;
            newClient.BeginReceiveTCP();
            Console.WriteLine("[TCPSERVER] : New Client Connected Completed");

            tcpServer.BeginAcceptTcpClient(OnConnectTcpCallBack, tcpServer);
        }
    }
}
