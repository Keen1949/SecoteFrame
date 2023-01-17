using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AutoFrameDll;
using CommonTool;
using AutoFrameVision;
using System.IO;
using ToolEx;
using System.Reflection;

namespace AutoFrame
{
    class StationMeshAssem : StationEx
    {
        public enum POINT
        {
            安全点 = 1,
            拍照点,
            复检点,
        }
        /// <summary>
        /// 构造函数，需要设置站位当前的IO输入，IO输出，轴方向及轴名称，以显示在手动页面方便操作
        /// </summary>
        /// <param name="strName"></param>
        public StationMeshAssem(string strName, string strEnName) : base(strName, strEnName)
        {
            io_in = new string[]
            {

            };

            io_out = new string[]
            {
                "Mesh组装上光源",
                "Mesh组装下光源",
                "Mesh对位光源",
                "MeshZ轴刹车"
            };

            m_Robot = RobotMgrEx.GetInstance().GetRobot("Mesh机器人");
        }
        /// <summary>
        /// 站位初始化，用来添加伺服上电，打开网口，站位回原点等动作
        /// </summary>
        public override void StationInit()
        {
            base.StationInit();//请在此语句下方增加代码
            //关闭光源
            SetDO("Mesh组装上光源", false);
            SetDO("Mesh组装下光源", false);
            SetDO("Mesh对位光源", false);

            if (!m_Robot.InitRobot())
            {
                ShowLog("Mesh机器人初始化失败");
                WarningMgr.GetInstance().Error(ErrorType.Err_Robot, "Mesh机器人", "Mesh机器人初始化失败");

                return;
            }

            //RobotCmd("Init", "InitOK", 6, SystemMgr.GetInstance().IsDryRunMode() ? "1" : "0");

            SetBit(SysBitReg.Mesh组装站初始化完成, true);

            ShowLog("初始化完成", LogLevel.Info, "Initialization is complete");
        }
        /// <summary>
        /// 站位退出退程时调用，用来关闭伺服，关闭网口等动作
        /// </summary>
        public override void StationDeinit()
        {
            //增加代码
            SetBit(SysBitReg.Mesh组装站初始化完成, false);
            SetBit(SysBitReg.Mesh机械手组装完成, false);


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
            SetBit(SysBitReg.Mesh组装站初始化完成, false);
            SetBit(SysBitReg.Mesh机械手组装完成, false);
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
            WaitBegin();

            ShowLog("等待转盘转到位");
            if (StationMgr.GetInstance().GetStation("转盘站").StationEnable)
            {
                WaitRegBit((int)SysBitReg.转盘站通知Mesh站转动到位, true, -1);
            }
            SetBit(SysBitReg.转盘站通知Mesh站转动到位, false);

            ProductData data = ProductMgr.GetInstance().Job.Get(3);

            //判断上一站结果，结果为false不用继续
            if (!data.m_bResult && StationMgr.GetInstance().GetStation("转盘站").StationEnable)
            {
                //上一站结果失败，不用组装
                ShowLog("上一站结果失败,跳过本站");
                return;
            }

            double speed = SystemMgr.GetInstance().GetParamDouble("MeshRobotSpeed") / 100;

            if (SystemMgr.GetInstance().GetParamBool("VisionEnable"))
            {
                //是否复检Strobe组装
                #region 复检
                if (SystemMgr.GetInstance().GetParamBool("ReCheckMeshEnable"))
                {
                    ShowLog("开始复检");

                    double dx, dy, du;
                    bool bReCheck = ReCheck(out dx, out dy, out du);

                    data.m_bResult = bReCheck;
                    data.m_dbRecheckX = dx;
                    data.m_dbRecheckY = dy;
                    data.m_dbRecheckU = du;

                    if (!bReCheck)
                    {
                        return;
                    }

                }
                #endregion

                //相机拍Band
                if (!SnapT3_1())
                {
                    ShowLog("T3_1拍照处理失败");

                    //更新数据
                    data.m_bResult = false;

                    return;
                }

                double dbOffsetX, dbOffsetY, dbOffsetZ, dbOffsetU;

                dbOffsetZ = DataMgr.GetInstance().m_dictDataGroup["Mesh机械手组装"].m_dictData["Z"].m_dbOffset.D;

                #region 调整角度并组装
                do
                {
                    //获取调整角度
                    GetAngleOffset(out dbOffsetU);

                    //机械手调整角度
                    ShowLog("机械手调整角度");

                    RobotCmd("Adjust", "AdjustOK", 6, speed.ToString("f3"), "0", "0", "0", dbOffsetU.ToString("f3"));

                    //第二次拍照开始组装
                    if (GetAssemOffset(out dbOffsetX, out dbOffsetY))
                    {
                        //机械手开始组装
                        ShowLog("机械手开始组装");
                        RobotCmd("Assem", "AssemOK", 6, speed.ToString("f3"),
                            dbOffsetX.ToString(), dbOffsetY.ToString(), dbOffsetZ.ToString());

                        break;
                    }
                    else
                    {
                        //抛料，重新取料组装
                        ShowLog("机械手抛料");
                        RobotCmd("Throw", "ThrowOK", 6, speed.ToString("f3"));

                        //通知Mesh取料站重新取料
                        ShowLog("机械手重新取料");
                        SetBit(SysBitReg.Mesh机械手组装完成, true);
                    }

                } while (true);
                #endregion

            }
            else
            {
                ShowLog("等待机械手取料完成");

                //等待机械手取料完成
                if (StationMgr.GetInstance().GetStation("Mesh上料站").StationEnable)
                {
                    WaitRegBit((int)SysBitReg.Mesh机械手取料完成, true, -1);
                }

                SetBit(SysBitReg.Mesh机械手取料完成, false);

                ShowLog("机械手盲装");
                RobotCmd("Assem", "AssemOK", 6, speed.ToString("f3"));
            }
            //通知Mesh组装完成
            ShowLog("机械手组装完成");
            SetBit(SysBitReg.Mesh机械手组装完成, true);

            ShowLog("本站完成", LogLevel.Info, "This station completed");
        }

