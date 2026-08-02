# Origem dos assets

> **Data:** 2026-08-02<br>
> **Status:** 🔵 Em andamento — aguardando a origem de cada item<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [PERMISSION.md](./PERMISSION.md)<br>

---

O SPT Forge exige que todo material embutido tenha origem conhecida e licença compatível. **Nenhum destes
arquivos vem do mod original** — a pasta `original/` não tem imagem nem áudio, então todos entraram no fork.

## O que o mod realmente usa (6 arquivos, ~1,9 MB)

Estes são carregados pelo código em execução e **vão no pacote de publicação**:

| Arquivo | Tamanho | Para que serve | Origem | Licença |
|---|---|---|---|---|
| `modded/Resources/breath_in.ogg` | 14 KB | Som de inspirar ao segurar a respiração | ⬜ | ⬜ |
| `modded/Resources/breath_out.ogg` | 19 KB | Som de expirar | ⬜ | ⬜ |
| `modded/Resources/heartbeat.ogg` | 467 KB | Batimento cardíaco sob esforço | ⬜ | ⬜ |
| `modded/Resources/mounting.png` | 474 KB | Ícone de apoio da arma (frontal) | ⬜ | ⬜ |
| `modded/Resources/mountingleft.png` | 468 KB | Ícone de apoio (esquerda) | ⬜ | ⬜ |
| `modded/Resources/mountingright.png` | 474 KB | Ícone de apoio (direita) | ⬜ | ⬜ |

Para cada um, basta dizer: **feito por você**, **gerado por IA**, **de banco livre** (qual, e sob que licença),
ou **origem desconhecida** — este último caso significa substituir antes de publicar.

## O que NÃO vai no pacote (~36 MB)

Material de trabalho que está no repositório mas **o código nunca carrega**:

| Arquivo | Tamanho | O que é |
|---|---|---|
| `assets/image.png` · `image copy.png` · `image copy 2/3/4.png` | ~11,5 MB somados | Cinco capturas de tela, pelos nomes de colagem. Não referenciadas em lugar nenhum do código |
| `stance slowed down.mp4` | 24,8 MB | Vídeo de referência de movimento |

**Decisão a tomar:** ficam de fora do `.zip` de publicação, certamente. A pergunta é se ficam no **repositório
público** — 36 MB de material de trabalho num repo que existe para dar transparência ao código é peso morto
para quem clona. Sugestão: mover para fora do repo do mod, ou para uma pasta de documentação claramente
separada, e citar na página do mod se forem úteis como demonstração.

## Observação de tamanho (não bloqueia)

Os três ícones de apoio pesam ~470 KB **cada**, o que é muito para ícone de interface — provavelmente são
imagens em resolução alta sendo reduzidas na tela. Otimizá-los cortaria mais de 1 MB do pacote. Não é
requisito do Forge; é cortesia com quem baixa.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-08-02 | Guilherme | Criação — levantamento dos 12 binários do mod, separando os 6 que o código usa dos ~36 MB de material de trabalho |
