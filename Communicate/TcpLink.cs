//2019-01-22 Binggoo  1.为防止发送或接收数据缓冲区未满而延时，修改TcpClient的NoDelay属性值为True.
//2019-07-08 Binggoo  1.加入是否正在连接属性
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using CommonTool;
using System.Threading;

namespace Communicate
{
    /// <summary>
    /// 网络连接封装类
    /// </summary>
    public class TcpLink : LogView
    {
        /// <summary>
        ///网口号 
        /// </summary>
        public int m_nIndex;
        /// <summary>
        ///网口定义 
        /// </summary>
        public string m_strName;
        /// <summary>
        ///对方IP地址 
        /// </summary>
        public string m_strIP;
        /// <summary>
        ///端口号 
        /// </summary>
        public int m_nPort;
        /// <summary>
        ///超时时间,单位毫秒
        /// </summary>
        public int m_nTime;
        /// <summary>
        ///命令分隔 
        /// </summary>
        public string m_strLineFlag;

        /// <summary>
        ///命令分隔符 
        /// </summary>
        private string m_strLine;

        private TcpClient m_client = null;
        private bool m_bTimeOut = false;

        private bool m_bErrorReport = true;  //错误报警，有些时候不需要错误报警，而需要重连

        private bool m_bAsysnReceive = false; //异步接收标志

        private byte[] m_byBuffer; //异步接收数据缓存

        private object m_lock = new object();

        /// <summary>
        /// 异步接收数据委托
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public delegate void DataReceivedHandler(byte[] data, int length);

        /// <summary>
        /// 异步接收数据事件
        /// </summary>
        public event DataReceivedHandler DataReceivedEvent;

        /// <summary>
        /// 是否需要错误报警
        /// </summary>
        public bool ErrorReport
        {
            get
            {
                return m_bErrorReport;
            }

            set
            {
                m_bErrorReport = value;
            }
        }

        /// <summary>
        /// 是否正在连接
        /// </summary>
        public bool IsConnecting { get; set; }

        /// <summary>
        /// 状态变更委托函数定义
        /// </summary>
        /// <param name="tcp"></param>
        public delegate void StateChangedHandler(TcpLink tcp);
        /// <summary>
        /// 状态变更委托事件
        /// </summary>
        public event StateChangedHandler StateChangedEvent;

        private static bool m_bConnectSuccess = false;
        private static Exception socketException;
        private static ManualResetEvent TimeoutObject = new ManualResetEvent(false);

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nIndex"></param>
        /// <param name="strName"></param>
        /// <param name="strIP"></param>
        /// <param name="nPort"></param>
        /// <param name="nTimeMs"></param>
        /// <param name="strLine"></param>
        public TcpLink(int nIndex, string strName, string strIP, int nPort, int nTimeMs, string strLine)
        {
            m_nIndex = nIndex;
            m_strName = strName;
            m_strIP = strIP;
            m_nPort = nPort;
            m_nTime = nTimeMs;

            m_strLineFlag = strLine;
            if (strLine == "CRLF")
            {
                m_strLine = "\r\n";
            }
            else if (strLine == "CR")
            {
                m_strLine = "\r";
            }
            else if (strLine == "LF")
            {
                m_strLine = "\n";
            }
            else if (strLine == "无")
            {
                m_strLine = "";
            }
            else if (strLine == "ETX")
            {
                m_strLine = "\u0003";
            }
        }

        /// <summary>
        /// 判断是否超时
        /// </summary>
        /// <returns></returns>
        public bool IsTimeOut()
        {
            return m_bTimeOut;
        }

        /// <summary>
        ///网口打开时通过回调检测是否连接超时。 5秒种 
        /// </summary>
        /// <param name="asyncResult"></param>
        private void CallBackMethod(IAsyncResult asyncResult)
        {
            try
            {
                m_bConnectSuccess = false;
                TcpClient tcpClient = asyncResult.AsyncState as TcpClient;
                if (tcpClient.Client != null)
                {
                    tcpClient.EndConnect(asyncResult);
                    m_bConnectSuccess = true;
                }
            }
            catch (Exception ex)
            {
                m_bConnectSuccess = false;
                socketException = ex;
            }
            finally
            {
                IsConnecting = false;
                TimeoutObject.Set();
            }
        }

