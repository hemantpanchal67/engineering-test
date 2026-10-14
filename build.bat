@ECHO OFF
SETLOCAL

echo Building Gilded Rose project...

REM Restore dependencies
echo.
echo Restoring NuGet packages...
dotnet restore
IF %ERRORLEVEL% NEQ 0 (
    echo Error: Failed to restore NuGet packages
    exit /b %ERRORLEVEL%
)

REM Clean
echo.
echo Cleaning previous builds...
dotnet clean --configuration Release
IF %ERRORLEVEL% NEQ 0 (
    echo Error: Failed to clean solution
    exit /b %ERRORLEVEL%
)

REM Build
echo.
echo Building solution...
dotnet build --configuration Release --no-restore
IF %ERRORLEVEL% NEQ 0 (
    echo Error: Failed to build solution
    exit /b %ERRORLEVEL%
)

REM Test
echo.
echo Running tests...
dotnet test --configuration Release --no-build --verbosity normal
IF %ERRORLEVEL% NEQ 0 (
    echo Error: Tests failed
    exit /b %ERRORLEVEL%
)

echo.
echo Build completed successfully!
pause
exit /b 0