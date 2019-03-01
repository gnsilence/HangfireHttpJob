@echo.服务启动......  
@echo off  
@echo 当前路径 %~dp0\JobsServer.exe
@sc create HangApiService binPath= "%~dp0\JobsServer.exe"  
@net start HangApiService   
@sc config HangApiService  start= AUTO  
@echo off  
@echo.服务启动完毕！  
@pause