        /// <summary>
        ///打开网口 
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            lock (m_lock)
            {
                if (m_client == null)
                {
                    m_client = new TcpClient();
                }
                else
                {
                    m_client.Close();
                    m_client = new TcpClient();
                }

                //if (m_client.Connected == false && !IsConnecting)
                if (m_client.Connected == false)
                {
                    m_client.SendBufferSize = 4096;
                    m_client.SendTimeout = m_nTime;
                    m_client.ReceiveTimeout = m_nTime;
                    m_client.ReceiveBufferSize = 4096;

                    //缓冲区未满时，禁止延迟
                    m_client.NoDelay = true;

                    m_byBuffer = new byte[4096];

                    try
                    {
                        TimeoutObject.Reset();
                        var result = m_client.BeginConnect(m_strIP, m_nPort, null, null);
                        var IsSucess = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));
                        if (m_client.Connected)//已连接上的，使用下面的代码，未连接上的不可使用，否则会报异常。
                            m_client.EndConnect(result);
                        if (IsSucess)
                        {
                            if (StateChangedEvent != null)
                                StateChangedEvent(this);
                            return m_client.Connected;
                        }
                        else
                        {
                            //                   m_client.Close();
                            throw new TimeoutException("TimeOut Exception");
                        }

                        //     m_client.Connect(m_strIP, m_nPort);
                    }
                    catch (Exception e)
                    {
                        m_bTimeOut = true;
                        Debug.WriteLine(string.Format("{0}:{1}{2}\r\n", m_strIP, m_nPort, e.Message));
                        if (m_bErrorReport && SystemMgr.GetInstance().IsSimulateRunMode() == false)
                        {
                            //WarningMgr.GetInstance().Error(string.Format("51210,ERR-SSW,{0}:{1}{2}", m_strIP, m_nPort, e.Message));
                            WarningMgr.GetInstance().Error(ErrorType.Err_Tcp_Open, m_strName,
                                string.Format("{0}:{1}{2}", m_strIP, m_nPort, e.Message));

                        }
                        if (StateChangedEvent != null)
                            StateChangedEvent(this);
                    }
                }
                return m_client.Connected;
            }

        }

        /// <summary>
        /// 判断网口是否打开
        /// </summary>
        /// <returns></returns>
        public bool IsOpen()
        {
            return m_client != null && m_client.Connected;
        }
        private static readonly object o_CheckTcpIsOpen = new object();
        /// <summary>
        /// 判断TCP是否正常连接，如果连接失效，尝试3次连接
        /// </summary>
        /// <param name="tcplink">TCPClient</param>
        public void CheckTcpIsOpen(TcpClient tcpclient)
        {
            //测试是否open时，需要加锁，
            lock (o_CheckTcpIsOpen)
            {
                //tcp异常判断，进入尝试重连
                if (tcpclient == null || !tcpclient.Connected)
                {
                    //记录尝试次数
                    int recCTime = 0;
                    string strCN = "tcpclient error,Try connecting again for the {0} time";
                    ShareModule.ShowLog("tcpclient error,Try connecting...", (int)LogLevel.Warn);
                    //ShareModule.Notify("tcpclient error,Try connecting...");
                    while (recCTime < 3)
                    {
                        string _timesTran =
                            recCTime == 0 ? "first" :
                            recCTime == 1 ? "second" :
                            recCTime == 2 ? "third" :
                            "";
                        //strCN = string.Format(strCN, _timesTran);
                        ShareModule.ShowLog(string.Format(strCN, _timesTran), (int)LogLevel.Warn);
                        Open();
                        if (!IsOpen())
                        {
                            ShowLog(string.Format("The {0} time，the TCP connection failed", _timesTran), LogLevel.Warn);
                            //ShareModule.Notify(string.Format("The {0} time，the TCP connection failed", _timesTran));
                            if (recCTime >= 2)
                            {
                                if (ShareModule.Alarm("M99IPDE-01-01", new string[] { "继续尝试", "取消" }, 12, false) == DialogResult.Yes)//12 为黄灯开，蜂鸣开
                                {
                                    recCTime = -1;
                                }
                                else
                                {
                                    //报急停
                                    ShareModule.ShowLog($"TCP Error，{m_strName} Disconnected,Pelese check！", (int)LogLevel.Error);
                                    //ShareModule.Notify($"TCP Error，{m_strName} Disconnected,Pelese check！");
                                    ShareModule.Error((int)ErrorType.Err_Tcp, "TCP Error", $"{m_strName} Disconnected");
                                }
                            }
                        }
                        else
                        {
                            ShareModule.ShowLog(string.Format("The {0} time，the TCP connection is successful", _timesTran), (int)LogLevel.Warn);
                            //ShareModule.Notify(string.Format("The {0} time，the TCP connection is successful", _timesTran));
                            return;//如果已经连接，直接退出。
                        }
                        recCTime++;
                    }
                }
            }
        }
        /// <summary>
        ///向网口写入数据 
        /// </summary>
        /// <param name="sendBytes"></param>
        /// <param name="nLen"></param>
        /// <returns></returns>
        public bool WriteData(byte[] sendBytes, int nLen)
        {
            if (m_client.Connected)
            {
                NetworkStream netStream = m_client.GetStream();
                if (netStream.CanWrite)
                {
                    netStream.Write(sendBytes, 0, nLen);

                    string strData = HelpTool.ByteToASCIIString(sendBytes);

                    string strLog = string.Format("Send to {0} : {1}", this.m_strName, strData);

                    ShowLog(strLog);

                    WarningMgr.GetInstance().Info(strLog);

                }
                //netStream.Close();
                return true;
            }
            return false;
        }

        /// <summary>
        ///向网口写入字符串 
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        public bool WriteString(string strData)
        {
            if (m_client.Connected)
            {
                NetworkStream netStream = m_client.GetStream();
                if (netStream.CanWrite)
                {
                    Byte[] sendBytes = Encoding.UTF8.GetBytes(strData);
                    netStream.Write(sendBytes, 0, sendBytes.Length);

                    string strLog = string.Format("Send to {0} : {1}", this.m_strName, strData);

                    ShowLog(strLog);

                    WarningMgr.GetInstance().Info(strLog);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        ///向网口写入一行字符 
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        public bool WriteLine(string strData)
        {
            CheckTcpIsOpen(m_client);
            if (m_client.Connected)
            {
                NetworkStream netStream = m_client.GetStream();
                if (netStream.CanWrite)
                {
                    Byte[] sendBytes = Encoding.UTF8.GetBytes(strData + m_strLine);
                    netStream.Write(sendBytes, 0, sendBytes.Length);

                    string strLog = string.Format("Send to {0} : {1}", this.m_strName, strData);

                    ShowLog(strLog);

                    WarningMgr.GetInstance().Info(strLog);
                }
                //netStream.Close();
                return true;
            }
            return false;
        }

        /// <summary>
        ///从网口读取数据 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="nLen"></param>
        /// <param name="bWait">等待接收完全</param>
        /// <returns></returns>
        public int ReadData(byte[] bytes, int nLen, bool bWait = false)
        {
            CheckTcpIsOpen(m_client);
            m_bTimeOut = false;
            int nReadLen = 0;
            if (m_client.Connected)
            {
                try
                {
                    int nStartTick = Environment.TickCount;
                    do
                    {
                        NetworkStream netStream = m_client.GetStream();
                        if (netStream.DataAvailable)
                        {
                            nReadLen += netStream.Read(bytes, nReadLen, nLen - nReadLen);

                            if (nReadLen > 0)
                            {
                                string strData = HelpTool.ByteToASCIIString(bytes);

                                string strLog = string.Format("Receive from {0} : {1}", this.m_strName, strData);

                                ShowLog(strLog);

                                WarningMgr.GetInstance().Info(strLog);
                            }
                        }

                        if (nReadLen >= nLen)
                        {
                            break;
                        }

                        if (Environment.TickCount - nStartTick > m_nTime)
                        {
                            m_bTimeOut = true;
                            if (StateChangedEvent != null)
                            {
                                StateChangedEvent(this);
                            }
                            break;
                        }

                        Thread.Sleep(1);

                    } while (nReadLen < nLen && bWait);

                }
                catch/*(TimeoutException e)*/
                {
                    m_bTimeOut = true;
                    if (StateChangedEvent != null)
                        StateChangedEvent(this);
                }
            }
            return nReadLen;
        }

        /// <summary>
        ///从网口读取一行数据 
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        public int ReadLine(out string strData)
        {
            CheckTcpIsOpen(m_client);
            m_bTimeOut = false;
            strData = "";
            if (m_client.Connected)
            {
                try
                {
                    NetworkStream netStream = m_client.GetStream();
                    int nStartTick = Environment.TickCount;
                    while (true)
                    {
                        if (netStream.DataAvailable)
                        {
                            byte[] bytes = new byte[m_client.ReceiveBufferSize];
                            int n = netStream.Read(bytes, 0, (int)m_client.ReceiveBufferSize);
                            strData += Encoding.UTF8.GetString(bytes, 0, n);

                        }

                        //判断结束符，防止没有接收全
                        if (strData.EndsWith(m_strLine))
                        {
                            break;
                        }
                        else if (Environment.TickCount - nStartTick > m_nTime)
                        {
                            m_bTimeOut = true;
                            if (StateChangedEvent != null)
                            {
                                StateChangedEvent(this);
                            }
                            break;
                        }
                        Thread.Sleep(1);
                    }

                    //2018-01-30 binggoo 删除结束符，wait_receice_cmd中不需要加结束了
                    strData = strData.TrimEnd(m_strLine.ToCharArray());

                    if (strData.Length > 0)
                    {
                        string strLog = string.Format("Receive from {0} : {1}", this.m_strName, strData);

                        ShowLog(strLog);

                        WarningMgr.GetInstance().Info(strLog);
                    }
                }
                catch /*(TimeoutException e)*/
                {
                    m_bTimeOut = true;
                    if (StateChangedEvent != null)
                        StateChangedEvent(this);
                }
            }
            return strData.Length;
        }

        /// <summary>
        ///关闭网口 
        /// </summary>
        public void Close()
        {
            lock (m_lock)
            {
                if (m_client != null)
                {
                    if (m_client.Connected)
                    {
                        NetworkStream netStream = m_client.GetStream();
                        netStream.Close();
                    }
                    m_client.Close();

                    m_client = null;
                    m_bTimeOut = false;
                    if (StateChangedEvent != null)
                        StateChangedEvent(this);
                }
            }

        }

        /// <summary>
        /// 清除缓冲区
        /// </summary>
        public void ClearBuffer()
        {
            CheckTcpIsOpen(m_client);
            if (m_client != null)
            {
                NetworkStream netStream = m_client.GetStream();
                //m_client.GetStream().Flush();
                //netStream.Close();
                if (netStream.DataAvailable)
                {
                    byte[] bytes = new byte[m_client.ReceiveBufferSize];
                    netStream.Read(bytes, 0, (int)m_client.ReceiveBufferSize);
                }
            }
        }

        /// <summary>
        /// 开始异步接收数据
        /// </summary>
        public void BeginAsynReceive(DataReceivedHandler handler)
        {
            CheckTcpIsOpen(m_client);
            if (m_client.Connected)
            {
                try
                {
                    m_bAsysnReceive = true;
                    DataReceivedEvent += handler;
                    m_client.Client.BeginReceive(m_byBuffer, 0, m_byBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), m_client.Client);
                }
                catch (Exception e)
                {
                    m_bTimeOut = true;
                    Debug.WriteLine(string.Format("{0}:{1}{2}\r\n", m_strIP, m_nPort, e.Message));
                    if (m_bErrorReport && SystemMgr.GetInstance().IsSimulateRunMode() == false)
                    {
                        //WarningMgr.GetInstance().Error(string.Format("51210,ERR-SSW,{0}:{1}{2}", m_strIP, m_nPort, e.Message));
                        WarningMgr.GetInstance().Error(ErrorType.Err_Tcp_Read, m_strName,
                            string.Format("{0}:{1}{2}", m_strIP, m_nPort, e.Message));

                    }

                    if (StateChangedEvent != null)
                    {
                        StateChangedEvent(this);
                    }

                }
            }
        }

        /// <summary>
        /// 停止异步接收数据
        /// </summary>
        public void EndAsynReceive()
        {
            m_bAsysnReceive = false;
            if (DataReceivedEvent != null)
            {
                foreach (var d in DataReceivedEvent.GetInvocationList())
                {
                    DataReceivedEvent -= d as DataReceivedHandler;
                }
            }

        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="ar"></param>
        public void ReceiveMessage(IAsyncResult ar)
        {
            try
            {
                var socket = ar.AsyncState as Socket;

                //方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.endreceive.aspx  
                var length = socket.EndReceive(ar);

                if (DataReceivedEvent != null && m_bAsysnReceive)
                {
                    if (length > 0)
                    {
                        DataReceivedEvent(m_byBuffer, length);
                    }


                    //接收下一个消息(因为这是一个递归的调用，所以这样就可以一直接收消息了）  
                    socket.BeginReceive(m_byBuffer, 0, m_byBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), socket);
                }


            }
            catch (Exception e)
            {
                m_bTimeOut = true;
                Debug.WriteLine(string.Format("{0}:{1}{2}\r\n", m_strIP, m_nPort, e.Message));
                if (m_bErrorReport && SystemMgr.GetInstance().IsSimulateRunMode() == false && m_bAsysnReceive)
                {
                    //WarningMgr.GetInstance().Error(string.Format("51210,ERR-SSW,{0}:{1}{2}", m_strIP, m_nPort, e.Message));
                    WarningMgr.GetInstance().Error(ErrorType.Err_Tcp_Read, m_strName,
                        string.Format("{0}:{1}{2}", m_strIP, m_nPort, e.Message));

                }

                if (StateChangedEvent != null)
                {
                    StateChangedEvent(this);
                }
            }
        }

        /// <summary>
        /// 互锁
        /// </summary>
        public void Lock()
        {
            System.Threading.Monitor.Enter(m_lock);
        }

        /// <summary>
        /// 取消互锁
        /// </summary>
        public void UnLock()
        {
            if (System.Threading.Monitor.IsEntered(m_lock))
            {
                System.Threading.Monitor.Exit(m_lock);
            }

        }

    }
}