        protected override void DryRun()
        {
            NormalRun();
        }

        protected override void AutoCalib()
        {
            ShowLog("按启动键开始自动标定");
            WaitIo("启动", true);

            string strCalib = SystemMgrEx.GetInstance().CurrentCalib;

            RunModeInfo info;
            if (SystemMgrEx.GetInstance().m_dictCalibs.TryGetValue(strCalib, out info))
            {
                MethodInfo method = GetType().GetMethod(info.m_strMethod);

                method.Invoke(this, null);
            }

            ShowLog("标定完成");
            ShowMessage("标定完成", true);
        }

        protected override void GrrRun()
        {

        }

        /// <summary>
        /// 拍Mesh
        /// </summary>
        /// <returns></returns>
        private bool SnapT4()
        {
            SetDO("Mesh对位光源", true);
            Thread.Sleep(100);

            bool bRet = VisionMgr.GetInstance().ProcessStep("T4");

            SetDO("Mesh对位光源", false);

            return bRet;
        }

        /// <summary>
        /// 拍Band
        /// </summary>
        /// <returns></returns>
        private bool SnapT3_1()
        {
            SetDO("Mesh组装上光源", true);
            SetDO("Mesh组装下光源", true);

            Thread.Sleep(100);

            bool bRet = VisionMgr.GetInstance().ProcessStep("T3_1");

            SetDO("Mesh组装上光源", false);
            SetDO("Mesh组装下光源", false);

            return bRet;
        }

        /// <summary>
        /// 复检拍Band
        /// </summary>
        /// <returns></returns>
        private bool SnapT3_2()
        {
            SetDO("Mesh组装下光源", true);

            Thread.Sleep(100);

            bool bRet = VisionMgr.GetInstance().ProcessStep("T3_2");

            SetDO("Mesh组装上光源", false);

            return bRet;
        }

        /// <summary>
        /// 复检拍Strobe
        /// </summary>
        /// <returns></returns>
        private bool SnapT3_3()
        {
            SetDO("Mesh组装上光源", true);
            SetDO("Mesh组装下光源", true);

            Thread.Sleep(100);

            bool bRet = VisionMgr.GetInstance().ProcessStep("T3_3");

            SetDO("Mesh组装上光源", false);
            SetDO("Mesh组装下光源", false);

            return bRet;
        }

        private bool ReCheck(out double dx, out double dy, out double du)
        {
            dx = dy = du = 0.0;
            DataGroup dataGroup = DataMgr.GetInstance().m_dictDataGroup["Strobe组装精度"];

            #region 复检第一次拍照
            if (!SnapT3_2())
            {
                ShowLog("T3_2拍照处理失败");

                return false;
            }
            #endregion

            #region 复检第二次拍照
            if (!SnapT3_3())
            {
                ShowLog("T3_3拍照处理失败");

                return false;
            }
            #endregion

            dx = SystemMgr.GetInstance().GetRegDouble((int)SysFloatReg.T3_3_X);
            dy = SystemMgr.GetInstance().GetRegDouble((int)SysFloatReg.T3_3_Y);
            du = SystemMgr.GetInstance().GetRegDouble((int)SysFloatReg.T3_3_A);

            #region 判断复检结果
            if (dx > dataGroup.m_dictData["X"].m_dbLimitU.D || dx < dataGroup.m_dictData["X"].m_dbLimitL.D
                || dy > dataGroup.m_dictData["Y"].m_dbLimitU.D || dy < dataGroup.m_dictData["Y"].m_dbLimitL.D
                || du > dataGroup.m_dictData["U"].m_dbLimitU.D || du < dataGroup.m_dictData["U"].m_dbLimitL.D
                )
            {

                if (!SystemMgr.GetInstance().GetParamBool("IgnoreRecheckEnable"))
                {
                    ShowLog("复检结果超出标准范围");

                    return false;
                }
            }
            #endregion

            return true;
        }

