
using AutoFrameDll;
using CommonTool;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace AutoFrame
{
    public partial class Form_MsgTest : Form
    {

        /// <summary>
        /// 
        /// </summary>
        StationBase m_station = null;
        private int m_nTimeRemain = 20;

        private string[] m_strArrBindIo = new string[]
        {
            "复位",
            "",
            ""
        };

        private bool m_bBindIoPress = false;

        private DateTime m_dtBindIoPressTime = DateTime.Now;

        private int m_nPressIndex = -1;

        /// <summary>
        /// 是否保存log
        /// </summary>
        private bool IfSaveTips = true;
        /// <summary>
        /// 弹窗开始时间
        /// </summary>
        private DateTime BeginTime = DateTime.Now;
        /// <summary>
        /// Tips内容
        /// </summary>
        private string[] Tips;
        /// <summary>
        /// Tips 字符串形式
        /// </summary>
        private string s_Tips = null;
        private static readonly object lock_log = new object();
        /// <summary>
        /// 是否包含Tips
        /// </summary>
        private bool b_PopWindowsTips = false;
        /// <summary>
        /// 是否正常停止
        /// </summary>
        private bool b_NormalStop = false;
        /// <summary>
        /// 弹窗标题
        /// </summary>
        private string s_Title = "";

        /// <summary>
        /// 确认键绑定的IO
        /// </summary>
        public string[] BindIO
        {
            get
            {
                return this.m_strArrBindIo;
            }
            set
            {
                this.m_strArrBindIo = value;
            }
        }

        /// <summary>
        /// 通知模式
        /// </summary>
        public bool NotifyMode
        {
            get;
            set;
        }

        /// <summary>
        /// 绑定IO保持时间,单位秒
        /// </summary>
        public int IoKeepTimeS
        {
            get;
            set;
        }

        System.Timers.Timer timer1 = new System.Timers.Timer();

        /// <summary>
        /// 线程专用消息对话框
        /// </summary>
        public Form_MsgTest(StationBase sb)
        {
            this.InitializeComponent();
            m_station = sb;
            this.m_nTimeRemain = SingletonTemplate<SystemMgr>.GetInstance().GetParamInt("MessageTimeOut");
            this.IoKeepTimeS = 3;

            timer1.Elapsed += new System.Timers.ElapsedEventHandler(timer1_Tick);
            timer1.Interval = 1000;
            timer1.Enabled = true;
            timer1.Start();

            SingletonTemplate<IoMgr>.GetInstance().IoChangedEvent += new IoMgr.IoChangedHandler(this.OnIoChangedEvent);
        }

        private void OnIoChangedEvent(int nCard)
        {
            long num = 0L;
            bool flag = this.m_strArrBindIo == null;
            if (!flag)
            {
                int j;
                int i;
                for (i = 0; i < this.m_strArrBindIo.Length; i = j + 1)
                {
                    bool flag2 = string.IsNullOrEmpty(this.m_strArrBindIo[i]) || (this.m_bBindIoPress && this.m_nPressIndex != i);
                    if (!flag2)
                    {
                        bool flag3 = SingletonTemplate<IoMgr>.GetInstance().m_dicIn.TryGetValue(this.m_strArrBindIo[i], out num);
                        if (flag3)
                        {
                            bool flag4 = (long)nCard == (num >> 8) - 1L;
                            if (flag4)
                            {
                                bool ioInState = SingletonTemplate<IoMgr>.GetInstance().GetIoInState((int)(num >> 8), (int)(num & 255L));
                                bool flag5 = !this.m_bBindIoPress & ioInState;
                                if (flag5)
                                {
                                    this.m_bBindIoPress = true;
                                    this.m_dtBindIoPressTime = DateTime.Now;
                                    this.m_nPressIndex = i;
                                }
                                bool flag6 = this.m_bBindIoPress && !ioInState && (DateTime.Now - this.m_dtBindIoPressTime).TotalSeconds > (double)this.IoKeepTimeS;
                                if (flag6)
                                {
                                    base.BeginInvoke(new MethodInvoker(delegate
                                    {
                                        switch (i)
                                        {
                                            case 0:
                                                {
                                                    bool visible = this.button_yes.Visible;
                                                    if (visible)
                                                    {
                                                        this.button_yes_Click(null, null);
                                                    }
                                                    break;
                                                }
                                            case 1:
                                                {
                                                    bool visible2 = this.button_no.Visible;
                                                    if (visible2)
                                                    {
                                                        this.button_no_Click(null, null);
                                                    }
                                                    break;
                                                }
                                            case 2:
                                                {
                                                    bool visible3 = this.button_cancel.Visible;
                                                    if (visible3)
                                                    {
                                                        this.button_cancel_Click(null, null);
                                                    }
                                                    break;
                                                }
                                        }
                                    }));
                                }
                                bool flag7 = !ioInState;
                                if (flag7)
                                {
                                    this.m_bBindIoPress = false;
                                }
                            }
                        }
                    }
                    j = i;
                }
            }
        }

        /// <summary>
        /// 显示弹窗(卡流程)
        /// </summary>
        /// <param name="strText">弹窗内容</param>
        /// <param name="b_Tips">是否显示解决方法</param>
        /// <param name="Tips">解决方法</param>
        /// <param name="Title">弹窗标题</param>
        /// <param name="btns">弹窗按钮显示</param>
        /// <param name="bSaveLog">是否保存Log</param>
        /// <returns></returns>
        public DialogResult MessageShow(string strText, string Title, MessageBoxButtons btns, string[] Tips, bool b_Tips = true, bool bSaveLog = true)
        {
            this.BeginTime = DateTime.Now;
            s_Title = this.Text = Title;
            this.richTextBox_warn.Text = "    " + strText;//首行缩进
            bool flag = btns == MessageBoxButtons.OKCancel;
            this.b_PopWindowsTips = b_Tips;
            this.Tips = Tips;
            this.IfSaveTips = bSaveLog;
            if (flag)
            {
                this.button_no.Visible = false;
                this.button_no.Enabled = false;
                this.button_yes.Location = this.button_yes.Location;
                this.button_cancel.Location = this.button_cancel.Location;
                this.button_no.Location = new Point(0, 0);
                this.button_yes.Focus();
            }
            else
            {
                bool flag2 = btns == MessageBoxButtons.OK;
                if (flag2)
                {
                    this.button_cancel.Visible = false;
                    this.button_cancel.Enabled = false;
                    this.button_no.Visible = false;
                    this.button_no.Enabled = false;
                    this.button_yes.Location = this.button_cancel.Location;
                    this.button_no.Location = new Point(0, 0);
                    this.button_cancel.Location = new Point(0, 0);
                    this.button_yes.Focus();
                }
            }


            if (Tips != null)
            {
                foreach (var item in Tips)
                {
                    s_Tips += item + ",";
                    checkedListBox_Tips.Items.Add(item);
                }

                s_Tips = s_Tips.TrimEnd(',');
            }
            else
                s_Tips = "解决方法未定义";

            if (!b_Tips)
            {
                groupBox_Tips.Enabled = false;
                groupBox_Tips.Visible = false;
            }


            //this.timer1.Enabled = true;
            return base.ShowDialog();
        }

        /// <summary>
        /// 显示弹窗(不卡流程)
        /// </summary>
        /// <param name="strText">弹窗内容</param>
        /// <param name="b_Tips">是否显示解决方法</param>
        /// <param name="Tips">解决方法</param>
        /// <param name="Title">弹窗标题</param>
        /// <param name="btns">弹窗按钮显示</param>
        /// <param name="bSaveLog">是否保存Log</param>
        /// <returns></returns>
        public void MessageShowNB(string strText, string Title, MessageBoxButtons btns, string[] Tips, bool b_Tips = true, bool bSaveLog = true)
        {
            Invoke(new Action(() =>
            {
                this.BeginTime = DateTime.Now;
                s_Title = this.Text = Title;
                this.richTextBox_warn.Text = "    " + strText;//首行缩进
                bool flag = btns == MessageBoxButtons.OKCancel;
                this.b_PopWindowsTips = b_Tips;
                this.Tips = Tips;
                this.timer1.Enabled = true;
                //Program.fm.CommonShow(this);

                this.IfSaveTips = bSaveLog;
                if (flag)
                {
                    this.button_no.Visible = false;
                    this.button_no.Enabled = false;
                    this.button_yes.Location = this.button_yes.Location;
                    this.button_cancel.Location = this.button_cancel.Location;
                    this.button_no.Location = new Point(0, 0);
                    this.button_yes.Focus();
                }
                else
                {
                    bool flag2 = btns == MessageBoxButtons.OK;
                    if (flag2)
                    {
                        this.button_cancel.Visible = false;
                        this.button_cancel.Enabled = false;
                        this.button_no.Visible = false;
                        this.button_no.Enabled = false;
                        this.button_yes.Location = this.button_cancel.Location;
                        this.button_no.Location = new Point(0, 0);
                        this.button_cancel.Location = new Point(0, 0);
                        this.button_yes.Focus();
                    }
                }


                if (Tips != null)
                {
                    foreach (var item in Tips)
                    {
                        s_Tips += item + ",";
                        checkedListBox_Tips.Items.Add(item);
                    }

                    s_Tips = s_Tips.TrimEnd(',');
                }
                else
                    s_Tips = "解决方法未定义";

                if (!b_Tips)
                {
                    groupBox_Tips.Enabled = false;
                    groupBox_Tips.Visible = false;
                }
                //this.timer1.Enabled = true;
                //base.Show();
            }));

        }

        /// <summary>
        /// 设置YES按钮的文本显示
        /// </summary>
        /// <param name="strText"></param>
        public void SetYesText(string strText)
        {
            bool invokeRequired = base.InvokeRequired;
            if (invokeRequired)
            {
                base.BeginInvoke(new MethodInvoker(delegate
                {
                    this.button_yes.Text = strText;
                }));
            }
            else
            {
                this.button_yes.Text = strText;
            }
        }

        /// <summary>
        /// 设置NO按钮的文本显示
        /// </summary>
        /// <param name="strText"></param>
        public void SetNoText(string strText)
        {
            bool invokeRequired = base.InvokeRequired;
            if (invokeRequired)
            {
                base.BeginInvoke(new MethodInvoker(delegate
                {
                    this.button_no.Text = strText;
                }));
            }
            else
            {
                this.button_no.Text = strText;
            }
        }

        /// <summary>
        /// 设置Cancel按钮的文本显示
        /// </summary>
        /// <param name="strText"></param>
        public void SetCancelText(string strText)
        {
            bool invokeRequired = base.InvokeRequired;
            if (invokeRequired)
            {
                base.BeginInvoke(new MethodInvoker(delegate
                {
                    this.button_cancel.Text = strText;
                }));
            }
            else
            {
                this.button_cancel.Text = strText;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (m_station != null)
            {
                try
                {

                    if (m_station.CurState == StationState.STATE_EMG || m_station.CurState == StationState.STATE_MANUAL)
                    {
                        throw new Exception("机台停止//手动状态");
                    }
                    //this.m_station.CheckContinue(false, false);
                    Invoke(new Action(() =>
                    {
                        Text = s_Title + "  " + Convert.ToInt32((DateTime.Now - BeginTime).TotalSeconds).ToString() + "s";
                    }));

                    // this.m_station.CheckContinue(false, false);

                    this.timer1.Enabled = true;
                }
                catch (Exception ex)
                {
                    Invoke(new Action(() =>
                    {
                        base.DialogResult = DialogResult.No;
                        this.timer1.Enabled = false;
                        base.Close();
                    }));
                }
            }
        }

        private void Form_Message_Load(object sender, EventArgs e)
        {
            base.Activate();
            base.TopMost = true;
            bool flag = SingletonTemplate<LanguageMgr>.GetInstance().LanguageID != 0;
            if (flag)
            {
                this.button_yes.Text = "OK";
                this.button_no.Text = "Abort";
                this.button_cancel.Text = "Ignore";
                this.groupBox_Warn.Text = "Warn";
            }
            bool flag2 = !this.NotifyMode;
            if (flag2)
            {
                this.button_no.Visible = false;
                this.OnModeChangedEvent();
                Security.ModeChangedEvent += new Security.ModeChangedHandler(this.OnModeChangedEvent);
            }
        }

        private void OnModeChangedEvent()
        {
            bool invokeRequired = base.InvokeRequired;
            if (invokeRequired)
            {
                base.BeginInvoke(new MethodInvoker(delegate
                {
                    this.OnModeChangedEvent();
                }));
            }
            else
            {
                UserMode userMode = Security.GetUserMode();
                if (userMode != UserMode.Engineer && userMode != UserMode.Administrator)
                {
                    this.button_cancel.Visible = false;
                }
                else
                {
                    this.button_cancel.Visible = true;
                }
            }
        }

        private int GetMessageTimeOut()
        {
            int paramInt = SingletonTemplate<SystemMgr>.GetInstance().GetParamInt("MessageTimeOut");
            bool flag = paramInt == 0;
            int result;
            if (flag)
            {
                result = 20;
            }
            else
            {
                result = paramInt;
            }
            return result;
        }

        private void Form_Message_KeyDown(object sender, KeyEventArgs e)
        {
            bool flag = e.KeyCode == Keys.Space;
            if (flag)
            {
                base.DialogResult = DialogResult.Yes;
                this.button_yes.PerformClick();
            }
            else
            {
                bool flag2 = e.KeyCode == Keys.Escape;
                if (flag2)
                {
                }
            }
        }

        private void button_yes_Click(object sender, EventArgs e)
        {
            b_NormalStop = true;
            string TipsChecked = "";
            foreach (string item in checkedListBox_Tips.CheckedItems)
            {
                TipsChecked += item + ",";
            }

            TimeSpan ts = DateTime.Now - BeginTime;

            if (IfSaveTips)
            {
                //报警内容    this.label_warn_Text.Text
                //所有Tips    Tips
                //选中Tip     TipsChecked
                //弹窗时间    ts

                SaveTipsLog($"[{DateTime.Now.ToString("HH:mm:ss")}]" + "  " + $"ErrorInformation:{this.richTextBox_warn.Text}");
                if (b_PopWindowsTips)
                {
                    SaveTipsLog($"[{DateTime.Now.ToString("HH:mm:ss")}]" + "  " + $"Tips:{s_Tips}");
                }
                string temp = TipsChecked == "" ? "Not choice Method" : TipsChecked;
                temp = b_PopWindowsTips ? temp : "Not has Tips";
                SaveTipsLog($"[{DateTime.Now.ToString("HH:mm:ss")}]" + "  " + $"SolveMethods:{temp.TrimEnd(',')}");
                SaveTipsLog($"[{DateTime.Now.ToString("HH:mm:ss")}]" + "  " + $"SpendTimes:{ts.TotalSeconds}s");
                SaveTipsLog($"*************************************************************\r\n");
            }

            base.DialogResult = DialogResult.Yes;
            m_station = null;
            this.timer1.Enabled = false;
            base.Close();
        }

        private void button_no_Click(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.No;
            m_station = null;
            this.timer1.Enabled = false;
            base.Close();
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            b_NormalStop = true;
            TimeSpan ts = DateTime.Now - BeginTime;
            SaveTipsLog($"[{DateTime.Now.ToString("HH:mm:ss")}]" + "  " + $"ErrorInformation:{this.richTextBox_warn.Text}");
            if (b_PopWindowsTips)
            {
                SaveTipsLog($"[{DateTime.Now.ToString("HH:mm:ss")}]" + "  " + $"Tips:{s_Tips}");
            }
            SaveTipsLog($"[{DateTime.Now.ToString("HH:mm:ss")}]" + "  " + $"SolveMethods:Click cancle button");
            SaveTipsLog($"[{DateTime.Now.ToString("HH:mm:ss")}]" + "  " + $"SpendTimes:{ts.TotalSeconds}s");
            SaveTipsLog($"*************************************************************\r\n");

            base.DialogResult = DialogResult.Cancel;
            m_station = null;
            this.timer1.Enabled = false;
            base.Close();
        }

        private void Form_Message_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.timer1.Enabled = false;
            this.Dispose(true);
        }
        public bool SaveTipsLog(string Message)
        {
            string path = @"d:\exe\Log";

            lock (lock_log)
            {
                try
                {
                    string strSavePath = path + "\\" + "MessageBoxLog";
                    string strFileSavePath = "";
                    DateTime TimeNow = DateTime.Now;

                    if (!Directory.Exists(strSavePath))
                    {
                        Directory.CreateDirectory(strSavePath);
                    }
                    strFileSavePath = strSavePath + "\\" + TimeNow.ToString("yyyy-MM-dd") + ".txt";
                    if (!File.Exists(strFileSavePath))
                    {
                        FileStream fs1 = new FileStream(strFileSavePath, FileMode.Create, FileAccess.ReadWrite);
                        fs1.Close();
                    }
                    try
                    {
                        File.AppendAllText(strFileSavePath, Message + Environment.NewLine);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        private void Form_MsgTest_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.timer1.Enabled = false;
            if (this.DialogResult == DialogResult.None)
            {
                this.DialogResult = DialogResult.Cancel;
            }

            if (!b_NormalStop)
            {
                TimeSpan ts = DateTime.Now - BeginTime;
                SaveTipsLog($"[{DateTime.Now.ToString("HH:mm:ss")}]" + "  " + $"ErrorInformation:{this.richTextBox_warn.Text}");
                if (b_PopWindowsTips)
                {
                    SaveTipsLog($"[{DateTime.Now.ToString("HH:mm:ss")}]" + "  " + $"Tips:{s_Tips}");
                }
                SaveTipsLog($"[{DateTime.Now.ToString("HH:mm:ss")}]" + "  " + $"SolveMethods:Click close button");
                SaveTipsLog($"[{DateTime.Now.ToString("HH:mm:ss")}]" + "  " + $"SpendTimes:{ts.TotalSeconds}s");
                SaveTipsLog($"*************************************************************\r\n");
            }
        }
    }
}
