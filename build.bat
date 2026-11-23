@echo off
chcp 65001 >nul
echo 正在编译「摸物品 · 开箱寻宝」...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o publish
if errorlevel 1 (
  echo.
  echo 编译失败，请确认已安装 .NET 8 SDK。
  pause
  exit /b 1
)
echo.
echo 完成：publish\LootSearch.exe
pause
