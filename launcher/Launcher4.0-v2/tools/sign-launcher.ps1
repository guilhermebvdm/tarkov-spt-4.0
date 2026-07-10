<#
.SYNOPSIS
    Item 018 (auto-update-security) — signs the released launcher exe, producing the detached
    "<exe>.sig" that the server serves and the client verifies against the embedded PUBLIC key.

.DESCRIPTION
    RSA-2048 PKCS#1 v1.5 over SHA-256 (algorithm "RSA-SHA256", matches
    SPT.Launcher.Security.UpdateTrust.SignatureAlgorithm). Uses openssl so it is reproducible on any
    machine. The .sig is RAW signature bytes; the server base64-encodes it into /redline/launcher/version.

    RELEASE FLOW (D-018.2 / Gate G-4):
      1. Build/publish "Tarkov Red Line.exe" e renomeie para "TRL.Launcher.exe".
      2. Run this script with the PRODUCTION private key (kept OFF the repo, off any synced path).
      3. Place "TRL.Launcher.exe" + the produced "TRL.Launcher.exe.sig" together in the server's Launcher-Updater/.
      4. Confirm the embedded PUBLIC key (UpdateTrust.EmbeddedPublicKeyPem) matches the private key used.

.PARAMETER ExePath
    Path to the exe to sign (após o rename: "TRL.Launcher.exe").

.PARAMETER PrivateKeyPath
    Path to the RSA private key PEM. DEV default: ..\.keys\launcher-update-dev-private.pem (gitignored).
    PRODUCTION: pass the real private key path; NEVER commit it.

.EXAMPLE
    pwsh ./sign-launcher.ps1 -ExePath "..\dist\TRL.Launcher.exe"
    # signs with the DEV key → "..\dist\TRL.Launcher.exe.sig"

.EXAMPLE
    pwsh ./sign-launcher.ps1 -ExePath "D:\release\TRL.Launcher.exe" -PrivateKeyPath "E:\secrets\prod-private.pem"
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ExePath,

    [string]$PrivateKeyPath = (Join-Path $PSScriptRoot '..\.keys\launcher-update-dev-private.pem'),

    [string]$PublicKeyPath = (Join-Path $PSScriptRoot '..\.keys\launcher-update-dev-public.pem')
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $ExePath)) {
    throw "Exe not found: $ExePath"
}
if (-not (Test-Path -LiteralPath $PrivateKeyPath)) {
    throw "Private key not found: $PrivateKeyPath (generate with generate-dev-keypair.ps1, or point to the production key)"
}

$openssl = (Get-Command openssl -ErrorAction SilentlyContinue)
if ($null -eq $openssl) {
    throw "openssl not found on PATH. Install OpenSSL (Git for Windows bundles it under usr/bin) or add it to PATH."
}

$sigPath = "$ExePath.sig"

Write-Host "Signing '$ExePath'"
Write-Host "  with key '$PrivateKeyPath'"
& openssl dgst -sha256 -sign $PrivateKeyPath -out $sigPath $ExePath
if ($LASTEXITCODE -ne 0) { throw "openssl signing failed (exit $LASTEXITCODE)" }

# Verify locally with the public key so a bad release is caught before publishing.
if (Test-Path -LiteralPath $PublicKeyPath) {
    & openssl dgst -sha256 -verify $PublicKeyPath -signature $sigPath $ExePath
    if ($LASTEXITCODE -ne 0) { throw "Local signature verification FAILED — do not publish." }
    Write-Host "Local verification against '$PublicKeyPath': OK"
} else {
    Write-Warning "Public key '$PublicKeyPath' not found — skipped local verification. Verify before publishing."
}

$sha = (Get-FileHash -LiteralPath $ExePath -Algorithm SHA256).Hash.ToLowerInvariant()
Write-Host ""
Write-Host "Done."
Write-Host "  exe  : $ExePath"
Write-Host "  sig  : $sigPath"
Write-Host "  sha256(exe): $sha"
Write-Host ""
Write-Host "Publish BOTH the exe and the .sig together in the server's Launcher-Updater/ folder."
