using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Communicate
{
    /// <summary>
    /// 共享方法，
    /// 该类中所有属性，均在使用方初始化，
    /// 
    /// 
    /// </summary>
    public static class ShareModule
    {
        /// <summary>
        /// 共享  AutoFrame的FormMian
        /// </summary>
        public static Form AutoFrame_FormMian { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public static Func<string, string[], uint, bool, DialogResult> Alarm { get; set; }

        //Error(ErrorType type, string strObject, string strMessage)
        /// <summary>
        /// Error(ErrorType type, string strObject, string strMessage)
        /// </summary>
        public static Func<int, string, string, bool> Error { get; set; }
        /// <summary>
        /// Form_Auto 总控信息显示
        /// </summary>
        public static Action<string> Notify { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public static Action<string, int> ShowLog { get; set; }
    }
}
