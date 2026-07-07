# 018 — Segurança do auto-update (assinatura + cert pinning) · Spec funcional

> **Data:** 2026-07-04<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [00-kickoff](./018-auto-update-security-00-kickoff.md) · [AUDIT-2026-07-04](../../AUDIT-2026-07-04-code-product-ds.md) §B1<br>

---

## Objetivo

Fechar o vetor de **RCE** (execução remota de código) do auto-update do launcher. Hoje o fluxo baixa `SPT.Launcher_Update.exe` de um servidor com **TLS desligado** (`ServerCertificateCustomValidationCallback => true`) e **executa sem verificar quem assinou/hash** — um `.bat` substitui o binário e reinicia. Depois desta entrega, **nenhum executável não-verificado pode ser promovido/executado**: o launcher só troca o próprio binário se a assinatura conferir contra uma chave pública embutida, e a conexão TLS deixa de aceitar qualquer certificado.

## Modelo de ameaça (recap concreto)

1. **MITM / DNS / Gist comprometido.** A `Server.Url` é semeada por um Gist público (`ConnectServerViewModel.cs:54`, dono `rockettechnology-dev`). Quem controlar o Gist, o DNS ou a rede redireciona `Server.Url` para um servidor atacante. Como o cert não é validado (`LauncherUpdateHelper.cs:22,76`), nem cert válido é preciso.
2. **`/redline/launcher/version`** responde versão maior → **`/redline/launcher/download`** serve um exe arbitrário → launcher baixa, o `.bat` renomeia por cima e roda → **RCE na máquina do jogador, em todo login**.

O ponto que realmente para o RCE é a **verificação de assinatura do exe**: mesmo com transporte 100% comprometido, o atacante não forja assinatura sem a chave privada. O cert pinning é defesa-em-profundidade (o transporte hoje pode passar por Tailscale/WireGuard, mas o fallback `LauncherSettingsProvider.cs:104,372` é IP público `147.15.29.24`, logo não dá pra confiar só no transporte).

## Critérios de aceite (testáveis — Given/When/Then)

### CA-1 — Assinatura válida: atualiza
- **Given** o servidor publica um exe novo (versão > atual) **com** assinatura válida para a chave pública embutida no launcher
- **When** o launcher roda a verificação de update no connect
- **Then** ele baixa para uma área de quarentena, recomputa o hash, a assinatura confere, promove o binário e reinicia na versão nova.

### CA-2 — Assinatura inválida/adulterada: fail-closed
- **Given** o exe servido foi adulterado (1 byte trocado) **ou** a assinatura não bate com a chave pública
- **When** a verificação roda
- **Then** o update é **abortado**: o `.bat` **nunca** é escrito, `Process.Start`/`Environment.Exit` **não** ocorrem, o arquivo de quarentena é apagado, um erro é logado, uma mensagem não-bloqueante aparece na tela de conexão e o launcher **segue rodando a versão atual** (jogo continua jogável).

### CA-3 — Servidor sem assinatura publicada: fail-closed
- **Given** o servidor anuncia versão nova mas **não** expõe assinatura (server antigo / `.sig` ausente)
- **When** a verificação roda
- **Then** o update é abortado (mesma UX do CA-2). **Nunca** cai em execução de exe não-assinado.

### CA-4 — TLS: certificado não confiável é rejeitado
- **Given** o endpoint de version/download apresenta um certificado cujo pin (SPKI SHA-256) **não** consta na lista embutida
- **When** o launcher tenta buscar version ou baixar
- **Then** a conexão falha, o update é abortado (fail-closed) e o callback `=> true` **não existe mais** no código.

### CA-5 — Sem update disponível: no-op
- **Given** a versão do servidor é ≤ a atual
- **When** a verificação roda
- **Then** nenhum download acontece e o fluxo de login segue normal (comportamento atual preservado).

### CA-6 — Hash recomputado é a autoridade
- **Given** o servidor envia um `sha256` no metadata que **não** corresponde aos bytes baixados
- **When** a verificação roda
- **Then** vale o hash **recomputado localmente** sobre os bytes em disco (não o hash anunciado); divergência → fail-closed. O exe verificado é **exatamente** o que o `.bat` vai renomear (sem janela TOCTOU entre verificar e promover).

### CA-7 — `DisableUpdates` respeitado
- **Given** `DisableUpdates = true` (`LauncherSettingsProvider.cs:320`)
- **When** o connect roda
- **Then** toda a verificação é pulada (comportamento atual em `ConnectServerViewModel.cs:102` preservado) — sem regressão.

### CA-8 — Comparação de versão robusta
- **Given** strings de versão malformadas (vazio, `"1.0"`, `"2.1.0+hash"`)
- **When** `IsNewerVersion` avalia
- **Then** entrada não-parseável → trata como "não é mais novo" (fail-safe, não dispara update), sem exceção não tratada.

## Regras de negócio

