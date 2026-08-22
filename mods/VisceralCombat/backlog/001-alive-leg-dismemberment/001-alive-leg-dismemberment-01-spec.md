---
title: Spec — Desmembramento de Perna em Bots Vivos (Prone, Agonia & Rastro de Sangue)
date: 2026-08-10
status: 🔵 Investigado / Aguardando Implementação
authors: [AI Assistant, USER]
---

# Spec 001 · Desmembramento de Perna em Bots Vivos (Prone, Agonia & Rastro de Sangue)

## 📌 Visão Geral
Esta funcionalidade visa adicionar um nível extremo de imersão ao combate: permitir que disparos de grosso calibre amputem a perna de um bot (AI) que sobreviva ao impacto inicial, forçando o bot a cair de bruços (*Prone*), entrar em animação de agonia, ficar permanentemente impedido de se levantar e rastejar pelo mapa deixando um rastro contínuo de sangue até a morte por sangramento.

---

## 🎯 Requisitos Funcionais

### 1. Escopo & Restrições
- **Apenas Bots (AI):** Aplicado estritamente a bots (Scavs, PMCs, Bosses). Jogadores humanos são totalmente ignorados para evitar travamentos de controle.
- **Específico para Pernas:** Válido exclusivamente para as partes corporais `EBodyPart.LeftLeg` (5) e `EBodyPart.RightLeg` (6).
- **Gatilho de Calibre:** Disparado apenas por calibres de alto impacto cadastrados com probabilidade ativa no `VD_Calibers.json` (ex: 12g, KS-23, 7.62x51, .338 Lapua, .277 Spear).

### 2. Fluxo de Animação & Comportamento
1. **Disparo & Amputação:** Ao receber um tiro de grosso calibre na perna e sobreviver, a perna é amputada via `KillPatch.DismemberLimb`.
2. **Queda Instantânea em Prone:** O bot é forçado para a postura de bruços via `botOwner.BotLay.IsLay = true`.
3. **Execução da Agonia:** A animação de agonia de perna é iniciada no chão.
4. **Bloqueio de Postura (Lock em Prone):** Um `MonoBehaviour` customizado (`LivingDismembermentController`) intercepta a IA e mantém `botOwner.BotLay.IsLay = true` e `botOwner.BotLay.NextPosibleGetUp = Time.time + 99999f` no `Update()` para impedir permanentemente o bot de agachar ou se levantar.

### 3. Sangramento Arterial & Rastro de Sangue
1. **Fluxo Contínuo de Sangue:** Iniciar um emissor de sangue contínuo no coto da perna amputada (`limbSquirter` / `ArterialSpray`).
2. **Rastro de Poças no Chão (Bleed Trail):** Conforme o bot rasteja em prone, instanciar decalques/poças de sangue em intervalos regulares (`GoreObjectPool`) na posição atual, criando um rastro visível.
3. **Morte por Exsanguição:** Chamar `player.ActiveHealthController.ApplyDamage(legBodyPart, damagePerSec, GClass3051.HeavyBleedingDamage)` em intervalos via `GClass855.WaitSeconds` até a morte.

---

## ⚙️ Especificação Técnica Detalhada — Resultado da Investigação

### 2.1 Prone Forçado (Confirmado no Assembly EFT)

A API de prone correta e nativa do bot é acionada por `BotLay.IsLay`:
```csharp
// EFT: BotLay.cs — aciona DoProne e trava pose da IA
botOwner.BotLay.IsLay = true;
botOwner.BotLay.NextPosibleGetUp = Time.time + 99999f;
```
> [!CAUTION]
> **Atenção:** `SetPose(0f)` ou `SetPoseLevel(0f, true)` apenas agacham o bot na altura mínima, permitindo que a IA levante em seguida. O uso correto é estritamente `BotLay.IsLay = true` mantido no `Update()`.

**Bloqueio de GetUp:** `BotLay.GetUp()` é neutralizado mantendo `NextPosibleGetUp` no futuro distante.

### 2.2 Sangramento via HealthController (Confirmado no Assembly EFT)

O sistema de dano por sangramento é nativo e seguro de chamar:
```csharp
// EFT: GClass3051.cs:L40-43 — HeavyBleedingDamage = EDamageType.HeavyBleeding
player.ActiveHealthController.ApplyDamage(
    EBodyPart.LeftLeg,
    3f,                               // HP por tick (configurável)
    GClass3051.HeavyBleedingDamage    // tipo: HeavyBleeding
);
```
`GClass3051.HeavyBleedingDamage` é um `DamageInfoStruct` pronto com `EDamageType.HeavyBleeding`, sem necessidade de instanciar manualmente.

