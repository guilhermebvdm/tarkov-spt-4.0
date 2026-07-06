# 020 — Integridade do cofre de senhas · Spec funcional

> **Data:** 2026-07-04<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [00-kickoff](./020-password-vault-integrity-00-kickoff.md) · [AUDIT-2026-07-04](../../AUDIT-2026-07-04-code-product-ds.md)<br>

---

## Objetivo

Corrigir a **integridade do cofre de senhas** (`user/profiles/redline_passwords.json`) e do fluxo de conta que o alimenta, eliminando quatro classes de defeito herdadas dos itens 005 (senha) e 010 (delete):

1. **Colisão de casing** — duas contas distintas do core (`Bob` ≠ `bob`) colapsam na mesma entrada do cofre e uma senha grava no perfil errado.
2. **Delete não-atômico** — o cofre é esvaziado **antes** do remove; se o remove falhar, a conta sobrevive no server sem senha (takeover livre).
3. **Plaintext exposto** — `/redline/profile/get` devolve a senha em texto puro a quem apenas **postar o username**, contornando qualquer gate.
4. **Entradas órfãs** — delete/wipe do core não tocam o cofre, deixando `redline_passwords.json` acumular chaves de contas que não existem mais.

O critério transversal é **paridade com o core**: a chave do cofre e o match de perfil devem usar exatamente o mesmo critério de igualdade de username que o SPT core usa no login/registro. Sem essa paridade, o cofre e o core divergem e a senha "some" ou vaza para outra conta.

## Contexto de arquitetura (relevante aos critérios)

- A "autenticação" do launcher é **client-side**: `LoginViewModel.cs:58-66` compara a senha digitada (`Login.Password`) com a senha que `/redline/profile/get` **ecoa** para dentro de `SelectedAccount.password`. O core (`/launcher/profile/login`) não valida senha em SPT 4.0 (o campo é limpo do `info`), por isso o cofre TRL existe.
- Consequência: o plaintext de `/redline/profile/get` **não é um vazamento acidental — é a fundação do gate atual**. Removê-lo exige mover a verificação de senha para o server (endpoint de verify) e passar a povoar `SelectedAccount.password` a partir da senha **digitada**, não da ecoada. Isso é o eixo da decisão de produto (ver [02-spec-tech](./020-password-vault-integrity-02-spec-tech.md) §Riscos e a seção "Decisões de produto" abaixo).
- Runtime é **Fika Coop PVE multiplayer** numa tailnet. O plaintext permite que qualquer par na tailnet colha todas as senhas postando usernames. Ameaça baixa (amigos), não nula.

## Regras de negócio

- **BR-020.1 — Um username, uma chave.** A chave do cofre e o match de perfil usam **o mesmo critério de igualdade do core**. Não pode haver dois registros no cofre que sejam "o mesmo usuário" sob esse critério. (Ver decisão de produto DP-020.A: qual critério — case-sensitive espelhando o core, ou case-insensitive canônico com bloqueio de registro colidente.)
- **BR-020.2 — Delete é destrutivo e ordenado.** Excluir uma conta remove **primeiro** a conta no server (fonte de verdade); só depois limpa o cofre. Se o remove falhar, o cofre **não** é tocado (nada de conta-sem-senha).
- **BR-020.3 — Limpar cofre = remover a chave, não esvaziar a senha.** "Limpar" a entrada do cofre significa **deletar a chave**, nunca gravar senha vazia. Senha vazia no cofre == gate aberto (troca livre) e é indistinguível de "conta sem senha", que é justamente o estado que dá takeover.
- **BR-020.4 — Sem órfãos.** Após um delete bem-sucedido, não existe entrada no cofre para o username excluído. Após um wipe, a entrada é reaproveitada (mesmo usuário) ou removida se o re-register falhar.
- **BR-020.5 — Nenhum endpoint devolve a senha ao cliente.** (Sujeito a DP-020.B.) A verificação de senha, se mantida, acontece no server (o cliente envia `username`+`password`, recebe OK/WRONG/NO_PASSWORD), nunca a senha de volta.
- **BR-020.6 — Fail-closed preservado.** O gate D2 (troca só livre quando não há senha) e a leitura fail-closed (cofre ilegível ⇒ nega troca) do trabalho anterior **continuam válidos** após a mudança de casing.

## Critérios de aceite (Given/When/Then testáveis)

### Colisão de casing

