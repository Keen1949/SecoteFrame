using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Communicate;
using CommonTool;

namespace PlcEx
{
    /// <summary>
    /// ModbusTcp
    /// </summary>
    public class Plc_ModbusTcp : PlcBase
    {
        #region 字段
        private TcpLink m_tcpLink;
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nIndex"></param>
        /// <param name="strName"></param>
        /// <param name="t"></param>
        /// <param name="nID"></param>
        public Plc_ModbusTcp(int nIndex, string strName, TcpLink t, int nID):base(nIndex,strName)
        {

        }
        #endregion

        #region 公有方法
        public override void Close()
        {
            throw new NotImplementedException();
        }

        public override bool Open()
        {
            throw new NotImplementedException();
        }

        public override bool Read(string address, int nLen, ref byte[] v)
        {
            throw new NotImplementedException();
        }

        public override bool ReadBit(string address, int bit, ref bool v)
        {
            throw new NotImplementedException();
        }

        public override bool Write(string address, byte[] v)
        {
            throw new NotImplementedException();
        }

        public override bool WriteBit(string address, int bit, bool v)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
