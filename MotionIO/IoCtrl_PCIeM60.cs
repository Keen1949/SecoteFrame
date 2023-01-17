/********************************************************************
	created:	2018/09/14
	filename: 	IOCTRL_InoEcat
	file ext:	cs
	author:		gxf
	purpose:	InoEcat运控卡的IO控制实现类
*********************************************************************/

using System;
using System.Threading;
using CommonTool;
using lctdevice;

namespace MotionIO
{
    /// <summary>
    /// 凌臣EtherCAT控制卡的IO控制，类名必须以"IoCtrl_"前导，否则加载不到
    /// 框架中的每个汇川总线IO卡只支持32个IO，超过32个需要配置多张卡
    /// </summary>
    public class IoCtrl_PCIeM60 : IoCtrl
    {
        /// <summary>
        /// DI组的数量，32个一组
        /// </summary>
        private int m_nDiGrpCnt = 0;

        /// <summary>
        /// DO组的数量
        /// </summary>
        private int m_nDoGrpCnt = 0;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nIndex">索引号</param>
        /// <param name="nCardNo">卡号</param>
        public IoCtrl_PCIeM60(int nIndex, int nCardNo) :base(nIndex,nCardNo)
        {
            m_strCardName = "PCIeM60";

            //框架中的每个汇川总线IO卡只支持32个IO，超过32个需要配置多张卡
            //m_strArrayIn = new string[InoEtherCatCard.Instance().m_tResoures.diNum];
            //m_strArrayOut = new string[InoEtherCatCard.Instance().m_tResoures.doNum];
            m_strArrayIn = new string[32];
            m_strArrayOut = new string[32];

            m_nDiGrpCnt = (LCTEtherCatCard.Instance().m_tResoures.DiNum  + 31) / 32;
            m_nDoGrpCnt = (LCTEtherCatCard.Instance().m_tResoures.DoNum + 31) / 32;
        }
        
        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public override bool  Init()
        {
            try
            {
                if (LCTEtherCatCard.Instance().IsInited)
                {
                    m_bEnable = true;
                    return true;
                }
                else
                {
                    m_bEnable = false;
                    return false;
                }
            }
            catch
            {
                m_bEnable = false;
                return false;
            }
        }

        /// <summary>
        /// 反初始化
        /// </summary>
        public override void DeInit()
        {
            LCTEtherCatCard.Instance().DeInit();
        }
        
        /// <summary>
        /// 获取输入信号
        /// </summary>
        /// <param name="nData">输入信号信息</param>
        /// <returns></returns>
        public override bool  ReadIOIn(ref int nData)
        {
            // 总线卡可以扩展多个IO模块，当I/O数量超过32个的时候，此函数只能读取前面32个I/O数据
            uint nInputData = 0;

            bool bRet = true;

            //从1开始
            short nCardNo = (short)(m_nCardNo * 32 + 1);

            short ret = ecat_motion.M_Get_Digital_Port_Input(nCardNo, ref nInputData, 0);
            if (ret != 0)
            {
                if (m_bEnable)
                {
                    string str1 = "PCIeM60板卡M_Get_Digital_Port_Input失败! result = {0}";
                    if (LanguageMgr.GetInstance().LanguageID != 0)
                    {
                        str1 = "PCIeM60 board card M_Get_Digital_Port_Input error! Result = {0}";
                    }
                    WarningMgr.GetInstance().Error(ErrorType.Err_IO_Read_In, "PCIeM60",
                        string.Format(str1, ret));
                }

                return false;
            }

            if (m_nInData != (int)nInputData)
            {
                m_bDataChange = true;
                m_nInData = nData = (int)nInputData;
            }
            else
            {
                m_bDataChange = false;
                nData = m_nInData;
            }
            return bRet;
        }

        /// <summary>
        /// 获取输入信号
        /// </summary>
        /// <param name="nIndex">输入信号位</param>
        /// <returns></returns>
        public override bool ReadIoInBit(int nIndex)
        {
            short diValue = 0;
            short diNo = (short)(m_nCardNo * 32 + nIndex);
            short ret = ecat_motion.M_Get_Digital_Chn_Input(diNo, out diValue,0);
            if (ret != 0)
            {
                if (m_bEnable)
                {
                    string str1 = "PCIeM60板卡M_Get_Digital_Chn_Input失败! result = {0}";
                    if (LanguageMgr.GetInstance().LanguageID != 0)
                    {
                        str1 = "PCIeM60 board card M_Get_Digital_Chn_Input error! Result = {0}";
                    }
                    WarningMgr.GetInstance().Error(ErrorType.Err_IO_Read_In, "PCIeM60",
                        string.Format(str1, ret));
                }

                return false;
            }

            return (diValue != 0);
        }

