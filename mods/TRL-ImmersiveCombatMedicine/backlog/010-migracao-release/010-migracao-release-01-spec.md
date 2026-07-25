# 010 — Migração de configs + release

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Backlog
**Criado:** 2026-07-25

## Visão geral

Item de fechamento do overhaul Trauma 2.0 (último do ciclo 003→010): aposentar de vez os 3 toggles legados que ficaram inertes desde os itens 003/005/006 ("Sistema de Pernas/Braços/Estomago"), remover as sondas de log `[DEBUG-ICM]` que sobraram do diagnóstico do prompt F (Sessão 2, já resolvido), consolidar o `PROPRIEDADES.md` refletindo essa limpeza, decidir e aplicar a distância de interação final de produção (hoje propositalmente alta para facilitar testes), migrar para o padrão i18n EN/PT (decisão 22) os textos do mod que ainda são PT hardcoded fora do Trauma 2.0 (cura Band-Aid, torniquete, ActionPanel nativo, HUD médico), e produzir um zip de release seguindo o padrão já usado por outros mods do repo.

## Comportamento atual

- **Configs legados inertes:** `ConfigLegsEnabled` ("Sistema de Pernas"), `ConfigArmsEnabled` ("Sistema de Braços") e `ConfigStomachEnabled` ("Sistema de Estomago") continuam expostos no F12 (`TRLImmersiveCombatMedicinePlugin.cs:97,99,101`), com tooltip avisando "(INERTE... Remoção da key no item 010.)". Nenhum patch lê `.Value` de nenhuma das 3 fora de uma migração histórica não relacionada (correção de encoding da key "Sistema de Braços", `:371`) — são vestígios puros do sistema legado que os itens 003/005/006 substituíram.
- **Sondas `[DEBUG-ICM]`:** ainda ativas em `TRLImmersiveCombatMedicinePlugin.cs` (heartbeat + campos `_debugHost`/`_debugCtrl`/`_debugNextBeat`), `Patches/Medical/BandAidController.cs` (lifecycle do controller + sweep de `MedicInteractable`, campos `_dbgUpdateAlive`/`_dbgInRaid`) e `Patches/Medical/MedicActionsPatch.cs` (log rate-limited do `GetAvailableActions`, campo `_dbgNextLog`). Foram propositalmente mantidas desde a Sessão 2 (diagnóstico do prompt F, já resolvido) para validar a Parte 2 do roteiro coop (P-2.9) — essa validação já ocorreu; as sondas seguem no código sem função de diagnóstico pendente.
- **`PROPRIEDADES.md`:** já é a fonte única de verdade das `ConfigEntry`, atualizado a cada item entregue — mas a Seção 2 ainda lista as 3 keys legadas como se estivessem "vivas" (só o tooltip avisa que são inertes) e a Seção 5 tem uma promessa em aberto ("migração dos textos antigos é o item 010").
- **Distância de interação:** `Medic Interact Distance` (`TRLImmersiveCombatMedicinePlugin.cs:120-125`) tem default `5` (faixa 1-15) com tooltip literal "Valor alto para testes; reduzir no pacote final" — nunca foi reduzido. O prompt nativo vanilla do jogo (loot/interação padrão) opera a ~2,5 m (`Patches/Medical/MedicActionsPatch.cs:53-57`; confirmado também no `reviews/code-review-01.md` CR-01-03). O mod usa uma regra única `MedicInteractDistance + 1f` para todos os usos (abrir/fechar/aplicar) — hoje 6 m efetivos.
- **i18n:** só `Patches/Trauma/TraumaLocale.cs` (item 002, decisão 22 parcial) implementa o padrão EN-default+PT para os 7 toasts do motor Trauma 2.0. Todo o resto do mod — notificações de cura (`BandAidController.cs`, `BandAidNetworkHandler.cs`), torniquete (`TourniquetManager.cs`), rótulos do ActionPanel nativo (`MedicInteractable.cs`) e textos fixos do HUD médico (`BandAidUI.cs`) — segue 100% em português hardcoded, sem nenhum fallback EN.
- **Release:** o mod não tem nenhum script de empacotamento próprio nem zip prévio em `dist/`. O único precedente no repo é `tools/trl-items-management/scripts/package-release.sh` (mod `TRL-ItemsManagement`), que gera `dist/trl-release-v<versão>.zip` a partir dos artefatos já instalados em `D:\SPT`.

## Comportamento desejado

