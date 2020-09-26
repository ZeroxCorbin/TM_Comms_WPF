@echo off
pushd "%~dp0"
powershell Compress-7Zip "Release" -ArchiveFileName "TM_Comms_WPF.zip" -Format Zip
:exit
popd
@echo on
