<#
RT-9a: builds the Play bundle (AAB) signed with YOUR upload key, for the RT-9b upload. Run it yourself in PowerShell
(the AI never runs it and never sees the passwords):

    powershell -ExecutionPolicy Bypass -File Tools\store\build-signed-aab.ps1 -Keystore C:\Users\<you>\keys\stampeep-upload.jks

It asks for the two passwords without showing them, passes them to this one Unity build only (process environment,
cleared at the end; nothing is written to the repository or to ProjectSettings: see ClaybreakBuild.UploadKeyScope),
builds C:\temp\rt9b\Stampeep-<version>-<code>.aab and prints the certificate of the key that signed it (the SHA-1 is
the upload key's: the Firebase key restriction needs it next to the app signing key's SHA-1 from the Play Console).
Close the Unity Editor of this project first.
#>
param(
    [Parameter(Mandatory = $true)][string]$Keystore,
    [string]$Alias = "upload",
    [string]$OutDir = "C:\temp\rt9b",
    [string]$Unity = "C:\Program Files\Unity\Hub\Editor\6000.6.7f1\Editor\Unity.exe"
)
$ErrorActionPreference = "Stop"
$project = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
if (-not (Test-Path $Keystore -PathType Leaf)) { throw "Keystore not found: $Keystore" }
$keystoreFull = (Resolve-Path $Keystore).Path
if ($keystoreFull.StartsWith($project, [StringComparison]::OrdinalIgnoreCase)) { throw "The keystore must live outside the repository." }
if (Get-Process Unity -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $Unity }) {
    Write-Warning "A Unity of this version is running. If it has this project open, close it first."
}

function Read-Secret([string]$prompt) {
    $secure = Read-Host -AsSecureString $prompt
    $pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    try { return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer) }
    finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer) }
}

$version = (Select-String -Path (Join-Path $project "ProjectSettings\ProjectSettings.asset") -Pattern "^\s*bundleVersion: (.+)$").Matches[0].Groups[1].Value.Trim()
$code = (Select-String -Path (Join-Path $project "ProjectSettings\ProjectSettings.asset") -Pattern "^\s*AndroidBundleVersionCode: (\d+)").Matches[0].Groups[1].Value
New-Item -ItemType Directory -Force $OutDir | Out-Null
$aab = Join-Path $OutDir "Stampeep-$version-$code.aab"
$log = Join-Path $OutDir "Stampeep-$version-$code.log"

$env:CLAYBREAK_KEYSTORE = $keystoreFull
$env:CLAYBREAK_KEY_ALIAS = $Alias
$env:CLAYBREAK_KEYSTORE_PASS = Read-Secret "Keystore password"
$env:CLAYBREAK_KEY_PASS = Read-Secret "Key password (Enter = same as the keystore)"
if ([string]::IsNullOrEmpty($env:CLAYBREAK_KEY_PASS)) { $env:CLAYBREAK_KEY_PASS = $env:CLAYBREAK_KEYSTORE_PASS }
try {
    Write-Host "Building $aab (version $version, code $code). This takes several minutes..."
    $unityArgs = @("-batchmode", "-nographics", "-quit", "-buildTarget", "Android", "-projectPath", $project,
        "-logFile", $log, "-executeMethod", "Claybreak.Editor.ClaybreakBuild.BuildAndroidAndQuit", "-claybreakAab", $aab)
    # Wait for Unity itself only (a Gradle daemon it leaves behind must not hold the passwords in this window), with a
    # time limit: Unity in batchmode sometimes hangs on exit after writing its output.
    $process = Start-Process -FilePath $Unity -ArgumentList $unityArgs -PassThru -NoNewWindow
    if (-not $process.WaitForExit(45 * 60 * 1000)) {
        if (Test-Path $aab) { Write-Warning "Unity did not exit after writing the AAB; stopping it."; Stop-Process -Id $process.Id -Force }
        else { Stop-Process -Id $process.Id -Force; throw "Build timed out; see $log" }
    }
    elseif ($process.ExitCode -ne 0 -or -not (Test-Path $aab)) { throw "Build failed (exit $($process.ExitCode)); see $log" }
}
finally {
    Remove-Item Env:CLAYBREAK_KEYSTORE_PASS, Env:CLAYBREAK_KEY_PASS, Env:CLAYBREAK_KEYSTORE, Env:CLAYBREAK_KEY_ALIAS -ErrorAction SilentlyContinue
    # The Gradle daemon inherited the passwords: stop it. The generated Gradle project may hold the signing passwords in
    # plain text: delete it (Unity makes it again on the next build; Library/ is not versioned, but other sessions read it).
    Get-CimInstance Win32_Process -Filter "Name='java.exe'" -ErrorAction SilentlyContinue |
        Where-Object { $_.CommandLine -like '*GradleDaemon*' } | ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }
    $gradle = Join-Path $project "Library\Bee\Android\Prj\IL2CPP\Gradle"
    if (Test-Path $gradle) { Remove-Item -Recurse -Force $gradle -ErrorAction SilentlyContinue }
}

# If Unity was stopped mid-build, the key path may have stayed in the project settings (never the passwords).
$settings = Get-Content (Join-Path $project "ProjectSettings\ProjectSettings.asset") -Raw
if ($settings -match "androidUseCustomKeystore: 1" -or $settings -match "AndroidKeystoreName: [^\s']") {
    Write-Warning "ProjectSettings.asset still points at the keystore: do not commit it; ask the AI to revert the keystore lines."
}

$keytool = Join-Path (Split-Path $Unity) "Data\PlaybackEngines\AndroidPlayer\OpenJDK\bin\keytool.exe"
Write-Host ""
Write-Host "AAB: $aab ($([math]::Round((Get-Item $aab).Length / 1MB, 1)) MB)"
Write-Host "Signed by (upload key):"
& $keytool -printcert -jarfile $aab | Select-String "Owner:|SHA1:|SHA256:"
Write-Host "If the owner says 'Android Debug', the upload key was NOT used: do not upload this file."
# The build touches a few settings files (line endings, keystore name restored): list them, change nothing.
Push-Location $project
Write-Host "Files the build touched (the next AI session reverts them; nothing about the key is in them):"
git status --short
Pop-Location