- **RN-1 — Fail-closed é a regra.** Qualquer dúvida (sem assinatura, assinatura inválida, hash divergente, pin errado, download parcial, IO) = **não atualiza e não executa nada**. Falha de update **nunca** bloqueia o jogo — degradação graciosa mantendo a versão atual.
- **RN-2 — A chave pública é embutida no binário do launcher** (compile-time), não baixada. Comprometer o servidor não muda a chave que o cliente usa para verificar.
- **RN-3 — Quarentena antes de promover.** Download vai para um arquivo temporário; só após assinatura OK é movido para `SPT.Launcher_Update.exe`. Antes disso o binário final e o `.bat` não existem.
- **RN-4 — Assinatura cobre o conteúdo do exe**, verificada sobre os bytes recomputados em disco (RN sobre CA-6).
- **RN-5 — TLS não aceita "qualquer" cert.** Substituir `=> true` por validação por **pin de chave pública** (SPKI SHA-256) com lista embutida que suporta 2 pins (rotação de cert sem quebrar clientes).

## Corner cases

- **Download 404/parcial/timeout** (versão anunciada mas download falha): abortar gracioso, logar, seguir no login (não travar).
- **Cliente sem chave pública embutida** (build de distribuição mal gerada): toda verificação falha → fail-closed → auto-update inoperante para aquele cliente. É seguro mas silencioso — ver Gate G-4.
- **Rotação de cert do servidor** sem atualizar os pins do cliente: clientes antigos rejeitam a conexão de update (fail-closed). Mitiga com 2 pins (RN-5); operação precisa publicar o novo pin em um release antes de rotacionar.
- **Quarentena não removível** (arquivo travado): logar; próxima tentativa sobrescreve o `.tmp`.
- **Coop (Fika PVE):** a verificação roda por cliente no connect. Servidor com `.sig` ausente/errado → **todos** os clientes ficam fail-closed e presos na versão atual (seguro, porém silencioso). Solo=host mascara isto se o host tiver a infra de assinatura montada mas o build enviado ao cliente não. Ver Gate G-3.

## Fora de escopo

- **Substituir/remover o Gist como fonte de `Server.Url`** (`ConnectServerViewModel.cs:54`). A assinatura neutraliza o RCE de update independentemente da URL; endurecer/remover o Gist afeta toda a conexão (login/profile), não só o update. Fica como **decisão de produto pendente** (D-018.3) e possível item próprio.
- **Assinar o manifesto de mods do motor de sync** (item 007) — o MD5 como âncora de integridade dos mods é achado 🟢 separado da AUDIT.
- **Authkey reusável do Tailscale** (`TailscaleHelper.cs:17`) — achado 🟡 de segurança distinto (item próprio).
- **Plaintext de senha em `/redline/profile/get`** — achado 🟡 server-side (item de 005/senha).
- **UI nova dedicada** — a mensagem de erro reusa a tela de conexão (`connectModel.InfoText`).

## Gates humanos (obrigatórios antes de produção)

> Regra do repo: escrita em arquivos SPT / troca de binário precisa de **validação no jogo**, não só build verde. O auto-update **substitui o próprio .exe do jogador em disco** — o caminho crítico só conta como validado se exercido de verdade.

- **G-1 — Build verde.** `dotnet build SPT.Launcher.csproj -c Release` · `dotnet test SPT.Launcher.Tests.csproj -c Release` · `dotnet build TarkovRedLine.Server.csproj -c Release`. Nunca rodar o exe pelos gates automáticos.
- **G-2 — Update feliz real (não-solo).** Publicar exe assinado válido no `Launcher-Updater` de produção e confirmar num **segundo cliente/máquina** que o launcher baixa, verifica, promove e reinicia na versão nova.
- **G-3 — Fail-closed real.** Publicar um exe **adulterado** (ou remover o `.sig`) e confirmar no cliente que o update é abortado, o binário atual **não** é tocado, o `.bat` **não** roda, aparece o aviso e o jogo segue jogável. Repetir num cliente coop (não só host).
- **G-4 — Inspeção de produção.** Conferir que o `Launcher-Updater` de produção contém o `.sig` correspondente ao exe publicado, que o servidor serve version+signature+download, e que o build de distribuição do cliente **tem a chave pública embutida**.
- **G-5 — Pin correto.** Confirmar que o(s) pin(s) SPKI embutido(s) correspondem ao certificado real do servidor de produção antes do deploy.

## Decisões de produto pendentes (precisam do humano)

- **D-018.1 — Esquema de assinatura.** (a) Keypair auto-gerenciado (RSA-2048 ou ECDSA P-256), chave pública embutida, `.sig` publicado junto ao exe — **grátis, casa com o TLS self-signed atual** (recomendado); ou (b) **Authenticode** com cert de code-signing comprado (remove também o SmartScreen, custa e exige emissão por CA). Recomendação: (a) agora, (b) como upgrade.
- **D-018.2 — Guarda da chave privada.** Onde vive e quem assina no release (máquina do mantenedor / CI). Chave pública é source no repo; chave privada **nunca** entra no repo nem em path sincronizado.
- **D-018.3 — Destino do Gist.** Manter o Gist como fonte de URL (fora de escopo aqui) ou tratar num item de hardening da conexão. Enquanto existir, a assinatura já contém o dano de update.
- **D-018.4 — Política de rotação de cert/pin.** Cadência e processo (publicar novo pin num release antes de rotacionar o cert do servidor).

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-04 | Guilherme | Criação — spec funcional do item 018 (fecha RCE do auto-update: assinatura + cert pinning + fail-closed). |