1. **Configs legados removidos:** as 3 `ConfigEntry` inertes somem do F12 e do código-fonte (`Config.Bind` apagado). Nenhuma migração de valor é necessária — nenhuma delas influencia gameplay hoje. `PROPRIEDADES.md` reflete a remoção (Seção 2 perde as 3 linhas; tabela "Removidas" ganha as 3 entradas).
2. **Sondas `[DEBUG-ICM]` removidas:** todo log com a tag `[DEBUG-ICM]` some do código, junto dos campos que existem exclusivamente para viabilizá-lo. Nenhuma estrutura de controle funcional (ex.: o `try/catch` de `BandAidController.CheckInit()`) é removida — só o texto/chamada de log dentro dela. O toggle "Invisível para Bots" (`Debugging/DebugBotInvisibility.cs`, Seção 5 do `PROPRIEDADES.md`) **não é tocado** — é uma feature de debug legítima que sobrevive ao release, distinta das sondas de diagnóstico do prompt F.
3. **`PROPRIEDADES.md` consolidado:** Seção 2 sem as 3 keys legadas; tabela "Removidas" com as 3 novas entradas (motivo: legado Trauma 2.0 aposentado, sem leitura funcional); a promessa em aberto da Seção 5 ("migração dos textos antigos é o item 010") atualizada para refletir a entrega; `Medic Interact Distance` com o valor final e tooltip sem a nota "para testes"; Histórico de Alterações com a entrada do item 010.
4. **Distância de interação final aplicada:** `Medic Interact Distance` passa do default de teste (5) para um valor de produção definido nesta spec (ver decisão abaixo), com tooltip atualizado (sem a nota "reduzir no pacote final").
5. **i18n EN/PT dos textos legados:** todo texto **visível ao jogador** (notificações, rótulos do ActionPanel, textos fixos do HUD) que hoje é PT hardcoded passa a seguir o mesmo padrão do `TraumaLocale.cs` — tabela própria do plugin, EN default, PT quando o idioma do jogo é português, sem nenhuma injeção de chave no locale do servidor. Logs técnicos (`ModLogger.LogInfo/LogWarning/LogError`) e enums de voz nativos do jogo (`EPhraseTrigger`) ficam **fora de escopo** — não são texto do mod.
6. **Zip de release:** um script próprio do mod (seguindo o padrão de `package-release.sh`) empacota os artefatos instalados (DLL do client) num zip versionado em `dist/`, pronto para distribuição.

### Decisão resolvida nesta spec — distância de interação final

Nenhuma decisão numérica prévia estava registrada em memória/spec/review para o valor final — resolvida aqui com a evidência disponível: o prompt/interação nativa do jogo (loot e afins) opera a ~2,5 m; a cura em coop, ao contrário do loot instantâneo, exige que médico e paciente fiquem parados lado a lado por vários segundos (a barra de progresso da cura não tolera reentrar no range a cada leve movimento) — uma margem levemente acima do vanilla puro evita frustração sem abrir mão de proximidade realista. **Decisão: `3.5` m** (faixa mantida 1–15, default passa de `5` para `3.5`). É um ajuste de 1 número no F12 (`Config.Bind` default) — se o usuário preferir outro valor após validar in-game, é trivial de recalibrar sem tocar em nenhuma outra lógica.

## Inventário de textos a migrar (i18n EN/PT)

Levantamento completo (arquivo:linha + texto atual), player-facing, excluindo logs técnicos (`ModLogger.Log*`) e enums de voz nativos (`EPhraseTrigger`/`.Say`/`.Speaker.Play`) — nenhum dos dois é "texto do mod":

**Notificações (`NotificationManagerClass.DisplayMessageNotification`) — `Patches/Medical/BandAidController.cs`:**
- `:227` — "Abortado!"
- `:244-245` — "Sem resposta do paciente (timeout)."
- `:356-357` — "Verificando {nome}..."
- `:364-365` — "{nome}: Sem ferimento compatível."
- `:445-447` — "Toque no ombro → {nickname}"
- `:539` — "Item dropado!"
- `:576` — "Aplicando {item}..." (nome do item já vem de `.Localized()` nativo — só o texto ao redor migra)
- `:658` — "Tratamento Completo{parte}."
- `:671` — "Tratamento Completo."
- `:676` — "Item perdido durante tratamento."
- `:755` — "Tratamento cancelado."
- `:906` — "MÉDICO: {nickname}"
- `:118-119` — `response.DenyReason` (ver corner case "texto vindo do servidor" abaixo — origem em `BandAidNetworkHandler.cs`)

**Notificações — `Patches/Medical/BandAidNetworkHandler.cs`:**
- `:399-400` — "Você foi tratado por um aliado."
- `:614-615` — "Você recebeu um toque no ombro de {nickname}"
- `:681,686,833` — "Item desconhecido." / "{nome}: Sem ferimento compatível." (geram o `DenyReason` acima)

