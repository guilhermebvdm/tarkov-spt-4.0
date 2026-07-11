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
| `BepInEx/**config**/` | **`preserve-divergent`** — atualiza **só quem não customizou** o arquivo (bate com o baseline da última sync). Quem customizou **mantém a versão dele**. Se o arquivo não existe, baixa. | Config padrão normal. Respeita o jogador. |
| `BepInEx/**config-server**/` | **`seed-and-mirror`** — **semeia** em `config/` **só se o arquivo faltar** (nunca sobrescreve) **e** espelha a última versão em `config-server/` no cliente (pasta de **referência**, de onde ele pode copiar manualmente). | Defaults de mod novo: o jogador já entra configurado, e tem a referência atualizada pra copiar se quiser. |
| `BepInEx/**config-force**/` | 🔨 **`force-to-config`** — **SOBRESCREVE o `config/` de TODOS**, sempre que o conteúdo divergir. **Ignora customização.** | "Essa config vai pra todo mundo, doa a quem doer" — ex.: corrigir um valor que quebra o coop. |

O sufixo é **removido** no destino: `config-server/X.cfg` e `config-force/X.cfg` **ambos** vão parar em `config/X.cfg` no cliente. Subpastas são preservadas.

## Regras importantes do `config-force`

- **Sem memória:** se o jogador editar de novo, o **próximo sync devolve a sua versão**. Não tem como ele "escapar".
- **Não deleta nada.** A pasta `config-force` **nunca é materializada** no cliente (é só fonte de download).
- **Dev Mode ganha:** um dev com Dev Mode LIGADO que editou o arquivo **mantém a edição local** (é o escape hatch de desenvolvimento). Jogador comum não tem Dev Mode.
- **⚠️ Não deixe o mesmo arquivo em `config/` E `config-force/`.** Se deixar, o launcher **faz o force vencer** e registra um aviso — mas o certo é ter o arquivo em **um canal só**.
- **Regenere o manifesto** depois de mexer: `GET /launcher/mods/refresh` ou reinicie o server. Sem isso o manifesto fica stale e o force **reaplica em todo sync** (o launcher loga um aviso).

## Versão mínima do cliente

O `config-force` foi entregue no launcher **2.2.0**. Clientes em versão anterior **não aplicam o force** (ignoram a regra) — então um fix de coop empurrado por esse canal **só chega em quem já atualizou o launcher**. Garanta o auto-update antes de depender dele.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-09 | Guilherme | Criação — os 3 canais (config, config-server, config-force) e as regras do force. |
