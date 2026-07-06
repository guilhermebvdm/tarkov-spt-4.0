<#
.SYNOPSIS
    Item 018 (auto-update-security) — generates an RSA-2048 keypair for signing the launcher exe.

.DESCRIPTION
    Writes the PRIVATE and PUBLIC key PEMs into ..\.keys\ (gitignored). After running:
      * Copy the PUBLIC key content into SPT.Launcher.Base/Security/UpdateTrust.cs
        (EmbeddedPublicKeyPem) — this is what every client trusts.
      * Keep the PRIVATE key OFF the repo and OFF any synced path (Syncthing). It is the release
        signing secret (D-018.2). Losing it means you cannot ship a verifiable update; leaking it
        reopens the RCE.

    For PRODUCTION prefer generating on the dedicated signer/CI machine and storing the private key in
    a secret manager — not on a dev box.

.EXAMPLE
    pwsh ./generate-dev-keypair.ps1
#>
[CmdletBinding()]
param(
    [string]$OutDir = (Join-Path $PSScriptRoot '..\.keys'),
    [string]$Name   = 'launcher-update-dev'
)

$ErrorActionPreference = 'Stop'

$openssl = (Get-Command openssl -ErrorAction SilentlyContinue)
if ($null -eq $openssl) { throw "openssl not found on PATH." }

if (-not (Test-Path -LiteralPath $OutDir)) {
    New-Item -ItemType Directory -Path $OutDir | Out-Null
}

$priv = Join-Path $OutDir "$Name-private.pem"
$pub  = Join-Path $OutDir "$Name-public.pem"

if (Test-Path -LiteralPath $priv) {
    throw "Refusing to overwrite existing private key: $priv (delete it manually if you really mean to rotate)."
}

& openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out $priv
if ($LASTEXITCODE -ne 0) { throw "key generation failed" }

& openssl rsa -in $priv -pubout -out $pub
if ($LASTEXITCODE -ne 0) { throw "public key export failed" }

Write-Host "Generated:"
Write-Host "  private: $priv  (gitignored — NEVER commit/sync)"
Write-Host "  public : $pub"
Write-Host ""
Write-Host "Embed this PUBLIC key in UpdateTrust.EmbeddedPublicKeyPem:"
Write-Host ""
Get-Content -LiteralPath $pub
