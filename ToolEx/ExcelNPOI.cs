using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToolEx
{
    /// <summary>
    /// 利用NPOI库操作Excel，无需安装Excel组件
    /// </summary>
    public class ExcelNPOI
    {

        private ISheet m_sheet = null;
        private IWorkbook m_workbook = null;

        /// <summary>
        /// Excel文件名称
        /// </summary>
        private string m_strFilename;

        /// <summary>
        /// 颜色索引枚举
        /// </summary>
        public enum ColorIndex
        {
            /// <summary>
            /// 
            /// </summary>
            Black = 8,
            /// <summary>
            /// 
            /// </summary>
            Brown = 60,
            /// <summary>
            /// 
            /// </summary>
            Olive_Green = 59,
            /// <summary>
            /// 
            /// </summary>
            Dark_Green = 58,
            /// <summary>
            /// 
            /// </summary>
            Dark_Teal = 56,
            /// <summary>
            /// 
            /// </summary>
            Dark_Blue = 18,
            /// <summary>
            /// 
            /// </summary>
            Indigo = 62,
            /// <summary>
            /// 
            /// </summary>
            Grey_80_PERCENT = 63,
            /// <summary>
            /// 
            /// </summary>
            Dark_Red = 16,
            /// <summary>
            /// 
            /// </summary>
            Orange = 53,
            /// <summary>
            /// 
            /// </summary>
            DARK_YELLOW = 19,
            /// <summary>
            /// 
            /// </summary>
            Green = 17,
            /// <summary>
            /// 
            /// </summary>
            Teal = 21,
            /// <summary>
            /// 
            /// </summary>
            Blue = 12,
            /// <summary>
            /// 
            /// </summary>
            Blue_Grey = 54,
            /// <summary>
            /// 
            /// </summary>
            Grey_50_PERCENT = 23,
            /// <summary>
            /// 
            /// </summary>
            Red = 10,
            /// <summary>
            /// 
            /// </summary>
            LIGHT_ORANGE = 52,
            /// <summary>
            /// 
            /// </summary>
            LIME = 50,
            /// <summary>
            /// 
            /// </summary>
            SEA_GREEN = 57,
            /// <summary>
            /// 
            /// </summary>
            AQUA = 49,
            /// <summary>
            /// 
            /// </summary>
            LIGHT_BLUE = 48,
            /// <summary>
            /// 
            /// </summary>
            VIOLET = 20,
            /// <summary>
            /// 
            /// </summary>
            GREY_40_PERCENT = 55,
            /// <summary>
            /// 
            /// </summary>
            Pink = 14,
            /// <summary>
            /// 
            /// </summary>
            Gold = 51,
            /// <summary>
            /// 
            /// </summary>
            Yellow = 13,
            /// <summary>
            /// 
            /// </summary>
            BRIGHT_GREEN = 11,
            /// <summary>
            /// 
            /// </summary>
            TURQUOISE = 15,
            /// <summary>
            /// 
            /// </summary>
            SKY_BLUE = 40,
            /// <summary>
            /// 
            /// </summary>
            Plum = 61,
            /// <summary>
            /// 
            /// </summary>
            GREY_25_PERCENT = 22,
            /// <summary>
            /// 
            /// </summary>
            Rose = 45,
            /// <summary>
            /// 
            /// </summary>
            Tan = 47,
            /// <summary>
            /// 
            /// </summary>
            LIGHT_YELLOW = 43,
            /// <summary>
            /// 
            /// </summary>
            LIGHT_GREEN = 42,
            /// <summary>
            /// 
            /// </summary>
            LIGHT_TURQUOISE = 41,
            /// <summary>
            /// 
            /// </summary>
            PALE_BLUE = 44,
            /// <summary>
            /// 
            /// </summary>
            LAVENDER = 46,
            /// <summary>
            /// 
            /// </summary>
            White = 9,
            /// <summary>
            /// 
            /// </summary>
            CORNFLOWER_BLUE = 24,
            /// <summary>
            /// 
            /// </summary>
            LEMON_CHIFFON = 26,
            /// <summary>
            /// 
            /// </summary>
            MAROON = 25,
            /// <summary>
            /// 
            /// </summary>
            ORCHID = 28,
            /// <summary>
            /// 
            /// </summary>
            CORAL = 29,
            /// <summary>
            /// 
            /// </summary>
            ROYAL_BLUE = 30,
            /// <summary>
            /// 
            /// </summary>
            LIGHT_CORNFLOWER_BLUE = 31,
            /// <summary>
            /// 
            /// </summary>
            AUTOMATIC = 64,
        }

        /// <summary>
        /// RGB灰度
        /// </summary>
        public struct RGB
        {
            /// <summary>
            /// 红色灰度
            /// </summary>
            public byte R;
            /// <summary>
            /// 绿色灰度
            /// </summary>
            public byte G;
            /// <summary>
            /// 蓝色灰度
            /// </summary>
            public byte B;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="r"></param>
            /// <param name="g"></param>
            /// <param name="b"></param>
            public RGB(byte r,byte g,byte b)
            {
                R = r;
                G = g;
                B = b;
            }
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        public ExcelNPOI()
        {

        }

        /// <summary>
        /// 是否打开
        /// </summary>
        public bool IsOpened
        {
            get
            {
                return m_workbook != null;
            }
        }

        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="strFileName"></param>
        /// <param name="bAppend"></param>
        public bool Open(string strFileName,bool bAppend = false)
        {
            try
            {
                m_strFilename = strFileName;
                //判断文件是否存在
                if (File.Exists(strFileName) && bAppend)
                {
                    using (FileStream fs = new FileStream(strFileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        if (strFileName.IndexOf(".xlsx") > 0) // 2007版本
                        {
                            m_workbook = new XSSFWorkbook(fs);
                        }
                        else if (strFileName.IndexOf(".xls") > 0) // 2003版本
                        {
                            m_workbook = new HSSFWorkbook(fs);
                        }
                    }
                }
                else
                {
                    if (strFileName.IndexOf(".xlsx") > 0) // 2007版本
                    {
                        m_workbook = new XSSFWorkbook();
                    }
                    else if (strFileName.IndexOf(".xls") > 0) // 2003版本
                    {
                        m_workbook = new HSSFWorkbook();
                    }
                }

                return m_workbook != null;
            }
            catch (Exception ex)
            {
                m_workbook = null;
                MessageBox.Show(ex.Message);
            }

            return false;
        }

        /// <summary>
        /// 获取工作表
        /// </summary>
        /// <param name="strSheetName"></param>
        /// <returns></returns>
        public ISheet GetSheet(string strSheetName)
        {
            if (m_workbook != null)
            {
                return m_workbook.GetSheet(strSheetName);
            }

            return null;
        }

        /// <summary>
        /// 添加一个工作表
        /// </summary>
        /// <param name="strSheetName"></param>
        /// <returns></returns>
        public ISheet AddSheet(string strSheetName)
        {
            if (m_workbook != null)
            {
                ISheet sheet = m_workbook.GetSheet(strSheetName);

                if (sheet == null)
                {
                    sheet = m_workbook.CreateSheet(strSheetName);
                }

                return sheet;
            }
            return null;
        }

        /// <summary>
        /// 删除一个工作表
        /// </summary>
        /// <param name="strSheetName"></param>
        public void DelSheet(string strSheetName)
        {
            if (m_workbook != null)
            {
                int index = m_workbook.GetSheetIndex(strSheetName);

                if (index >= 0)
                {
                    m_workbook.RemoveSheetAt(index);
                }
            }
        }

        /// <summary>
        /// 重命名一个工作表
        /// </summary>
        /// <param name="strOldSheetName"></param>
        /// <param name="strNewSheetName"></param>
        /// <returns></returns>
        public ISheet ReNameSheet(string strOldSheetName, string strNewSheetName)
        {
            if (m_workbook != null)
            {
                int index = m_workbook.GetSheetIndex(strOldSheetName);

                if (index >= 0)
                {
                    m_workbook.SetSheetName(index, strNewSheetName);

                    return m_workbook.GetSheetAt(index);
                }
                
            }

            return null;
        }


        public static object GetCellValue(ICell cell)
        {
            if (cell == null)
            {
                return "";
            }

            object o = "";

            switch (cell.CellType)
            {
                case CellType.Numeric:
                    {
                        if (DateUtil.IsCellDateFormatted(cell))
                        {
                            o = cell.DateCellValue;
                        }
                        else
                        {
                            o = cell.NumericCellValue;
                        }
                    }
                    break;

                case CellType.String:
                    o = cell.StringCellValue;
                    break;
                case CellType.Boolean:
                    o = cell.BooleanCellValue;
                    break;
                default:
                    o = cell.ToString();
                    break;
            }

            return o;
        }

        /// <summary>
        /// 获取单元格值
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public static object GetCellValue(ISheet sheet,int row,int col)
        {
            IRow Row = sheet.GetRow(row);
            if (Row != null)
            {
                ICell cell = Row.GetCell(col);
                if (cell != null)
                {
                    return GetCellValue(cell);
                }
            }

            return null;
        }

        /// <summary>
        /// 获取单元格值
        /// </summary>
        /// <param name="strSheetName"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        public object GetCellValue(string strSheetName, int row, int col)
        {
            ISheet sheet = GetSheet(strSheetName);
            if (sheet != null)
            {
                return GetCellValue(sheet, row, col);
            }

            return null;
        }

        /// <summary>
        /// 设置单元格的值
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="value"></param>
        public static void SetCellValue(ISheet sheet, int row, int col, object value)
        {
            IRow Row = sheet.GetRow(row);
            if (Row == null)
            {
                Row = sheet.CreateRow(row);
            }
            if (Row != null)
            {
                ICell cell = Row.GetCell(col);
                if (cell == null)
                {
                    cell = Row.CreateCell(col);
                }
                //cell.SetCellValue(value.ToString());
                SetCellValue(cell, value);
            }
        }

        /// <summary>
        /// 设置单元格值
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="value"></param>
        public static void SetCellValue(ICell cell, object value)
        {
            Type type = value.GetType();

            if (type == typeof(double)
                || type == typeof(float)
                || type == typeof(int)
                || type == typeof(long)
                || type == typeof(short)
                || type == typeof(uint)
                || type == typeof(ulong)
                || type == typeof(ushort)
                )
            {
                cell.SetCellValue(Convert.ToDouble(value));
            }
            else if (type == typeof(DateTime))
            {
                cell.SetCellValue(Convert.ToDateTime(value));
            }
            else if (type == typeof(bool))
            {
                cell.SetCellValue(Convert.ToBoolean(value));
            }
            else
            {
                cell.SetCellValue(value.ToString());
            }

        }

        /// <summary>
        /// ws：要设值的工作表的名称 X行Y列 value 值
        /// </summary>
        /// <param name="strSheetName"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="value"></param>
        public void SetCellValue(string strSheetName, int row, int col, object value)
        {
            ISheet sheet = GetSheet(strSheetName);
            if (sheet != null)
            {
                SetCellValue(sheet, row, col, value);
            }
        }

        /// <summary>
        /// 设置背景颜色
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="color"></param>
        public static void SetCellBackColor(ISheet sheet, int row, int col, NPOI.HSSF.Util.HSSFColor color)
        {
            IRow Row = sheet.GetRow(row);
            if (Row == null)
            {
                Row = sheet.CreateRow(row);
            }

            if (Row != null && sheet.Workbook != null)
            {
                ICellStyle style = sheet.Workbook.CreateCellStyle();
                style.FillForegroundColor = (short)color.Indexed;
                style.FillPattern = FillPattern.SolidForeground;

                ICell cell = Row.GetCell(col);
                if (cell == null)
                {
                    cell = Row.CreateCell(col);
                }
                cell.CellStyle = style;
            }
        }

        /// <summary>
        /// 设置背景颜色
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="color"></param>
        public static void SetCellBackColor(ISheet sheet, int row, int col, ColorIndex color)
        {
            IRow Row = sheet.GetRow(row);
            if (Row == null)
            {
                Row = sheet.CreateRow(row);
            }

            if (Row != null && sheet.Workbook != null)
            {
                ICellStyle style = sheet.Workbook.CreateCellStyle();
                style.FillForegroundColor = (short)color;
                style.FillPattern = FillPattern.SolidForeground;

                ICell cell = Row.GetCell(col);
                if (cell == null)
                {
                    cell = Row.CreateCell(col);
                }
                cell.CellStyle = style;
            }
        }

        /// <summary>
        /// 设置背景颜色
        /// </summary>
        /// <param name="strSheetName"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="color"></param>

        public void SetCellBackColor(string strSheetName, int row, int col, NPOI.HSSF.Util.HSSFColor color)
        {
            ISheet sheet = GetSheet(strSheetName);
            if (sheet != null)
            {
                SetCellBackColor(sheet, row, col, color);
            }
        }

        /// <summary>
        /// 设置背景颜色
        /// </summary>
        /// <param name="strSheetName"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="color"></param>

        public void SetCellBackColor(string strSheetName, int row, int col, ColorIndex color)
        {
            ISheet sheet = GetSheet(strSheetName);
            if (sheet != null)
            {
                SetCellBackColor(sheet, row, col, color);
            }
        }

        /// <summary>
        /// 将内存中数据表格插入到Microsoft.Office.Interop.Excel指定工作表的指定位置二
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="sheet"></param>
        /// <param name="startRow"></param>
        /// <param name="startCol"></param>
        public static void InsertTable(DataTable dt, ISheet sheet, int startRow, int startCol)
        {
            if (sheet == null)
            {
                return;
            }

            sheet.ShiftRows(startRow, startRow + 1, dt.Rows.Count);

            for (int i = 0; i <= dt.Rows.Count - 1; i++)
            {
                IRow Row = sheet.GetRow(startRow + i);
                if (Row == null)
                {
                    Row = sheet.CreateRow(startRow + i);
                }

                for (int j = 0; j <= dt.Columns.Count - 1; j++)
                {
                    ICell cell = Row.GetCell(startCol + j);
                    if (cell == null)
                    {
                        cell = Row.CreateCell(startCol + j);
                    }
                    //cell.SetCellValue(dt.Rows[i][j].ToString());
                    SetCellValue(cell, dt.Rows[i][j]);
                }
            }

        }

        /// <summary>
        /// 将内存中数据表格插入到l指定工作表的指定位置 为在使用模板时控制格式时使用一
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="strSheetName"></param>
        /// <param name="startRow"></param>
        /// <param name="startCol"></param>
        public void InsertTable(DataTable dt, string strSheetName, int startRow, int startCol)
        {
            ISheet sheet = GetSheet(strSheetName);

            InsertTable(dt, sheet, startRow, startCol);
        }

        /// <summary>
        /// 将内存中数据表格添加到Microsoft.Office.Interop.Excel指定工作表的指定位置二
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="sheet"></param>
        /// <param name="startRow"></param>
        /// <param name="startCol"></param>
        public static void AddTable(DataTable dt, ISheet sheet, int startRow, int startCol)
        {

            if (sheet == null)
            {
                return;
            }

            for (int i = 0; i <= dt.Rows.Count - 1; i++)
            {
                IRow Row = sheet.GetRow(startRow + i);
                if (Row == null)
                {
                    Row = sheet.CreateRow(startRow + i);
                }

                for (int j = 0; j <= dt.Columns.Count - 1; j++)
                {
                    ICell cell = Row.GetCell(startCol + j);
                    if (cell == null)
                    {
                        cell = Row.GetCell(startCol + j);
                    }
                    //cell.SetCellValue(dt.Rows[i][j].ToString());
                    SetCellValue(cell, dt.Rows[i][j]);
                }
            }

        }


        /// <summary>
        /// 将内存中数据表格添加到Microsoft.Office.Interop.Excel指定工作表的指定位置一
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="strSheetName"></param>
        /// <param name="startRow"></param>
        /// <param name="startCol"></param>
        public void AddTable(DataTable dt, string strSheetName, int startRow, int startCol)
        {
            ISheet sheet = GetSheet(strSheetName);

            AddTable(dt, sheet, startRow, startCol);
        }

        /// <summary>
        /// 保存文档
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            if (m_strFilename == "")
            {
                return false;
            }
            else
            {
                try
                {
                    using (FileStream fs = File.OpenWrite(m_strFilename))
                    {
                        m_workbook.Write(fs); //写入到excel
                        m_workbook.Close();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        #region 静态方法
        /// <summary>
        /// 将DataTable数据导入到excel中
        /// </summary>
        /// <param name="data">要导入的数据</param>
        /// <param name="fileName">写入的目标Excel的完整名称</param>
        /// <param name="isColumnWritten">DataTable的列名是否要导入</param>
        /// <param name="sheetName">要导入的excel的sheet的名称</param>
        /// <param name="bAppend">是否追加</param>
        /// <returns>导入数据行数(包含列名那一行)</returns>
        public static int DataTableToExcel(DataTable data, string fileName, string sheetName, bool isColumnWritten,bool bAppend)
        {
            int count = 0;
            ISheet sheet = null;
            IWorkbook workbook = null;

            try
            {
                //判断文件是否存在
                FileMode mode = FileMode.OpenOrCreate;
                FileAccess access = FileAccess.ReadWrite;
                if (!File.Exists(fileName))
                {
                    bAppend = false;
                }
                else if (bAppend)
                {
                    isColumnWritten = false;
                }

                if (fileName.IndexOf(".xlsx") > 0) // 2007版本
                {
                    if (!bAppend)
                    {
                        workbook = new XSSFWorkbook();
                    }
                    else
                    {
                        using (FileStream fs = new FileStream(fileName, mode, access))
                            workbook = new XSSFWorkbook(fs);
                    }

                }
                else if (fileName.IndexOf(".xls") > 0) // 2003版本
                {
                    if (!bAppend)
                    {
                        workbook = new HSSFWorkbook();
                    }
                    else
                    {
                        using (FileStream fs = new FileStream(fileName, mode, access))
                            workbook = new HSSFWorkbook(fs);
                    }
                }

                //using (FileStream fs = new FileStream(fileName, mode, access))
                {

                    if (workbook != null)
                    {
                        sheet = workbook.GetSheet(sheetName);
                        if (sheet == null)
                        {
                            sheet = workbook.CreateSheet(sheetName);
                        }                 
                    }
                    else
                    {
                        return -1;
                    }

                    if (isColumnWritten == true) //写入DataTable的列名
                    {
                        IRow row = sheet.CreateRow(0);
                        for (int iCol = 0; iCol < data.Columns.Count; ++iCol)
                        {
                            row.CreateCell(iCol).SetCellValue(data.Columns[iCol].ColumnName);
                        }
                        count = 1;
                    }
                    else
                    {
                        count = 0;
                        if (bAppend)
                        {
                            count = sheet.LastRowNum;
                        }
                        
                    }

                    for (int iRow = 0; iRow < data.Rows.Count; ++iRow)
                    {
                        IRow row = sheet.GetRow(count);
                        if (row == null)
                        {
                            row = sheet.CreateRow(count);
                        }
                        for (int iCol = 0; iCol < data.Columns.Count; ++iCol)
                        {
                            ICell cell = row.GetCell(iCol);
                            if (cell == null)
                            {
                                row.CreateCell(iCol);
                            }
                            //cell.SetCellValue(data.Rows[iRow][iCol].ToString());
                            SetCellValue(cell, data.Rows[iRow][iCol]);
                        }
                        ++count;
                    }

                    using (FileStream fs = File.OpenWrite(fileName))
                    {
                        workbook.Write(fs); //写入到excel
                        workbook.Close();
                    }

                    return count;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
                return -1;
            }

        }

        /// <summary>
        /// 从某行某列开始写数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fileName"></param>
        /// <param name="sheetName"></param>
        /// <param name="startRow"></param>
        /// <param name="startCol"></param>
        /// <returns></returns>
        public static int DataTableToExcel(DataTable data, string fileName, string sheetName, int startRow,int startCol)
        {
            int count = 0;
            ISheet sheet = null;
            IWorkbook workbook = null;

            try
            {
                //判断文件是否存在
                FileMode mode = FileMode.OpenOrCreate;
                FileAccess access = FileAccess.ReadWrite;
                bool bExist = File.Exists(fileName);

                if (fileName.IndexOf(".xlsx") > 0) // 2007版本
                {
                    if (!bExist)
                    {
                        workbook = new XSSFWorkbook();
                    }
                    else
                    {
                        using (FileStream fs = new FileStream(fileName, mode, access))
                            workbook = new XSSFWorkbook(fs);
                    }

                }
                else if (fileName.IndexOf(".xls") > 0) // 2003版本
                {
                    if (!bExist)
                    {
                        workbook = new HSSFWorkbook();
                    }
                    else
                    {
                        using (FileStream fs = new FileStream(fileName, mode, access))
                            workbook = new HSSFWorkbook(fs);
                    }
                }

                //using (FileStream fs = new FileStream(fileName, mode, access))
                {

                    if (workbook != null)
                    {
                        sheet = workbook.GetSheet(sheetName);
                        if (sheet == null)
                        {
                            sheet = workbook.CreateSheet(sheetName);
                        }
                    }
                    else
                    {
                        return -1;
                    }

                    if (!bExist) //写入DataTable的列名
                    {
                        IRow row = sheet.CreateRow(0);
                        for (int iCol = 0; iCol < data.Columns.Count; ++iCol)
                        {
                            row.CreateCell(iCol + startCol).SetCellValue(data.Columns[iCol].ColumnName);
                        }
                        count = 1;
                    }
                    else
                    {
                        count = 0;

                    }

                    ////如果不足，用空行补
                    //for(int iRow = Math.Max(count,sheet.LastRowNum);iRow < startRow;iRow++)
                    //{
                    //    IRow row = sheet.CreateRow(iRow);
                    //}

                    for (int iRow = 0; iRow < data.Rows.Count; ++iRow)
                    {
                        IRow row = sheet.GetRow(startRow + iRow);
                        if (row == null)
                        {
                            row = sheet.CreateRow(startRow + iRow);
                        }
                        for (int iCol = 0; iCol < data.Columns.Count; ++iCol)
                        {
                            ICell cell = row.GetCell(iCol + startCol);
                            if (cell == null)
                            {
                                cell = row.CreateCell(iCol + startCol);
                            }

                            object o = data.Rows[iRow][iCol];
                            //cell.SetCellValue(data.Rows[iRow][iCol].ToString());
                            SetCellValue(cell, o);
                        }
                        ++count;
                    }

                    using (FileStream fs = File.OpenWrite(fileName))
                    {
                        workbook.Write(fs); //写入到excel
                        workbook.Close();
                    }

                    return count;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
                return -1;
            }

        }


        /// <summary>
        /// 将excel中的数据导入到DataTable中
        /// </summary>
        /// <param name="fileName">读取的Excel的完整名称</param>
        /// <param name="sheetName">excel工作薄sheet的名称</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名</param>
        /// <returns>返回的DataTable</returns>
        public static DataTable ExcelToDataTable(string fileName, string sheetName, bool isFirstRowColumn)
        {
            ISheet sheet = null;
            DataTable data = new DataTable();
            int startRow = 0;
            try
            {
                IWorkbook workbook = null;
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    if (fileName.IndexOf(".xlsx") > 0) // 2007版本
                        workbook = new XSSFWorkbook(fs);
                    else if (fileName.IndexOf(".xls") > 0) // 2003版本
                        workbook = new HSSFWorkbook(fs);

                    if (sheetName != null)
                    {
                        sheet = workbook.GetSheet(sheetName);
                        if (sheet == null) //如果没有找到指定的sheetName对应的sheet，则尝试获取第一个sheet
                        {
                            sheet = workbook.GetSheetAt(0);
                        }
                    }
                    else
                    {
                        sheet = workbook.GetSheetAt(0);
                    }
                    if (sheet != null)
                    {
                        IRow firstRow = sheet.GetRow(0);
                        int cellCount = firstRow.LastCellNum; //一行最后一个cell的编号 即总的列数

                        if (isFirstRowColumn)
                        {
                            for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                            {
                                ICell cell = firstRow.GetCell(i);
                                if (cell != null)
                                {
                                    string cellValue = cell.StringCellValue;
                                    if (cellValue != null)
                                    {
                                        DataColumn column = new DataColumn(cellValue);
                                        data.Columns.Add(column);
                                    }
                                }
                            }
                            startRow = sheet.FirstRowNum + 1;
                        }
                        else
                        {
                            startRow = sheet.FirstRowNum;
                        }

                        //最后一列的标号
                        int rowCount = sheet.LastRowNum;
                        for (int i = startRow; i <= rowCount; ++i)
                        {
                            IRow row = sheet.GetRow(i);
                            if (row == null) continue; //没有数据的行默认是null　　　　　　　

                            DataRow dataRow = data.NewRow();
                            for (int j = row.FirstCellNum; j < cellCount; ++j)
                            {
                                if (row.GetCell(j) != null) //同理，没有数据的单元格都默认是null
                                    dataRow[j] = row.GetCell(j).ToString();
                            }
                            data.Rows.Add(dataRow);
                        }
                    }

                    return data;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 把Excel的stream导入到表
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="fileName"></param>
        /// <param name="sheetName"></param>
        /// <param name="isFirstRowColumn"></param>
        /// <returns></returns>
        public static DataTable ExcelToDataTable(Stream fs, string fileName, string sheetName, bool isFirstRowColumn)
        {
            ISheet sheet = null;
            DataTable data = new DataTable();
            int startRow = 0;
            try
            {
                IWorkbook workbook = null;
                if (fileName.IndexOf(".xlsx") > 0) // 2007版本
                    workbook = new XSSFWorkbook(fs);
                else if (fileName.IndexOf(".xls") > 0) // 2003版本
                    workbook = new HSSFWorkbook(fs);

                if (sheetName != null)
                {
                    sheet = workbook.GetSheet(sheetName);
                    if (sheet == null) //如果没有找到指定的sheetName对应的sheet，则尝试获取第一个sheet
                    {
                        sheet = workbook.GetSheetAt(0);
                    }
                }
                else
                {
                    sheet = workbook.GetSheetAt(0);
                }
                if (sheet != null)
                {
                    IRow firstRow = sheet.GetRow(0);
                    int cellCount = firstRow.LastCellNum; //一行最后一个cell的编号 即总的列数

                    if (isFirstRowColumn)
                    {
                        for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                        {
                            ICell cell = firstRow.GetCell(i);
                            if (cell != null)
                            {
                                string cellValue = cell.StringCellValue;
                                if (cellValue != null)
                                {
                                    DataColumn column = new DataColumn(cellValue);
                                    data.Columns.Add(column);
                                }
                            }
                        }
                        startRow = sheet.FirstRowNum + 1;
                    }
                    else
                    {
                        startRow = sheet.FirstRowNum;
                    }

                    //最后一列的标号
                    int rowCount = sheet.LastRowNum;
                    for (int i = startRow; i <= rowCount; ++i)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue; //没有数据的行默认是null　　　　　　　

                        DataRow dataRow = data.NewRow();
                        for (int j = row.FirstCellNum; j < cellCount; ++j)
                        {
                            if (row.GetCell(j) != null) //同理，没有数据的单元格都默认是null
                                dataRow[j] = row.GetCell(j).ToString();
                        }
                        data.Rows.Add(dataRow);
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 把DataGridView转换成DataTable
        /// </summary>
        /// <param name="dgv"></param>
        /// <returns></returns>
        public static DataTable DataGridViewToDataTable(DataGridView dgv)
        {
            DataTable dt = new DataTable();

            // 列强制转换
            for (int count = 0; count < dgv.Columns.Count; count++)
            {
                DataColumn dc = new DataColumn(dgv.Columns[count].Name.ToString());
                dt.Columns.Add(dc);
            }

            // 循环行
            for (int count = 0; count < dgv.Rows.Count; count++)
            {
                DataRow dr = dt.NewRow();
                for (int countsub = 0; countsub < dgv.Columns.Count; countsub++)
                {
                    dr[countsub] = Convert.ToString(dgv.Rows[count].Cells[countsub].Value);
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        public static void Test(DataTable dt)
        {
            ExcelNPOI excel = new ExcelNPOI();

            excel.Open(@"D:\123.xlsx",true);

            excel.AddSheet("Data");

            excel.InsertTable(dt, "Data", 1, 1);

            excel.SetCellBackColor("Data", 10, 10, ExcelNPOI.ColorIndex.Blue);
            excel.SetCellValue("Data", 10, 10, "111");

            

            excel.Save();
        }

        #endregion
    }
}
