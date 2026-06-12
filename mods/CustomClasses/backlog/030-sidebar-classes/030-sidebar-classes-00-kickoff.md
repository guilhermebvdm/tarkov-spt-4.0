# 030 — Sidebar persistente de classes · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-10 · **Origem:** comparação de UX com o viewer de perfis do RZ (`tools/tarkov-itemdb/viewer/profiles.html`)
**Épico:** UX do editor (030–037) · **Wave:** UX-W1 (paralelo ao 031) · **Deps:** 037 (cache de entries — o `ListClassSummaries` é uma view do cache, não uma segunda implementação)

> Brief de kickoff — insumo para `/create-spec 030`. Não é a spec.

## Problema (UX)

No viewer antigo, trocar de classe = **1 clique** num sidebar sempre visível (220px, nome + custo, estado ativo destacado). No editor atual, trocar = voltar pra lista (`/customclasses/classes`) + clicar a linha (**2+ cliques e perda de contexto**); o drawer atual só tem Home/Classes.

## Escopo

- **NavMenu/drawer vira sidebar de classes:** lista TODAS as classes (ícone tingido + nome na `nameColor` + custo de skills compacto), 1 clique → navega pra MESMA vista da classe atual (se está num detalhe, vai pro detalhe da outra; se está em `/edit`, vai pro `/edit` da outra — preservar a "vista corrente"). Item ativo com destaque (strip lateral + fundo, padrão do viewer antigo: `profiles.css:21-60`).
- **Filtro + status na sidebar (review #10):** campo de filtro por nome no topo (classes criadas pelo editor vão crescer) e **dot de status** por classe — vermelho = inválida, cinza = disabled, laranja = custo fora do budget 28–32. A sidebar vira painel de saúde das classes com 0 cliques.
- **Guard de unsaved changes (review #5 — CRÍTICO):** a troca em 1 clique a partir do `/edit` com formulário sujo NÃO pode descartar edição em silêncio. Dirty-flag no `ClassEditModel` + dialog "Save / Discard / Cancel" ao navegar com mudanças pendentes (o "opcional" registrado no 025 deixa de ser opcional com o sidebar).
- **Fallback de vista (review #6):** `edit→edit` para classe **inválida** (parse error) não tem o que editar → cair no detail com diagnostics.
- Links utilitários (Home, Skills matrix do 032 quando existir, New class) ficam no topo/rodapé do mesmo drawer.
- **Performance:** resolvida pelo **037** (cache de entries por arquivo, invalidado em Save/Delete/Create + mtime). `ListClassSummaries()` aqui é só uma projeção leve do cache (name/displayName/nameColor/iconFile/enabled/status/custo) — NÃO reimplementar parse nem dry-run.
- Drawer responsivo (colapsa pra ícones em tela estreita — padrão do viewer antigo).

## Refs

- `tools/tarkov-itemdb/viewer/profiles.js:69-91` (renderSidebar/selectProfile — referência de UX)
- `Web/Shared/NavMenu.razor`, `Web/Layouts/BaseLayout.razor` (031 NÃO toca estes arquivos — território seu)
- `ClassEditorService.cs` (summaries/cache), `CostService.cs`

## DoD (resumo)

- Trocar de classe a partir de qualquer página = **1 clique**, preservando a vista (detail→detail, edit→edit; classe inválida cai no detail).
- Sidebar mostra ícone+cor+custo+dot de status; filtro por nome; ativa destacada; sem lag perceptível (sem dry-run por render).
- Sair do edit com mudanças pendentes SEMPRE pergunta (Save/Discard/Cancel) — nunca descarta em silêncio.
