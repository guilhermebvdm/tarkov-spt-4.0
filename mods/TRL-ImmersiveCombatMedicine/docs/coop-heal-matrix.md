# Matriz Coop de Cura Player→Player — Critérios de Aceite

> **Data:** 2026-07-12<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [../reviews/code-review-01.md](../reviews/code-review-01.md), [../memory/sessions.md](../memory/sessions.md)<br>

---

Rastreamento **estático** (código do mod + Fika 2.3.4 de referência + EFT decompilado) dos 4 cenários de aceite do mod. Cada etapa tem veredito com evidência `arquivo:linha` (detalhe completo nos transcripts dos agentes da sessão 2026-07-12). Validação **in-game** pendente — ver protocolo no fim.

## Fatos-base (valem para todos os cenários)

- Paciente remoto é `ObservedPlayer` com `ObservedHealthController` (**não** é `ActiveHealthController`) → todo heal remoto vai por **pacote** (`ApplyFullTreatmentLocally` roda na máquina do paciente, no ActiveHC dele). O resultado volta ao médico pelo sync nativo do Fika (`ClientHealthController.SendNetworkSyncPacket`).
- Topologia Fika é estrela: client→host→clients. O mod envia com `broadcast=false`, então **o relay é feito pelo handler do próprio mod no host** — não pelo transporte do Fika.
- Headless nunca tem `MainPlayer`; os handlers do mod tratam isso (relay-only, sem NRE).

## Matriz cenário × etapa

| Etapa | 1. Host→Client | 2. Client→Host | 3. C1→C2 (host player) | 4. C1→C2 (headless) |
|---|---|---|---|---|
| (a) Prompt/detecção | ✅ | ✅ | ✅ | ✅ |
| (b) Handshake (check→resposta) | ✅ | ✅ | ✅ relay ok | ✅ relay ok (MainPlayer null tratado) |
| (c) Animação/redirect | ✅ (branch remoto, sem MedEffect) | ✅ (idêntico; nunca testado in-game de client) | ✅ | ✅ |
| (d) Aplicação no paciente | ✅ (paciente aplica no próprio ActiveHC; sync volta) | ✅ | ✅ (G-1 aplicado 2026-07-12: FullTreatment é exclusivo do paciente) | ✅ (headless retorna antes de aplicar; bots locais tratados via CR-01-01) |
| (e) Consumo do item no médico | ⚠️ total=networked ✅ · parcial=local-only (CR-01-23) | ⚠️ idem | ⚠️ idem | ⚠️ idem |
| (f) UI do médico (HUD) | ✅ HP/ECG + ícones por interface (G-2 aplicado) + membro-alvo via TreatmentReport | ✅ idem | ✅ idem | ✅ idem |

**Leitura executiva:** o caminho feliz 2-player fecha ponta-a-ponta nos 4 cenários. Nenhum bloqueador novo no fluxo player→player — os riscos são de **borda** (3º participante, consumo parcial, UI cega a efeitos) e de **deploy**.

## ❗ Requisito de deploy (o achado mais importante)

**O mod PRECISA estar instalado em TODAS as máquinas, incluindo o host-player (cenário 3) e o headless (cenário 4) — e na MESMA BUILD.** (CR-03: o wire-format do `TraumaFaintPacket` mudou e o `BandAidTreatmentReportPacket` foi adicionado sem versionamento de pacote — DLLs mistas geram `ParseException` no receptor e perda dos demais eventos de rede do frame. Atualizar todos os peers juntos, sempre.)

- Sem o mod no host: não há relay (o transporte Fika não retransmite `broadcast=false`) → handshake sempre expira em 3 s; e pior: o host sem o pacote registrado lança `ParseException` **que descarta o resto do batch de eventos de rede daquele frame** — dano colateral a pacotes de outros mods/sistemas.
- O backend SPT não precisa de nada (mod é 100% client-side).

## Gaps consolidados (novos — além dos CR-01 já registrados)

| ID | Sev | Gap | Fix |
|---|---|---|---|
| G-1 ✅ (2026-07-12) | 🟡 | Receptor que não é o paciente (host-player no cenário 3; qualquer 3º client em lobby maior) processa `ApplyFullTreatment` pelo branch de "tratamento específico" e tenta cirurgia via reflection no boneco observado do paciente | 1 linha em `OnBandAidHealPacketReceived`: `if (packet.ApplyFullTreatment && PatientProfileId != meu) return;` |
| G-2 ✅ (2026-07-12) | 🟡 | Médico "cego": ícones de bleed/fratura nunca renderizam para paciente remoto (`HasEffect` compara tipos nested do ActiveHC; efeitos do ObservedHC são `NetworkBodyEffectsAbstractClass`) | `HasEffect` por interface de efeito (padrão que o Fika usa no `ObservedHealthController.Store`) |
| G-3 | 🟢 | Débito do medkit calculado com HP observado (stale) no médico; cura real capada no paciente → débito divergente | ACK do paciente com HP efetivamente curado; médico debita depois |
| G-4 | 🟢 | Sem revalidação entre approve e apply (~3-8 s): ferimento pode sumir → no-op com item já debitado | mesmo ACK/NACK do G-3 |
| G-5 ✅ (2026-07-12) | 🟢 | Resposta do handshake não confere `ItemTemplateId` com o item pendente — resposta atrasada pode aprovar item errado | 1 if antes do `HealRoutine` |
| G-6 | ℹ️ | Relay manual ecoa pacote de volta ao originador (filtrado por checks, mas custo/ruído) | usar overload `SendData(..., NetPeer peerToIgnore)` do Fika |

Relacionados do CR-01 (estado 2026-07-12): **CR-01-10/01/02/04 APLICADOS** (handshake endurecido; client cura bot via autoridade do host; desmaio sincronizado com duração no pacote; defib com chamada tipada). Permanece: **CR-01-23** (consumo parcial local-only — junto de G-3/G-4).

## Protocolo de teste in-game (ordem de custo)

1. **2 PCs (cenários 1 e 2)** — cada um cura o outro com Salewa/CALOK/splint em ferimento real: verificar HP subindo **na tela do paciente e do médico**, consumo do item (1×), animação completa, e log `owner=<nick> | doutorOp=...` dos dois lados. Repetir 2× seguidas no mesmo alvo (regressão da mão travada, P-2.8).
2. **3 players, host jogador (cenário 3)** — C1 cura C2: conferir no host se nada acontece com ele (G-1: usar med cirúrgico para expor o fallthrough) e se C2 recebe a cura.
3. **Headless + 2 clients (cenário 4)** — mesmo teste; conferir log do headless (`Headless: pacote retransmitido`, zero exceptions).
4. **Teste negativo de deploy** — 1 raid com o mod removido do host: confirmar timeout gracioso no médico e registrar o comportamento (justifica a exigência de deploy nas 3 pontas).

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-12 | Guilherme | Criação — rastreamento estático dos 4 cenários (2 agentes, evidência arquivo:linha). |
| 2026-07-12 | Guilherme | G-1 e G-5 aplicados (+ CR-01-10); G-2/G-3/G-4/G-6 pendentes. |
| 2026-07-12 | Guilherme | Sessão autônoma: G-2 aplicado (efeitos por interface) + CR-01-01/02 (bloqueadores coop) — matriz de riscos reduzida; G-3/G-4/G-6 seguem pendentes. |
| 2026-07-12 | Guilherme | CR-03: células (d)/(f) e relacionados atualizados ao código; requisito de deploy ganhou MESMA BUILD (wire-format dos pacotes mudou). Nota: alcance do prompt agora é config (default 5 m), não os 2,5 m nativos citados no rastreamento original. |
