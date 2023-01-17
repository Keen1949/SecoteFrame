using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlcEx
{
    /// <summary>
    /// 应用于多字节数据的解析或是生成格式
    /// </summary>
    public enum DataFormat
    {
        /// <summary>
        /// 按照顺序排序
        /// </summary>
        ABCD = 0,
        /// <summary>
        /// 按照单字反转
        /// </summary>
        BADC = 1,
        /// <summary>
        /// 按照双字反转
        /// </summary>
        CDAB = 2,
        /// <summary>
        /// 按照倒序排序
        /// </summary>
        DCBA = 3,
    }

    /// <summary>
    /// PLC基类
    /// </summary>
    public abstract class PlcBase
    {
        /// <summary>
        /// 指示PLC设备是否打开
        /// </summary>
        protected bool m_bOpen = false;

        /// <summary>
        /// PLC设备的序号
        /// </summary>
        protected int m_nIndex;

        /// <summary>
        /// PLC设备的名称
        /// </summary>
        protected string m_strName;

        /// <summary>
        /// 获取或设置数据解析的格式，默认DCBA，也即是无修改，可选ABCD,BADC，CDAB，DCBA格式，对于Modbus协议来说，默认ABCD
        /// </summary>
        public DataFormat DataFormat { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nIndex">索引</param>
        /// <param name="strName">名称</param>
        public PlcBase(int nIndex,string strName)
        {
            m_nIndex = nIndex;
            m_strName = strName;
        }

        #region 抽象方法
        /// <summary>
        /// 连接PLC
        /// </summary>
        /// <returns></returns>
        public abstract bool Open();

        /// <summary>
        /// 断开PLC连接
        /// </summary>
        /// <returns></returns>
        public abstract void Close();

        /// <summary>
        /// 是否连接
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return m_bOpen;
            }
        }
        /// <summary>
        /// 读寄存器的值
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="nLen">长度</param>
        /// <param name="v">寄存器值</param>
        /// <returns></returns>

        public abstract bool Read(string address, int nLen, ref byte[] v);

        /// <summary>
        /// 写寄存器的值
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="v">寄存器值</param>
        /// <returns></returns>

        public abstract bool Write(string address, byte[] v);

        /// <summary>
        /// 读取位寄存器
        /// </summary>
        /// <param name="address">寄存器地址：X100</param>
        /// <param name="bit">bit位</param>
        /// <param name="v">寄存器值</param>
        /// <returns></returns>
        public abstract bool ReadBit(string address, int bit, ref bool v);

        /// <summary>
        /// 写位寄存器
        /// </summary>
        /// <param name="address">寄存器地址：X100</param>
        /// <param name="bit">bit位</param>
        /// <param name="v">寄存器值</param>
        /// <returns></returns>
        public abstract bool WriteBit(string address, int bit, bool v);

        #endregion

        #region 公有方法
        /// <summary>
        /// 读字节寄存器
        /// </summary>
        /// <param name="address">寄存器地址：D100</param>
        /// <param name="v">寄存器值</param>
        /// <returns></returns>
        public bool ReadByte(string address, ref byte v)
        {
            byte[] data = new byte[sizeof(byte)];

            bool bRet = Read(address, data.Length, ref data);

            if (bRet)
            {
                v = data[0];
            }

            return bRet;
        }

        /// <summary>
        /// 写字节寄存器
        /// </summary>
        /// <param name="address">寄存器地址：D100</param>
        /// <param name="v">寄存器值</param>
        /// <returns></returns>
        public bool WriteByte(string address, byte v)
        {
            byte[] data = BitConverter.GetBytes(v);

            bool bRet = Write(address,data);

            return bRet;
        }

        /// <summary>
        /// 读取字寄存器
        /// </summary>
        /// <param name="address">寄存器地址：D100</param>
        /// <param name="v">寄存器值</param>
        /// <returns></returns>
        public bool ReadWord(string address, ref UInt16 v)
        {
            byte[] data = new byte[sizeof(UInt16)];

            bool bRet = Read(address, data.Length, ref data);

            if (bRet)
            {
                v = BitConverter.ToUInt16(ByteTransDataFormat2(data), 0);
            }

            return bRet;
        }

        /// <summary>
        /// 写字寄存器
        /// </summary>
        /// <param name="address">寄存器地址：D100</param>
        /// <param name="v">寄存器值</param>
        /// <returns></returns>
        public bool WriteWord(string address, UInt16 v)
        {
            byte[] data = BitConverter.GetBytes(v);

            bool bRet = Write(address, ByteTransDataFormat2(data));

            return bRet;
        }

        /// <summary>
        /// 读取双字寄存器
        /// </summary>
        /// <param name="address">寄存器地址：D100</param>
        /// <param name="v">寄存器值</param>
        /// <returns></returns>
        public bool ReadDWord(string address, ref UInt32 v)
        {
            byte[] data = new byte[sizeof(UInt32)];

            bool bRet = Read(address, data.Length, ref data);

            if (bRet)
            {
                v = BitConverter.ToUInt32(ByteTransDataFormat4(data), 0);
            }

            return bRet;
        }

        /// <summary>
        /// 写双字寄存器
        /// </summary>
        /// <param name="address">寄存器地址：D100</param>
        /// <param name="v">寄存器值</param>
        /// <returns></returns>
        public bool WriteDWord(string address, UInt32 v)
        {
            byte[] data = BitConverter.GetBytes(v);

            bool bRet = Write(address,ByteTransDataFormat4(data));

            return bRet;
        }


        /// <summary>
        /// 读多个字寄存器
        /// </summary>
        /// <param name="address">寄存器地址：D100</param>
        /// <param name="v">寄存器值</param>
        /// <returns></returns>
        public bool ReadMultWord(string address, ref UInt16[] v)
        {
            byte[] data = new byte[sizeof(UInt16) * v.Length];

            bool bRet = Read(address, data.Length, ref data);

            if (bRet)
            {
                for(int i = 0; i < v.Length;i++)
                {
                    byte[] tmp = new byte[sizeof(UInt16)];
                    Array.Copy(data, i * sizeof(UInt16),tmp, 0,tmp.Length);

                    v[i] = BitConverter.ToUInt16(ByteTransDataFormat2(tmp),0);
                }
            }

            return bRet;
        }

        /// <summary>
        /// 写多个字寄存器
        /// </summary>
        /// <param name="address">寄存器地址：X100</param>
        /// <param name="v">寄存器值</param>
        /// <returns></returns>
        public bool WriteMultWord(string address, UInt16[] v)
        {
            byte[] data = new byte[sizeof(UInt16) * v.Length];

            for(int i = 0; i < v.Length;i++)
            {
                byte[] buffer = BitConverter.GetBytes(v[i]);

                buffer = ByteTransDataFormat2(buffer);

                for (int k = 0; k < sizeof(UInt16); k++)
                {
                    data[i * 2 + k] = buffer[k];
                }
            }

            bool bRet = Write(address, data);

            return bRet;
        }
        /// <summary>
        /// 读多个双字寄存器
        /// </summary>
        /// <param name="address">寄存器地址：D100</param>
        /// <param name="v">寄存器值</param>
        /// <returns></returns>
        public bool ReadMultDWord(string address, ref UInt32[] v)
        {
            byte[] data = new byte[sizeof(UInt32) * v.Length];

            bool bRet = Read(address, data.Length, ref data);

            if (bRet)
            {
                for (int i = 0; i < v.Length; i++)
                {
                    byte[] tmp = new byte[sizeof(UInt32)];
                    Array.Copy(data, i * sizeof(UInt32), tmp, 0, tmp.Length);
                    v[i] = BitConverter.ToUInt32(ByteTransDataFormat4(tmp),0);
                }
            }

            return bRet;
        }

        /// <summary>
        /// 写多个双字寄存器
        /// </summary>
        /// <param name="address">寄存器地址：D100</param>
        /// <param name="v">寄存器值</param>
        /// <returns></returns>
        public bool WriteMultDWord(string address, UInt32[] v)
        {
            byte[] data = new byte[sizeof(UInt32) * v.Length];

            for (int i = 0; i < v.Length; i++)
            {
                byte[] buffer = BitConverter.GetBytes(v[i]);

                buffer = ByteTransDataFormat4(buffer);

                for(int k = 0; k < sizeof(UInt32);k++)
                {
                    data[i * 2 + k] = buffer[k];
                }
            }

            bool bRet = Write(address, data);

            return bRet;
        }

        #endregion

        #region  数据格式转换方法
        /// <summary>
        /// 反转多字节的数据信息
        /// </summary>
        /// <param name="value">数据字节</param>
        /// <param name="index">起始索引，默认值为0</param>
        /// <returns>实际字节信息</returns>
        protected byte[] ByteTransDataFormat2(byte[] value, int index = 0)
        {
            byte[] buffer = new byte[2];
            switch (DataFormat)
            {
                case DataFormat.ABCD:
                case DataFormat.CDAB:
                    {
                        buffer[0] = value[index + 1];
                        buffer[1] = value[index + 0];
                        break;
                    }
                case DataFormat.BADC:
                case DataFormat.DCBA:
                    {
                        buffer[0] = value[index + 0];
                        buffer[1] = value[index + 1];
                        break;
                    }
            }
            return buffer;
        }
        /// <summary>
        /// 反转多字节的数据信息
        /// </summary>
        /// <param name="value">数据字节</param>
        /// <param name="index">起始索引，默认值为0</param>
        /// <returns>实际字节信息</returns>
        protected byte[] ByteTransDataFormat4(byte[] value, int index = 0)
        {
            byte[] buffer = new byte[4];
            switch (DataFormat)
            {
                case DataFormat.ABCD:
                    {
                        buffer[0] = value[index + 3];
                        buffer[1] = value[index + 2];
                        buffer[2] = value[index + 1];
                        buffer[3] = value[index + 0];
                        break;
                    }
                case DataFormat.BADC:
                    {
                        buffer[0] = value[index + 2];
                        buffer[1] = value[index + 3];
                        buffer[2] = value[index + 0];
                        buffer[3] = value[index + 1];
                        break;
                    }

                case DataFormat.CDAB:
                    {
                        buffer[0] = value[index + 1];
                        buffer[1] = value[index + 0];
                        buffer[2] = value[index + 3];
                        buffer[3] = value[index + 2];
                        break;
                    }
                case DataFormat.DCBA:
                    {
                        buffer[0] = value[index + 0];
                        buffer[1] = value[index + 1];
                        buffer[2] = value[index + 2];
                        buffer[3] = value[index + 3];
                        break;
                    }
            }
            return buffer;
        }


        /// <summary>
        /// 反转多字节的数据信息
        /// </summary>
        /// <param name="value">数据字节</param>
        /// <param name="index">起始索引，默认值为0</param>
        /// <returns>实际字节信息</returns>
        protected byte[] ByteTransDataFormat8(byte[] value, int index = 0)
        {
            byte[] buffer = new byte[8];
            switch (DataFormat)
            {
                case DataFormat.ABCD:
                    {
                        buffer[0] = value[index + 7];
                        buffer[1] = value[index + 6];
                        buffer[2] = value[index + 5];
                        buffer[3] = value[index + 4];
                        buffer[4] = value[index + 3];
                        buffer[5] = value[index + 2];
                        buffer[6] = value[index + 1];
                        buffer[7] = value[index + 0];
                        break;
                    }
                case DataFormat.BADC:
                    {
                        buffer[0] = value[index + 6];
                        buffer[1] = value[index + 7];
                        buffer[2] = value[index + 4];
                        buffer[3] = value[index + 5];
                        buffer[4] = value[index + 2];
                        buffer[5] = value[index + 3];
                        buffer[6] = value[index + 0];
                        buffer[7] = value[index + 1];
                        break;
                    }

                case DataFormat.CDAB:
                    {
                        buffer[0] = value[index + 1];
                        buffer[1] = value[index + 0];
                        buffer[2] = value[index + 3];
                        buffer[3] = value[index + 2];
                        buffer[4] = value[index + 5];
                        buffer[5] = value[index + 4];
                        buffer[6] = value[index + 7];
                        buffer[7] = value[index + 6];
                        break;
                    }
                case DataFormat.DCBA:
                    {
                        buffer[0] = value[index + 0];
                        buffer[1] = value[index + 1];
                        buffer[2] = value[index + 2];
                        buffer[3] = value[index + 3];
                        buffer[4] = value[index + 4];
                        buffer[5] = value[index + 5];
                        buffer[6] = value[index + 6];
                        buffer[7] = value[index + 7];
                        break;
                    }
            }
            return buffer;
        }

        /// <summary>
        /// 字节反转
        /// </summary>
        /// <param name="buffer">需要反转的字节</param>
        /// <param name="index">起始索引</param>
        /// <param name="len">反转字节的长度，-1表示到结尾，长度超过会按照最大长度</param>
        /// <returns></returns>
        protected byte[] ReverseBytes(byte[] buffer,int index,int len = -1)
        {
            if (len == -1 || len > buffer.Length - index)
            {
                len = buffer.Length - index;
            }

            byte[] tmp = new byte[len];

            Array.Copy(buffer, index, tmp, 0, len);

            Array.Reverse(tmp);

            return tmp;
        }     

        /// <summary>
        /// 把缓存区数据反转转换为short
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected short ReverseBytesToInt16(byte[] buffer, int index)
        {
            byte[] tmp = new byte[2];
            tmp[0] = buffer[1 + index];
            tmp[1] = buffer[0 + index];
            return BitConverter.ToInt16(tmp, 0);
        }

        /// <summary>
        /// 把缓存区数据反转转换为ushort
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected ushort ReverseBytesToUInt16(byte[] buffer,int index)
        {
            byte[] tmp = new byte[2];
            tmp[0] = buffer[1 + index];
            tmp[1] = buffer[0 + index];
            return BitConverter.ToUInt16(tmp, 0);
        }

        /// <summary>
        /// 把缓存区数据反转转换为short
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected int ReverseBytesToInt32(byte[] buffer, int index)
        {
            byte[] tmp = new byte[4];
            tmp[0] = buffer[3 + index];
            tmp[1] = buffer[2 + index];
            tmp[2] = buffer[1 + index];
            tmp[3] = buffer[0 + index];
            return BitConverter.ToInt32(tmp, 0);
        }

        /// <summary>
        /// 把缓存区数据反转转换为ushort
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected uint ReverseBytesToUInt32(byte[] buffer, int index)
        {
            byte[] tmp = new byte[4];
            tmp[0] = buffer[3 + index];
            tmp[1] = buffer[2 + index];
            tmp[2] = buffer[1 + index];
            tmp[3] = buffer[0 + index];
            return BitConverter.ToUInt32(tmp, 0);
        }
        #endregion
    }
}
