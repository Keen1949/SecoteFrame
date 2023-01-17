using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communicate;
using CommonTool;


namespace PlcEx
{
    /// <summary>
    /// 欧姆龙的Fins协议的数据类型
    /// </summary>
    public class OmronFinsDataType
    {
        /// <summary>
        /// 实例化一个Fins的数据类型
        /// </summary>
        /// <param name="bitCode">进行位操作的指令</param>
        /// <param name="wordCode">进行字操作的指令</param>
        public OmronFinsDataType(byte bitCode, byte wordCode)
        {
            BitCode = bitCode;
            WordCode = wordCode;
        }



        /// <summary>
        /// 进行位操作的指令
        /// </summary>
        public byte BitCode { get; private set; }

        /// <summary>
        /// 进行字操作的指令
        /// </summary>
        public byte WordCode { get; private set; }



        /// <summary>
        /// DM Area
        /// </summary>
        public static readonly OmronFinsDataType DM = new OmronFinsDataType(0x02, 0x82);
        /// <summary>
        /// CIO Area
        /// </summary>
        public static readonly OmronFinsDataType CIO = new OmronFinsDataType(0x30, 0xB0);
        /// <summary>
        /// Work Area
        /// </summary>
        public static readonly OmronFinsDataType WR = new OmronFinsDataType(0x31, 0xB1);
        /// <summary>
        /// Holding Bit Area
        /// </summary>
        public static readonly OmronFinsDataType HR = new OmronFinsDataType(0x32, 0xB2);
        /// <summary>
        /// Auxiliary Bit Area
        /// </summary>
        public static readonly OmronFinsDataType AR = new OmronFinsDataType(0x33, 0xB3);
    }

    /// <summary>
    /// 欧姆龙PLC FinsTCP
    /// </summary>
    public class Plc_OmronFinsTcp : PlcBase
    {
        #region 字段
        //TCP连接
        private TcpLink m_tcpLink;

        // 握手信号
        // 46494E530000000C0000000000000000000000D6 
        private readonly byte[] HandSingle = new byte[]
        {
            0x46, 0x49, 0x4E, 0x53, // FINS
            0x00, 0x00, 0x00, 0x0C, // 后面的命令长度
            0x00, 0x00, 0x00, 0x00, // 命令码
            0x00, 0x00, 0x00, 0x00, // 错误码
            0x00, 0x00, 0x00, 0x01  // 节点号
        };

        /// <summary>
        /// 信息控制字段，默认0x80
        /// </summary>
        public byte ICF { get; set; } = 0x80;
        /// <summary>
        /// 系统使用的内部信息
        /// </summary>
        public byte RSV { get; private set; } = 0x00;

        /// <summary>
        /// 网络层信息，默认0x02，如果有八层消息，就设置为0x07
        /// </summary>
        public byte GCT { get; set; } = 0x02;

        /// <summary>
        /// PLC的网络号地址，默认0x00
        /// </summary>
        public byte DNA { get; set; } = 0x00;


        /// <summary>
        /// PLC的节点地址，假如你的PLC的Ip地址为192.168.0.10，那么这个值就是10
        /// </summary>
        /// <remarks>
        /// <note type="important">假如你的PLC的Ip地址为192.168.0.10，那么这个值就是10</note>
        /// </remarks>
        public byte DA1 { get; set; } = 0x13;

        /// <summary>
        /// PLC的单元号地址
        /// </summary>
        /// <remarks>
        /// <note type="important">通常都为0</note>
        /// </remarks>
        public byte DA2 { get; set; } = 0x00;

        /// <summary>
        /// 上位机的网络号地址
        /// </summary>
        public byte SNA { get; set; } = 0x00;


        private byte m_computerSA1 = 0x0B;

        /// <summary>
        /// 上位机的节点地址，假如你的电脑的Ip地址为192.168.0.13，那么这个值就是13
        /// </summary>
        /// <remarks>
        /// <note type="important">假如你的电脑的Ip地址为192.168.0.13，那么这个值就是13</note>
        /// </remarks>
        public byte SA1
        {
            get { return m_computerSA1; }
            set
            {
                m_computerSA1 = value;
                HandSingle[19] = value;
            }
        }