- [ ] **AC-020.1** — *Dado* que existem os perfis do core `Bob` e `bob` (dois arquivos distintos), *quando* `Bob` troca a senha, *então* apenas o cofre/perfil de `Bob` é alterado e o de `bob` permanece intacto (nenhuma escrita cruzada).
- [ ] **AC-020.2** — *Dado* o cofre com as chaves `Bob` e `bob`, *quando* `/redline/profile/get` é chamado para `bob`, *então* a senha retornada/verificada é a de `bob`, nunca a de `Bob` (sem "primeiro arquivo enumerado vence").
- [ ] **AC-020.3** — *Dado* o critério de igualdade escolhido (DP-020.A), *quando* o cofre é gravado, *então* a chave persistida usa **exatamente** esse critério e o mesmo `Directory.GetFiles` match do perfil usa o **mesmo** critério (paridade verificável lendo os dois pontos do código).
- [ ] **AC-020.4** — *Dado* que DP-020.A escolha o modelo canônico com bloqueio, *quando* alguém tenta registrar `bob` já existindo `Bob`, *então* o registro é recusado com mensagem clara (não cria colisão). *(Só se DP-020.A = canônico.)*

### Delete atômico / ordem segura

- [ ] **AC-020.5** — *Dado* uma conta logada, *quando* o usuário confirma "excluir conta" e o remove do server **falha** (ex.: `NoConnection`), *então* o cofre **não** foi alterado e a conta permanece com sua senha original (nenhuma janela de conta-sem-senha).
- [ ] **AC-020.6** — *Dado* um delete confirmado, *quando* o remove do server **sucede**, *então* a chave do username some do `redline_passwords.json` (não fica com senha vazia) e o auto-login/last-credentials são limpos.
- [ ] **AC-020.7** — *Dado* um delete no meio de uma **sessão coop** (host apagando conta com clientes na raid), *quando* o comando é acionado, *então* ele é bloqueado/adiado pelo mesmo gate de estado local (`!GameRunning && !IsUpdating`) — o item não remove esse gate. *(Gap de coop registrado; não regride.)*

### Plaintext / exposição

- [ ] **AC-020.8** — *(Sujeito a DP-020.B)* *Dado* o endpoint de conta, *quando* um cliente posta apenas o `username`, *então* a resposta **não** contém a senha em texto puro (o campo `password` do `info` vem ausente/mascarado).
- [ ] **AC-020.9** — *(Se DP-020.B = verify server-side)* *Dado* login com senha errada, *quando* o launcher chama o verify, *então* o server responde `WRONG` e o login falha sem que o cliente jamais receba a senha correta.
- [ ] **AC-020.10** — *(Se DP-020.B = verify server-side)* *Dado* conta sem senha (cofre sem a chave), *quando* o launcher chama o verify, *então* o server responde `NO_PASSWORD` e o launcher força o diálogo de criar senha (paridade com o comportamento atual de `LoginViewModel.cs:69-95`).

### Órfãos / reconciliação

- [ ] **AC-020.11** — *Dado* um `redline_passwords.json` com chaves de perfis que não existem mais em `user/profiles/*.json`, *quando* a rotina de limpeza roda (no delete/wipe ou reconciliação), *então* as chaves órfãs são removidas e chaves de perfis vivos são preservadas.
- [ ] **AC-020.12** — *Dado* um wipe (delete+recreate), *quando* o re-register **falha** após o remove, *então* não sobra entrada de cofre apontando para uma conta inexistente (órfã), OU a falha é sinalizada e a limpeza acontece.

### Não-regressão dos gates existentes

- [ ] **AC-020.13** — Os gates D2 (troca livre só sem senha) e a leitura fail-closed (cofre corrompido ⇒ `FAILED` no change) continuam passando após a mudança de casing (reusar/adaptar a cobertura de 005).
- [ ] **AC-020.14** — Reset por HWID (`LoginViewModel.cs:156-214`) e criação de senha inicial (`ClassSelectionViewModel.cs:132`) continuam funcionais com o novo critério de chave.

## Corner cases

