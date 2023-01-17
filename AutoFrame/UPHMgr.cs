using CommonTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using AutoFrameDll;

namespace AutoFrame
{
    /// <summary>
    /// 格式:OKCount,NGCount,yyyyMMdd-HHmmss
    /// </summary>
    public class UPHInfo
    {
        /// <summary>
        /// OK计数
        /// </summary>
        public int okCount;
        /// <summary>
        /// NG计数
        /// </summary>
        public int ngCount;
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime dt;
        /// <summary>
        /// 格式
        /// </summary>
        public static readonly string format = "yyyyMMdd-HH:mm:ss";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="okCount"></param>
        /// <param name="ngCount"></param>
        /// <param name="dt"></param>
        public UPHInfo(int okCount, int ngCount, DateTime dt)
        {
            this.okCount = okCount;
            this.ngCount = ngCount;
            this.dt = dt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static UPHInfo GetUPHFromString(string s)
        {
            string[] ss = s.Split(',');
            try
            {
                if (ss.Length != 3)
                {
                    return new UPHInfo(0, 0, DateTime.Now);
                }
                int i = Convert.ToInt32(ss[0]);
                int j = Convert.ToInt32(ss[1]);
                DateTime tmp = DateTime.ParseExact(ss[2], format, System.Globalization.CultureInfo.InstalledUICulture);
                return new UPHInfo(i, j, tmp);
            }
            catch (Exception ex)
            {
                StationMgr.GetInstance().ShowLog($"加载时，UPH本地文件格式存储异常：{ex.ToString()}，时间点：{ss[2]}");
                return new UPHInfo(0, 0, DateTime.Now);
            }
        }

        /// <summary>
        /// 保存UPH文件
        /// </summary>
        /// <returns></returns>
        public bool SaveToFile()
        {
            string filename = SystemMgr.GetInstance().GetParamString("UPHDataSavePath") + @"\" + dt.ToString("yyyyMMdd") + ".csv";
            bool flag = File.Exists(filename);
            try
            {
                if (!new FileInfo(filename).Directory.Exists)
                {
                    new FileInfo(filename).Directory.Create();
                }
                using (FileStream fs = new FileStream(filename, FileMode.Append, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.Default))
                    {
                        if (!flag)
                        {
                            sw.WriteLine($"okCount,ngCount,DateTime");
                        }
                        sw.WriteLine($"{okCount},{ngCount},{dt.ToString(format)}");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
                //throw;
            }
        }
    }

    /// <summary>
    /// UPH管理类
    /// </summary>
    public class UPHMgr : SingletonTemplate<UPHMgr>
    {
        List<UPHInfo> ls1 = new List<UPHInfo>();//用来记录按制定时间段显示UPH的信息
        List<UPHInfo> ls2 = new List<UPHInfo>();//用来记录按制定时间段显示UPH的信息
        int[] ok_UPH = new int[24];//用来记录在界面按小时显示UPH的ok数量
        int[] ng_UPH = new int[24];//用来记录在界面按小时显示UPH的NG数量
        DateTime currentTime = DateTime.Now;

        /// <summary>
        /// UPH更新事件
        /// </summary>
        public event Action<int[], int[]> UPHChangeEvent;

        int _flag1 = 0;
        int _flag2 = 0;
        /// <summary>
        /// 加载UPH存储文件
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="isAutoForm"></param>
        public void LoadFromFile(DateTime dt, bool isAutoForm = false)
        {
            if (isAutoForm)
            {
                ok_UPH = new int[24];
                ng_UPH = new int[24];
            }
            int nHour = SystemMgr.GetInstance().GetParamInt("DayShiftTime");
            //选择的UPH日期文件
            #region 当天8:00-24:00
            string filename1 = SystemMgr.GetInstance().GetParamString("UPHDataSavePath") + @"\" + dt.ToString("yyyyMMdd") + ".csv";
            if (File.Exists(filename1))
            {
                using (FileStream fi1 = new FileStream(filename1, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    using (StreamReader sr1 = new StreamReader(fi1, Encoding.Default))
                    {
                        sr1.ReadLine();//读题头
                        while (!sr1.EndOfStream)
                        {
                            string str1 = sr1.ReadLine();
                            string[] tmpStr1 = str1.Split(',');
                            if (tmpStr1.Length == 3)
                            {
                                ls1.Add(UPHInfo.GetUPHFromString(str1));
                                int hours1 = DateTime.ParseExact(tmpStr1[2], UPHInfo.format, System.Globalization.CultureInfo.InstalledUICulture).Hour;
                                if (hours1 >= nHour && hours1 < 24)
                                {
                                    if (isAutoForm)
                                    {
                                        try
                                        {
                                            ok_UPH[hours1] += Convert.ToInt32(tmpStr1[0]);
                                            ng_UPH[hours1] += Convert.ToInt32(tmpStr1[1]);
                                        }
                                        catch (Exception ex)
                                        {
                                            ok_UPH[hours1] += 0;//异常计 0
                                            ng_UPH[hours1] += 0;
                                            StationMgr.GetInstance().ShowLog($"UPH本地文件格式存储异常：{ex.ToString()}，时间点：{tmpStr1[2]}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    UPHChangeEvent?.Invoke(ok_UPH, ng_UPH);
                }
            }
            else
            {
                if (_flag1 != 0)
                {
                    MessageBox.Show(string.Format("{0} 未生产", dt.ToString("yyyyMMdd")));
                }
                _flag1 = 1;
            }
            #endregion

            #region 隔天0:00-8:00
            string filename2 = SystemMgr.GetInstance().GetParamString("UPHDataSavePath") + @"\" + dt.AddDays(1).ToString("yyyyMMdd") + ".csv";
            if (File.Exists(filename2))
            {
                using (FileStream fi2 = new FileStream(filename2, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    using (StreamReader sr2 = new StreamReader(fi2, Encoding.Default))
                    {
                        sr2.ReadLine();//读题头
                        while (!sr2.EndOfStream)
                        {
                            string str2 = sr2.ReadLine();
                            string[] tmpStr2 = str2.Split(',');
                            if (tmpStr2.Length == 3)
                            {
                                ls2.Add(UPHInfo.GetUPHFromString(str2));
                                int hours2 = DateTime.ParseExact(tmpStr2[2], UPHInfo.format, System.Globalization.CultureInfo.InstalledUICulture).Hour;
                                if (hours2 >= 0 && hours2 < nHour)
                                {
                                    if (isAutoForm)
                                    {
                                        try
                                        {
                                            ok_UPH[hours2] += Convert.ToInt32(tmpStr2[0]);
                                            ng_UPH[hours2] += Convert.ToInt32(tmpStr2[1]);
                                        }
                                        catch (Exception ex)
                                        {
                                            ok_UPH[hours2] += 0;//异常计 0
                                            ng_UPH[hours2] += 0;
                                            StationMgr.GetInstance().ShowLog($"UPH本地文件格式存储异常：{ex.ToString()}，时间点：{tmpStr2[2]}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    UPHChangeEvent?.Invoke(ok_UPH, ng_UPH);
                }
            }
            else
            {
                if (_flag2 != 0)
                {
                    MessageBox.Show(string.Format("{0} 未生产", dt.AddDays(1).ToString("yyyyMMdd")));
                }
                _flag2 = 1;
            }
            #endregion
        }

        /// <summary>
        /// 选取查看UPH的时间段
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public bool SelectUPHFromRange(DateTime start, DateTime end, out string s)
        {
            ls1.Clear();
            s = "";
            if ((end - start).TotalSeconds <= 0)
            {
                return false;
            }
            LoadFromFile(start);
            if (end.Year != start.Year)
            {
                LoadFromFile(end);
            }
            int okCount = 0, ngCount = 0;
            foreach (var item in ls1)
            {
                if ((item.dt - start).TotalSeconds > 0 && (item.dt - end).TotalSeconds < 0)
                {
                    okCount += item.okCount;
                    ngCount += item.ngCount;
                }
            }
            s = $"{okCount},{ngCount}";
            ls1.Clear();
            return true;
        }

        int _Flag = 0;
        /// <summary>
        /// 更新上传UPH记录数据
        /// </summary>
        /// <param name="okCount"></param>
        /// <param name="ngCount"></param>
        public void UpdateUPH(int okCount = 1, int ngCount = 0)
        {
            int nHour = SystemMgr.GetInstance().GetParamInt("DayShiftTime");
            if (currentTime.Day != DateTime.Now.Day)//每天一更
            {
                _Flag = 0;
            }
            if (DateTime.Now.Hour == nHour && _Flag == 0)//每天8点更新一次UPH记录队列
            {
                ok_UPH = new int[24];
                ng_UPH = new int[24];
                _Flag = 1;
            }
            currentTime = DateTime.Now;
            new UPHInfo(okCount, ngCount, currentTime).SaveToFile();
            int hours = currentTime.Hour;
            ok_UPH[hours] += okCount;
            ng_UPH[hours] += ngCount;
            UPHChangeEvent?.Invoke(ok_UPH, ng_UPH);//触发OnUPHChangeEvent
        }
    }
}
