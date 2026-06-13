# 025 — Edição de campos simples + outfit — Spec

**Mod:** CustomClasses
**Status:** Implementado (save-roundtrip no browser pendente — ver as-built)
**Criado:** 2026-06-10
**Origem:** [025-edit-campos-simples-00-kickoff.md](./025-edit-campos-simples-00-kickoff.md)

## Visão geral

Primeira edição persistida — fecha o MVP do editor web (W0–W3). Página `/customclasses/classes/{file}/edit` (nome sem extensão na rota, mesma resolução do detalhe) com **shell de abas** que os itens 026/028 estendem: Geral | Skills | Multipliers | Hideout | Outfit | Equipped (placeholder 026) | Stash (placeholder 028). Save via `ClassEditorService.Save` (validação boot-equivalente → backup `.bak` rotativo → write → hot-apply).

## Comportamento desejado

- **Geral:** `name` READ-ONLY (disabled + tooltip apontando duplicação no item 027 — rename órfã perfis existentes); displayName e description en/pt; `nameColor` hex `#RRGGBB` com swatch + validação; `enabled` (switch — salvar desabilitado hot-remove a edition); `baseEdition` (dropdown só de editions vanilla — classes do mod excluídas via `ClassVisualRegistry`); `iconFile` (dropdown dos PNGs de `wwwroot/icons/` do install, preview via `/CustomClasses-Server/icons/`, opção "(none)" com aviso de degradação pra texto).
- **Skills:** tabela editável skill→nível (0–51), peso/origem/custo por linha ao vivo (CostService do estado atual do form), add (select de TODOS os SkillTypes não usados, alfabético) / remove. Total + chip de budget [28, 32] + warnings.
- **Multipliers:** tabela editável skill→fator (double ≥0, step 0.1), cor verde >1 / vermelho <1, badge "Skills-Extended" nas 4 skills do SE, MudAlert se SE ausente. Add/remove.
- **Hideout:** estação (enum `HideoutAreas`, sem `NotSet`) → nível ≥1. Add/remove.
- **Outfit:** 4 seções (USEC/BEAR × Upper/Lower) com `CustomizationPicker` (023), nome atual resolvido + botão limpar (= default do template).
- **Custo ao vivo:** toolbar sticky com total ponderado de skills (recalcula on-change) + total ₽ do loadout (read-only neste item).
- **Save:** botão fixo; sucesso → snackbar verde + banner com os limites do hot-apply (perfis NOVOS no launcher imediatamente; jogo aberto não vê identidade/multiplicadores novos; perfis existentes não mudam); falha → diagnostics (Code+Message) em MudAlert, NADA salvo. **Discard** recarrega do arquivo.
- **Navegação:** botão "Edit" no header do detalhe (ClassDetail).

## Critérios de aceite

- [x] Save passa pelo MESMO pipeline de validação do boot (`ValidateAndBuild` dry-run via `ClassEditorService.Save`); Error bloqueia o write.
- [x] Rename bloqueado (campo disabled + tooltip).
- [x] Custo de skills recalcula a cada mudança de nível/linha; warnings de budget visíveis.
- [x] Equipped/Stash preservados intactos no save (pass-through do `Loadout`).
- [ ] Editar e salvar reflete em perfil novo sem reiniciar o server (validar no browser — DoD do kickoff).

## Fora de escopo

- Edição de equipped (026), criar/duplicar/deletar (027), stash (028).
- Aviso de unsaved changes ao navegar (opcional no kickoff — não implementado).
