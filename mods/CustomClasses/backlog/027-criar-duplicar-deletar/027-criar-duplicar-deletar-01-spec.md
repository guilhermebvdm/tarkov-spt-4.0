# 027 — Criar / duplicar / deletar classe — Spec

**Mod:** CustomClasses
**Status:** Implementado (build integrado + smoke pelo orquestrador pendentes — ver as-built)
**Criado:** 2026-06-10
**Origem:** [027-criar-duplicar-deletar-00-kickoff.md](./027-criar-duplicar-deletar-00-kickoff.md)

## Visão geral

Ciclo de vida completo de classes pelo editor web (Etapa 10 do plano): **criar** (template mínimo → abre direto no form do 025), **duplicar** (caminho oficial de "rename" — o 025 bloqueia rename in-place porque órfã perfis existentes) e **deletar/desabilitar** (com aviso listando os perfis reais que usam a edition). Tudo com hot-apply: criar/duplicar aparecem no launcher sem restart; deletar/desabilitar somem na hora.

## Comportamento desejado

- **Criar** (botão "New class" na toolbar da lista): dialog com input de nome + validação ao vivo (vazio; colisão com QUALQUER edition existente — vanilla, ocultas pelo 009 e classes do mod, incluindo classes em arquivo desabilitadas/não registradas; **case-insensitive**, mais estrito que o loader de propósito). Aviso fixo no dialog: classe nova **nasce sem ícone** → launcher/UI degradam pra texto colorido (edição de imagem é fase futura). Template mínimo válido: `name`, `baseEdition` explícito = default `"SPT Zero to hero"`, `enabled: true`, `displayName`/`description` `{en,pt}` = nome. Nome do arquivo: slug ASCII do nome (`caçador & cia` → `cacador-cia.jsonc`), sufixo `-2`, `-3`… em colisão. Sucesso → navega para `/customclasses/classes/{novo}/edit`.
- **Duplicar** (ícone por linha na lista + botão no detalhe): dialog pede o novo nome (mesma validação). Copia o conteúdo do arquivo **verbatim** (skills, multipliers, loadout, outfit, hideout, description, iconFile, nameColor, enabled) trocando só `name` e `displayName` (= novo nome, en+pt). Arquivo novo independente (slug próprio). Sucesso → navega pro detalhe da cópia.
- **Deletar** (ícone por linha + botão no detalhe): dialog de confirmação que **varre `user/profiles/*.json`** e lista os perfis (`username (arquivo.json)`) criados com essa edition; se houver, aviso forte: "esses perfis continuam funcionando, mas ficam **sem identidade e sem multiplicadores de skill**". Três saídas:
  - **Delete file** — `Delete(hotRemove:true)`: backup `.bak1` + remove arquivo + hot-remove da edition.
  - **Disable instead** — `enabled:false` via `Save(hotApply:true)` (hot-remove da edition, arquivo preservado); só aparece para classe parseável e atualmente habilitada.
  - **Cancel.**
- Arquivo **inválido** (parse error): Duplicate desabilitado (não há o que copiar); Delete funciona (remoção só do arquivo, aviso de que o uso por perfis não pôde ser checado).

## Critérios de aceite

- [x] Validação de nome bloqueia: vazio, colisão com edition vanilla, colisão com classe do mod (registrada OU só em arquivo), case-insensitive. Re-validação autoritativa no serviço antes de escrever (validação live é só UX).
- [x] Criar/duplicar passam pelo MESMO pipeline de validação do boot (`Save` → `ValidateAndBuild` dry-run); Error bloqueia o write.
- [x] Slug de arquivo seguro (sem chars inválidos, sem colisão `.json`/`.jsonc`, fold de acentos).
- [x] Delete-dialog lista perfis reais lendo `info.edition` (evidência no spec-tech); arquivo de perfil corrompido é pulado sem quebrar a varredura.
- [ ] Criar → aparece no launcher **sem restart**; deletar → some (DoD do kickoff — validar no smoke integrado).
- [ ] Duplicar gera `.jsonc` válido e independente do original (validar no smoke).

## Fora de escopo

- Upload/edição de ícone PNG (fase futura — kickoff).
- Rename in-place (decisão do 025: duplicar é o caminho).
- Migração de perfis existentes para outra edition.
