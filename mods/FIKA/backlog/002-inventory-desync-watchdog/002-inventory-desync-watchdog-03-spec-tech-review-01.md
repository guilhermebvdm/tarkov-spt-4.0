# 002 — Watchdog de Timeout de Inventário e Desync em Coop/Headless · Spec Tech Review 01

**Mod:** FIKA  
**Data:** 2026-09-05  
**Autor:** Antigravity / saraiva  
**Spec Técnica:** [002-inventory-desync-watchdog-02-spec-tech.md](002-inventory-desync-watchdog-02-spec-tech.md)  

---

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · Total: 0

- A abordagem de watchdog interno no getter de `WaitingForCallback` é não-intrusiva e preserva 100% da compatibilidade de API com todos os mods dependentes.
- `CleanupExpiredCallbacks()` utiliza remoção prévia ao despacho do callback, prevenindo loops de reentrada.
- Timestamps de callbacks residuais são limpos automaticamente quando as coleções são zeradas, eliminando risco de leak de memória.

Aprovado sem ressalvas 🟢.
