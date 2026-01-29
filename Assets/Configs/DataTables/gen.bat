set WORKSPACE=..\..\..
set LUBAN_DLL=%WORKSPACE%\Luban\Luban.dll
set CODE_DIR=%WORKSPACE%\Assets\Configs\OutputCode
set DATA_DIR=%WORKSPACE%\Assets\StreamingAssets\OutputData
set CONF_ROOT=.

dotnet %LUBAN_DLL% ^
    -t all ^
    -c cs-simple-json ^
    -d json ^
    --conf %CONF_ROOT%\luban.conf ^
    -x outputCodeDir=%CODE_DIR% ^
    -x outputDataDir=%DATA_DIR%

pause