### 2.3 Componentes Envolvidos
- **`LimbKillPatch.cs`:** Detectar dano de perna em bot vivo → instanciar `LivingDismembermentController`.
- **`LivingDismembermentController` (Novo `MonoBehaviour`):**
  - `Start()`: chama `DoProne(true)` + `SetPose(0f)`, inicia ArterialSpray.
  - `Update()`: mantém `NextPosibleGetUp` alto para impedir levantamento.
  - Coroutine de `HeavyBleedingDamage` a cada N segundos.
  - Coroutine de rastro de poças (`GoreObjectPool.Instance.Spawn`) a cada X metros.
  - `OnDestroy()`: para o ArterialSpray e limpa decalques.
- **`GoreObjectPool`:** Reuso dos prefabs para manter FPS estável durante o rastro.

---

## 🌐 Compatibilidade FIKA (Multiplayer Coop) — Investigado ✅

> [!IMPORTANT]
> **RESULTADO DA INVESTIGAÇÃO:** A solução via FIKA-Server é **100% VIÁVEL** com uma mod de servidor SPT simples. **Não é necessário criar um packet FIKA customizado.**

### Mecanismo Nativo Existente: `FikaServer/ClientService.cs`

O FIKA já possui um sistema completo de verificação de mods obrigatórios entre host e clientes:

```csharp
// fika-server/FikaServer/Services/ClientService.cs:L15
private readonly List<string> _requiredMods = ["com.fika.core", "com.SPT.custom", ...];

// Config: fika-server/FikaServer/Models/Fika/Config/FikaConfigClient.cs
// "Mods.Required" → Lista de GUIDs obrigatórios configurável
```

**Fluxo de verificação:**
1. No boot, cada cliente FIKA envia todos os seus plugins carregados (GUID + CRC32 hash) via `POST /fika/client/check/mods`.
2. O servidor FIKA verifica se os `_requiredMods` estão presentes. Se ausentes → `MissingRequired` → cliente é expulso.
3. A lista `_requiredMods` é configurável via `FikaConfigClient.Mods.Required` no `Fika.jsonc` do servidor.

### Solução para VisceralCombat

**Abordagem A — Zero código extra (via configuração):**
O host adiciona `"com.nexus.visceralcombat"` (GUID do plugin) na lista `Client.Mods.Required` do arquivo `Fika.jsonc`. Clientes sem o mod são impedidos de joinar.

**Abordagem B — Mod de Servidor SPT (recomendada, mais elegante):**
Criar um mod servidor TypeScript mínimo que, no `postDBLoad`, adiciona programaticamente o GUID do VisceralCombat à lista `_requiredMods` do FIKA. Assim o host nunca precisa editar configs manualmente.

```typescript
// Exemplo conceitual de mod server SPT (src/mod.ts)
class VisceralCombatServerMod implements IPreSptLoadMod {
    public preSptLoad(container: DependencyContainer): void {
        // Adiciona VisceralCombat como mod obrigatório no FIKA
        const clientService = container.resolve<ClientService>("ClientService");
        clientService.addRequiredMod("com.nexus.visceralcombat");
    }
}
```

> [!NOTE]
> A Abordagem B requer investigar se `ClientService` do FIKA expõe um método público `addRequiredMod()` ou se é necessário acessar `_requiredMods` via reflection. Se não exposto, Abordagem A (configuração manual) é suficiente para a primeira versão.

---

## ✅ Critérios de Aceite
1. Bot atingido por calibre pesado na perna perde o membro e cai instantaneamente em prone.
2. Bot não consegue se colocar em pé ou agachado sob nenhuma circunstância.
3. Rastro de sangue visível se forma no chão ao longo do trajeto de rastejamento do bot.
4. Bot morre naturalmente por `HeavyBleeding` após o tempo configurado.
5. **Em sessão FIKA:** feature ativa APENAS se todos os jogadores tiverem o mod — garantido via `Mods.Required` no FIKA server (Abordagem A) ou mod SPT server (Abordagem B).
6. Zero erros de console e sem vazamento de RAM ao descarregar a raid.