        /// <summary>
        /// 获取输出信号
        /// </summary>
        /// <param name="nIndex">输出点位</param>
        /// <returns></returns>
        public override bool ReadIoOutBit(int nIndex)
        {
            short doValue = 0;
            short doNo = (short)(m_nCardNo * 32 + nIndex);

            short ret = ecat_motion.M_Get_Digital_Chn_Output(doNo, out doValue, 0);
            if (ret != 0)
            {
                if (m_bEnable)
                {
                    string str1 = "PCIeM60板卡M_Get_Digital_Chn_Output失败! result = {0}";
                    if (LanguageMgr.GetInstance().LanguageID != 0)
                    {
                        str1 = "PCIeM60 board card M_Get_Digital_Chn_Output error! Result = {0}";
                    }
                    WarningMgr.GetInstance().Error(ErrorType.Err_IO_Read_In, "PCIeM60",
                        string.Format(str1, ret));
                }

                return false;
            }

            return (doValue != 0);
        }

        /// <summary>
        /// 获取输出信号
        /// </summary>
        /// <param name="nData"></param>
        /// <returns></returns>
        public override bool  ReadIOOut(ref int nData)
        {
            uint nOutputData = 0;

            //从1开始
            short nCardNo = (short)(m_nCardNo * 32 + 1);

            short ret = ecat_motion.M_Get_Digital_Port_Output(nCardNo, ref nOutputData, 0);
            if (ret != 0)
            {
                if (m_bEnable)
                {
                    string str1 = "PCIeM60板卡M_Get_Digital_Port_Output失败! result = {0}";
                    if (LanguageMgr.GetInstance().LanguageID != 0)
                    {
                        str1 = "PCIeM60 board card M_Get_Digital_Port_Output error! Result = {0}";
                    }
                    WarningMgr.GetInstance().Error(ErrorType.Err_IO_Read_Out, "PCIeM60",
                        string.Format(str1, ret));
                }

                return false;
            }

            nData = (int)nOutputData;
            return true;
        }
        
        /// <summary>
        /// 输出信号
        /// </summary>
        /// <param name="nIndex">输出点</param>
        /// <param name="bBit">输出值</param>
        /// <returns></returns>
        public override bool  WriteIoBit(int nIndex, bool bBit)
        {
            if (m_bEnable)
            {
                short doNo = (short)(m_nCardNo * 32 + nIndex);

                short ret = ecat_motion.M_Set_Digital_Chn_Output(doNo, bBit ? (short)1 : (short)0, 0);

                if (ret != 0)
                {

                    string str1 = "PCIeM60板卡M_Set_Digital_Chn_Output失败! result = {0}";
                    if (LanguageMgr.GetInstance().LanguageID != 0)
                    {
                        str1 = "PCIeM60 board card M_Set_Digital_Chn_Output error! Result = {0}";
                    }
                    WarningMgr.GetInstance().Error(ErrorType.Err_IO_Write, "PCIeM60",
                        string.Format(str1, ret));


                    return false;
                }

                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 输出行信号
        /// </summary>
        /// <param name="nData">输出信息</param>
        /// <returns></returns>
        public override bool  WriteIo(int nData)
        {
            if (m_bEnable)
            {
                //从1开始
                short nCardNo = (short)(m_nCardNo * 32 + 1);

                short ret = ecat_motion.M_Set_Digital_Port_Output(nCardNo, (uint)nData,0xFFFFFFFF, 0);
                if (ret != 0)
                {
                    if (m_bEnable)
                    {
                        string str1 = "PCIeM60板卡M_Set_Digital_Port_Output失败! result = {0}";
                        if (LanguageMgr.GetInstance().LanguageID != 0)
                        {
                            str1 = "PCIeM60 board card M_Set_Digital_Port_Output error! Result = {0}";
                        }
                        WarningMgr.GetInstance().Error(ErrorType.Err_IO_Write, "PCIeM60",
                            string.Format(str1, ret));
                    }

                    return false;
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}