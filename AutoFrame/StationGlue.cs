using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AutoFrameDll;
using CommonTool;
using ToolEx;

namespace AutoFrame
{
    class StationGlue : StationEx
    {
        /// <summary>
        /// 构造函数，需要设置站位当前的IO输入，IO输出，轴方向及轴名称，以显示在手动页面方便操作
        /// </summary>
        /// <param name="strName"></param>
        public StationGlue(string strName, string strEnName) : base(strName, strEnName)
        {
            io_in = new string[] { "点胶开始1", "点胶开始2" };

            io_out = new string[] { "通知ABB允许点胶" };
        }
        /// <summary>
        /// 站位初始化，用来添加伺服上电，打开网口，站位回原点等动作
        /// </summary>
        public override void StationInit()
        {
            base.StationInit();//请在此语句下方增加代码

            SetDO("通知ABB允许点胶", true);
            ShowLog("初始化完成", LogLevel.Info, "Initialization is complete");
        }
        /// <summary>
        /// 站位退出退程时调用，用来关闭伺服，关闭网口等动作
        /// </summary>
        public override void StationDeinit()
        {
            //增加代码
            SetDO("通知ABB允许点胶", false);


            base.StationDeinit();
        }

        //当所有站位均为全自动运行模式时，不需要重载该函数
        //当所有站位为半自动运行模式时，也不需要重载该函数， 只需要在站位流程开始时插入WaitBegin()即可保证所有站位同步开始。
        //当所有站位中，有的为半自动，有的为全自动时，半自动的站位不重载该函数，使用WaitBegin()控制同步，全自动的站位重载该函数返回true即可。
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //public override bool IsReady()
        //{
        //    return true;
        //}


        /// <summary>
        /// 初始化时设置站位安全状态，用来设备初始化时各站位之间相互卡站，
        /// 控制各站位先后动作顺序用，比如流水线组装，肯定需要组装的Z轴升
        /// 起后，流水线才能动作这种情况
        /// </summary>
        public override void InitSecurityState()
        {
            //
            base.InitSecurityState();//请在此语句上方增加代码
        }
        public override void EmgStop()
        {
            //
            base.EmgStop();
        }
        public override void OnPause()
        {
            //
            base.OnPause();
        }
        public override void OnResume()
        {
            //
            base.OnResume();
        }

        protected override void NormalRun()
        {
            ProductData tempData = new ProductData();
            //等待点胶开始1或者点胶开始2

            string strIo = WaitAnyIo(new string[] { "点胶开始1", "点胶开始2" }, true, -1);

            ShowLog(strIo);

            if (strIo == "点胶开始1")
            {
                tempData.m_nGlueIndex = 1;
            }
            else
            {
                tempData.m_nGlueIndex = 2;
            }

            //设置点胶开始时间
            tempData.m_dtStartGlue = DateTime.Now;
            ShowLog("开始时间：" + tempData.m_dtStartGlue.ToString("HH:mm:ss"));

            //等待点胶结束
            WaitIo(strIo, false, -1);

            if (strIo == "点胶开始1")
            {
                ShowLog("点胶结束1");
            }
            else
            {
                ShowLog("点胶结束2");
            }

            //设置点胶结束时间
            tempData.m_dtEndTime = DateTime.Now;

            ShowLog("结束时间：" + tempData.m_dtEndTime.ToString("HH:mm:ss"));

            //把点胶数据加入队列
            ProductMgr.GetInstance().m_queGlueData.Enqueue(tempData);

            ShowLog("本站完成", LogLevel.Info, "This station completed");
        }
    }
}
