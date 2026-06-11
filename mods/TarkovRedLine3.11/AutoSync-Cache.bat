@echo off
:: Nome do arquivo: AutoSync-Cache.bat
:: Este arquivo força o pedido de Admin e depois roda o script PowerShell
cd /d "%~dp0"

:: Verifica se tem admin
net session >nul 2>&1
if %errorLevel% == 0 (
    goto :run
) else (
    echo Solicitando Permissao de Administrador...
    powershell -Command "Start-Process '%~sdpnx0' -Verb RunAs"
    exit
)

:run
:: Roda o script PowerShell ignorando as travas de segurança
powershell -NoExit -NoProfile -ExecutionPolicy Bypass -File "%~dp0AutoSync-Cache.ps1"