**Notificações de torniquete — `Patches/Medical/TourniquetManager.cs`:**
- `:67-69` — "Torniquete já aplicado: {parte}"
- `:90-92` — "Torniquete aplicado: {parte}. Remova após parar o sangramento!"
- `:105-107` — "Nenhum torniquete em: {parte}"
- `:116-118` — "Torniquete removido: {parte} ({duração}s). Item devolvido."
- `:173-175` — "Torniquete em {parte}: risco de necrose! Remova agora!"
- `:181-183` — "{parte} destruído por necrose do torniquete!"
- `:210-223` (`GetBodyPartName`) — dicionário de nomes de parte usado pelas notificações acima: "Cabeça"/"Tórax"/"Estômago"/"Braço Esquerdo"/"Braço Direito"/"Perna Esquerda"/"Perna Direita"

**Rótulos do ActionPanel nativo (visíveis toda vez que o jogador mira em outro player/bot vivo) — `Patches/Medical/MedicInteractable.cs`:**
- `:45` — "Examinar (Médico)"
- `:50` — "Tocar no ombro"

**HUD médico (`BandAidUI.cs`, textos fixos desenhados no Canvas):**
- `:310` — título "SITUAÇÃO DO OPERADOR"
- `:368`/`:660` — rodapé "Utilize as suas teclas de atalhos para curar\n[Pressione F / {tecla}] Fechar Examinador"
- `:683-688` (`PartLabelPt`) — dicionário de rótulo curto por membro: "CABEÇA"/"TÓRAX"/"ESTÔMAGO"/"BRAÇO ESQ."/"BRAÇO DIR."/"PERNA ESQ."/"PERNA DIR."
- `:788` — `"INDISPONÍVEL"`

**Fora de escopo (não são texto do mod, não migram):** `DebugBotInvisibility.cs:116-117,124-125` ("DEBUG: invisível/visível para bots") — feature de debug que sobrevive ao release, não faz parte do texto de produção normal; `.ShortName.Localized()` (nomes de item, já localizados pelo jogo nativamente).

## Critérios de aceite

- [ ] `Sistema de Pernas`, `Sistema de Braços` e `Sistema de Estomago` não aparecem mais no menu F12 (Seção 2 "Mecanicas (Trauma)").
- [ ] Nenhuma linha de log contendo a tag `[DEBUG-ICM]` aparece no `LogOutput.log` em nenhum fluxo (boot, raid, menu, sweep de `MedicInteractable`).
- [ ] `PROPRIEDADES.md` reflete exatamente as `ConfigEntry` existentes no F12 pós-limpeza (nenhuma key documentada que não exista, nenhuma key existente não documentada).
- [ ] `Medic Interact Distance` tem o valor de produção definido acima como default, com tooltip sem menção a "testes".
- [ ] Toast/notificação/rótulo de UI listado no mapeamento desta spec aparece em inglês quando o idioma do jogo NÃO é português, e em português quando é — mesmo padrão observável do `TraumaLocale.cs` (troca ao vivo, sem exigir reiniciar o jogo).
- [ ] Um zip de release é gerado em `dist/` com o nome/versão corretos, contendo só os artefatos de distribuição do client (sem configs/logs de usuário).
- [ ] **Fika/multiplayer:** a migração i18n não altera o WIRE FORMAT de nenhum pacote (`BandAidNetworkHandler.cs`) — strings viram chaves resolvidas **no cliente que exibe**, nunca são serializadas já traduzidas; cada peer vê o texto no PRÓPRIO idioma do jogo, independente do idioma de quem originou o evento (ex.: torniquete aplicado por um peer PT deve aparecer em EN pro peer com jogo em inglês).
- [ ] **Estado entre raids:** nenhuma das mudanças deste item introduz estado persistente novo entre raids — remoção de config/log/i18n é 100% estática (F12/`.cfg`/strings), sem contrapartida em runtime que precise resetar.

## Corner cases

