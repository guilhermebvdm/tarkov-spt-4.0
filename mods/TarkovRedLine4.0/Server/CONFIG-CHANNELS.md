# Canais de config (server → cliente)

> **Data:** 2026-07-09<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Para:** operador do server — onde colocar uma config no `Launcher-Updater/mods_repo/` dependendo de **quem** você quer que a receba.<br>

---

## Os 3 canais

Todos ficam sob `Launcher-Updater/mods_repo/`. O que muda é **como o launcher aplica no cliente**:

| Pasta no **server** | O que acontece no **`BepInEx/config/`** do jogador | Quando usar |
|---|---|---|
| `BepInEx/**config**/` | **`preserve-divergent`** — atualiza **só quem não customizou** o arquivo (bate com o baseline da última sync). Quem customizou **mantém a versão dele**. Se o arquivo não existe, baixa. **É o canal que distribui defaults** (inclusive pra instalação nova, que recebe tudo). | Config padrão normal. Respeita o jogador. |
| `BepInEx/**config-server**/` | 📚 **`mirror-reference`** — **só biblioteca de referência.** Espelha a última versão em `config-server/` no cliente (pasta pristina, de onde o jogador pode copiar manualmente). **NUNCA toca `config/`** e **não deleta extras**. | Guardar a versão "de fábrica" de uma config pro jogador consultar/copiar sem afetar a que ele usa. |
| `BepInEx/**config-force**/` | 🔨 **`force-to-config`** — **SOBRESCREVE o `config/` de TODOS**, sempre que o conteúdo divergir. **Ignora customização.** A config anterior do jogador é preservada em `config-disabled/` (backup versionado). | "Essa config vai pra todo mundo, doa a quem doer" — ex.: corrigir um valor que quebra o coop. |

**Onde cada canal escreve no cliente:**
- `config/X.cfg` → distribuído por **`config`** (respeita customização) e sobrescrito por **`config-force`** (o sufixo `-force` é removido: `config-force/X.cfg` → `config/X.cfg`).
- `config-server/X.cfg` → espelhado **na própria** `config-server/X.cfg` (mantém o nome da pasta; **não** vira `config/X.cfg`).
- Subpastas são preservadas em todos os canais.

> **Mudança na 2.3.0:** antes o `config-server` era `seed-and-mirror` e **também semeava** em `config/` quando o arquivo faltava. Agora ele é **só referência** — quem distribui defaults é o canal `config`. Os arquivos que a 2.2.1 já semeou em `config/` **permanecem** (viram "customizados" pro preserve-divergent, preservados). **Antes de publicar:** garanta que todo default que estava só em `config-server/` também exista em `config/` — senão instalações novas deixam de recebê-lo.

## Regras importantes do `config-force`

- **Sem memória:** se o jogador editar de novo, o **próximo sync devolve a sua versão**. Não tem como ele "escapar".
- **Não deleta nada.** A pasta `config-force` **nunca é materializada** no cliente (é só fonte de download).
- **Dev Mode ganha:** um dev com Dev Mode LIGADO que editou o arquivo **mantém a edição local** (é o escape hatch de desenvolvimento). Jogador comum não tem Dev Mode.
- **⚠️ Não deixe o mesmo arquivo em `config/` E `config-force/`.** Se deixar, o launcher **faz o force vencer** e registra um aviso — mas o certo é ter o arquivo em **um canal só**.
- **Regenere o manifesto** depois de mexer: `GET /launcher/mods/refresh` ou reinicie o server. Sem isso o manifesto fica stale e o force **reaplica em todo sync** (o launcher loga um aviso).

## Pastas `-disabled/` são quarentena intocável

Qualquer pasta com sufixo `-disabled` (o backup do `config-force` em `config-disabled/`, um `plugins-disabled/` de quarentena etc.) **nunca é re-sincronizada nem deletada** pelo launcher — é a garantia "nada excluído misteriosamente". Isso inclui a lista `deleteFiles` do manifesto: **um `deleteFile` apontando pra uma pasta `-disabled/` é ignorado** (com log). **Consequência:** limpeza de quarentena é **manual** — o operador não consegue apagar um `-disabled/` via manifesto; precisa remover o arquivo direto na máquina.

## Versão mínima do cliente

- O `config-force` foi entregue no launcher **2.2.0**. Clientes em versão anterior **não aplicam o force** (ignoram a regra) — então um fix de coop empurrado por esse canal **só chega em quem já atualizou o launcher**. Garanta o auto-update antes de depender dele.
- O `config-server` virou **só referência** (`mirror-reference`) na **2.3.0**. Clientes 2.2.x ainda tratam o `config-server` como `seed-and-mirror` (semeiam em `config/`). Não é um problema: os defaults já são distribuídos pelo canal `config`.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-09 | Guilherme | Criação — os 3 canais (config, config-server, config-force) e as regras do force. |
| 2026-07-17 | Guilherme | `config-server` passa a ser **só referência** (`mirror-reference`) na 2.3.0 — não semeia mais em `config/`. Backup do force em `config-disabled/` documentado. Nova seção: pastas `-disabled/` são quarentena intocável (guard do `deleteFiles`). |
