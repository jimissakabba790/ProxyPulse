# ProxyPulse Troubleshooting Guide

## Common First-Run Issues

### 1. WinDivert Driver Installation Failure

**Symptoms**:
- Error: "Failed to install WinDivert driver"
- Event Log shows driver signature verification failed

**Resolution**:

1. Verify Windows version and architecture:
```powershell
systeminfo | findstr /B /C:"OS Name" /C:"OS Version" /C:"System Type"
```

2. Check driver signing status:
```powershell
Get-AuthenticodeSignature "C:\Program Files\ProxyPulse\drivers\WinDivert64.sys"
```

3. Enable test signing if needed:
```powershell
bcdedit /set testsigning on
shutdown /r /t 0
```

4. Manual driver installation:
```powershell
sc create WinDivert type= kernel binPath= "%ProgramFiles%\ProxyPulse\drivers\WinDivert64.sys"
sc start WinDivert
```

### 2. Firewall Blocking Traffic

**Symptoms**:
- No connection through proxy
- Applications timeout
- Windows Security alerts

**Resolution**:

1. Check Windows Defender status:
```powershell
Get-MpPreference | Select-Object -Property DisableRealtimeMonitoring
```

2. Add ProxyPulse exceptions:
```powershell
netsh advfirewall firewall add rule name="ProxyPulse" dir=in action=allow program="%ProgramFiles%\ProxyPulse\ProxyPulse.exe"
netsh advfirewall firewall add rule name="ProxyPulse-Out" dir=out action=allow program="%ProgramFiles%\ProxyPulse\ProxyPulse.exe"
```

3. Verify port access:
```powershell
Test-NetConnection -ComputerName localhost -Port 1080
```

### 3. Invalid API Key Authentication

**Symptoms**:
- "Authentication failed" in logs
- No proxy servers fetched
- Red status indicator

**Resolution**:

1. Verify API key format:
```powershell
Get-Content "%AppData%\ProxyPulse\appsettings.json" | ConvertFrom-Json | Select-Object -ExpandProperty ApiKey
```

2. Reset API credentials:
```powershell
mkdir -Force "%AppData%\ProxyPulse\backup"
move "%AppData%\ProxyPulse\appsettings.json" "%AppData%\ProxyPulse\backup\appsettings.json.bak"
```

3. Re-enter API key through UI

### 4. No Processes Listed for Routing

**Symptoms**:
- Empty process list
- "Access denied" errors
- Cannot select applications

**Resolution**:

1. Check administrator privileges:
```powershell
$elevated = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $elevated) { Start-Process ProxyPulse.exe -Verb RunAs }
```

2. Reset process cache:
```powershell
Remove-Item "%AppData%\ProxyPulse\cache\processes.dat"
```

3. Grant process query permission:
```powershell
$sid = [System.Security.Principal.WindowsIdentity]::GetCurrent().User.Value
$path = "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options"
New-ItemProperty -Path $path -Name "ProxyPulse.exe" -Value 1 -PropertyType DWORD
```

### 5. Self-Update Download/Install Failure

**Symptoms**:
- Update download fails
- Installation errors
- Version mismatch

**Resolution**:

1. Clear update cache:
```powershell
Remove-Item "$env:TEMP\ProxyPulse-Setup.exe" -ErrorAction SilentlyContinue
Remove-Item "$env:TEMP\ProxyPulse-Update" -Recurse -ErrorAction SilentlyContinue
```

2. Force manual update:
```powershell
Start-Process "$env:ProgramFiles\ProxyPulse\ProxyPulse.exe" -ArgumentList "--force-update"
```

3. Verify file signatures:
```powershell
Get-AuthenticodeSignature "$env:TEMP\ProxyPulse-Setup.exe" | Select-Object Status,SignerCertificate
```

## Getting Additional Help

If issues persist:

1. Collect diagnostic logs:
```powershell
Compress-Archive -Path "$env:AppData\ProxyPulse\logs" -DestinationPath "$env:USERPROFILE\Desktop\ProxyPulse-Diagnostics.zip"
```

2. Submit logs through Help → Send Support Request

3. Check GitHub issues for similar problems
