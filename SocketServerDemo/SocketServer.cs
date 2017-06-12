using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketServerDemo
{
    public class SocketServer
    {
        #region 委托
        public delegate void MessageNotifyHandler(object sender,MessageNotifyArgs e);
        public delegate void ClientManageHandler(object sender,ClientChangeArgs e);
        #endregion

        #region 事件
        public event MessageNotifyHandler MessageNotify;
        public event ClientManageHandler ClientManage;
        #endregion

        public SocketServer(string ip,int port)
        {
            this.ip = ip;
            this.port = port;
        }

        private string ip;
        private int port;

        private string RemoteEndPoint;     //客户端的网络结点  

        Thread threadwatch = null;//负责监听客户端的线程  
        Socket socketwatch = null;//负责监听客户端的套接字  
        //创建一个和客户端通信的套接字  
        Dictionary<string, Socket> dic = new Dictionary<string, Socket> { };   //定义一个集合，存储客户端信息
        #region  事件保护函数
        protected virtual void OnMessageNotify(MessageNotifyArgs e)
        {
            if (MessageNotify != null)
            {
                MessageNotify(this,e);
            }
        }

        protected virtual void OnClientManage(ClientChangeArgs e)
        {
            if (ClientManage != null)
            {
                ClientManage(this, e);
            }
        }
        #endregion
        /// <summary>
        /// 开启监听服务
        /// </summary>
        public void StartServer()
        {
            //定义一个套接字用于监听客户端发来的消息，包含三个参数（IP4寻址协议，流式连接，Tcp协议）  
            socketwatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //服务端发送信息需要一个IP地址和端口号  
            IPAddress address = IPAddress.Parse(ip);//获取文本框输入的IP地址  
            //将IP地址和端口号绑定到网络节点point上  
            IPEndPoint point = new IPEndPoint(address, port);//获取文本框上输入的端口号  
            //此端口专门用来监听的  
            //监听绑定的网络节点  
            socketwatch.Bind(point);
            //将套接字的监听队列长度限制为20  
            socketwatch.Listen(20);
            //创建一个监听线程  
            threadwatch = new Thread(watchconnecting);
            //将窗体线程设置为与后台同步，随着主线程结束而结束  
            threadwatch.IsBackground = true;
            //启动线程     
            threadwatch.Start();
            //启动线程后 textBox3文本框显示相应提示  
            DoMessageNotify("开始监听客户端传来的信息!");
        }
        /// <summary>
        /// 通知事件调用
        /// </summary>
        /// <param name="msg"></param>
        void DoMessageNotify(string msg)
        {
            MessageNotifyArgs e = new MessageNotifyArgs();
            e.Msg = msg;
            OnMessageNotify(e);
        }
        /// <summary>
        /// 客户端状态改变事件调用
        /// </summary>
        /// <param name="info">客户端名</param>
        /// <param name="changeCode">1-上线 2-离线</param>
        void OnlineList_Disp(string info,int changeCode)
        {
            ClientChangeArgs e = new ClientChangeArgs();
            e.ClientName = info;
            e.ChangeCode = changeCode;
            OnClientManage(e);
        }
        /// <summary>
        /// 监听
        /// </summary>
        private void watchconnecting()
        {
            Socket connection = null;
            while (true)  //持续不断监听客户端发来的请求     
            {
                try
                {
                    connection = socketwatch.Accept();
                }
                catch (Exception ex)
                {
                    DoMessageNotify(ex.Message);
                    break;
                }
                //获取客户端的IP和端口号  
                IPAddress clientIP = (connection.RemoteEndPoint as IPEndPoint).Address;
                int clientPort = (connection.RemoteEndPoint as IPEndPoint).Port;
                //让客户显示"连接成功的"的信息  
                string sendmsg = "连接服务端成功！\r\n" + "本地IP:" + clientIP + "，本地端口" + clientPort.ToString();
                byte[] arrSendMsg = Encoding.UTF8.GetBytes(sendmsg);
                connection.Send(arrSendMsg);
                RemoteEndPoint = connection.RemoteEndPoint.ToString(); //客户端网络结点号  
                dic.Add(RemoteEndPoint, connection);    //添加客户端信息  
                OnlineList_Disp(RemoteEndPoint,1);    //显示在线客户端  
                //IPEndPoint netpoint = new IPEndPoint(clientIP,clientPort);  
                IPEndPoint netpoint = connection.RemoteEndPoint as IPEndPoint;
                //创建一个通信线程      
                ParameterizedThreadStart pts = new ParameterizedThreadStart(recv);
                Thread thread = new Thread(pts);
                thread.IsBackground = true;//设置为后台线程，随着主线程退出而退出     
                //启动线程     
                thread.Start(connection);
            }
        }
        ///     
        /// 接收客户端发来的信息      
        ///     
        ///客户端套接字对象    
        private void recv(object socketclientpara)
        {
            Socket socketServer = socketclientpara as Socket;
            while (true)
            {
                //创建一个内存缓冲区 其大小为1024*1024字节  即1M     
                byte[] arrServerRecMsg = new byte[1024 * 1024];
                //将接收到的信息存入到内存缓冲区,并返回其字节数组的长度    
                try
                {
                    int length = socketServer.Receive(arrServerRecMsg);
                    //将机器接受到的字节数组转换为人可以读懂的字符串     
                    string strSRecMsg = Encoding.UTF8.GetString(arrServerRecMsg, 0, length);
                    DoMessageNotify("客户端:" + socketServer.RemoteEndPoint + "\r\n" + "时间:" + DateTime.Now + "\r\n" + strSRecMsg);

                    string sendmsg = "已经收到消息！";
                    byte[] arrSendMsg = Encoding.UTF8.GetBytes(sendmsg);
                    socketServer.Send(arrSendMsg);
                }
                catch (Exception ex)
                {
                    DoMessageNotify("客户端" + socketServer.RemoteEndPoint + "已经中断连接" + "\r\n");
                    OnlineList_Disp(socketServer.RemoteEndPoint.ToString(),2);
                    socketServer.Close();//关闭之前accept出来的和客户端进行通信的套接字  
                    break;
                }
            }
        }


    }
}
