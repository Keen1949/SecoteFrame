@echo off
echo *******************************
echo      开始复制文件
echo *******************************
pause
xcopy .\exe ..\..\..\实例\CSharp框架\AutoFrame\exe\ /e /f /y
xcopy .\AutoFrame ..\..\..\实例\CSharp框架\AutoFrame\AutoFrame\ /e /f /y
xcopy .\AutoFrameVision ..\..\..\实例\CSharp框架\AutoFrame\AutoFrameVision\ /e /f /y
xcopy .\Communicate ..\..\..\实例\CSharp框架\AutoFrame\Communicate\ /e /f /y
xcopy .\MotionIO ..\..\..\实例\CSharp框架\AutoFrame\MotionIO\ /e /f /y
xcopy .\Plc ..\..\..\实例\CSharp框架\AutoFrame\Plc\ /e /f /y
xcopy .\ToolEx ..\..\..\实例\CSharp框架\AutoFrame\ToolEx\ /e /f /y
xcopy .\packages ..\..\..\实例\CSharp框架\AutoFrame\packages\ /e /f /y
copy .\更新说明.txt ..\..\..\实例\CSharp框架\ /y
pause
