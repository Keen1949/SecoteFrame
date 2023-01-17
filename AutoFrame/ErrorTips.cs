using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using AutoFrameDll;
using CommonTool;
using Communicate;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using MotionIO;

namespace AutoFrame
{
    /// <summary>
    /// 存储弹窗信息的数据结构
    /// </summary>
    public struct PopWindowsInfo
    {
        /// <summary>
        /// 弹窗的Error code
        /// </summary>
        public string ErrorCode;
        /// <summary>
        /// 弹窗信息
        /// </summary>
        public string WindowsInfo;
        /// <summary>
        /// 弹窗信息英文翻译
        /// </summary>
        public string WindowsInfoEn;
        /// <summary>
        /// 弹窗信息其他语言翻译
        /// </summary>
        public string WindowsInfoOther;
        /// <summary>
        /// 弹窗Tips
        /// </summary>
        public string[] Tips;
        /// <summary>
        /// 弹窗Tips英文翻译
        /// </summary>
        public string[] TipsEn;
        /// <summary>
        /// 弹窗Tips其他语言翻译
        /// </summary>
        public string[] TipsOther;
    }



    class ErrorTips : SingletonTemplate<ErrorTips>
    {
        /// <summary>
        /// 字典类型，存储弹窗Tips信息
        /// </summary>
        private Dictionary<string, PopWindowsInfo> m_dictWinInfo = new Dictionary<string, PopWindowsInfo>();

        public ErrorTips()
        {
            string cfg = Application.StartupPath + "\\ErrorTips.xml";
            if (File.Exists(cfg))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(cfg);

                ReadPopWindowsInfoXML(doc);
            }
        }




        public void ReadPopWindowsInfoXML(XmlDocument doc)
        {
            m_dictWinInfo.Clear();

            XmlNodeList nodeList = doc.SelectSingleNode("ErrorTips").ChildNodes;

            foreach (XmlNode xn in nodeList)
            {
                XmlElement xe = (XmlElement)xn;
                PopWindowsInfo temp = new PopWindowsInfo();
                temp.ErrorCode = xe.Name;
                temp.Tips = new string[xe.ChildNodes.Count];
                temp.TipsEn = new string[xe.ChildNodes.Count];
                temp.TipsOther = new string[xe.ChildNodes.Count];
                temp.WindowsInfo = xe.GetAttribute("弹窗信息").Trim();
                temp.WindowsInfoEn = xe.GetAttribute("翻译").Trim() == "" ? xe.GetAttribute("弹窗信息").Trim() : xe.GetAttribute("翻译").Trim();
                temp.WindowsInfoOther = xe.GetAttribute("其它翻译").Trim() == "" ? xe.GetAttribute("弹窗信息").Trim() : xe.GetAttribute("其它翻译").Trim();
                foreach (XmlNode item in xe.ChildNodes)
                {
                    if (item.NodeType != XmlNodeType.Element)
                    {
                        continue;
                    }
                    XmlElement xeItem = (XmlElement)item;

                    temp.Tips[Convert.ToInt32(xeItem.GetAttribute("序号").Trim()) - 1] = xeItem.GetAttribute("解决方法").Trim();
                    temp.TipsEn[Convert.ToInt32(xeItem.GetAttribute("序号").Trim()) - 1] = xeItem.GetAttribute("翻译").Trim() == "" ? xeItem.GetAttribute("解决方法").Trim() : xeItem.GetAttribute("翻译").Trim();
                    temp.TipsOther[Convert.ToInt32(xeItem.GetAttribute("序号").Trim()) - 1] = xeItem.GetAttribute("其它翻译").Trim() == "" ? xeItem.GetAttribute("解决方法").Trim() : xeItem.GetAttribute("其它翻译").Trim();
                }

                if (!m_dictWinInfo.ContainsKey(temp.ErrorCode))
                {
                    m_dictWinInfo.Add(temp.ErrorCode, temp);
                }
            }

        }

        /// <summary>
        /// 获取弹窗Tips
        /// </summary>
        /// <param name="strKey"></param>
        /// <param name="Msg"></param>
        /// <returns></returns>
        public bool GetWindowsInfo(string strKey, out PopWindowsInfo Msg)
        {
            if (m_dictWinInfo.ContainsKey(strKey))
            {
                Msg = m_dictWinInfo[strKey];
                return true;
            }
            else
            {
                Msg = new PopWindowsInfo();
                return false;
            }
        }

    }


}