- **CC-1 — Cofre legado com duplicatas de casing.** Instalações atuais podem ter `Bob` e `bob` no JSON (efeito do bug). A migração precisa ser **determinística e não-destrutiva por padrão**: decidir qual vence (o mais recente? ambos preservados sob DP-020.A case-sensitive?) e **nunca** apagar silenciosamente uma senha que ainda corresponde a um perfil vivo. É exatamente o que o gate humano inspeciona.
- **CC-2 — Perfil sem entrada no cofre.** Estado legítimo (conta nova/sem senha): não é erro, o launcher força criar senha. Não confundir com "cofre ilegível" (fail-closed).
- **CC-3 — Username reciclado.** Deletar `Bob` e alguém registrar `Bob` de novo não pode herdar a senha antiga (BR-020.4). Coberto por AC-020.6/AC-020.11.
- **CC-4 — Remove sucede, limpeza de cofre falha (IO).** Órfão temporário. A reconciliação (AC-020.11) precisa ser idempotente e rodar de novo para convergir.
- **CC-5 — Concorrência multi-cliente.** Dois clientes coop mexendo no mesmo `redline_passwords.json` (troca simultânea). Escrita do cofre deve ser atômica (temp+move) para não corromper o JSON. Registrar como risco (o controller hoje faz `WriteAllText` direto).
- **CC-6 — `password/change` para conta inexistente.** Se o username não casa nenhum perfil, retorna `FAILED` sem criar entrada de cofre fantasma.

## Fora de escopo

- Hash/salt de senhas (o cofre continua texto puro em disco — mudança de storage é outro item).
- Reescrever a autenticação do core ou introduzir sessão/token real.
- Migrar `SettingsView`/DS (B3), auto-update RCE (B1), `deleteFiles` guard (B2) — itens próprios.
- Rotação do `password_debug_log.txt` (🟢 menor separado).
- UI nova para gestão de senhas.

## Decisões de produto (precisam do humano)

- **DP-020.A — Critério de igualdade de username.** Duas opções, ambas eliminam a colisão:
  - **(A1) Case-sensitive espelhando o core** — chave do cofre e match de perfil viram `StringComparison.Ordinal`. Simples, mas **exige confirmar** que o core `/launcher/profile/login` também casa case-sensitive; se o core for case-insensitive, `bob` logaria no perfil `Bob` mas com chave de cofre ausente ⇒ senha "some". **Verificação obrigatória do comportamento real do core antes de escolher A1.**
  - **(A2) Canônico case-insensitive + bloqueio de registro** — mantém a chave lowercase (como está), mas **impede** registrar `bob` se `Bob` existe (e vice-versa), tornando a colisão impossível na origem. Mais robusto contra divergência com o core, mas toca o fluxo de registro.
  - **Recomendação:** decidir por verificação empírica do core (spec-tech tem o plano). Sem confirmação, **A2** é o default mais seguro.
- **DP-020.B — Plaintext: corrigir agora ou deferir.** Remover o eco de senha exige o endpoint de verify server-side + repovoar `SelectedAccount.password` da senha digitada (blast radius: login, reset-HWID, create-password, change, remove, wipe). Alternativa: **deferir** o plaintext para um item de segurança dedicado e entregar 020 só com casing + delete atômico + órfãos (ameaça mitigada pelo contexto LAN/tailnet). **Precisa da decisão do humano sobre escopo.**
- **DP-020.C — Reconciliação de órfãos: quando roda?** Só no delete/wipe (barato, cobre o caso comum) ou também um sweep no boot do server (converge instalações legadas, custo de IO no start). Recomendo delete/wipe + um sweep único documentado.

## Gates

### Build/test (nunca rodar o exe)
- `dotnet build SPT.Launcher.csproj -c Release` — verde.
- `dotnet test SPT.Launcher.Tests.csproj -c Release` — verde (inclui os testes novos da lógica extraível, ver spec-tech).
- `dotnet build TarkovRedLine.Server.csproj -c Release` — verde (mudanças no `PasswordController`).

### Gates humanos (obrigatórios — escrita em arquivo SPT precisa de validação em jogo, não só build)
- **GH-1 — Inspeção de produção.** Antes do deploy da DLL do server: abrir o `redline_passwords.json` **de produção** e verificar chaves colidentes de casing (`Bob`/`bob`) e chaves órfãs. Registrar o achado; a migração (CC-1) não pode apagar senha de perfil vivo.
- **GH-2 — Validação in-game do ciclo completo.** Com o server buildado: registrar conta → definir senha → relogar (senha correta e errada) → trocar senha → **excluir conta** → tentar relogar (deve falhar) → registrar o mesmo username de novo (não pode herdar senha). Confirmar no `redline_passwords.json` que não sobrou órfã.
- **GH-3 — Validação de casing.** Criar `Bob` e `bob`, dar senhas diferentes, confirmar in-game que cada um loga com a sua e que trocar a de um não afeta o outro.
- **GH-4 — Coop.** Confirmar que excluir conta fica bloqueado durante sessão coop ativa (host + cliente na raid) e que a limpeza do cofre não corrompe o JSON sob acesso concorrente.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-04 | Guilherme | Criação — spec funcional a partir do kickoff 020 e da auditoria 2026-07-04. |