        /// <summary>
        /// 上位机的单元号地址
        /// </summary>
        public byte SA2 { get; set; }

        /// <summary>
        /// 设备的标识号
        /// </summary>
        public byte SID { get; set; } = 0x00;

        /// <summary>
        /// DM Area
        /// </summary>
        public static readonly OmronFinsDataType DM = new OmronFinsDataType( 0x02, 0x82 );
        /// <summary>
        /// CIO Area
        /// </summary>
        public static readonly OmronFinsDataType CIO = new OmronFinsDataType( 0x30, 0xB0 );
        /// <summary>
        /// Work Area
        /// </summary>
        public static readonly OmronFinsDataType WR = new OmronFinsDataType( 0x31, 0xB1 );
        /// <summary>
        /// Holding Bit Area
        /// </summary>
        public static readonly OmronFinsDataType HR = new OmronFinsDataType( 0x32, 0xB2 );
        /// <summary>
        /// Auxiliary Bit Area
        /// </summary>
        public static readonly OmronFinsDataType AR = new OmronFinsDataType( 0x33, 0xB3 );


        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nIndex">索引</param>
        /// <param name="strName">名称</param>
        /// <param name="t">TCP连接</param>
        /// <param name="nID">本机节点：1 - 254</param>
        public Plc_OmronFinsTcp(int nIndex, string strName,TcpLink t,int nID):base(nIndex,strName)
        {
            m_tcpLink = t;
            SA1 = (byte)nID;

            DataFormat = DataFormat.CDAB;
        }

        #region 公有方法

        /// <summary>
        /// 连接PLC
        /// </summary>
        /// <returns></returns>
        public override bool Open()
        {
            lock(this)
            {
                if (m_tcpLink.Open() && !m_bOpen)
                {
                    //发送握手信号
                    if (m_tcpLink.WriteData(HandSingle, HandSingle.Length))
                    {
                        //获取返回值并比较
                        //先读8个字节，以确定数据长度
                        byte[] head = new byte[8];
                        m_tcpLink.ReadData(head, 8, true);

                        int len = BitConverter.ToInt32(ReverseBytes(head, 4, 4), 0);

                        //再读取后面的数据
                        byte[] data = new byte[len];
                        m_tcpLink.ReadData(data, len, true);

                        //获取错误代码
                        int nErrorCode = BitConverter.ToInt32(ReverseBytes(data, 4, 4), 0);

                        if (nErrorCode != 0)
                        {
                            m_bOpen = false;

                            string str1 = "欧姆龙连接失败 - {0}";
                            if (LanguageMgr.GetInstance().LanguageID != 0)
                            {
                                str1 = "Omron connection failed - {0}";
                            }
                            //WarningMgr.GetInstance().Error(string.Format("欧姆龙连接失败 - {0}", nErrorCode));
                            WarningMgr.GetInstance().Error(ErrorType.Err_Plc_Open,m_strName,
                                string.Format(str1, nErrorCode));

                        }
                        else
                        {
                            m_bOpen = true;

                            SA1 = data[11];
                            DA1 = data[15];
                        }

                    }

                }
                return m_bOpen;
            }
            
        }

        /// <summary>
        /// 断开PLC连接
        /// </summary>
        /// <returns></returns>
        public override void Close()
        {
            lock(this)
            {
                m_tcpLink.Close();

                m_bOpen = false;
            }
            
        }

