using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AutoFrameDll;
using CommonTool;
using ToolEx;
using Communicate;
using System.Reflection;
using System.Collections;

namespace AutoFrame
{
    class StationAlarm : StationEx
    {
        /// <summary>
        /// 所有站位列表
        /// </summary>
        List<StationBase> StationList = new List<StationBase>();//站位链表，不能清空！！！
        /// <summary>
        /// 不需要报警的站位列表
        /// </summary>
        List<string> sList = new List<string>();
        public StationAlarm(string strName, string strEnName) : base(strName, strEnName)
        {
            //英文名
            this.m_strEnName = strEnName;
            io_in = new string[] { };
            io_out = new string[] { };
            m_cylinders = new string[] { };
            Communicate.ShareModule.AutoFrame_FormMian = Program.fm;//共享20221103FormMian
            Communicate.ShareModule.Alarm = Alarm;//共享
            Communicate.ShareModule.Error = ShareError;//共享
            Communicate.ShareModule.Notify = StationMgrEx.Notify;//共享
            Communicate.ShareModule.ShowLog = ShowLogEx;//共享
        }
        uint nLastBuzzer;
        uint nLastLight;

        public override void StationInit()
        {
            this.nLastBuzzer = 0u;
            this.nLastLight = 0u;
            //StationList.Clear();//站位链表，不能清空！！！
            sList.Clear();
            sList.Add("StationAlarm");//添加不需要等待的站位
            StationList = StationMgr.GetInstance().m_lsStation;   //站位链表，不能清空！！！
            foreach (StationEx se in StationList)
            {
                if (sList.Contains(se.Name) || !se.StationEnable)
                    continue;
                while (!se.nStationStationInit)
                    WaitTimeDelay(100);
            }
            ShowLog("初始化完成", LogLevel.Info, "Initialization is complete");
        }



        /// <summary>
        /// 站位退出退程时调用，用来关闭伺服，关闭网口等动作
        /// </summary>
        public override void StationDeinit()
        {

        }
        /// <summary>
        /// 响应流程暂停的处理，比如流水线在暂停时需要停止,如果不需要可以不用重载
        /// </summary>
        public override void OnPause()
        {

            base.OnPause();
        }
        /// <summary>
        /// 响应流程恢复的处理，比如流水线在恢复时需要继续运行，如果不需要可以不用重载
        /// </summary>
        public override void OnResume()
        {
            nLastLight = 0u;
            nLastBuzzer = 0u;
            base.OnResume();
        }

        /// <summary>
        /// 站位急停时调用，如果站位急停时只需要停轴，不需要重载
        /// 如果需要关闭流水线，停止机器人等IO操作，则必须重载此
        /// 函数响应急停的处理
        /// </summary>
        public override void EmgStop()
        {

            base.EmgStop();
        }

        public override void InitSecurityState()
        {

        }

        public override void StationProcess()
        {
            try
            {
                WaitTimeDelay(30);
                NormalRun();
            }
            catch (SafeException ex)
            {
                //WarningMgr.GetInstance().Info(ex.ToString());

                throw ex;
            }
            catch (StationException ex)
            {
                //WarningMgr.GetInstance().Info(ex.ToString());

                throw ex;
            }
            catch (Exception ex)
            {
                WarningMgr.GetInstance().Info(ex.ToString());
                throw ex;
            }

        }

        /// <summary>
        /// 正常运行
        /// </summary>
        protected override void NormalRun()
        {
            int nNum = StationList.Count - sList.Count;//总计有多少站位
            uint[] nAlarmState = new uint[nNum];
            int nnn = 0;//报警计数
            bool b_IsDeinit = false;//是否停止
            foreach (StationEx se in StationList)//遍历并记录每个站的报警
            {
                if (sList.Contains(se.Name) || !se.StationEnable)
                    continue;
                nAlarmState[nnn] = se.nAlarmLight;
                nnn++;
                b_IsDeinit = b_IsDeinit || se.nStationStationInit;
            }
            if (!b_IsDeinit)
            {
                ShowLog("其他站位已关闭", LogLevel.Info, "Other station are closed");
                return;
            }
            uint nAlarm = 0;
            foreach (uint uAi in nAlarmState)//合并每个站的报警
            { nAlarm = nAlarm | uAi; }

            bool b_绿灯开 = (nAlarm & LightState.绿灯开) > 0u;//1
            bool b_红灯开 = (nAlarm & LightState.红灯开) > 0u; //2
            bool b_黄灯开 = (nAlarm & LightState.黄灯开) > 0u; //4
            bool b_蜂鸣开 = (nAlarm & LightState.蜂鸣开) > 0u; //8
            bool b_绿灯闪 = (nAlarm & LightState.绿灯闪) > 0u; //16
            bool b_红灯闪 = (nAlarm & LightState.红灯闪) > 0u; //32
            bool b_黄灯闪 = (nAlarm & LightState.黄灯闪) > 0u; //64
            bool b_蜂鸣闪 = (nAlarm & LightState.蜂鸣闪) > 0u; //128

            ShowLog($"绿灯开:{b_绿灯开}_红灯开:{b_红灯开}_黄灯开:{b_黄灯开}_蜂鸣开:{b_蜂鸣开}_绿灯闪:{b_绿灯闪}_红灯闪:{b_红灯闪}_黄灯闪:{b_黄灯闪}_蜂鸣闪:{b_蜂鸣闪}"
                , LogLevel.Info, $"Green light open:{b_绿灯开}_Red light open:{b_红灯开}_Yellow light open:{b_黄灯开}_Buzzer open:{b_蜂鸣开}_Green light flash:{b_绿灯闪}_Red light flash:{b_红灯闪}_Yellow light flash:{b_黄灯闪}_Buzzer flash:{b_蜂鸣闪}");

            uint nLight;//记录三色灯状态，下面为优先级

            if (b_红灯开)
                nLight = 2u;
            else if (b_红灯闪)
                nLight = 32u;
            else if (b_黄灯开)
                nLight = 4u;
            else if (b_黄灯闪)
                nLight = 64u;
            else if (b_绿灯闪)
                nLight = 16u;
            else if (b_绿灯开)
                nLight = 1u;
            else
                nLight = 0u;

            //uint aa = nLight << 1;
            uint nBuzzer;//记录蜂鸣器状态
            if (b_蜂鸣开)
                nBuzzer = 8u;
            else if (b_蜂鸣闪)
                nBuzzer = 128u;
            else
                nBuzzer = 0u;

            if (nLastLight == nLight && nLastBuzzer == nBuzzer)//报警信息未更新，不产生新报警
            {
                WaitTimeDelay(50);
                return;
            }
            else
            {
                IoMgr.GetInstance().AlarmLight(LightState.所有关);
                WaitTimeDelay(10);
            }
            nLastLight = nLight;
            nLastBuzzer = nBuzzer;
            IoMgr.GetInstance().AlarmLight(nLight | nBuzzer);
        }
        /// <summary>
        /// 共享Error方法
        /// </summary>
        /// <param name="type"></param>
        /// <param name="strObject"></param>
        /// <param name="strMessage"></param>
        /// <returns></returns>
        public bool ShareError(int type, string strObject, string strMessage)
        {
            Error((ErrorType)type, strObject, strMessage);
            return true;
        }

        private void ShowLogEx(string msg, int _logLevel)
        {
            ShowLog(msg, (LogLevel)_logLevel);
        }

    }
}
