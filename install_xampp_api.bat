@echo off
setlocal

set "SOURCE_DIR=%~dp0Server\XamppApi"
set "TARGET_DIR=%~1"
if "%TARGET_DIR%"=="" set "TARGET_DIR=D:\webserver\ClassroomQuizApi"

if not exist "%SOURCE_DIR%\api.php" (
    echo Could not find the XAMPP API source folder:
    echo   "%SOURCE_DIR%"
    exit /b 1
)

if not exist "%TARGET_DIR%" mkdir "%TARGET_DIR%"
if errorlevel 1 (
    echo Could not create destination folder:
    echo   "%TARGET_DIR%"
    exit /b 1
)

xcopy "%SOURCE_DIR%\*" "%TARGET_DIR%\" /E /I /Y
if errorlevel 1 (
    echo Failed to copy the XAMPP API files.
    exit /b 1
)

echo XAMPP API installed to:
echo   "%TARGET_DIR%"
echo Start Apache and MySQL, then open http://127.0.0.1/ClassroomQuizApi/api.php
echo The classromquiz database and tables are created on the first API request.
endlocal
