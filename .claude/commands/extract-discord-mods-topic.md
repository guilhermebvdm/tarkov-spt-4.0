# /extract-discord-mods-topic

Captura e analisa um **thread do Discord sobre um mod** (de qualquer mod, não só ORBIT): faz a transcrição fiel completa, baixa e analisa as imagens/logs, cruza com fontes públicas e gera uma análise estruturada — tudo em `docs/discord-mods-topics/<slug>/`.

> **Pré-requisito:** o **Chrome MCP** precisa estar com uma sessão **autenticada no Discord** (o thread fica atrás de login). Se não estiver logado, o command pausa e pede para você logar na janela do Chrome controlado.
> **Objetivo da análise:** permitir **entrar na discussão já entendendo tudo** — mesmo padrão e profundidade da análise do ORBIT em `docs/discord-mods-topics/orbit/` (use-a como referência de qualidade).

## Uso

```
/extract-discord-mods-topic <discord-url> [<slug>]
```

- `<discord-url>` — link do thread/canal: `https://discord.com/channels/<guildId>/<channelId>`.
- `<slug>` — nome curto kebab-case do mod (ex.: `orbit`). Opcional: se omitido, derive do título do thread / página do Forge e **confirme com o usuário** antes de criar a pasta.

## O que fazer

### 0. Escopo e estrutura

1. Faça o parse da URL → `guildId` e `channelId/threadId`.
2. Defina o `slug` (arg, ou derive do título do thread/Forge). Se ambíguo, use `AskUserQuestion`.
3. Crie a pasta `docs/discord-mods-topics/<slug>/assets/` (Bash `mkdir -p`).
4. Padrão de saída (igual ORBIT): `01-transcricao.md` (bruto, idioma original) + `02-analise.md` (PT) + `README.md` + `assets/`.

### 1. Acessar o Discord (Chrome MCP)

1. Carregue as tools deferred via `ToolSearch`: `mcp__chrome-devtools__navigate_page`, `take_snapshot`, `take_screenshot`, `evaluate_script`, `press_key`, `list_pages`.
2. `navigate_page` → a URL. Rode um `evaluate_script` curto para detectar estado: se houver `input[name="email"]`/`type="password"`, ou o título for só "Discord", é **login wall** → **instrua o usuário a logar na janela do Chrome e pause o turno**; ao retomar, confirme autenticado por snapshot.
3. **Probe**: `evaluate_script` para confirmar os seletores reais e medir o tamanho do thread (autor/data da 1ª msg visível, nº aproximado, participantes). Reporte o tamanho antes de capturar tudo.

### 2. Capturar o thread (DOM virtualizado)

A lista do Discord é **virtualizada** (só ~50–100 msgs no DOM por vez). Instale um acumulador em `window.__cap` e faça **seek até o topo** + **down-sweep contíguo** até o fundo, com **dedupe por ID de mensagem**.

> ⚠️ **Seletores corretos (as duas maiores armadilhas de fidelidade — aprendidas no ORBIT):**
> - **Autor:** `li.querySelector('h3 [class*="username"]')` — **NÃO** o primeiro `[class*="username"]` (em respostas, o primeiro é o autor do *reply-context*, não quem escreveu). Para mensagens **agrupadas** (sem header), herde o autor da anterior em ordem cronológica.
> - **Texto:** `li.querySelector('#message-content-' + snowflake)` (id exato da própria msg) — **NÃO** um `querySelector` genérico de `[id^="message-content-"]`, porque o **reply-preview compartilha** `message-content-<id-citado>` e vaza o texto citado.
> - **replyUser:** `li.querySelector('[id^="message-reply-context-"] [class*="username"]')`.

Harvester de referência:

