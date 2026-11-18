@echo off
setlocal

set "APP=%~dp0Build\ClassroomQuiz.exe"
if not exist "%APP%" (
    echo Could not find the game build:
    echo   "%APP%"
    echo Build the Windows app into the Build folder first.
    pause
    exit /b 1
)

start "" "%APP%" --cq-profile=teacher
timeout /t 2 /nobreak >nul
start "" "%APP%" --cq-profile=student1
timeout /t 2 /nobreak >nul
start "" "%APP%" --cq-profile=student2

endlocal