- [ ] **Config órfã no `.cfg` do usuário:** após remover os 3 `Config.Bind`, o `.cfg` existente do usuário mantém as 3 linhas antigas (BepInEx ignora keys sem `Bind` correspondente — mesmo comportamento já documentado para o precedente Shoulder Tap, `PROPRIEDADES.md` tabela Removidas). Não deve gerar erro nem warning no boot.
- [ ] **Sonda com campo de suporte dedicado:** vários pontos `[DEBUG-ICM]` têm um campo (`_debugHost`/`_debugCtrl`/`_debugNextBeat`/`_dbgUpdateAlive`/`_dbgInRaid`/`_dbgNextLog`) que existe só para viabilizar aquele log — remover só a chamada de log sem remover o campo correspondente deixa código morto (mesmo padrão de warning `CS0414` já visto no achado histórico CR-01-15 do shoulder-tap). A variável local `attached` do sweep em `BandAidController.cs` é usada tanto pelo `for` quanto pelo log — conferir se sobra em uso após a remoção do log (senão vira `CS0219`).
- [ ] **`try/catch` funcional colado a uma sonda:** `BandAidController.CheckInit()` tem um `catch` cujo **texto** é `[DEBUG-ICM]`, mas a estrutura `try/catch` em si é funcional (comentário no código explica que sem ela uma exceção mataria o `Update()` todo frame) — remover a sonda não pode remover o guard.
- [ ] **Texto vindo do servidor/handshake vs. texto local:** algumas notificações (`DenyReason` em `BandAidNetworkHandler.cs`) são geradas do lado do paciente e trafegam como string já pronta até o médico. Migrar essas para i18n exige decidir ONDE a tradução acontece (no paciente que gera, ou no médico que exibe) — dado o critério Fika acima, a tradução deve acontecer no ponto de EXIBIÇÃO (quem vê o toast), não de geração; o pacote deve carregar um identificador/enum, não a string finalizada. Se algum pacote hoje trafega string finalizada, isso é mudança de wire format e precisa ser marcado explicitamente na spec técnica (não silenciosamente).
- [ ] **Idioma trocado mid-raid:** mesma regra do `TraumaLocale.IsGamePortuguese()` — nunca cachear o idioma no `Awake()`/spawn; ler no momento de exibir. Corner já resolvido pelo item 002, só precisa ser REUSADO (não uma nova implementação) para os textos legados.
- [ ] **Distância de interação e o toque no ombro:** `Emergency Drop`/torniquete e outras ações que hoje herdam a mesma distância (`MedicInteractDistance + 1f`, regra única) precisam continuar coerentes após o valor mudar — não pode sobrar um caminho de código com a distância antiga hardcoded em paralelo.
- [ ] **Zip de release rodando com working tree sujo:** seguir o precedente do `package-release.sh` (avisa e não empacota silenciosamente se houver mudança não commitada) — evita releasar um build que não corresponde a nenhum commit rastreável.

## Fora de escopo

- [ ] Extrair o helper `TryFindOrphan` para o `MigrateOrphanedConfigKeys()` (achado deferido `CR-01-01` do item 008) — **não é necessário aqui**: a remoção das 3 keys legadas segue o precedente Shoulder Tap (remoção simples, sem migração de valor), não adiciona uma 7ª/8ª/9ª cópia do bloco de resgate de órfã. Esse refactor fica sem gatilho natural deste item; se quiser, vira um item 012 separado no futuro.
- [ ] Reduzir a distância de TODAS as interações do mod para exatamente o valor vanilla (2,5 m) — a proposta desta spec é um valor levemente acima, com justificativa registrada; não é uma tentativa de igualar 1:1 o vanilla.
- [ ] Publicar o zip em algum canal de distribuição (Discord, GitHub Releases) — esta spec cobre só a GERAÇÃO do artefato local em `dist/`.
- [ ] Mudar o padrão de `package-release.sh` do `TRL-ItemsManagement` (mod diferente) — esta spec só usa esse script como referência de padrão, criando um script próprio equivalente para o ICM.
- [ ] Migrar os textos de `Debugging/DebugBotInvisibility.cs` para i18n — é feature de DEBUG que sobrevive ao release (prefixo "DEBUG:" já no próprio texto), não faz parte do polimento de produção que esta spec cobre.

## Referências

- [PROPRIEDADES.md](../../PROPRIEDADES.md)
- [docs/trauma-behavior-matrix.md](../../docs/trauma-behavior-matrix.md) (item 011 — decisão 22 documentada)
- [Patches/Trauma/TraumaLocale.cs](../../modded/Patches/Trauma/TraumaLocale.cs) (padrão i18n a reusar)
- [008-desmaio-duracao-aleatoria-04-code-review-01.md](../008-desmaio-duracao-aleatoria/008-desmaio-duracao-aleatoria-04-code-review-01.md) (CR-01-01, contexto do achado deferido sobre `MigrateOrphanedConfigKeys`)

## Histórico

| Data | Evento |
|---|---|
| 2026-07-25 | Item criado via `/create-spec`, escopo mapeado por pesquisa dedicada (6 sub-itens: configs legados, sondas DEBUG-ICM, PROPRIEDADES.md, distância final, i18n EN/PT, zip de release). |
| 2026-07-25 | Revisão `/review-spec` — decisão de distância de interação (3,5 m) resolvida com evidência (era `<!-- review: -->` aberto); adicionado inventário completo dos ~25 pontos de texto a migrar para i18n (antes só referenciado em prosa); adicionado corner case de `CS0219` na variável `attached` e exclusão explícita de `DebugBotInvisibility.cs` do escopo i18n. |
