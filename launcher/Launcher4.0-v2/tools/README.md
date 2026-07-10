# Launcher auto-update signing (item 018 — auto-update-security)

The launcher auto-update is protected by a **detached signature** over the exe. The client embeds the
**public** key (`SPT.Launcher.Base/Security/UpdateTrust.cs → EmbeddedPublicKeyPem`) and refuses
to promote/run any downloaded exe whose signature does not verify. This is the **primary barrier against
the RCE** — it holds even if the transport is fully compromised (only the private-key holder can produce
a signature the client accepts).

## Crypto scheme

- **Algorithm:** `RSA-SHA256` — RSA-2048, PKCS#1 v1.5 over SHA-256 (what `openssl dgst -sha256 -sign`
  produces, and what `RSA.VerifyData(payload, sig, SHA256, Pkcs1)` verifies).
- **`.sig` format:** raw signature bytes in a file `TRL.Launcher.exe.sig` next to the exe. The server
  base64-encodes it into `GET /redline/launcher/version` (`signature` field); the client base64-decodes
  and verifies over the exact bytes it downloaded (authority = locally recomputed hash, no announced hash
  is trusted).
- The verifier also accepts `ECDSA-SHA256` (DER/Rfc3279 sig) if you switch `SignatureAlgorithm` and embed
  an EC public key instead.

## Key custody (D-018.2)

| Key | Where | Rule |
|-----|-------|------|
| PUBLIC | `UpdateTrust.EmbeddedPublicKeyPem` (source, committed) | Safe to publish. |
| PRIVATE | `.keys/…-private.pem` (gitignored) or a secret manager | **NEVER** commit / sync (Syncthing). It is the release signing secret. Leaking it reopens the RCE; losing it means no more verifiable updates. |

The keypair currently embedded is a **DEV** key generated on this machine. **Production must generate its
own keypair** (`generate-dev-keypair.ps1` produces one; for prod, run it on the signer/CI machine),
embed the new public key, and keep the private key in secure custody.

## Release flow (Gate G-2 / G-4)

1. Build & publish → `Tarkov Red Line.exe` (AssemblyName), then **rename to `TRL.Launcher.exe`** (o nome que o mod procura no `Launcher-Updater/`).
2. Sign it:
   ```powershell
   pwsh ./sign-launcher.ps1 -ExePath "..\dist\TRL.Launcher.exe"           # DEV key
   pwsh ./sign-launcher.ps1 -ExePath "<exe>" -PrivateKeyPath "<prod-private.pem>"  # PROD
   ```
   → produces `TRL.Launcher.exe.sig` and self-verifies it against the public key.
3. Put **both** `TRL.Launcher.exe` and the `.sig` in the server's `Launcher-Updater/` folder.
4. `GET /redline/launcher/version` now returns `version` + `signature` + `algorithm`.
   `GET /redline/launcher/download` serves the exe; `GET /redline/launcher/signature` serves the raw
   `.sig` (diagnostic).

If the `.sig` is missing, the server returns `signature: null` and the client **fails closed** (aborts
the update, keeps the current version playable). **Every** client — including coop peers — stays on the
current version until a valid `.sig` is published (invisible in solo=host: the host must publish a
correctly-signed exe that the *other* clients can verify).

## Cert pinning (Gate G-5, defense-in-depth)

`UpdateTrust.SpkiPins` is **empty by default → transport pinning is inert (bootstrap)**. The signature
still fail-closes the RCE. To turn pinning on, extract the production server's SPKI pin and paste it in:

```powershell
pwsh ./get-spki-pin.ps1 -HostPort "147.15.29.24:6969"
```

Keep **two** pins (current + next) so a cert can be rotated without bricking clients — publish the new
pin in a client release **before** rotating the server cert (D-018.4).

## Scripts

| Script | Purpose |
|--------|---------|
| `generate-dev-keypair.ps1` | Generate an RSA-2048 keypair into `.keys/`. |
| `sign-launcher.ps1` | Sign the exe → `.sig`, self-verify. |
| `get-spki-pin.ps1` | Compute the SPKI SHA-256 pin of a server cert (for `SpkiPins`). |
