# Handoff — Recalibração "grafite + chrome neutro" do TRL Design System

> **Data:** 2026-07-04<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme (via sessão design-system)<br>
> **Referências:** [design-system/tokens.css](../../design-system/tokens.css), [design-system/PATTERNS.md](../../design-system/PATTERNS.md)<br>

---

**Para:** sessão do launcher (Avalonia, `launcher/Launcher4.0beta/` — não é editor web).
**O quê:** o TRL Design System foi recalibrado (commits `e29fced`/`76f34ff`, presentes na `feat/launcher-2.0` e na `feat/design-system-trl`). Se o launcher espelha a paleta do DS, ajustar os brushes/estilos XAML para a calibração nova.

## A mudança em uma frase

O gold saiu do **chrome** (fundos, bordas, labels) e virou **accent de significado**; a base agora é **grafite neutro** — o dourado só aparece em: item ativo/selecionado, botão primário, títulos-assinatura, tags e progress. Vermelho segue reservado à marca (laser, dot "live", destrutivo).

## Mapa de cores (antigo oliva → novo grafite)

| Papel | Antes | **Agora** |
|---|---|---|
| Fundo do app (ground) | `#12130D` | **`#131314`** |
| Fundo profundo (inputs/topbar) | `#0D0E09` | **`#0D0D0E`** |
| Painel (surface-1) | `#1B1D14` | **`#1B1B1D`** |
| Card/raised (surface-2) | `#22251A` | **`#222225`** |
| Elevado (surface-3/4) | `#282C1E` / `#2F3424` | **`#29292C` / `#303034`** |
| Texto principal (ink) | `#E9E7DD` | **`#E8E7E4`** |
| Texto secundário (muted) | `#9A978A` | **`#9B9A96`** |
| Texto apagado (faint, decorativo) | `#6F6D60` | **`#706F6B`** |
| Borda padrão | tan translúcido `rgba(199,180,138,.20)` | **neutra `#FFFFFF` 9% alpha** (faint 5%, strong 20%) |
| Moldura tan (só elementos de accent: tag/badge) | — | `rgba(199,180,138,.42)` |
| Accent gold (inalterado) | `#C7B48A` (strong `#D8C9A4`, dim `#8F8560`) | igual |
| Danger/status (inalterados) | red-soft `#D27A7A`, red-500 `#D92C20`, green `#9AD27A`, amber `#CC9A3E` | iguais |
| Vermelho de marca (inalterado) | `#FF0000` — só laser/glow/logo, nunca texto ou fill | igual |

## Regras que valem também no launcher

1. **Labels/chrome nunca dourados** — títulos de seção, labels de campo e cabeçalhos usam o cinza `#9B9A96`; gold é defeito fora de: ativo, selecionado, primário, assinaturas (regra R4 do PATTERNS).
2. **Bordas neutras** (branco ~9% alpha); moldura tan só em chips/badges de accent (R2).
3. **Vermelho `#FF0000` é luz, não pigmento**: linha laser, dot pulsante de estado, logo. Texto de erro = `#D27A7A`; botão destrutivo = `#D92C20`. Mais de ~5% da tela em vermelho = errado (R1).
4. Botões: base neutra (borda + texto cinza), **primário** = gold sólido `#C7B48A` com texto `#131314`.
5. Elevação = superfície mais clara (ladder acima), não só sombra.

## Onde conferir a fonte de verdade

- `design-system/tokens.css` — todos os valores (o launcher traduz para brushes XAML; não há binding automático).
- `design-system/PATTERNS.md` — R1 (vermelho), R2 (bordas), R4 (labels), R5 (contrastes medidos AA).
- `design-system/design-system.html` — showcase visual (servir a pasta e abrir, ou `file://`).
- Contexto extra: fonte display é **Bender** (`design-system/fonts/`), corpo Segoe UI.

## O que fazer nesta sessão do launcher

1. Localizar onde o launcher define paleta (App.axaml / ResourceDictionary / estilos) e mapear os brushes existentes para a tabela acima.
2. Trocar oliva→grafite e bordas tan→neutras; rebaixar labels dourados para `#9B9A96`.
3. Manter gold/red apenas nos papéis listados (ativo/primário/assinaturas; laser/live/destrutivo).
4. Conferir contraste visual após a troca (os pares acima são AA medidos no DS).

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-04 | Guilherme | Criação — handoff da recalibração grafite p/ sessão do launcher |
