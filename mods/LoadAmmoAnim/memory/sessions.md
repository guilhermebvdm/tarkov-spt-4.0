# LoadAmmoAnim — Memória de Sessões

## Snapshot Delta
- **Versão:** 1.8.14 (SPT 4.0 / EFT 0.16.9)
- **Estado:** Restauração incondicional dos patches de coexistência com o ContinuousLoadAmmo (`ContinuousLoadAmmoCompatPatches.cs`), vinculação defensiva de `WeaponRootAnim` no `vmethod_0` e no `VisualPassNullGuardPatch` para desobstrução da cinemática em 1ª pessoa do EFT, refinamento da desativação de malhas embutidas em `DynamicItemAttachmentService`, bump para v1.8.14 e compilação Release com 0 erros/avisos.
- **Pendências:** 🟢 Nenhuma pendência registrada.

---

## 2026-09-06 — Sessão 4: Correção de Concorrência com ContinuousLoadAmmo, Desbloqueio do VisualPass e Release v1.8.14

**Tema central:** Eliminação do bloqueio que impedia a reprodução da animação de mãos e dos modelos 3D nativos no cliente do jogo durante o municiamento de carregadores.

**Decisões-chave:**
1. **Reativação Incondicional de Patches do CLA (`ContinuousLoadAmmoCompatPatches.cs`):**
   - Reativados incondicionalmente `ClaSetEmptyHandsPatch`, `ClaStopOnHandsChangePatch` e `ClaTrySetLastEquippedWeaponPatch`. A supressão por versão causava colisão onde o CLA chamava `SetEmptyHands()` antes do controller do bundle terminar seu spawn, forçando o player para mãos vazias e abortando a sessão na 1ª bala.
2. **Desbloqueio de Renderização em 1ª Pessoa (`VisualPassNullGuardPatch` & `vmethod_0`):**
   - No `vmethod_0` de `LoadAmmoBundleController.cs`: Se `player.ProceduralWeaponAnimation.HandsContainer.WeaponRootAnim` for nulo, busca e vincula imediatamente o nó `Weapon_root_anim` do prefab instanciado.
   - No `VisualPassNullGuardPatch`: Se o player estiver sob controle de `LoadAmmoBundleController`, recupera o nó `Weapon_root_anim` e permite a execução ininterrupta de `Player.VisualPass()`, restaurando a atualização dos braços e marcadores de IK na câmera.
3. **Refinamento de Malhas Embutidas (`DynamicItemAttachmentService.cs`):**
   - O método `DisableEmbeddedMeshes()` foi ajustado para desativar exclusivamente as malhas de magazine e bala embutidas (`pmag`, `stanag`, `bullet`, `patron`), preservando intacta toda a hierarquia de esqueleto e animadores.
4. **SemVer & Isolamento de Build:** Bump para `1.8.14` em `Directory.Build.props`. Binários compilados exclusivamente em `mods/LoadAmmoAnim/builds/` (`LoadAmmoAnimClient.dll`, `LoadAmmoAnimClientFika.dll`, `LoadAmmoAnimServer.dll`) com 0 erros e 0 avisos.

---

**Tema central:** Eliminação de `NullReferenceException` remanescente ao instanciar o bundle de animação 3D de municiamento (`CreateAndSpawnBundleController`).

**Decisões-chave:**
1. **Resolução de Calibre Segura (`LoadAmmoBundleController.cs`):**
   - Substituição do acesso ao campo `Items_1` por `mag.Cartridges.Items?.OfType<AmmoItemClass>().FirstOrDefault() ?? mag.Cartridges.Last as AmmoItemClass`.
2. **Defensividade em Dimensões (`MagOffsetRegistry.cs`):**
   - Proteção na leitura de largura e altura do magazine: `magWidth = mag.Template != null ? mag.Width : 1;`, impedindo NRE caso o template não esteja em memória.
3. **Try/Catch Granular e Logging Completo:**
   - Cada etapa de anexação de prefabs de magazine e projétil isolada em try/catch para evitar falha catastrófica da FSM de mãos.
   - Captura de `{ex}` com StackTrace completo em `CreateAndSpawnBundleController`.
4. **SemVer & Build:** Bump para `1.8.13` em `Directory.Build.props`. Compilação Release em `mods/LoadAmmoAnim/modded/` gerada com 0 erros e 0 avisos.

---

## 2026-09-05 — Sessão 2: Correção de NRE em MagOffsetRegistry (`ItemTemplate.Name` $\rightarrow$ `_name`) e Release v1.8.12

**Tema central:** Eliminação do crash `NullReferenceException` ao municiar carregadores via inventário com o mod ativo em raids no FIKA Headless/Client.

**Decisões-chave:**
1. **Desserialização EFT/SPT (`MagOffsetRegistry.cs`):** No EFT 0.16.9 / SPT 4.0, o campo `public string Name;` em `ItemTemplate` é nulo em tempo de execução (`_name` é a propriedade desserializada do JSON). O acesso direto a `mag.Template.Name.IndexOf(...)` lançava NRE dentro de `CreateAndSpawnBundleController`, abortando silenciosamente a criação do bundle e o playback da animação 3D nas mãos.
2. **Defensividade Global (`MagOffsetRegistry.cs`, `BanAnimationStore.cs`, `Plugin.cs`):** Unificação para `mag.Template?._name ?? mag.Template?.ShortName ?? mag.Template?.Name ?? string.Empty`.
3. **SemVer & Build:** Bump para `1.8.12` em `Directory.Build.props`. Compilação Release em `mods/LoadAmmoAnim/modded/` gerada com 0 erros e 0 avisos.

---

## 2026-09-05 — Sessão 1: Resolução de Rede Cooperativa (FIKA), Gating de Patches do CLA e Release v1.8.11

**Tema central:** Correção da vulnerabilidade crítica de rede do FIKA (Causa 1 e 2 do guia canônico) que causava `ParseException` ao receber pacotes de animação, e modernização da interoperabilidade com o `SPT-ContinuousLoadAmmo`.

**Decisões-chave:**
1. **Registro Antecipado de Pacotes no FIKA (`FikaCompatModule.cs`):** Substituída a tentativa síncrona no `Awake` pela subscrição oficial de evento:
   `FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnNetworkManagerCreated);`
   Garante que os pacotes `AmmoLoadingStartedPacket`, `AmmoLoadingEndedPacket` e `AmmoLoadingBulletCountSyncPacket` estejam 100% registrados no frame zero da raid antes do processamento de qualquer datagrama pelo LiteNetLib.
2. **Gating de Compatibilidade com ContinuousLoadAmmo (`ContinuousLoadAmmoCompatPatches.cs`):** Se o `ContinuousLoadAmmo` detectado for `>= 1.1.9`, os patches legados de reflection (`ClaSetEmptyHandsPatch`, `ClaStopOnHandsChangePatch`, `ClaTrySetLastEquippedWeaponPatch`) são suprimidos. A coexistência passa a ser gerenciada nativamente pela FSM limpa do CLA moderno.
3. **Isolamento de Build e SemVer:** Versão elevada para `1.8.11` em `Directory.Build.props`. Compilação Release com 0 avisos e 0 erros.
