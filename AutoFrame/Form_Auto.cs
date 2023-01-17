//2019-04-30 Binggoo 1. 界面保存数据超过2000条时，自动清空数据，防止数据过多，界面显示不出来。
//2019-06-12 Binggoo 1. 加入是否保存ShowLog参数
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using AutoFrameDll;
using CommonTool;
using Communicate;
using System.IO;
using ToolEx;

namespace AutoFrame
{
    public partial class Form_Auto : Form
    {
        private List<UPH> m_listUPH = new List<UPH>();//一共两个，第一个为白班，第二个为夜班

        public struct UPH
        {
            public Dictionary<string, int> m_dicUPHTotal;//20221104 增加UPH柱状图
            public Dictionary<string, int> m_dicUPHOK;
            public Dictionary<string, int> m_dicUPHNG;
        }


        private int m_okCount = 0;
        private int m_ngCount = 0;

        private DateTime m_tmCTBegin;
        private TimeSpan m_tsBestCT;
        private TimeSpan m_tsSoftware;
        private TimeSpan m_tsMachine;

        public Form_Auto()
        {
            InitializeComponent();

            InitCtrlAnchor();

            OnDataShowChanged();

            m_tsBestCT = TimeSpan.Zero;
            m_tsSoftware = TimeSpan.Zero;
            m_tsMachine = TimeSpan.Zero;

        }

        private void OnLanguageChangeEvent(string strLanguage, bool bChange)
        {
            IniHelper ini = new IniHelper();

            ini.IniFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "language", strLanguage, this.GetType().Namespace + ".ini");

            if (bChange)
            {
                LanguageMgr.GetInstance().ChangeUIText(this.GetType().Name, this, ini);
            }
            else
            {
                LanguageMgr.GetInstance().SaveUIText(this.GetType().Name, this, ini);
            }
        }

