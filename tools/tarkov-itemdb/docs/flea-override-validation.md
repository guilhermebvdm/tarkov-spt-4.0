# Ação 0 — Smoke test do `itemPriceOverrideRouble` em SPT 4.0.13

Validação prévia para o plano de override-only em `C:\Users\guime\.claude\plans\curried-dreaming-marble.md`.

**Objetivo**: confirmar que `ragfair.json:dynamic.itemPriceOverrideRouble` sobrescreve toda a matemática vanilla do flea (passos 1+2 do boot) e produz oferta = `override × variance`.

## Pré-requisitos

- [ ] `D:/SPT/SPT/SPT_Data` em estado vanilla — sem edits residuais em `handbook.json` ou `prices.json` (outra sessão limpou os 12 tpls de teste).
- [ ] `LiveFleaPrices` mod **DESATIVADO** (renomeado `.disabled`).
- [ ] Nenhum outro mod modificando `ragfair.json` em runtime.

## Procedimento

```bash
node tools/tarkov-itemdb/scripts/action0-override-smoke-test.js prep
```

Esse script:
1. Faz backup de `ragfair.json` em `ragfair.json.pre-action0-backup`.
2. Injeta 2 overrides:
   - Bolts (`57347c5b245977448d35f6e1`) — craft item → `123456`
   - GPU   (`57347ca924597744596b4e71`) — non-craft → `654321`
3. Recalcula MD5 e atualiza `checks.dat`.

Depois:
4. Restart **full** do SPT server.
5. Entrar no jogo, abrir flea, filtrar Bolts → registrar ofertas mínima/máxima observadas.
6. Idem para GPU.

## Resultados

### Bolts (`57347c5b245977448d35f6e1`, hipótese craft, override = 123,456)

| Métrica | Esperado (sucesso) | Observado |
|---|---|---|
| Min observado | ≈ 98,765 (override × 0.8) | _preencher_ |
| Max observado | ≈ 148,147 (override × 1.2) | _preencher_ |
| Média observada | ≈ 123,456 | _preencher_ |

Cenários de FALHA:
- Ofertas em **6M–9M** → override ignorado, vanilla com Bolts NÃO-craft (`H=5M × 1.5 = 7.5M`).
- Ofertas em **9.2M–13.8M** → override ignorado, vanilla com Bolts CRAFT (`H=5M × 2.3 = 11.5M`).
- Ofertas em **123K–148K + bonus_vanilla** → override aplicado antes da soma vanilla (ordem invertida).

Qualquer falha invalida a arquitetura override-only do plano. Resultado vai indicar se Bolts é craft (info útil pra Ação 1 depois).

### GPU (`57347ca924597744596b4e71`, hipótese não-craft, override = 654,321)

| Métrica | Esperado (sucesso) | Observado |
|---|---|---|
| Min observado | ≈ 523,456 (override × 0.8) | _preencher_ |
| Max observado | ≈ 785,185 (override × 1.2) | _preencher_ |
| Média observada | ≈ 654,321 | _preencher_ |

Cenários de FALHA:
- Ofertas em **6M–9M** → override ignorado, vanilla com GPU NÃO-craft (`H=5M × 1.5 = 7.5M`).
- Ofertas em **9.2M–13.8M** → override ignorado, vanilla com GPU CRAFT (`H=5M × 2.3 = 11.5M`).

Coincidência: Bolts e GPU têm o mesmo handbook (5M), então as faixas de falha vanilla são idênticas — diferenciação só pelo valor do override que cada um recebeu.

## Conclusão

- [ ] **SUCESSO** — ofertas batem com `override × 0.8..1.2`. Plano segue para Ação 1.
- [ ] **FALHA modo A** — override ignorado, ofertas batem fórmula vanilla. Replanejar.
- [ ] **FALHA modo B** — ofertas em `(override + bonus_vanilla) × 0.8..1.2`. Override aplicado, depois somado. Replanejar.
- [ ] **FALHA modo C** — comportamento inconsistente / inexplicável. Investigar interferência.

Resultado observado: _preencher após testar_.

## Reversão

```bash
node tools/tarkov-itemdb/scripts/action0-override-smoke-test.js revert
```

Restaura `ragfair.json` do backup e refrescha `checks.dat`. Backup é removido após sucesso.

## Notas extras

- Variância tem bias 2,2 (favor centro), então com poucas ofertas observadas o min/max real pode ficar entre 0.85 e 1.15 em vez de 0.80 e 1.20.
- Quality modifier não se aplica aos tpls escolhidos (Bolts e GPU são `conditionType: none`).
- `priceRanges.default` (0.8..1.2) é o range aplicável (não são preset nem pack).