```js
() => {
  window.__cap = window.__cap || {};
  window.__scroller = Array.from(document.querySelectorAll('div[class*="scroller"]'))
    .find(s => s.querySelector('li[id^="chat-messages-"]'));
  window.__harvest = function () {
    let added = 0;
    for (const li of document.querySelectorAll('li[id^="chat-messages-"]')) {
      const id = li.id, snow = id.split('-').pop();
      const text = (li.querySelector('#message-content-' + snow) || {}).innerText || '';
      const author = li.querySelector('h3 [class*="username"]')?.innerText || '';
      const ts = li.querySelector('time[datetime]')?.getAttribute('datetime') || '';
      const replyUser = li.querySelector('[id^="message-reply-context-"] [class*="username"]')?.innerText || '';
      const imgs = [...li.querySelectorAll('img')].map(i => i.src).filter(s => /\/attachments\//.test(s));
      const aLinks = [...li.querySelectorAll('a[href]')].map(a => a.href).filter(h => /\/attachments\//.test(h));
      const prev = window.__cap[id];
      if (!prev) { window.__cap[id] = { id, snow, author, ts, text, replyUser, imgs, aLinks }; added++; }
      else {
        if ((text || '').length >= (prev.text || '').length) prev.text = text;
        if (author && !prev.author) prev.author = author;
        if (imgs.length > prev.imgs.length) prev.imgs = imgs;
        if (aLinks.length > prev.aLinks.length) prev.aLinks = aLinks;
        if (replyUser && !prev.replyUser) prev.replyUser = replyUser;
      }
    }
    return added;
  };
  window.__harvest();
  window.__scroller.scrollTop = 0;
  return Object.keys(window.__cap).length;
}
```

Procedimento:
1. **Seek topo:** repita `__harvest()` + `scroller.scrollTop = 0` (com pequenos `await sleep(~420ms)` num loop assíncrono dentro de um único `evaluate_script` para economizar round-trips) até o **menor snowflake estabilizar** (chegou no post-raiz; geralmente `snowflake ≈ threadId`).
2. **Down-sweep contíguo (autoritativo):** do topo, repita `__harvest()` + `scrollTop += clientHeight*0.6` até o fundo (`scrollTop+clientHeight >= scrollHeight`). Faça em lotes assíncronos.
3. **Verifique a completude:** `count` deve ficar **estável** durante o down-sweep (sem buracos), a 1ª msg = post-raiz e a última = a mais recente.
4. **Exporte** `assets/_capture.json` ordenado por snowflake, **herdando autor** para msgs agrupadas (use o `filePath` do `evaluate_script`).

### 3. Anexos (imagens + logs)

1. Dedupe por **attachment id** (`/attachments/<channel>/<attId>/...`); prefira a URL `cdn.discordapp.com` (full-res) à `media.discordapp.net` (redimensionada). Gere `assets/_manifest.json` com nomes `att-NN-<data>-<autor>.<ext>`.
2. **Baixe** para `assets/` via Bash `Invoke-WebRequest` com **User-Agent de browser**. URLs são **assinadas e expiram** → se falhar, fallback `take_screenshot` do elemento `<img>`.
3. **Analise**: `Read` em cada imagem para descrever (prints de config, gráficos, comportamento). Para `.log`/`.txt`, use `Grep` por arquitetura/erros (load order, GUIDs, exceptions). _(Nota: `*.log` está no `.gitignore` do repo → logs não versionam; extraia o conteúdo-chave para a análise.)_

### 4. Timestamps e transcrição

1. **Derive o horário do próprio snowflake** (confiável; o `time` do DOM erra em replies): `ms = Number((BigInt(snow) >> 22n) + 1420070400000n)`; converta para **GMT-3**.
2. Gere `01-transcricao.md` com um **script Node** (`assets/_gen-transcript.js`, espelhe o do ORBIT): frontmatter, cabeçalho de metadados (link, IDs, datas, nº de msgs, data da captura), `## DD/MM/YYYY` por dia, por mensagem `**autor** · \`HH:MM\` ↳@reply — texto`, anexos como linha `📎 [arquivo](./assets/...)`, e **limpeza** de artefatos (`(editado)` + tooltips de data). Rode com `node`.

