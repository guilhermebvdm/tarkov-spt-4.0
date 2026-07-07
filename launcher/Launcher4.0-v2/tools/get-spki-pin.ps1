<#
.SYNOPSIS
    Item 018 (auto-update-security / Gate G-5) — computes the SPKI SHA-256 pin (base64) of a server's
    TLS certificate, to paste into SPT.Launcher.Base/Security/UpdateTrust.cs → SpkiPins.

.DESCRIPTION
    The pin = base64( SHA-256( DER SubjectPublicKeyInfo of the cert's public key ) ). Same formula as
    CertificatePinning.ComputeSpkiPin. Two sources:
      * -HostPort "host:port"  → fetches the live cert via `openssl s_client`.
      * -CertPath "cert.pem"   → reads a PEM/DER cert file.

    G-5: run this against the PRODUCTION server, then put the returned pin (and, for rotation, a second
    pre-provisioned pin) into UpdateTrust.SpkiPins BEFORE relying on the pinning layer. While SpkiPins is
    empty the transport is not pinned (bootstrap) — the exe signature still fail-closes the RCE.

.EXAMPLE
    pwsh ./get-spki-pin.ps1 -HostPort "147.15.29.24:6969"

.EXAMPLE
    pwsh ./get-spki-pin.ps1 -CertPath ".\server-cert.pem"
#>
[CmdletBinding(DefaultParameterSetName = 'Host')]
param(
    [Parameter(ParameterSetName = 'Host', Mandatory = $true)]
    [string]$HostPort,

    [Parameter(ParameterSetName = 'File', Mandatory = $true)]
    [string]$CertPath
)

$ErrorActionPreference = 'Stop'

$openssl = (Get-Command openssl -ErrorAction SilentlyContinue)
if ($null -eq $openssl) { throw "openssl not found on PATH." }

$tmp = New-TemporaryFile
try {
    if ($PSCmdlet.ParameterSetName -eq 'Host') {
        Write-Host "Fetching certificate from $HostPort ..."
        # -servername omitted (self-signed IP cert). Pull the leaf cert PEM.
        $pem = (& cmd /c "echo | openssl s_client -connect $HostPort 2>nul")
        $pem | Set-Content -LiteralPath $tmp -Encoding ascii
        $certArg = $tmp
    } else {
        if (-not (Test-Path -LiteralPath $CertPath)) { throw "Cert file not found: $CertPath" }
        $certArg = $CertPath
    }

    # Extract the public key SPKI (DER), SHA-256 it, base64 the digest.
    $pinLine = & cmd /c "openssl x509 -in `"$certArg`" -pubkey -noout | openssl pkey -pubin -outform der | openssl dgst -sha256 -binary | openssl base64"
    $pin = ($pinLine | Select-Object -First 1).Trim()

    if ([string]::IsNullOrWhiteSpace($pin)) {
        throw "Could not compute pin — is the host reachable / the cert file valid?"
    }

    Write-Host ""
    Write-Host "SPKI SHA-256 pin (base64):"
    Write-Host "  $pin"
    Write-Host ""
    Write-Host "Paste into UpdateTrust.SpkiPins, e.g.:"
    Write-Host "    public static readonly string[] SpkiPins = { `"$pin`" };"
}
finally {
    Remove-Item -LiteralPath $tmp -ErrorAction SilentlyContinue
}
