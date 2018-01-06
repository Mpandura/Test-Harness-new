@echo off


cd "%~dp0\TestExecutive\bin\Debug"
start TestExecutive.exe

cd "%~dp0\TestHarness\bin\Debug"
start TestHarness.exe

cd "%~dp0\Repository\bin\Debug"
start Repository.exe

cd "%~dp0\Client\bin\Debug"
start Client.exe

@pause