        /// <summary>
        /// 读数据
        /// </summary>
        /// <param name="address">PLC地址</param>
        /// <param name="nLen">读取长度</param>
        /// <param name="v">读取数据结果</param>
        /// <returns>是否成功</returns>
        public override bool Read(string address, int nLen, ref byte[] v)
        {
            string str1 = "欧姆龙地址[{0}]解析失败";
            string str2 = "欧姆龙PLC读数据失败 - {0}";
            string str3 = "欧姆龙PLC读数据失败 - 数据长度不够";
            string str4 = "欧姆龙PLC读数据失败,结束码错误 - {0}";
            if (LanguageMgr.GetInstance().LanguageID != 0)
            {
                str1 = "Omron address [{0}] parsing failed";
                str2 = "Omron PLC failed to read data - {0}";
                str3 = "Omron PLC failed to read data - insufficient data length";
                str4 = "Omron PLC reading data failed, end code error - {0}";
            }
            //解析地址
            byte[] startAddr;
            if (!AnalysisAddress(address,out startAddr))
            {
                //WarningMgr.GetInstance().Error(string.Format("欧姆龙地址[{0}]解析失败", address));
                WarningMgr.GetInstance().Error(ErrorType.Err_Plc_Read,m_strName,
                    string.Format(str1, address));

                return false;
            }

            lock(this)
            {
                //FINS帧命令
                byte[] PLCCommand = new byte[8];
                //读命令
                PLCCommand[0] = 0x01;    // 读取存储区数据
                PLCCommand[1] = 0x01;

                //起始数据地址
                Array.Copy(startAddr, 0, PLCCommand, 2, 4);

                //读数据长度
                nLen = Math.Min(nLen, v.Length);
                nLen = (nLen + 1) / 2;

                byte[] tmp = ReverseBytes(BitConverter.GetBytes((ushort)nLen), 0);
                Array.Copy(tmp, 0, PLCCommand, 6, 2);

                //打包数据
                byte[] cmd = PackCommand(PLCCommand);

                m_tcpLink.WriteData(cmd, cmd.Length);

                //读取返回
                //先读8个字节，以确定数据长度
                byte[] head = new byte[8];
                m_tcpLink.ReadData(head, 8, true);

                int len = BitConverter.ToInt32(ReverseBytes(head, 4, 4), 0);

                //再读取后面的数据
                byte[] data = new byte[len];
                m_tcpLink.ReadData(data, len, true);

                //获取错误代码
                int nErrorCode = BitConverter.ToInt32(ReverseBytes(data, 4, 4), 0);

                if (nErrorCode != 0)
                {
                    WarningMgr.GetInstance().Error(ErrorType.Err_Plc_Read,m_strName,
                        string.Format(str2, nErrorCode));

                    return false;
                }

                if (len < 22)
                {                    
                    WarningMgr.GetInstance().Error(ErrorType.Err_Plc_Read,m_strName,
                        string.Format(str3));

                    return false;
                }

                //获取结束码
                ushort usEndCode = BitConverter.ToUInt16(data, 20);

                if (usEndCode != 0)
                {
                    WarningMgr.GetInstance().Error(ErrorType.Err_Plc_Read, m_strName,
                        string.Format(str4, usEndCode));

                    return false;
                }


                //获取数据
                //实际数据长度
                int dataLen = len - 22;
                if (dataLen > 0)
                {
                    v = new byte[dataLen];
                    Array.Copy(data, 22, v, 0, dataLen);
                }

                return true;
            }
            
        }