        /// <summary>
        /// 获取角度偏移
        /// </summary>
        /// <returns></returns>
        private bool GetAngleOffset(out double dbOffsetU)
        {
            dbOffsetU = 0.0;
            //角度调整过大，循环取料
            while (true)
            {
                CheckContinue(false);

                //机械手组装偏移数据和机械手组装标准值
                DataGroup groupDataAssem;
                groupDataAssem = DataMgr.GetInstance().m_dictDataGroup["Mesh机械手组装"];

                ShowLog("等待机械手取料完成");

                //等待机械手取料完成
                if (StationMgr.GetInstance().GetStation("Mesh上料站").StationEnable)
                {
                    WaitRegBit((int)SysBitReg.Mesh机械手取料完成, true, -1);
                }

                SetBit(SysBitReg.Mesh机械手取料完成, false);

                //此时已经在Mesh取料站拍过一次,直接取结果
                dbOffsetU = SystemMgr.GetInstance().GetRegDouble((int)SysFloatReg.T3_1_A)
                        - SystemMgr.GetInstance().GetRegDouble((int)SysFloatReg.T4_A) + groupDataAssem.m_dictData["U"].m_dbOffset.D;

                if (dbOffsetU <= groupDataAssem.m_dictData["U"].m_dbLimitU.D && dbOffsetU >= groupDataAssem.m_dictData["U"].m_dbLimitL.D)
                {
                    ShowLog("角度在可调范围内");

                    return true;
                }
                else
                {
                    ShowLog("角度调整超过范围");
                    //抛料
                    ShowLog("通知机械手抛料");
                    double speed = SystemMgr.GetInstance().GetParamDouble("MeshRobotSpeed") / 100;
                    RobotCmd("Throw", "ThrowOK", 6, speed.ToString("f3"));

                    //通知Mesh取料站重新取料
                    ShowLog("通知机械手重新取料");
                    SetBit(SysBitReg.Mesh机械手组装完成, true);
                }

            }
        }

        /// <summary>
        /// 获取组装位置偏移
        /// </summary>
        private bool GetAssemOffset(out double dbOffsetX, out double dbOffsetY)
        {
            dbOffsetX = 0;
            dbOffsetY = 0;
            //机械手的组装精度要求
            DataGroup groupDataStand;
            groupDataStand = DataMgr.GetInstance().m_dictDataGroup["Mesh组装精度"];

            //机械手的组装位置偏移
            DataGroup groupDataOffset = DataMgr.GetInstance().m_dictDataGroup["Mesh机械手组装"];

            //调整角度和偏移，开始组装
            int nAdjustTime = 3;
            while (nAdjustTime > 0)
            {
                //拍照判断角度是否在误差范围内
                if (SnapT4())
                {
                    double dbOffsetU = SystemMgr.GetInstance().GetRegDouble((int)SysFloatReg.T3_1_A)
                        - SystemMgr.GetInstance().GetRegDouble((int)SysFloatReg.T4_A);

                    //判断角度是否在标准范围内
                    if (dbOffsetU >= groupDataStand.m_dictData["U"].m_dbLimitL.D && dbOffsetU <= groupDataStand.m_dictData["U"].m_dbLimitU.D)
                    {
                        //角度在误差范围内
                        dbOffsetX = SystemMgr.GetInstance().GetRegDouble((int)SysFloatReg.T1_1_X)
                            - SystemMgr.GetInstance().GetRegDouble((int)SysFloatReg.T1_2_X)
                            + groupDataOffset.m_dictData["X"].m_dbOffset.D;

                        dbOffsetY = SystemMgr.GetInstance().GetRegDouble((int)SysFloatReg.T1_1_Y)
                            - SystemMgr.GetInstance().GetRegDouble((int)SysFloatReg.T1_2_Y)
                            + groupDataOffset.m_dictData["Y"].m_dbOffset.D;

                        //判断位置偏移是否超限
                        if (dbOffsetX > groupDataOffset.m_dictData["X"].m_dbLimitU.D
                            || dbOffsetX < groupDataOffset.m_dictData["X"].m_dbLimitL.D
                            || dbOffsetY > groupDataOffset.m_dictData["Y"].m_dbLimitU.D
                            || dbOffsetY < groupDataOffset.m_dictData["Y"].m_dbLimitL.D)
                        {
                            ShowLog("位置偏移超过范围");
                            return false;
                        }
                        else
                        {
                            ShowLog("位置偏移在可调范围内");
                            return true;
                        }

                    }
                    else
                    {
                        double speed = SystemMgr.GetInstance().GetParamDouble("MeshRobotSpeed") / 100;
                        //通知机械手重新调整角度
                        RobotCmd("Adjust", "AdjustOK", 6
                            , speed.ToString("f3"), "0", "0", "0", dbOffsetU.ToString("f3"));

                        nAdjustTime--;
                    }
                }

            }

            return false;
        }

