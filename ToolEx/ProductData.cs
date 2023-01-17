using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolEx;

#pragma warning disable 1591

namespace ToolEx
{
    [Serializable]
    public partial class ProductData
    {
        public string m_strBarCode = "";   //物料码
        public string m_strJigCode = "";   //载具码
        public int m_nGlueIndex = 1;      //点胶位，1：左点胶位 2：右点胶位
        public DateTime m_dtBeginTime = DateTime.Now;     //开始时间
        public DateTime m_dtEndTime = DateTime.Now;       //结束时间
        public DateTime m_dtStartGlue = DateTime.Now;     //开始点胶时间
        public DateTime m_dtEndGlue = DateTime.Now;       //结束点胶时间
        public DateTime m_dtStartAssem = DateTime.Now;    //开始组装时间
        public double m_dbRecheckX = 0.0;  //复检X
        public double m_dbRecheckY = 0.0;  //复检Y
        public double m_dbRecheckU = 0.0;  //复检U
        public bool m_bResult = false;      //最终结果


        public double ExposureGlueTimeS
        {
            get
            {
                return (m_dtStartAssem - m_dtStartGlue).TotalSeconds;
            }
        }


        public double GlueTimeS
        {
            get
            {
                return (m_dtEndGlue - m_dtStartGlue).TotalSeconds;
            }
        }
    }

    public partial class ProductMgr
    {
        /// <summary>
        /// 点胶数据
        /// </summary>
        public Queue<ProductData> m_queGlueData = new Queue<ProductData>();

        /// <summary>
        /// 前置二维码信息
        /// </summary>
        public Queue<string> m_queScanFrontCode = new Queue<string>();

        
    }
}
