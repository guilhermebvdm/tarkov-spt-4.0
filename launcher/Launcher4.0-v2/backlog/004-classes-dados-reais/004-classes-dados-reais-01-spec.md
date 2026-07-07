# 004 — Tela de classes: dados reais · Spec funcional

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Kickoff:** [004-classes-dados-reais-00-kickoff.md](./004-classes-dados-reais-00-kickoff.md)

> Escopo: consumir `GET /customclasses/classes` (item 058 do CustomClasses, contrato SP0 congelado) na tela de seleção de classe, com fallback vanilla e restyle TRL. Inclui o fix D1 do item 005 (senha do registro descartada).

## Critérios de aceite

1. **Lista real.** A tela lista exatamente as classes servidas por `GET /customclasses/classes` (só `Enabled && Registered`), na ordem do array. Nada de mock/classe fantasma. Nome exibido = `displayName.pt` (fallback `en` → `editionKey`).
2. **Detalhe.** Painel direito mostra o nome da classe selecionada e a `description.pt` (fallback `en` → `profileDescriptions[editionKey]` vanilla → vazio), com wrap. Seções Vantagens/Desvantagens/Habilidades e o painel "[Imagem do Personagem]" **saem da tela** (decisão do usuário no kickoff).
3. **Ícone.** Ícone pequeno (24px) na lista quando `iconUrl` presente; download via infra de request do launcher com cache em disco; ausência/erro de download → item sem ícone, sem crash.
4. **Cor do nome.** `nameColor` presente e válido → nome na lista com essa cor; ausente/inválido → foreground padrão do `trl-nav`.
5. **Seleção default** = primeira classe da lista (não mais índice mágico `[3]`).
6. **Registro correto.** "Escolher classe" envia `editionKey` EXATO como edition no `POST /launcher/profile/register` (não o displayName).
7. **Fix D1 (item 005).** Após registro OK, o launcher chama a troca de senha (`/redline/password/change`) com a senha digitada no registro, antes do auto-login. Falha na troca → warning + notificação, fluxo segue (usuário cai no dialog de senha no próximo login manual — comportamento pré-fix).
8. **Fallback obrigatório.** Rota indisponível/erro/JSON inválido/array vazio → lista montada de `SelectedServer.editions[]` + `profileDescriptions{}` (sem ícone/cor), Warning no log, sem crash.
9. **Restyle TRL.** Fundo `bg-hero.jpg` + `TrlPhotoOverlayBrush`; painéis `TrlPanelOverPhotoBrush`; lista `trl-nav`; painel de detalhe `TrlPanel` + `TrlScreenBar`; botões `.primary`/`.ghost`; erro de registro em `TrlDangerBrush`; footer hardcoded → `TrlVersionFooter` (defaults; 013L liga o dado). Zero hex novo hardcoded — só `{DynamicResource Trl*}`.
10. **Sem regressão de fluxo.** Commands e navegação (Voltar → Register; sucesso → auto-login → Profile; falha login → Login) inalterados.

## Corner cases

| Caso | Comportamento esperado |
|---|---|
| Server sem o mod CustomClasses (404/erro na rota) | Fallback vanilla (AC-8), tela funcional |
| Rota responde array vazio | Fallback vanilla (AC-8) |
| Resposta zlib (default SPT) | Descomprimida pela infra `Request.GetJson` existente — nunca `HttpClient` cru |
| `editionKey` duplicada no array (defesa P-058.4) | Primeira ocorrência vence; duplicata ignorada com Warning |
| Item sem `displayName`/`description` (nulls omitidos) | Fallbacks do AC-1/AC-2; nunca `NullReferenceException` |
| `iconUrl` aponta para arquivo inexistente | Item sem ícone; log; sem crash |
| `nameColor` malformado (ex. "xyz") | Foreground padrão |
| `SelectedServer == null` E rota falhou | Lista vazia + mensagem de erro na tela; botão não faz nada (guard `SelectedClass == null`) |
| Load lento (Tailscale) | UI não bloqueia: load async pós-ativação, indicador "Carregando classes..." |
| Duplo clique em "Escolher classe" | `ReactiveCommand` já serializa execução (não reentra) |
| Troca de senha pós-registro falha (D1) | Notificação + segue auto-login; conta fica "sem senha" como hoje |

## Fora de escopo

- Render de skills/multiplicadores (ficam no DTO para uso futuro).
- Painel de arte grande.
- Versões dinâmicas no footer (item 013L).
- Validação E2E com server real (gate humano — P-058.1).
