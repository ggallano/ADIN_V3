call "%VS140COMNTOOLS%VsMSBuildCmd.bat"

set Telerik_Source=%OneDrive%\git\Telerik_UI_for_WPF_Source_2018_3_1016
set Telerik_Configuration=Release45
set Telerik_Output=%Telerik_Source%\Binaries\WPF45\Dev
set Telerik_Target=Build

del Telerik.Windows.Controls.*
del "%Telerik_Output%\Telerik.Windows.Controls.*"
del Telerik.Windows.Data.*
del "%Telerik_Output%\Telerik.Windows.Data.*"

msbuild.exe "%Telerik_Source%\Core\XCore_WPF.sln" /p:Configuration="%Telerik_Configuration%" /target:%Telerik_Target% 
if %ERRORLEVEL% == 1 (
    echo ErrorLevel is %ERRORLEVEL%
    echo Problem!
    pause
)

set control=DataVisualization
msbuild.exe "%Telerik_Source%\Controls\%control%\%control%_WPF.sln" /p:Configuration="%Telerik_Configuration%" /target:%Telerik_Target%
if %ERRORLEVEL% == 1 (
    echo ErrorLevel is %ERRORLEVEL%
    echo Problem!
    pause
)

set control=Navigation
msbuild.exe "%Telerik_Source%\Controls\%control%\%control%_WPF.sln" /p:Configuration="%Telerik_Configuration%" /target:%Telerik_Target%
if %ERRORLEVEL% == 1 (
    echo ErrorLevel is %ERRORLEVEL%
    echo Problem!
    pause
)

set control=Input
msbuild.exe "%Telerik_Source%\Controls\%control%\%control%_WPF.sln" /p:Configuration="%Telerik_Configuration%" /target:%Telerik_Target%
if %ERRORLEVEL% == 1 (
    echo ErrorLevel is %ERRORLEVEL%
    echo Problem!
    pause
)

msbuild.exe "%Telerik_Source%\Controls\Data\DataControls_WPF.sln" /p:Configuration="%Telerik_Configuration%" /target:%Telerik_Target%
if %ERRORLEVEL% == 1 (
    echo ErrorLevel is %ERRORLEVEL%
    echo Problem!
    pause
)

set control=GridView
msbuild.exe "%Telerik_Source%\Controls\%control%\%control%_WPF.sln" /p:Configuration="%Telerik_Configuration%" /target:%Telerik_Target%
if %ERRORLEVEL% == 1 (
    echo ErrorLevel is %ERRORLEVEL%
    echo Problem!
    pause
)

copy "%Telerik_Output%\Telerik.Windows.Controls.*" .
copy "%Telerik_Output%\Telerik.Windows.Data.*" .

dir Telerik.Windows.Controls.Data.dll
dir Telerik.Windows.Controls.DataVisualization.dll
dir Telerik.Windows.Controls.dll
dir Telerik.Windows.Controls.Input.dll
dir Telerik.Windows.Controls.Navigation.dll
dir Telerik.Windows.Data.dll

dir *.dll
