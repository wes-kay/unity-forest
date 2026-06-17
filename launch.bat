@echo off

REM Set PATH (prepend your dirs)
set PATH=%USERPROFILE%\.local\bin;%USERPROFILE%\bin;C:\usr\local\bin;%PATH%

REM Set environment variables
set ANTHROPIC_BASE_URL=http://192.168.1.80:1234
set ANTHROPIC_AUTH_TOKEN=fake-key

REM Run command
claude --model qwen/qwen3.6-35b-a3b