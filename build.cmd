@echo off
set build_dir="bin\PAM-Release"
set config="Release"

echo ======================
echo ==== Building PAM ====
echo ======================

msbuild "pam.sln" /t:Clean;Rebuild "/p:configuration=%config%" /verbosity:normal
if ERRORLEVEL 1 goto END
setlocal ENABLEDELAYEDEXPANSION

if NOT EXIST %build_dir% mkdir %build_dir%
pushd %build_dir%
mkdir Scripts
popd

copy Scripts %build_dir%\Scripts >nul
copy Resources %build_dir% >nul
copy LICENSE %build_dir% >nul
copy README.md %build_dir% >nul

pushd src\PAM.UI\bin\release
for %%f in (*.exe ^ *.dll ^ *.config) do (copy %%f ..\..\..\..\%build_dir% >nul)
popd

pushd src\PAM.BackgroundProcess\bin\release
for %%f in (*.exe ^ *.config) do (copy %%f ..\..\..\..\%build_dir%\ >nul)
popd

pushd src\PAM.Setup\bin\release
for %%f in (*.exe ^ *.config) do (copy %%f ..\..\..\..\%build_dir%\ >nul)
popd

if ERRORLEVEL 0 echo === Build Complete ===

pushd %build_dir%\..
7z a PAM.zip *
popd

:END
