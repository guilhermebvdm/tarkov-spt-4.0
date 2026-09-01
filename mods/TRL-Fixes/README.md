# TRL-Fixes

Coleção de patches e correções essenciais de baixo nível para o ecossistema Tarkov Red Line (SPT 4.0 / EFT 0.16.9 / FIKA).

---

## 🛠️ Correções e Patches Incluídos

### 1. IA e Combate
- **`BotMountWeaponFixPatch`**: Permite que bots (Rogues exUsec e Scav Bosses) operem metralhadoras fixas (NSV) e lança-granadas (AGS-30), corrigindo descarte acidental de armas, ativação da Camada 10 e travamentos de rede no FIKA.
- **`FlashbangBotPatch`**: Suspende tomadas de decisão do SAIN e força estado de tiro cego enquanto a IA estiver sob efeito de cegueira profunda.
- **`FlashbangRadiusPatch`**: Amplia o raio de percepção periférica de flashbangs para 20m com checagem de oclusão por terreno/paredes.

### 2. Estabilidade do Jogo Base
- **`PickupAimingSafetyPatch`**: Previne a trava e congelamento total dos controles do jogador ao pegar/equipar itens volumosos do chão.
- **`BotWeaponManagerSafetyPatch`**: Protege contra NREs durante o descarte ou transição de armas em `LateUpdate` ao abater IAs.
- **`DynamicMapsSafetyPatch`**: Absorve exceções durante o descarte de telas de interface no encerramento de raid (`OnRaidEnd`).

### 3. Integração e Sincronização FIKA Coop
- **`FikaInventoryDesyncSafetyPatch`**: Previne e corrige desyncs de grid, itens invisíveis/fantasmas e rejeições de inventário (`is taken by another item` / `GClass1543`) através de reserva virtual em `Ctrl+Click` rápido e auto-recuperação visual na Main Thread.
- **`FixFikaReviveRagdollPatch`**: Restaura hitboxes (`Layer 12 HitCollider`), placas balísticas e congela Rigidbodies após reanimar jogadores em partidas cooperativas.
- **`FikaProceedEmptyHandsSafetyPatch`**: Resolve falhas de rejeição no `FikaServer` para pacotes de mãos vazias (`ProceedType.EmptyHands`).
- **`FikaRefreshSlotViewsSafetyPatch`**: Elimina erros críticos de colisão de dicionário em armas com múltiplos trilhos táticos idênticos.
- **`FikaMainThreadUISafetyPatch`**: Despacha alertas e mensagens de interface do FIKA de forma thread-safe para a Unity Main Thread.

---

## 📚 Documentação Técnica

A documentação arquitetural e funcional completa está disponível em [docs/](./docs/README.md):
- [01. Visão Geral e Arquitetura](./docs/01-visao-geral-e-arquitetura.md)
- [02. Patches de IA e Mecânicas de Combate](./docs/02-patches-de-ia-e-combate.md)
- [03. Estabilidade e Tolerância a Falhas](./docs/03-estabilidade-e-tolerancia-a-falhas.md)
- [04. Integração e Sincronização FIKA Coop](./docs/04-integracao-e-sincronizacao-fika-coop.md)
- [Handoff Técnico — Pickup Aiming Safety](./docs/handoff-pickup-aiming-safety.md)

---

## ⚙️ Configurações (F12)

Consulte o catálogo de opções em [PROPRIEDADES.md](./PROPRIEDADES.md).
*(Este mod executa patches e proteções automáticas de inicialização e não expõe opções configuráveis no menu F12).*

---

## 📁 Estrutura do Mod

- `modded/` — Código-fonte C# dos patches e plugin ativo.
- `modded-V2-audit/` — Cópia de trabalho e auditoria técnica de código.
- `docs/` — Documentação técnica modular e detalhada.
- `PROPRIEDADES.md` — Mapeamento de configurações do mod.
- `backlog/` — Especificações funcionais e itens de backlog.
- `memory/sessions.md` — Histórico de sessões do mod.
