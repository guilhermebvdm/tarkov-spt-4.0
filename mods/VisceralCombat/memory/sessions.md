# Visceral Combat — Memória de Sessões

## Snapshot Delta
- **Versão:** 3.7.0 (SPT 4.0 / FIKA 2.2.6)
- **Estado:** Código C# extraído das DLLs originais e estruturado no repositório.
- **Pendências:** 🟢 Nenhuma pendência blocker registrada.

## Sessão 2026-07-28 — Extração e Inicialização do Mod
- **Decisão:** Extração direta via `ilspycmd` (versão 9.0) das DLLs `VisceralCombat.dll`, `VolumetricBloodFX.dll` e `bundleloader.dll`.
- **Organização:** Código salvo intacto em `original/` e duplicado em `modded/` para correções ativas.
- **Observação:** O mod faz uso direto das APIs de rede do FIKA (`FikaEventDispatcher`, `RegisterPacket<DismembermentPacket>`, `RegisterPacket<RagdollSyncPacket>`).