### 5. Contexto público cruzado

`WebFetch`/`WebSearch`: página do mod no **Forge**, **GitHub/README**, **dependências** e mods relacionados/comparáveis. Para detalhes de servidor SPT 4.0, a fonte canônica é `github.com/sp-tarkov/server-csharp` (via `gh api`).

### 6. Escrever `02-analise.md` (PT) — mesmo objetivo do ORBIT

Estrutura-padrão (adapte os títulos ao mod; nem toda seção se aplica a todo mod):

- **0. TL;DR** · **1. O que é** (tabela: nome/autor/versão/licença/repo/GUID) · **2. Quem é quem** (papéis de cada participante + nº de msgs) · **3. Etapa/linha do tempo** · **4. Como funciona / interação com a(s) dependência(s)-chave** · **5. Config/preset recomendado** · **6. Arquitetura e dependências** · **7. Diferencial técnico** · **8. Bugs reportados e respostas do dev** (tabela) · **9. Performance** · **10. Compatibilidade** (categorias de mod, o que combina/conflita) · **11. Roadmap** · **12. Pontos legais e curiosos** · **13. Glossário** · **14. Cheat-sheet** (como entrar na discussão sabendo de tudo) · **Fontes**.
- **Princípios:** fidelidade > paráfrase; **marque `_(inferência)_`** quando não for literal do chat; ancore afirmações na transcrição/fontes; idioma original na transcrição, **PT na análise**.

### 7. README, índice e validação

1. `README.md` da pasta (**sem frontmatter**): fonte (link + período + nº msgs + data captura), conteúdo (tabela dos arquivos), "como foi capturado", resumo de 1 linha.
2. Atualize `docs/README.md`: adicione a pasta à *Estrutura* (se ainda não existir) e os novos docs à tabela *Status atual*.
3. Valide headers: `bash .agents/hooks/validate-doc-header.sh docs/discord-mods-topics/<slug>/01-transcricao.md` (e `02-`).

### 8. Confirmar ao usuário

Informe: pasta criada, **nº de mensagens** (e período), **nº de imagens/logs** baixados, validação de headers, pendências/perguntas em aberto, e **avise sobre o peso de `assets/`** (imagens podem somar dezenas de MB) — ofereça `.gitignore` se desejado. **Não commitar** sem o usuário pedir.

## Regras / lições aprendidas

- **Convenções de doc** (`.agents/conventions.md`): frontmatter YAML obrigatório (`title`/`date`/`status`/`authors`) em `01-`/`02-`; **README de pasta sem frontmatter**; **NÃO** editar a seção `## Histórico` (o git pre-commit hook a gera). Nomeie com prefixo `NN-` para ordenar (`01-transcricao`, `02-analise`).
- **Fidelidade:** os dois bugs mais comuns são **autor** (use `h3 .username`) e **texto** (use `#message-content-<snowflake>` exato). Dedupe por **message id**. Timestamp por **snowflake**, não pelo `time` do DOM.
- **Completude:** sempre verifique 1ª msg = post-raiz, última = mais recente, e `count` estável no down-sweep (sem buracos). Escreva a transcrição **em lotes/incremental** se o thread for grande.
- **Encoding:** nunca use PowerShell `Add-Content`/`Set-Content` em `.md` (corrompe UTF-8/emojis). Use `Write`/`Edit` ou Node `fs.writeFileSync(..., 'utf8')`.
- **Acesso:** uso somente de **leitura** de uma sessão que o próprio usuário autenticou (sem envio de mensagens / self-bot). Se o login não rolar, fallback: o usuário exporta o texto + salva imagens em `assets/` e você segue da etapa 3.
- **Reutilização:** mantenha os arquivos de trabalho (`_capture.json`, `_manifest.json`, `_gen-transcript.js`) em `assets/` para permitir regenerar a transcrição.