        /// <summary>
        /// 写数据
        /// </summary>
        /// <param name="address">PLC地址</param>
        /// <param name="v">写入的数据</param>
        /// <returns></returns>
        public override bool Write(string address, byte[] v)
        {
            string str1 = "欧姆龙地址[{0}]解析失败";
            string str2 = "欧姆龙PLC写数据失败 - {0}";
            string str3 = "欧姆龙PLC写数据失败 - 数据长度不够";
            string str4 = "欧姆龙PLC写数据失败,结束码错误 - {0}";
            if (LanguageMgr.GetInstance().LanguageID != 0)
            {
                str1 = "Omron address [{0}] parsing failed";
                str2 = "Omron PLC failed to write data - {0}";
                str3 = "Omron PLC failed to write data - insufficient data length";
                str4 = "Omron PLC failed to write data, end code error - {0}";
            }

            //解析地址
            byte[] startAddr;
            if (!AnalysisAddress(address, out startAddr))
            {
                WarningMgr.GetInstance().Error(ErrorType.Err_Plc_Write, m_strName,
                    string.Format(str1, address));

                return false;
            }

            lock(this)
            {
                //FINS帧命令
                byte[] PLCCommand = new byte[8 + v.Length];
                //读命令
                PLCCommand[0] = 0x01;    // 读取存储区数据
                PLCCommand[1] = 0x02;

                //起始数据地址
                Array.Copy(startAddr, 0, PLCCommand, 2, 4);

                //写数据长度
                int nLen = v.Length / 2;

                byte[] tmp = ReverseBytes(BitConverter.GetBytes((ushort)nLen), 0);
                Array.Copy(tmp, 0, PLCCommand, 6, 2);

                //把要写入的数据追加到最后
                Array.Copy(v, 0, PLCCommand, 8, v.Length);

                //打包数据
                byte[] cmd = PackCommand(PLCCommand);

                //发送数据
                m_tcpLink.WriteData(cmd, cmd.Length);

                //读取返回
                //先读8个字节，以确定数据长度
                byte[] head = new byte[8];
                m_tcpLink.ReadData(head, 8, true);

                int len = BitConverter.ToInt32(ReverseBytes(head, 4, 4), 0);

                //再读取后面的数据
                byte[] data = new byte[len];
                m_tcpLink.ReadData(data, len, true);

                //获取错误代码
                int nErrorCode = BitConverter.ToInt32(ReverseBytes(data, 4, 4), 0);

                if (nErrorCode != 0)
                {
                    WarningMgr.GetInstance().Error(ErrorType.Err_Plc_Write, m_strName,
                        string.Format(str2, nErrorCode));

                    return false;
                }

                if (len < 22)
                {
                    WarningMgr.GetInstance().Error(ErrorType.Err_Plc_Write, m_strName,
                        string.Format(str3));

                    return false;
                }

                //获取结束码
                ushort usEndCode = BitConverter.ToUInt16(data, 20);

                if (usEndCode != 0)
                {
                    WarningMgr.GetInstance().Error(ErrorType.Err_Plc_Write, m_strName,
                        string.Format(str4, usEndCode));

                    return false;
                }

                return true;
            }
            
        }

        /// <summary>
        /// 读取位寄存器
        /// </summary>
        /// <param name="address">寄存器地址：D100</param>
        /// <param name="bit">bit位</param>
        /// <param name="v">寄存器值</param>
        /// <returns></returns>
        public override bool ReadBit(string address, int bit, ref bool v)
        {
            string str1 = "欧姆龙地址[{0}]解析失败";
            string str2 = "欧姆龙PLC读数据失败 - {0}";
            string str3 = "欧姆龙PLC读数据失败 - 数据长度不够";
            string str4 = "欧姆龙PLC读数据失败,结束码错误 - {0}";
            if (LanguageMgr.GetInstance().LanguageID != 0)
            {
                str1 = "Omron address [{0}] parsing failed";
                str2 = "Omron PLC failed to read data - {0}";
                str3 = "Omron PLC failed to read data - insufficient data length";
                str4 = "Omron PLC reading data failed, end code error - {0}";
            }

            //解析地址
            byte[] startAddr;
            if (!AnalysisAddress(address, bit, out startAddr))
            {
                WarningMgr.GetInstance().Error(ErrorType.Err_Plc_Read, m_strName,
                    string.Format(str1, address));

                return false;
            }

            lock(this)
            {
                //FINS帧命令
                byte[] PLCCommand = new byte[8];
                //读命令
                PLCCommand[0] = 0x01;    // 读取存储区数据
                PLCCommand[1] = 0x01;

                //起始数据地址
                Array.Copy(startAddr, 0, PLCCommand, 2, 4);

                //读数据长度
                int nLen = 1;

                byte[] tmp = ReverseBytes(BitConverter.GetBytes((ushort)nLen), 0);
                Array.Copy(tmp, 0, PLCCommand, 6, 2);

                //打包数据
                byte[] cmd = PackCommand(PLCCommand);

                m_tcpLink.WriteData(cmd, cmd.Length);

                //读取返回
                //先读8个字节，以确定数据长度
                byte[] head = new byte[8];
                m_tcpLink.ReadData(head, 8, true);

                int len = BitConverter.ToInt32(ReverseBytes(head, 4, 4), 0);

                //再读取后面的数据
                byte[] data = new byte[len];
                m_tcpLink.ReadData(data, len, true);

                //获取错误代码
                int nErrorCode = BitConverter.ToInt32(ReverseBytes(data, 4, 4), 0);

                if (nErrorCode != 0)
                {
                    WarningMgr.GetInstance().Error(ErrorType.Err_Plc_Read, m_strName,
                        string.Format(str2, nErrorCode));

                    return false;
                }

                if (len < 22)
                {
                    WarningMgr.GetInstance().Error(ErrorType.Err_Plc_Read, m_strName,
                        string.Format(str3));

                    return false;
                }

                //获取结束码
                ushort usEndCode = BitConverter.ToUInt16(data, 20);

                if (usEndCode != 0)
                {
                    WarningMgr.GetInstance().Error(ErrorType.Err_Plc_Read, m_strName,
                        string.Format(str4, usEndCode));

                    return false;
                }


                //获取数据
                //实际数据长度
                int dataLen = len - 22;
                if (dataLen > 0)
                {
                    tmp = new byte[dataLen];
                    Array.Copy(data, 22, tmp, 0, dataLen);

                    v = (tmp[0] == 1);
                }

                return true;
            }
            
        }