        private void InitCtrlAnchor()
        {
            //Anchor属性最好在程序里修改，这样便于修改界面，否则界面上的控件会随着窗口大小变化而变化，对添加控件产生困扰
            groupBox_Info.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox_WorkMode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            groupBox_MachineState.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            groupBox_WorkOrder.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            groupBox_Stastic.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            //tableLayoutPanel_Log.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tabControl_Data.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        }

        private void OnDataShowChanged()
        {
            dataGridView_Result.Columns.Clear();
            dataGridView_Result.RowsDefaultCellStyle.BackColor = Color.Ivory;
            dataGridView_Result.AlternatingRowsDefaultCellStyle.BackColor = Color.PaleTurquoise;
            foreach (string text in DataMgr.GetInstance().m_dictDataShow.Keys)
            {
                int col = dataGridView_Result.Columns.Add(text, text);
                dataGridView_Result.Columns[col].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView_Result.Columns[col].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
        }

        private void OnDataSaveChanged()
        {
            /*
            if (DataMgr.GetInstance().SaveType == SaveType.DB)
            {
                //删除表
                SqlBase sql = new MySQL();

                if (!sql.Connect(DataMgr.GetInstance().Server, 
                    DataMgr.GetInstance().Port, 
                    DataMgr.GetInstance().UserID, 
                    DataMgr.GetInstance().Password,
                    DataMgr.GetInstance().Database))
                {
                    return;
                }

                if (sql.IsTableExist(DataMgr.GetInstance().TableName))
                {
                    sql.DropTable(DataMgr.GetInstance().TableName);
                }
            }
            */
        }

        private void Form_Auto_Load(object sender, EventArgs e)
        {
            #region UPH柱状图
            m_listUPH.Clear();
            UPH uphDay = new UPH();
            uphDay.m_dicUPHTotal = new Dictionary<string, int>();
            uphDay.m_dicUPHOK = new Dictionary<string, int>();
            uphDay.m_dicUPHNG = new Dictionary<string, int>();
            m_listUPH.Add(uphDay);

            UPH uphNight = new UPH();
            uphNight.m_dicUPHTotal = new Dictionary<string, int>();
            uphNight.m_dicUPHOK = new Dictionary<string, int>();
            uphNight.m_dicUPHNG = new Dictionary<string, int>();
            m_listUPH.Add(uphNight);
            #endregion

            dataGridView1.Rows.Add(13);//UPH界面
            UPHMgr.GetInstance().UPHChangeEvent += OnUPHChangeEvent;//UPH变化时的响应函数。
            UPHMgr.GetInstance().LoadFromFile(DateTime.Now, true);


            SystemMgr.GetInstance().BitChangedEvent += OnSystemBitChanged;  //委托中添加系统位寄存器响应函数操作 
            SystemMgr.GetInstance().IntChangedEvent += OnSystemIntChanged;  //委托中添加系统整型寄存器响应函数操作 
            SystemMgr.GetInstance().DoubleChangedEvent += OnSystemDoubleChanged;  //委托中添加系统浮点型寄存器响应函数操作 

            StationMgr.GetInstance().StateChangedEvent += OnStationStateChanged; //委托中添加站位状态变化响应函数操作
            StationMgr.GetInstance().StopRun();

            DataMgr.GetInstance().DataShowChangeEvent += OnDataShowChanged;
            DataMgr.GetInstance().DataSaveChangeEvent += OnDataSaveChanged;


            //关联站位对应的ListBox，站位中的ShowLog会显示在关联的ListBox中
            StationMgrEx.GetInstance().SetLogListBox(tableLayoutPanel_Log, OnLogView);

            if (SystemMgr.GetInstance().GetParamBool("AutoCycle"))
            {
                StationMgr.GetInstance().BAutoMode = true;  //设置半自动运行属性
                roundButton_auto.BaseColor = Color.FromArgb(0xb3, 0xca, 0xff);   //给自动按钮赋初始化颜色
                roundButton_manual.BaseColor = Color.FromArgb(220, 221, 224); //给手动操作按钮赋初始化颜色
            }
            else
            {
                StationMgr.GetInstance().BAutoMode = false;  //设置半自动运行属性
                roundButton_auto.BaseColor = Color.FromArgb(220, 221, 224);   //给自动按钮赋初始化颜色
                roundButton_manual.BaseColor = Color.FromArgb(0xb3, 0xca, 0xff); //给手动操作按钮赋初始化颜色
            }

            WarningMgr.GetInstance().WarningEventHandler += OnWarning;//添加自动界面报警信息响应函数委托
            OnWarning(this, EventArgs.Empty);  //清除自动界面报警信息

            //增加权限等级变更通知
            OnChangeMode();
            Security.ModeChangedEvent += OnChangeMode;

            IoMgr.GetInstance().IoChangedEvent += OnIoChanged;

            ProductMgr.GetInstance().SendProductDataEvent += OnSendProductDataEvent;


            OnLanguageChangeEvent(LanguageMgr.GetInstance().Language, true);

            LanguageMgr.GetInstance().LanguageChangeEvent += OnLanguageChangeEvent;
        }

        private void OnTcpStateChangedEvent(TcpLink tcp)
        {

        }

        private void OnPDCADataReceivedEvent(byte[] data, int length)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                textBox_Receive.Text += System.Text.Encoding.Default.GetString(data, 0, length);
            });
        }

        private void OnSendProductDataEvent(ProductData data)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                try
                {
                    #region 更新界面
                    if (data.m_bResult)
                    {
                        m_okCount++;
                        UPHMgr.GetInstance().UpdateUPH(1, 0);//更新UPH
                    }
                    else
                    {
                        m_ngCount++;
                        UPHMgr.GetInstance().UpdateUPH(0, 1);
                    }

                    //更新统计信息
                    label_ok_num.Text = m_okCount.ToString();
                    label_ng_num.Text = m_ngCount.ToString();

                    label_count.Text = (m_okCount + m_ngCount).ToString();
                    label_ok_percent.Text = (m_okCount * 100.0 / (m_okCount + m_ngCount)).ToString("f2");

                    if (dataGridView_Result.Rows.Count > 2000)
                    {
                        dataGridView_Result.Rows.Clear();
                    }

                    //更新表格
                    int nDataRow = dataGridView_Result.Rows.Add();

                    //滚动条跟随滚动
                    dataGridView_Result.CurrentCell = dataGridView_Result.Rows[nDataRow].Cells[0];

                    int nDataCol = 0;

                    //dataGridView_Result.Rows[nDataRow].Cells[nDataCol++].Value = data.m_strHSGCode;
                    //dataGridView_Result.Rows[nDataRow].Cells[nDataCol++].Value = data.m_strJigCode;
                    //dataGridView_Result.Rows[nDataRow].Cells[nDataCol++].Value = data.m_nGlueIndex.ToString() ;
                    //dataGridView_Result.Rows[nDataRow].Cells[nDataCol++].Value = data.m_dtBeginTime.ToString("yyyyMMdd HH:mm:ss");
                    //dataGridView_Result.Rows[nDataRow].Cells[nDataCol++].Value = data.m_dtEndTime.ToString("yyyyMMdd HH:mm:ss");
                    //dataGridView_Result.Rows[nDataRow].Cells[nDataCol++].Value = data.GlueTimeS.ToString("f3");
                    //dataGridView_Result.Rows[nDataRow].Cells[nDataCol++].Value = data.ExposureGlueTimeS.ToString("f3");
                    //dataGridView_Result.Rows[nDataRow].Cells[nDataCol++].Value = data.m_dbRecheckX.ToString("f3");
                    //dataGridView_Result.Rows[nDataRow].Cells[nDataCol++].Value = data.m_dbRecheckY.ToString("f3");
                    //dataGridView_Result.Rows[nDataRow].Cells[nDataCol++].Value = data.m_dbRecheckU.ToString("f2");
                    //dataGridView_Result.Rows[nDataRow].Cells[nDataCol++].Value = data.m_bResult ? "OK":"NG";
                    foreach (var item in DataMgr.GetInstance().m_dictDataShow)
                    {
                        Type t = data.GetType(item.Value);

                        if (t == typeof(DateTime))
                        {
                            dataGridView_Result.Rows[nDataRow].Cells[nDataCol++].Value
                            = ((DateTime)data.GetValue(item.Value)).ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        else if (t == typeof(double))
                        {
                            dataGridView_Result.Rows[nDataRow].Cells[nDataCol++].Value
                            = ((double)data.GetValue(item.Value)).ToString("f3");
                        }
                        else if (t == typeof(bool))
                        {
                            dataGridView_Result.Rows[nDataRow].Cells[nDataCol++].Value
                            = ((bool)data.GetValue(item.Value)) ? "OK" : "NG";
                        }
                        else
                        {
                            dataGridView_Result.Rows[nDataRow].Cells[nDataCol++].Value
                            = data.GetValue(item.Value).ToString();
                        }
                    }



                    #endregion

                    #region 保存显示文件
                    if (DataMgr.GetInstance().DataShowSaveEnable)
                    {
                        Task.Run(delegate
                        {
                            //记录文件
                            CsvOperation csv = new CsvOperation(ProductMgr.GetInstance().DeviceName);

                            int row = 0, col = 0;
                            if (!File.Exists(csv.FileName))
                            {
                                foreach (DataGridViewColumn head in dataGridView_Result.Columns)
                                {
                                    csv[0, col++] = head.HeaderText;
                                }

                                row = 1;
                            }

                            col = 0;
                            foreach (DataGridViewCell cell in dataGridView_Result.Rows[nDataRow].Cells)
                            {
                                if (cell.Value != null)
                                {
                                    csv[row, col] = cell.Value.ToString();
                                }

                                col++;

                            }

                            csv.Save();


                        });
                    }
                    #endregion

                    ProductMgr.GetInstance().SaveData(data);

                    /* 2020-2-10 保存文件和上传移到站位去坐，便于管控
                    #region 保存文件
                    if (DataMgr.GetInstance().DataSaveEnable && !string.IsNullOrEmpty(DataMgr.GetInstance().SavePath))
                    {
                        Task.Run(delegate
                        {
                            string strSavePath = DataMgr.GetInstance().SavePath;
                            string fileName = String.Format("{0}_{1}.csv", ProductMgr.GetInstance().DeviceName, DateTime.Now.ToString("yyyyMMdd"));

                            //记录文件
                            CsvOperation csv = new CsvOperation(strSavePath, fileName);

                            int row = 0, col = 0;
                            if (!File.Exists(csv.FileName))
                            {
                                foreach (var item in DataMgr.GetInstance().m_dictDataSave.Keys)
                                {
                                    csv[0, col++] = item;
                                }

                                row = 1;
                            }

                            col = 0;

                            foreach (var item in DataMgr.GetInstance().m_dictDataSave)
                            {
                                Type t = data.GetType(item.Value);
                                if (t == typeof(DateTime))
                                {
                                    csv[row, col++] = ((DateTime)data.GetValue(item.Value)).ToString("yyyyMMdd HH:mm:ss");
                                }
                                else if (t == typeof(double))
                                {
                                    csv[row, col++] = ((double)data.GetValue(item.Value)).ToString("f3");
                                }
                                else if (t == typeof(bool))
                                {
                                    csv[row, col++] = ((bool)data.GetValue(item.Value)) ? "OK" : "NG";
                                }
                                else
                                {
                                    csv[row, col++] = data.GetValue(item.Value).ToString();
                                }
                            }

                            csv.Save();
                        });
                    }

                    #endregion

                    #region 上传PDCA
                    DataGroup pdca;
                    if (DataMgr.GetInstance().m_dictDataGroup.TryGetValue("PDCA数据", out pdca)
                        && SystemMgr.GetInstance().GetParamBool("PDCAEnable"))
                    {
                        string strPDCA = ProductMgr.GetInstance().GetPDCAString(pdca, data.m_strBarCode, data);

                        //此处上传PDCA
                        if (m_tcpPDCA != null)
                        {
                            if (m_tcpPDCA.IsOpen())
                            {
                                Task.Run(delegate
                                {
                                    if (m_tcpPDCA.WriteString(strPDCA))
                                    {
                                        //此处采用异步读取结果，也可同步读取，但会卡时间

                                    }
                                });

                            }
                        }



                        strPDCA = strPDCA.Replace("\n", "\r\n");

                        textBox_Send.Text = strPDCA;
                    }
                    #endregion
                    */
                }
                catch (Exception ex)
                {
                    WarningMgr.GetInstance().Error(ex.Message);

                    MessageBox.Show(ex.ToString());
                }


            });
        }

        /// <summary>
        /// 权限变更响应
        /// </summary>
        private void OnChangeMode()
        {

        }

        /// <summary>
        /// 站位状态变化委托响应函数
        /// </summary>
        /// <param name="state">站位状态值</param>
        private void OnStationStateChanged(StationState OldState, StationState NewState)
        {
            switch (NewState)
            {
                case StationState.STATE_MANUAL:  //手动状态
                    label_sta_manual.ImageIndex = 1;
                    label_sta_pause.ImageIndex = 0;
                    label_sta_auto.ImageIndex = 0;
                    label_sta_ready.ImageIndex = 0;
                    label_sta_emg.ImageIndex = 0;

                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        label_stop_time.Text = DateTime.Now.ToString("HH:mm:ss");
                    });
                    break;
                case StationState.STATE_AUTO:   //自动运行状态
                    label_sta_auto.ImageIndex = 1;
                    label_sta_manual.ImageIndex = 0;
                    label_sta_pause.ImageIndex = 0;
                    label_sta_ready.ImageIndex = 0;
                    label_sta_emg.ImageIndex = 0;

                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        label_start_time.Text = DateTime.Now.ToString("HH:mm:ss");
                    });
                    break;
                case StationState.STATE_READY:  //等待开始
                    label_sta_ready.ImageIndex = 1;
                    break;
                case StationState.STATE_EMG:         //急停状态
                    label_sta_emg.ImageIndex = 2;
                    label_sta_pause.ImageIndex = 0;
                    label_sta_ready.ImageIndex = 0;

                    break;
                case StationState.STATE_PAUSE:       //暂停状态
                    label_sta_pause.ImageIndex = 1;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 响应IO变化事件
        /// </summary>
        /// <param name="nCard"></param>
        private void OnIoChanged(int nCard)
        {
            if (nCard == 1)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {

                });
            }


        }

        /// <summary>
        /// 报警信息委托调用函数
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        private void OnWarning(object Sender, EventArgs e)
        {
            if (WarningMgr.GetInstance().HasErrorMsg())
            {
                if (label_warning.BackColor == Color.FromKnownColor(KnownColor.Control))
                {
                    label_warning.BackColor = Color.Red;
                }
                if (label_warning.InvokeRequired)  //c#中禁止跨线程直接访问控件，InvokeRequired是为了解决这个问题而产生的,用一个异步执行委托
                {
                    Action<string> actionDelegate = (x) => { this.label_warning.Text = x.ToString(); };
                    // 或者
                    // Action<string> actionDelegate = delegate(string txt) { this.label2.Text = txt; };
                    this.label_warning.BeginInvoke(actionDelegate, WarningMgr.GetInstance().GetLastMsg().strMsg);

                }
                else
                {
                    label_warning.Text = WarningMgr.GetInstance().GetLastMsg().strMsg;
                }

            }
            else
            {
                label_warning.BackColor = Color.FromKnownColor(KnownColor.Control);
                if (label_warning.InvokeRequired) //c#中禁止跨线程直接访问控件，InvokeRequired是为了解决这个问题而产生的,用一个异步执行委托
                {
                    Action<string> actionDelegate = (x) => { this.label_warning.Text = x.ToString(); };
                    // 或者
                    // Action<string> actionDelegate = delegate(string txt) { this.label2.Text = txt; };
                    this.label_warning.BeginInvoke(actionDelegate, string.Empty);

                }
                else
                    label_warning.Text = string.Empty;
            }
        }

        /// <summary>
        /// 系统位寄存器变化委托响应函数
        /// </summary>
        /// <param name="nIndex"></param>
        /// <param name="bBit"></param>
        protected void OnSystemBitChanged(int nIndex, bool bBit)
        {
            SysBitReg sbr = (SysBitReg)nIndex;
            switch (sbr)
            {
            }
        }

        /// <summary>
        /// 进度条时间刷新
        /// </summary>
        /// <param name="nStep"></param>
        void ProcessStep(int nStep)
        {
            if (nStep <= 100 && nStep >= 0)
            {
                progressBar_all.Value = nStep;
                label_percent.Text = string.Format("{0}%", nStep);

                if (nStep == 0)
                {
                    label_current_CT.Text = "0";
                    m_tmCTBegin = DateTime.Now;
                }
                else if (nStep == 100)
                {
                    int number = 0;
                    if (Int32.TryParse(label_current_num.Text, out number))
                    {
                        label_current_num.Text = (++number).ToString();

                        int nTarNum;
                        if (Int32.TryParse(numericUpDown1.Text, out nTarNum))
                        {
                            if (number >= nTarNum && nTarNum != 0)//订单已完成
                            {
                                this.roundButton_manual.PerformClick();//切换为半自动
                            }
                        }
                    }

                    TimeSpan ts = DateTime.Now - m_tmCTBegin;
                    label_current_CT.Text = ts.TotalSeconds.ToString("f2");
                    label_prev_CT.Text = ts.TotalSeconds.ToString("f2");

                    if (ts < m_tsBestCT || m_tsBestCT.TotalSeconds < 0.01)
                    {
                        m_tsBestCT = ts;
                        label_best_CT.Text = m_tsBestCT.TotalSeconds.ToString("f2");
                    }
                    m_tmCTBegin = DateTime.MinValue;
                }
            }
        }

        //定义一个关联进度条时间刷新的委托
        public delegate void CrossDelegate(int nStep);

        /// <summary>
        /// 系统整型寄存器变化委托响应函数
        /// </summary>
        /// <param name="nIndex">寄存器索引</param>
        /// <param name="nData">寄存器值</param>
        protected void OnSystemIntChanged(int nIndex, int nData)
        {
            switch (nIndex)
            {
                case (int)SysIntReg.Int_Process_Step:
                    // ProcessStep(nData);
                    CrossDelegate da = new CrossDelegate(ProcessStep);
                    this.BeginInvoke(da, nData); // 异步调用委托,调用后立即返回并立即执行下面的语句
                    break;
            }
        }

        public delegate void CrossDelegateDouble(int nIndex);
        void ProcessDoubleChange(int nIndex)
        {

        }

        /// <summary>
        /// 系统浮点型寄存器变化委托响应函数
        /// </summary>
        /// <param name="nIndex">寄存器索引</param>
        /// <param name="fData">寄存器值</param>
        protected void OnSystemDoubleChanged(int nIndex, double fData)
        {
            CrossDelegateDouble dl = new CrossDelegateDouble(ProcessDoubleChange);
            this.BeginInvoke(dl, nIndex);
        }

        /// <summary>
        /// 清除界面数据记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_clean_Click(object sender, EventArgs e)
        {
            m_okCount = 0;
            m_ngCount = 0;
            label_ok_num.Text = string.Empty;
            label_ng_num.Text = string.Empty;
            label_ok_percent.Text = string.Empty;

            dataGridView_Result.Rows.Clear();

            label_time_soft_total.Text = string.Empty;
            label_current_CT.Text = string.Empty;
            label_best_CT.Text = string.Empty;

            label_current_num.Text = "0";
            //m_timeRunBegin = DateTime.Now;
            m_tsBestCT = TimeSpan.Zero;

            textBox_Send.Text = "";
            textBox_Receive.Text = "";

        }

        /// <summary>
        /// 清除界面最后一条报警信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_warning_clean_Click(object sender, EventArgs e)
        {
            if (WarningMgr.GetInstance().HasErrorMsg())
            {
                WarningMgr.GetInstance().ClearWarning(WarningMgr.GetInstance().Count - 1);
            }
            else
            {
                label_warning.Text = "";
            }
        }

        /// <summary>
        /// 计时,定时1000ms
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            string strTimeFormat = "{0:00} {1:00}:{2:00}:{3:00}";

            m_tsSoftware += new TimeSpan(0, 0, 1);
            label_time_soft_total.Text = string.Format(strTimeFormat, m_tsSoftware.Days,
                                    m_tsSoftware.Hours, m_tsSoftware.Minutes, m_tsSoftware.Seconds);
            if (StationMgr.GetInstance().IsAutoRunning())
            {
                m_tsMachine += new TimeSpan(0, 0, 1);
                label_time_machine_total.Text = string.Format(strTimeFormat, m_tsMachine.Days,
                                    m_tsMachine.Hours, m_tsMachine.Minutes, m_tsMachine.Seconds);

                if (m_tmCTBegin != DateTime.MinValue)
                {
                    TimeSpan ts = DateTime.Now - m_tmCTBegin;
                    label_current_CT.Text = ts.TotalSeconds.ToString("f2");
                }
            }
            if (WarningMgr.GetInstance().HasErrorMsg())
            {
                if (label_warning.BackColor == Color.Red)
                {
                    label_warning.BackColor = Color.FromKnownColor(KnownColor.Control);
                }
                else
                {
                    label_warning.BackColor = Color.Red;
                }
            }
            else if (IoMgr.GetInstance().IsSafeDoorOpen() && !SystemMgr.GetInstance().GetParamBool("SafetyDoor"))
            {
                string strMsg = "安全门被打开，存在安全隐患";
                if (LanguageMgr.GetInstance().LanguageID != 0)
                {
                    strMsg = "The door has been opened, and there is potential safety hazard.";
                }

                label_warning.Text = strMsg;
            }
            else
            {
                label_warning.Text = "";
            }

        }

        /// <summary>
        /// 双击报警框,打开界面报警信息 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void label_warning_DoubleClick(object sender, EventArgs e)
        {
            if (label_warning.Text != string.Empty)
            {
                Form_Warning fw = new Form_Warning();
                fw.ShowDialog(this);
            }
        }

        /// <summary>
        /// 自动循环模式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void roundButton_auto_Click(object sender, EventArgs e)
        {
            StationMgr.GetInstance().BAutoMode = true;
            roundButton_auto.BaseColor = Color.FromArgb(0xb3, 0xca, 0xff);
            roundButton_manual.BaseColor = Color.FromArgb(220, 221, 224);
        }

        /// <summary>
        /// 切换单步作业模式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void roundButton_manual_Click(object sender, EventArgs e)
        {
            StationMgr.GetInstance().BAutoMode = false;
            roundButton_auto.BaseColor = Color.FromArgb(220, 221, 224);
            roundButton_manual.BaseColor = Color.FromArgb(0xb3, 0xca, 0xff);
        }


        /// <summary>
        /// 在列表框中显示字符串
        /// </summary>
        /// <param name="strLog"></param>
        /// <param name="level"></param>
        public void OnLogView(Control ctrl, string strLog, LogLevel level = LogLevel.Info)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                try
                {
                    ListBoxEx logListBox = ctrl as ListBoxEx;

                    if (logListBox.Items.Count > 2000)
                    {
                        logListBox.Clear();
                    }

                    Color color = logListBox.BackColor;

                    switch (level)
                    {
                        case LogLevel.Info:
                            color = logListBox.BackColor;
                            break;

                        case LogLevel.Warn:
                            color = Color.Yellow;
                            break;

                        case LogLevel.Error:
                            color = Color.Red;
                            break;

                    }

                    logListBox.Append(strLog, color, logListBox.ForeColor);

                    logListBox.TopIndex = logListBox.Items.Count - (int)(logListBox.Height / logListBox.ItemHeight);

                    if (SystemMgr.GetInstance().GetParamBool("SaveShowLogEnable"))
                    {
                        WarningMgr.GetInstance().Info(strLog);
                    }
                }
                catch (Exception ex)
                {
                    WarningMgr.GetInstance().Error(ex.Message);

                    MessageBox.Show(ex.ToString());
                }

            });
        }


        private void Form_Auto_FormClosed(object sender, FormClosedEventArgs e)
        {
            ProductMgr.GetInstance().SoftwareTime += (int)m_tsSoftware.TotalSeconds;
            ProductMgr.GetInstance().MachineTime += (int)m_tsMachine.TotalSeconds;
        }

        private void button_Test_Click(object sender, EventArgs e)
        {
            ProductMgr.GetInstance().Test();
        }

        private void button_Clear_Click(object sender, EventArgs e)
        {
            textBox_Send.Text = "";
            textBox_Receive.Text = "";
        }

        private void textBox_Receive_TextChanged(object sender, EventArgs e)
        {
            if (textBox_Receive.Lines.Length > 1000)
            {
                textBox_Receive.Clear();
            }
        }

        private void textBox_Send_TextChanged(object sender, EventArgs e)
        {
            if (textBox_Send.Lines.Length > 1000)
            {
                textBox_Send.Clear();
            }
        }

        private void dateTimePicker_Date_ValueChanged(object sender, EventArgs e)
        {
            UPHMgr.GetInstance().LoadFromFile(dateTimePicker_Date.Value, true);
        }

        /// <summary>
        /// UPH更新事件
        /// </summary>
        /// <param name="ok"></param>
        /// <param name="ng"></param>
        private void OnUPHChangeEvent(int[] ok, int[] ng)
        {
            int _okDay = 0, _ngDay = 0;
            int _okNight = 0, _ngNight = 0;
            int nHour = SystemMgr.GetInstance().GetParamInt("DayShiftTime");//白班开始时间 例;白班开始时间为8:00，则夜班开始时间为20:00
            this.BeginInvoke(new Action(() =>
            {
                m_listUPH[0].m_dicUPHTotal.Clear();
                m_listUPH[0].m_dicUPHOK.Clear();
                m_listUPH[0].m_dicUPHNG.Clear();

                m_listUPH[1].m_dicUPHTotal.Clear();
                m_listUPH[1].m_dicUPHOK.Clear();
                m_listUPH[1].m_dicUPHNG.Clear();
                for (int i = 0; i < 24; i++)
                {
                    if (i >= nHour && i < nHour + 12)
                    {

                        #region 白班UPH
                        int nHourEnd = (i + 1) == 24 ? 0 : i + 1;//24点为0点
                        dataGridView1.Rows[i - nHour].Cells[0].Value = $"{i}:00-{nHourEnd}:00";//时间段8:00-20:00
                        dataGridView1.Rows[i - nHour].Cells[0].Style.BackColor = Color.LightCyan;
                        dataGridView1.Rows[i - nHour].Cells[1].Value = ok[i];//OK数量
                        dataGridView1.Rows[i - nHour].Cells[2].Value = ng[i];//NG数量
                        if (ok[i] == 0)
                            dataGridView1.Rows[i - nHour].Cells[3].Value = 0.00;
                        else
                            dataGridView1.Rows[i - nHour].Cells[3].Value = Math.Round((double)ok[i] * 100 / (ok[i] + ng[i]), 2);//良率

                        _okDay += ok[i];
                        _ngDay += ng[i];
                        #endregion


                        m_listUPH[0].m_dicUPHTotal.Add($"{i}:00-{nHourEnd}:00", ok[i] + ng[i]);
                        m_listUPH[0].m_dicUPHOK.Add($"{i}:00-{nHourEnd}:00", ok[i]);
                        m_listUPH[0].m_dicUPHNG.Add($"{i}:00-{nHourEnd}:00", ng[i]);
                    }
                    else
                    {
                        #region 夜班UPH
                        int nHourEnd = (i + 1) == 24 ? 0 : i + 1;//24点为0点
                        if (i >= 0 && i < nHour)
                        {
                            dataGridView1.Rows[i + (12 - nHour)].Cells[4].Value = $"{i}:00-{nHourEnd}:00";//时间段0:00-8:00
                            dataGridView1.Rows[i + (12 - nHour)].Cells[4].Style.BackColor = Color.Gainsboro;
                            dataGridView1.Rows[i + (12 - nHour)].Cells[5].Value = ok[i];//OK数量
                            dataGridView1.Rows[i + (12 - nHour)].Cells[6].Value = ng[i];//NG数量
                            if (ok[i] == 0)
                                dataGridView1.Rows[i + (12 - nHour)].Cells[7].Value = 0.00;
                            else
                                dataGridView1.Rows[i + (12 - nHour)].Cells[7].Value = Math.Round((double)ok[i] * 100 / (ok[i] + ng[i]), 2);//良率

                            _okNight += ok[i];
                            _ngNight += ng[i];
                            m_listUPH[1].m_dicUPHTotal.Add($"{i}:00-{nHourEnd}:00", ok[i] + ng[i]);
                            m_listUPH[1].m_dicUPHOK.Add($"{i}:00-{nHourEnd}:00", ok[i]);
                            m_listUPH[1].m_dicUPHNG.Add($"{i}:00-{nHourEnd}:00", ng[i]);
                        }
                        else if (i >= nHour + 12 && i < 24)
                        {
                            dataGridView1.Rows[i - (nHour + 12)].Cells[4].Value = $"{i}:00-{nHourEnd}:00";//时间段20:00-24:00
                            dataGridView1.Rows[i - (nHour + 12)].Cells[4].Style.BackColor = Color.Gainsboro;
                            dataGridView1.Rows[i - (nHour + 12)].Cells[5].Value = ok[i];//OK数量
                            dataGridView1.Rows[i - (nHour + 12)].Cells[6].Value = ng[i];//NG数量
                            if (ok[i] == 0)
                                dataGridView1.Rows[i - (nHour + 12)].Cells[7].Value = 0.00;
                            else
                                dataGridView1.Rows[i - (nHour + 12)].Cells[7].Value = Math.Round((double)ok[i] * 100 / (ok[i] + ng[i]), 2);//良率

                            _okNight += ok[i];
                            _ngNight += ng[i];
                            m_listUPH[1].m_dicUPHTotal.Add($"{i}:00-{nHourEnd}:00", ok[i] + ng[i]);
                            m_listUPH[1].m_dicUPHOK.Add($"{i}:00-{nHourEnd}:00", ok[i]);
                            m_listUPH[1].m_dicUPHNG.Add($"{i}:00-{nHourEnd}:00", ng[i]);
                        }
                        #endregion



                    }
                }
                //白夜班统计
                dataGridView1.Rows[12].Cells[0].Value = "总计";
                dataGridView1.Rows[12].Cells[0].Style.BackColor = Color.LightCyan;
                dataGridView1.Rows[12].Cells[1].Value = _okDay;//OK数量统计
                dataGridView1.Rows[12].Cells[2].Value = _ngDay;//NG数量统计
                dataGridView1.Rows[12].Cells[3].Value = _okDay == 0 ? 0.00 : Math.Round((double)_okDay * 100 / (_okDay + _ngDay), 2);//总良率
                dataGridView1.Rows[12].Cells[4].Value = "总计";
                dataGridView1.Rows[12].Cells[4].Style.BackColor = Color.Gainsboro;
                dataGridView1.Rows[12].Cells[5].Value = _okNight;//OK数量统计
                dataGridView1.Rows[12].Cells[6].Value = _ngNight;//NG数量统计
                dataGridView1.Rows[12].Cells[7].Value = _okNight == 0 ? 0.00 : Math.Round((double)_okNight * 100 / (_okNight + _ngNight), 2);//总良率

                int temp = DateTime.Now.Hour / 2;
                dataGridView1.CurrentCell = dataGridView1.Rows[temp].Cells[0];

                chart1.Series[0].Points.DataBindXY(m_listUPH[0].m_dicUPHTotal.Keys, m_listUPH[0].m_dicUPHTotal.Values);
                chart1.Series[1].Points.DataBindXY(m_listUPH[0].m_dicUPHOK.Keys, m_listUPH[0].m_dicUPHOK.Values);
                chart1.Series[2].Points.DataBindXY(m_listUPH[0].m_dicUPHNG.Keys, m_listUPH[0].m_dicUPHNG.Values);

                chart2.Series[0].Points.DataBindXY(m_listUPH[1].m_dicUPHTotal.Keys, m_listUPH[1].m_dicUPHTotal.Values);
                chart2.Series[1].Points.DataBindXY(m_listUPH[1].m_dicUPHOK.Keys, m_listUPH[1].m_dicUPHOK.Values);
                chart2.Series[2].Points.DataBindXY(m_listUPH[1].m_dicUPHNG.Keys, m_listUPH[1].m_dicUPHNG.Values);
            }));
            //throw new NotImplementedException();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UPHMgr.GetInstance().UpdateUPH(1, 0);
            UPHMgr.GetInstance().UpdateUPH(0, 1);
        }
    }
}