        private void CalibT3()
        {
            SetDO("Mesh组装下光源", true);
            Thread.Sleep(100);
            ShowLog("T3相机标定开始");

            //通知机械手标定开始,采用11点标定
            RobotCmd("Calib", "CalibOK", 6, "11");

            ShowLog("机械手标定准备就绪，手动放标定板");
            WaitIo("启动", true);

            //VisionMgr.GetInstance().m_CalibTrans.InitData();
            VisionMgr.GetInstance().m_CalibTrans.ClearAllData();

            double pixelX, pixelY, robotX, robotY;
            //9点标定
            //11点标定最后三点必须是基于标定点旋转
            for (int i = 0; i < 11; i++)
            {
                //通知机械手到标定位
                string[] strSplits = RobotCmd("CalibT3", "CurPos", 6, i.ToString());

                if (VisionMgr.GetInstance().ProcessStep("CalibT3"))
                {
                    pixelX = SystemMgr.GetInstance().GetRegDouble((int)SysFloatReg.T3_X);
                    pixelY = SystemMgr.GetInstance().GetRegDouble((int)SysFloatReg.T3_Y);

                    robotX = Convert.ToDouble(strSplits[1]);
                    robotY = Convert.ToDouble(strSplits[2]);

                    VisionMgr.GetInstance().m_CalibTrans.AppendPointData(pixelX, pixelY, robotX, robotY);
                }
                else
                {
                    ShowLog("T3相机标定图像处理失败");
                    WarningMgr.GetInstance().Error(ErrorType.Err_Vision_Process, "CCD3", "T3相机标定图像处理失败");
                    return;
                }
            }
            SetDO("Mesh组装下光源", false);
            //标定文件
            string strDir = VisionMgr.GetInstance().ConfigDir + "CalibT3";
            if (!Directory.Exists(strDir))
            {
                Directory.CreateDirectory(strDir);
            }
            string strFileName = String.Format("{0}\\{1}.cal", strDir, "CalibT3");


            VisionMgr.GetInstance().m_CalibTrans.CalcCalib();
            VisionMgr.GetInstance().m_CalibTrans.SaveCaliData(strFileName);
            VisionMgr.GetInstance().LoadParam();

        }

        private void CalibT4()
        {
            SetDO("Mesh对位光源", true);
            Thread.Sleep(100);
            ShowLog("T4相机标定开始");

            //通知机械手标定开始,采用11点标定
            RobotCmd("Calib", "CalibOK", 6, "11");

            ShowLog("机械手标定准备就绪，手动放标定板");
            WaitIo("启动", true);

            //VisionMgr.GetInstance().m_CalibTrans.InitData();
            VisionMgr.GetInstance().m_CalibTrans.ClearAllData();

            double pixelX, pixelY, robotX, robotY;
            //9点标定
            //11点标定最后三点必须是基于标定点旋转
            for (int i = 0; i < 11; i++)
            {
                //通知机械手到标定位
                string[] strSplits = RobotCmd("CalibT4", "CurPos", 6, i.ToString());

                if (VisionMgr.GetInstance().ProcessStep("CalibT4"))
                {
                    pixelX = SystemMgr.GetInstance().GetRegDouble((int)SysFloatReg.T4_X);
                    pixelY = SystemMgr.GetInstance().GetRegDouble((int)SysFloatReg.T4_Y);

                    robotX = Convert.ToDouble(strSplits[1]);
                    robotY = Convert.ToDouble(strSplits[2]);

                    VisionMgr.GetInstance().m_CalibTrans.AppendPointData(pixelX, pixelY, robotX, robotY);

                }
                else
                {
                    ShowLog("T4相机标定图像处理失败");
                    WarningMgr.GetInstance().Error(ErrorType.Err_Vision_Process, "CCD4", "T4相机标定图像处理失败");
                    return;
                }
            }
            SetDO("Mesh对位光源", false);

            //标定文件
            string strDir = VisionMgr.GetInstance().ConfigDir + "CalibT4";
            if (!Directory.Exists(strDir))
            {
                Directory.CreateDirectory(strDir);
            }
            string strFileName = String.Format("{0}\\{1}.cal", strDir, "CalibT4");


            VisionMgr.GetInstance().m_CalibTrans.CalcCalib();
            VisionMgr.GetInstance().m_CalibTrans.SaveCaliData(strFileName);
            VisionMgr.GetInstance().LoadParam();
        }
    }
}