        /// <summary>
        /// 写取位寄存器
        /// </summary>
        /// <param name="address">寄存器地址：D100</param>
        /// <param name="bit">bit位</param>
        /// <param name="v">寄存器值</param>
        /// <returns></returns>
        public override bool WriteBit(string address, int bit, bool v)
        {
            string str1 = "欧姆龙地址[{0}]解析失败";
            string str2 = "欧姆龙PLC写数据失败 - {0}";
            string str3 = "欧姆龙PLC写数据失败 - 数据长度不够";
            string str4 = "欧姆龙PLC写数据失败,结束码错误 - {0}";
            if (LanguageMgr.GetInstance().LanguageID != 0)
            {
                str1 = "Omron address [{0}] parsing failed";
                str2 = "Omron PLC failed to write data - {0}";
                str3 = "Omron PLC failed to write data - insufficient data length";
                str4 = "Omron PLC failed to write data, end code error - {0}";
            }

            //解析地址
            byte[] startAddr;
            if (!AnalysisAddress(address,bit, out startAddr))
            {
                WarningMgr.GetInstance().Error(ErrorType.Err_Plc_Write, m_strName,
                    string.Format(str1, address));

                return false;
            }

            lock(this)
            {
                //FINS帧命令
                byte[] PLCCommand = new byte[8 + 1];
                //读命令
                PLCCommand[0] = 0x01;    // 读取存储区数据
                PLCCommand[1] = 0x02;

                //起始数据地址
                Array.Copy(startAddr, 0, PLCCommand, 2, 4);

                //写数据长度
                int nLen = 1;

                byte[] tmp = ReverseBytes(BitConverter.GetBytes((ushort)nLen), 0);
                Array.Copy(tmp, 0, PLCCommand, 6, 2);

                //把要写入的数据追加到最后
                PLCCommand[8] = v ? (byte)1 : (byte)0;

                //打包数据
                byte[] cmd = PackCommand(PLCCommand);

                //发送数据
                m_tcpLink.WriteData(cmd, cmd.Length);

                //读取返回
                //先读8个字节，以确定数据长度
                byte[] head = new byte[8];
                m_tcpLink.ReadData(head, 8, true);

                int len = BitConverter.ToInt32(ReverseBytes(head, 4, 4), 0);

                //再读取后面的数据
                byte[] data = new byte[len];
                m_tcpLink.ReadData(data, len, true);

                //获取错误代码
                int nErrorCode = BitConverter.ToInt32(ReverseBytes(data, 4, 4), 0);

                if (nErrorCode != 0)
                {
                    WarningMgr.GetInstance().Error(ErrorType.Err_Plc_Write, m_strName,
                        string.Format(str2, nErrorCode));

                    return false;
                }

                if (len < 22)
                {
                    WarningMgr.GetInstance().Error(ErrorType.Err_Plc_Write, m_strName,
                        string.Format(str3));

                    return false;
                }

                //获取结束码
                ushort usEndCode = BitConverter.ToUInt16(data, 20);

                if (usEndCode != 0)
                {
                    WarningMgr.GetInstance().Error(ErrorType.Err_Plc_Write, m_strName,
                        string.Format(str4, usEndCode));

                    return false;
                }

                return true;
            }
            
        }

