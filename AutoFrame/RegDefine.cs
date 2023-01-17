using System;
namespace AutoFrame
{
    /// <summary>
    /// 
    /// </summary>
    public enum SysBitReg
    {
        xxx, //xxx位寄存器，可用汉字描述

        Strobe组装站初始化完成,
        Mesh上料站初始化完成,
        Mesh料盘站初始化完成,
        转盘站初始化完成,
        Strobe上料站初始化完成,
        Strobe撕料站初始化完成,
        Mesh组装站初始化完成,
        保压站初始化完成,

        转盘站通知Strobe站转动到位,
        转盘站通知Mesh站转动到位,
        转盘站通知扫码站转动到位,

        撕料站准备就绪,
        Mesh料盘准备就绪,
        Strobe上料站准备就绪,

        Strobe上料站放料完成,
        撕料平台夹料完成,
        Strobe机械手取料完成,
        Strobe机械手组装完成,
        Mesh机械手全部取完料,
        Mesh机械手取料完成,
        Mesh机械手组装完成,

        ABB机械手上下料完成,

        最终结果,
    }

    /// <summary>
    /// 系统整型寄存器索引枚举声明
    /// </summary>
    public enum SysIntReg 
    {
        /// <summary>
        /// 站位运行进度百分比，用于界面显示进度条      
        /// </summary>
        Int_Process_Step,

        Int_Mesh_Index, //取Mesh的位置
    };

    /// <summary>
    /// 浮点型整型寄存器索引枚举声明
    /// </summary>
    public enum SysDoubleReg
    {
        Test_1,
        Test_2,
        Test_3,
        Test_4,
    };

    /// <summary>
    /// 系统字符串寄存器索引枚举声明
    /// </summary>
    public enum SysStrReg
    {
        Str_BarCode,
    };
}