        #endregion

        #region 私有方法
        /// <summary>
        /// 解析位地址
        /// </summary>
        /// <param name="address">地址：D100,C100,W100,H100,A100</param>
        /// <param name="bit">位数</param>
        /// <param name="startAddress">转化为起始地址数据</param>
        /// <returns></returns>
        private bool AnalysisAddress(string address,int bit,out byte[] startAddress)
        {
            startAddress = new byte[4];
            ushort addr = ushort.Parse(address.Substring(1));
            switch (address[0])
            {
                case 'D':
                case 'd':
                    {
                        // DM区数据
                        startAddress[0] = DM.BitCode;
                        break;
                    }
                case 'C':
                case 'c':
                    {
                        // CIO区数据
                        startAddress[0] = CIO.BitCode;
                        break;
                    }
                case 'W':
                case 'w':
                    {
                        // WR区
                        startAddress[0] = WR.BitCode;
                        break;
                    }
                case 'H':
                case 'h':
                    {
                        // HR区
                        startAddress[0] = HR.BitCode;
                        break;
                    }
                case 'A':
                case 'a':
                    {
                        // AR区
                        startAddress[0] = AR.BitCode;
                        break;
                    }

                default:
                    return false;
            }

            startAddress[1] = BitConverter.GetBytes(addr)[1];
            startAddress[2] = BitConverter.GetBytes(addr)[0];
            startAddress[3] = (byte)bit;

            return true;
        }

        /// <summary>
        /// 解析地址
        /// </summary>
        /// <param name="address">地址：D100,C100,W100,H100,A100</param>
        /// <param name="startAddress">转化为起始地址数据</param>
        /// <returns></returns>
        private bool AnalysisAddress(string address, out byte[] startAddress)
        {
            startAddress = new byte[4];
            ushort addr = ushort.Parse(address.Substring(1));
            switch (address[0])
            {
                case 'D':
                case 'd':
                    {
                        // DM区数据
                        startAddress[0] = DM.WordCode;
                        break;
                    }
                case 'C':
                case 'c':
                    {
                        // CIO区数据
                        startAddress[0] = CIO.WordCode;
                        break;
                    }
                case 'W':
                case 'w':
                    {
                        // WR区
                        startAddress[0] = WR.WordCode;
                        break;
                    }
                case 'H':
                case 'h':
                    {
                        // HR区
                        startAddress[0] = HR.WordCode;
                        break;
                    }
                case 'A':
                case 'a':
                    {
                        // AR区
                        startAddress[0] = AR.WordCode;
                        break;
                    }

                default:
                    return false;
            }

            startAddress[1] = BitConverter.GetBytes(addr)[1];
            startAddress[2] = BitConverter.GetBytes(addr)[0];

            return true;
        }

        /// <summary>
        /// 将普通的指令打包成完整的指令
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private byte[] PackCommand(byte[] cmd)
        {
            byte[] buffer = new byte[26 + cmd.Length];
            //FINS
            buffer[0] = (byte)'F';
            buffer[1] = (byte)'I';
            buffer[2] = (byte)'N';
            buffer[3] = (byte)'S';

            //数据长度
            byte[] tmp = BitConverter.GetBytes(buffer.Length - 8);
            Array.Reverse(tmp);
            tmp.CopyTo(buffer, 4);

            //命令码，发送读写指令时，固定为00000002
            buffer[11] = 0x02;

            //帧格式
            buffer[16] = ICF;
            buffer[17] = RSV;
            buffer[18] = GCT;
            buffer[19] = DNA;
            buffer[20] = DA1;
            buffer[21] = DA2;
            buffer[22] = SNA;
            buffer[23] = SA1;
            buffer[24] = SA2;
            buffer[25] = SID;

            //FINS命令帧
            cmd.CopyTo(buffer, 26);

            return buffer;
        }
        #endregion
    }
}
