using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;                  // ItemHelper
using SPTarkov.Server.Core.Services;                 // DatabaseService
using SPTarkov.Server.Core.Utils.Cloners;            // ICloner
using SPTarkov.Server.Core.Models.Common;            // MongoId
using SPTarkov.Server.Core.Models.Utils;             // ISptLogger
using SPTarkov.Server.Core.Models.Enums;             // EquipmentSlots
using SPTarkov.Server.Core.Models.Eft.Common;        // PmcData, Preset
using SPTarkov.Server.Core.Models.Eft.Common.Tables; // Item

namespace CustomClasses;

/// <summary>
///     Builds a class's starting inventory onto a character. Item 003.
///     Fatia 1: equipado-simples (1 tpl/slot) + slot-occupancy/subtree-removal (PA-02-01/02).
///     Fatia 2: <b>preset</b> (re-id da árvore, PA-01-02/04) e <b>árvore manual</b> (mods por slot).
///     Fatia 3: carregador/câmara (`ammo`). Fatia 4: contents em grade (GridPacker).
///     CR-EP-01: linhas de stash/contents honram a MESMA semântica dos slots equipados —
///     `preset` explícito/`mods`/`ammo`/`contents` recursivo (antes só `tpl`+`count`).
///     Estrutura de item: ver docs/technical/inventario-itens-spt4.md.
/// </summary>
[Injectable]
public class InventoryBuilder(DatabaseService databaseService, ItemHelper itemHelper, InventoryHelper inventoryHelper, ICloner cloner, ISptLogger<InventoryBuilder> logger)
{
    /// <summary>Returns the number of equipped root items added.</summary>
    public int Apply(PmcData? character, Loadout loadout, string className)
    {
        var inv = character?.Inventory;
        if (inv?.Items is null || inv.Equipment is null)   // ref: BotBase.cs:368/371
        {
            logger.Warning($"[CustomClasses] '{className}': base has no Inventory.Equipment/Items — loadout skipped.");
            return 0;
        }

        var equipmentId = inv.Equipment.Value.ToString();   // não-nulo (guardado acima)
        var added = 0;

        foreach (var (slotName, spec) in loadout.Equipped ?? new Dictionary<string, ItemSpec>())
        {
            if (!Enum.TryParse<EquipmentSlots>(slotName, ignoreCase: true, out var slot)
                || !Enum.IsDefined(typeof(EquipmentSlots), slot))
            {
                logger.Warning($"[CustomClasses] '{className}': unknown equipment slot '{slotName}' — ignored.");
                continue;
            }

            var slotId = slot.ToString();   // nome do enum == slotId do EFT

            try   // CR-01-01: um item inválido (ex.: tpl mal formado) pula só este slot, não a classe inteira
            {
                // Monta a árvore (preset/manual/tpl). tpl sem preset/mods → auto-completa com o preset
                // default do item (arma/armadura/capacete/rig com placas); sem preset → item simples.
                var tree = BuildItemTree(spec.Preset, spec.Mods, spec.Tpl, spec.Premium, equipmentId, slotId, slotName, className);
                if (tree is null || tree.Count == 0)
                {
                    continue;   // falha logada no builder
                }

                if (spec.Count > 1)
                {
                    logger.Debug($"[CustomClasses] '{className}': count>1 ignorado no slot equipado '{slotName}' (PA-02-05).");
                }
                // Fatia 3: carregador carregado + bala na câmara (`ammo` obrigatório).
                // CR2-EP-04: BEFORE packing contents — a content line may be an assembled weapon with
                // its own mod_magazine, and the whole-tree scan in LoadAmmo must never fill a magazine
                // that belongs to an item nested inside this container (cartridges don't change the
                // container footprint, so running first is safe — same rationale as PlaceSpecTrees).
                LoadAmmo(tree, spec, tree.FirstOrDefault(i => i.SlotId == slotId), slotId, className);

                // Fatia 4: conteúdo do contêiner (rig/mochila) empacotado nas grades dele.
                if (spec.Contents is { Count: > 0 })
                {
                    var containerRoot = tree.FirstOrDefault(i => i.SlotId == slotId);
                    if (containerRoot is not null)
                    {
                        PackSpecsIntoGrids(tree, containerRoot.Id.ToString(), GetGrids(containerRoot.Template), spec.Contents, className);
                    }
                }

                // CR-02: arma equipada sem óptica (preset base, ex.: AKMS) → garante mira mínima (red dot).
                var equippedRoot = tree.FirstOrDefault(i => i.SlotId == slotId);
                if (equippedRoot is not null && itemHelper.IsOfBaseclass(equippedRoot.Template, BaseClasses.WEAPON))
                {
                    EnsureMinimumOptic(tree, className);
                }

                // PA-02-01/02: substitui — remove o ocupante do slot (+ subárvore) antes de equipar.
                RemoveSlotOccupant(inv.Items, equipmentId, slotId);
                inv.Items.AddRange(tree);
                added++;
            }
            catch (Exception ex)
            {
                logger.Warning($"[CustomClasses] '{className}': slot '{slotName}' falhou ({ex.Message}) — item pulado.");
            }
        }

        // Fatia 4: itens soltos no stash, empacotados na grade do stash.
        if (loadout.Stash is { Count: > 0 } && inv.Stash is not null)
        {
            var stashItem = inv.Items.FirstOrDefault(i => i.Id == inv.Stash.Value);
            if (stashItem is null)
            {
                logger.Warning($"[CustomClasses] '{className}': contêiner de stash não encontrado — {loadout.Stash.Count} item(ns) pulado(s).");
            }
            else
            {
                PackSpecsIntoGrids(inv.Items, inv.Stash.Value.ToString(), GetGrids(stashItem.Template), loadout.Stash, className);
            }
        }

        return added;
    }

    /// <summary>
    ///     Monta a árvore de um item: `preset` explícito, árvore `manual` (mods), ou `tpl` — e nesse
    ///     último caso **auto-completa com o preset default** se o item tiver um (arma com mods/mira,
    ///     armadura/capacete/rig com placas+soft armor). Sem preset → item simples (mochila, rig sem
    ///     placa, munição, meds). Raiz re-raizada em (parentId, slotId). Null = falha (logada).
    /// </summary>
    private List<Item>? BuildItemTree(string? presetKey, List<ModSpec>? mods, string? tpl, bool premium, string parentId, string slotId, string slotName, string className)
    {
        if (presetKey is not null)
        {
            // `premium` → preset mais kitado da arma (mira/foregrip/tac); senão → default.
            var preset = premium ? ResolvePremiumPreset(new MongoId(presetKey)) : ResolvePreset(new MongoId(presetKey));
            if (preset?.Items is null || !preset.Items.Any())
            {
                logger.Warning($"[CustomClasses] '{className}': preset/arma '{presetKey}' não encontrado — slot '{slotName}' pulado.");
                return null;
            }

            return RebaseClonedPreset(preset, parentId, slotId);
        }

        if (mods is not null)
        {
            if (string.IsNullOrWhiteSpace(tpl))
            {
                logger.Warning($"[CustomClasses] '{className}': slot '{slotName}' tem 'mods' mas não tem 'tpl' raiz — pulado.");
                return null;
            }

            return BuildManualTree(tpl, mods, parentId, slotId);
        }

        if (!string.IsNullOrWhiteSpace(tpl))
        {
            // auto-completar: se o item tem preset default, montar a árvore; senão, item simples.
            var preset = ResolvePreset(new MongoId(tpl));
            if (preset?.Items is not null && preset.Items.Any())
            {
                return RebaseClonedPreset(preset, parentId, slotId);
            }

            return [new Item { Id = new MongoId(), Template = new MongoId(tpl), ParentId = parentId, SlotId = slotId }];
        }

        logger.Warning($"[CustomClasses] '{className}': slot '{slotName}' sem tpl/preset/mods — pulado.");
        return null;
    }

    /// <summary>Clona+re-id o preset e re-raiz a raiz em (parentId, slotId).</summary>
    private List<Item> RebaseClonedPreset(Preset preset, string parentId, string slotId)
    {
        var (tree, root) = ClonePresetTree(preset);
        root.ParentId = parentId;
        root.SlotId = slotId;
        return tree;
    }

    /// <summary>
    ///     Clona (deep) os itens de um preset e re-id preservando os links pai-filho (PA-01-02).
    ///     NÃO seta parentId/slotId da raiz — o caller decide (slot de equipamento ou posição no stash).
    /// </summary>
    private (List<Item> Tree, Item Root) ClonePresetTree(Preset preset)
    {
        var items = cloner.Clone(preset.Items)!.ToList();   // deep clone — NÃO mutar o globals (ICloner.cs)
        var root = items.FirstOrDefault(i => string.IsNullOrEmpty(i.ParentId)) ?? items[0];

        var map = new Dictionary<string, MongoId>();
        foreach (var it in items)
        {
            map[it.Id.ToString()] = new MongoId();
        }
        foreach (var it in items)
        {
            var oldParent = it.ParentId;
            it.Id = map[it.Id.ToString()];
            it.ParentId = oldParent is not null && map.TryGetValue(oldParent, out var np) ? np.ToString() : oldParent;
        }

        return (items, root);
    }

    /// <summary>
    ///     Resolve um preset de <c>databaseService.GetGlobals().ItemPresets</c> (id de preset OU tpl de arma).
    ///     NÃO usa <c>PresetHelper</c>: o cache dele (<c>PresetCache</c>) só é hidratado por
    ///     <c>PresetController.Initialize</c>, que roda DEPOIS do nosso <c>PostDBModLoader+1</c> — no nosso
    ///     momento o cache está vazio. O dict <c>ItemPresets</c> já existe desde o DB load.
    /// </summary>
    private Preset? ResolvePreset(MongoId key)
    {
        var itemPresets = databaseService.GetGlobals().ItemPresets;   // ref: Globals.cs:23

        // key é um id de preset?
        if (itemPresets.TryGetValue(key, out var byId))
        {
            return byId;
        }

        // key é um tpl de arma → achar o preset default (Encyclopedia != null), senão o primeiro.
        Preset? first = null;
        foreach (var p in itemPresets.Values)
        {
            var root = p.Items?.FirstOrDefault();   // raiz = 1º item (ref: PresetController.cs:32)
            if (root is null || root.Template != key)
            {
                continue;
            }

            first ??= p;
            if (p.Encyclopedia is not null)   // marca o default da arma (ref: PresetController.cs:37)
            {
                return p;
            }
        }

        return first;
    }

    /// <summary>
    ///     Resolve o preset **mais kitado** (maior nº de itens) de uma arma — build "premium" com mira/
    ///     foregrip/tac quando existir. Se `key` for id de preset, retorna-o. Sem preset → null.
    /// </summary>
    private Preset? ResolvePremiumPreset(MongoId key)
    {
        var itemPresets = databaseService.GetGlobals().ItemPresets;

        if (itemPresets.TryGetValue(key, out var byId))
        {
            return byId;   // id de preset explícito
        }

        // CR-02-01: prefere o maior preset SEM óptica térmica/NV (premium ≠ térmico); só usa térmico se não houver outro.
        Preset? best = null;
        var bestCount = -1;
        Preset? bestNoThermal = null;
        var bestNoThermalCount = -1;
        foreach (var p in itemPresets.Values)
        {
            var root = p.Items?.FirstOrDefault();
            if (root is null || root.Template != key)
            {
                continue;
            }

            var c = p.Items!.Count();
            if (c > bestCount)
            {
                bestCount = c;
                best = p;
            }
            if (c > bestNoThermalCount && !p.Items!.Any(i => IsThermalOrNv(i.Template)))
            {
                bestNoThermalCount = c;
                bestNoThermal = p;
            }
        }

        return bestNoThermal ?? best;
    }

    /// <summary>Baseclasses de óptica REAL (exclui iron sight) — ver Models/Enums/BaseClasses.cs.</summary>
    private static readonly MongoId[] OpticBaseclasses =
    [
        BaseClasses.ASSAULT_SCOPE, BaseClasses.COLLIMATOR, BaseClasses.COMPACT_COLLIMATOR,
        BaseClasses.OPTIC_SCOPE, BaseClasses.SPECIAL_SCOPE,
    ];

    private bool IsRealOptic(MongoId tpl) => itemHelper.IsOfBaseclasses(tpl, OpticBaseclasses);   // ref: ItemHelper.cs:310

    /// <summary>Red dots simples (sem sub-mods obrigatórios) — preferidos p/ mira mínima.</summary>
    private static readonly MongoId[] RedDotBaseclasses = [BaseClasses.COLLIMATOR, BaseClasses.COMPACT_COLLIMATOR];

    /// <summary>Térmica/NV — evitar como mira automática (overkill). (ref: BaseClasses.cs:83/119)</summary>
    private static readonly MongoId[] ThermalNvBaseclasses = [BaseClasses.THERMAL_VISION, BaseClasses.NIGHT_VISION];

    private bool IsThermalOrNv(MongoId tpl) => itemHelper.IsOfBaseclasses(tpl, ThermalNvBaseclasses);

    /// <summary>CR-02-02: óptica mais simples do filtro — red dot &gt; assault scope &gt; resto; evita térmica/NV; determinístico.</summary>
    private MongoId? PickSimpleOptic(IEnumerable<MongoId> filter)
    {
        var optics = filter.Where(IsRealOptic).Where(t => !IsThermalOrNv(t)).ToList();
        if (optics.Count == 0)
        {
            return null;
        }

        var redDots = optics.Where(t => itemHelper.IsOfBaseclasses(t, RedDotBaseclasses)).ToList();
        if (redDots.Count > 0)
        {
            return redDots.OrderBy(t => t.ToString(), StringComparer.Ordinal).First();
        }

        var scopes = optics.Where(t => itemHelper.IsOfBaseclass(t, BaseClasses.ASSAULT_SCOPE)).ToList();
        if (scopes.Count > 0)
        {
            return scopes.OrderBy(t => t.ToString(), StringComparer.Ordinal).First();
        }

        return optics.OrderBy(t => t.ToString(), StringComparer.Ordinal).First();
    }

    private TemplateItem? GetTemplate(MongoId tpl)
    {
        var ci = itemHelper.GetItem(tpl);   // ref: ItemHelper.cs:494
        return ci is { Key: true, Value: not null } ? ci.Value : null;
    }

    /// <summary>
    ///     Etapa 2: preset para armas/itens do STASH. Prefere o MENOR preset que já traga óptica real
    ///     (preset "normal" com mira mínima); senão, o default. Não-armas (armadura) caem no default
    ///     (com placas). Itens sem preset → null (vira item simples).
    /// </summary>
    private Preset? ResolveStashPreset(MongoId key)
    {
        var itemPresets = databaseService.GetGlobals().ItemPresets;
        if (itemPresets.TryGetValue(key, out var byId))
        {
            return byId;   // id de preset explícito
        }

        var matches = itemPresets.Values.Where(p => p.Items?.FirstOrDefault()?.Template == key).ToList();
        if (matches.Count == 0)
        {
            return null;
        }

        var scoped = matches
            .Where(p => p.Items!.Any(i => IsRealOptic(i.Template)))
            .OrderBy(p => p.Items!.Count())
            .FirstOrDefault();
        if (scoped is not null)
        {
            return scoped;
        }

        return matches.FirstOrDefault(p => p.Encyclopedia is not null) ?? matches[0];
    }

    /// <summary>
    ///     Etapa 2: se a árvore (arma) não tem óptica real, adiciona uma mira mínima no 1º slot vazio
    ///     compatível — óptica direta, ou mount→óptica (2 níveis), sempre validando pelo `_props.Slots`
    ///     filter (compatibilidade garantida por construção). Sem slot compatível → mantém mira de ferro.
    /// </summary>
    private void EnsureMinimumOptic(List<Item> tree, string className)
    {
        if (tree.Any(i => IsRealOptic(i.Template)))
        {
            return;
        }

        foreach (var host in tree.ToList())
        {
            var slots = GetTemplate(host.Template)?.Properties?.Slots;   // ref: TemplateItem.cs:357
            if (slots is null)
            {
                continue;
            }

            foreach (var slot in slots)
            {
                if (string.IsNullOrEmpty(slot.Name))
                {
                    continue;
                }
                if (tree.Any(i => i.ParentId == host.Id.ToString() && i.SlotId == slot.Name))
                {
                    continue;   // slot já ocupado pelo preset
                }

                var filter = slot.Properties?.Filters?.FirstOrDefault()?.Filter;   // tpls permitidos (TemplateItem.cs:1776)
                if (filter is null || filter.Count == 0)
                {
                    continue;
                }

                // (a) óptica direta no slot (red dot simples preferido — CR-02-02)
                var direct = PickSimpleOptic(filter);
                if (direct is { } opticTpl)
                {
                    tree.Add(new Item { Id = new MongoId(), Template = opticTpl, ParentId = host.Id.ToString(), SlotId = slot.Name });
                    return;
                }

                // (b) mount → óptica (2 níveis)
                foreach (var mountTpl in filter)
                {
                    if (!itemHelper.IsOfBaseclass(mountTpl, BaseClasses.MOUNT))
                    {
                        continue;
                    }

                    var mountSlots = GetTemplate(mountTpl)?.Properties?.Slots;
                    if (mountSlots is null)
                    {
                        continue;
                    }

                    foreach (var ms in mountSlots)
                    {
                        if (string.IsNullOrEmpty(ms.Name))
                        {
                            continue;
                        }

                        var msFilter = ms.Properties?.Filters?.FirstOrDefault()?.Filter;
                        var scopeTpl = msFilter is not null ? PickSimpleOptic(msFilter) : null;
                        if (scopeTpl is { } sc)
                        {
                            var mountId = new MongoId();
                            tree.Add(new Item { Id = mountId, Template = mountTpl, ParentId = host.Id.ToString(), SlotId = slot.Name });
                            tree.Add(new Item { Id = new MongoId(), Template = sc, ParentId = mountId.ToString(), SlotId = ms.Name });
                            return;
                        }
                    }
                }
            }
        }

        logger.Debug($"[CustomClasses] '{className}': arma do stash sem slot de óptica compatível — mantém mira de ferro.");
    }

    /// <summary>Árvore manual: item raiz + mods recursivos por slotId.</summary>
    private List<Item> BuildManualTree(string tpl, List<ModSpec> mods, string parentId, string slotId)
    {
        var list = new List<Item>();
        var root = new Item { Id = new MongoId(), Template = new MongoId(tpl), ParentId = parentId, SlotId = slotId };
        list.Add(root);
        AddMods(list, root.Id.ToString(), mods);
        return list;
    }

    private void AddMods(List<Item> list, string parentId, List<ModSpec> mods)
    {
        foreach (var m in mods)
        {
            if (string.IsNullOrWhiteSpace(m.Tpl) || string.IsNullOrWhiteSpace(m.SlotId))
            {
                logger.Warning($"[CustomClasses] mod inválido (tpl/slotId ausente) — ignorado.");
                continue;
            }

            var child = new Item { Id = new MongoId(), Template = new MongoId(m.Tpl), ParentId = parentId, SlotId = m.SlotId };
            list.Add(child);
            if (m.Mods is { Count: > 0 })
            {
                AddMods(list, child.Id.ToString(), m.Mods);
            }
        }
    }

    /// <summary>
    ///     Fatia 3: carrega o carregador (filhos `cartridges`) e a câmara (`patron_in_weapon`) com `ammo`.
    ///     Reusa `ItemHelper.FillMagazineWithCartridge` (capacidade via `_props.Cartridges._max_count`).
    ///     CR-EP-01: `root` (a arma) vem do caller — slot equipado OU linha de stash/contents.
    /// </summary>
    private void LoadAmmo(List<Item> tree, ItemSpec spec, Item? root, string context, string className)
    {
        if (!spec.LoadedMag && !spec.Chambered)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(spec.Ammo))
        {
            logger.Warning($"[CustomClasses] '{className}': '{context}' loadedMag/chambered sem 'ammo' — ignorado (PA-01-03).");
            return;
        }

        var ammo = new MongoId(spec.Ammo);

        if (spec.LoadedMag)
        {
            var mag = tree.FirstOrDefault(i => i.SlotId == "mod_magazine");
            if (mag is null)
            {
                logger.Warning($"[CustomClasses] '{className}': '{context}' sem 'mod_magazine' — carregador não carregado.");
            }
            else if (tree.Any(i => i.ParentId == mag.Id.ToString() && i.SlotId == "cartridges"))
            {
                logger.Debug($"[CustomClasses] '{className}': '{context}' carregador do preset já tem cartuchos — fill pulado (CR-01-03).");
            }
            else
            {
                var magItem = itemHelper.GetItem(mag.Template);   // ref: ItemHelper.cs:494
                if (magItem is { Key: true, Value: not null })
                {
                    var magList = new List<Item> { mag };
                    // 1.0 = encher até a capacidade máxima (GetInt inclusivo — CR-01-05)  // ref: ItemHelper.cs:1342
                    itemHelper.FillMagazineWithCartridge(magList, magItem.Value, ammo, 1.0);
                    tree.AddRange(magList.Skip(1));   // adiciona os cartuchos (o mag já está na árvore)
                }
            }
        }

        if (spec.Chambered && root is not null)
        {
            var wpn = itemHelper.GetItem(root.Template);
            // slot real da câmara declarado no template (patron_in_weapon / _000…)  // ref: TemplateItem.cs:801
            var chamberSlot = wpn is { Key: true, Value: not null }
                ? wpn.Value.Properties?.Chambers?.FirstOrDefault()?.Name
                : null;

            if (string.IsNullOrEmpty(chamberSlot))   // CR-01-02: só chambrear se a arma tem câmara
            {
                logger.Warning($"[CustomClasses] '{className}': '{context}' arma sem slot de câmara — câmara ignorada.");
            }
            else
            {
                tree.Add(new Item
                {
                    Id = new MongoId(),
                    Template = ammo,
                    ParentId = root.Id.ToString(),
                    SlotId = chamberSlot,
                    Upd = new Upd { StackObjectsCount = 1 },
                });
            }
        }
    }

    /// <summary>
    ///     Empacota itens nas grades de um contêiner (stash/rig/mochila). CR-EP-01: linhas honram a MESMA
    ///     semântica dos slots equipados — `preset` explícito (premium opcional), árvore manual (`mods`),
    ///     `ammo` (loadedMag/chambered) e `contents` recursivo nas grades do item colocado. Sem
    ///     preset/mods: `tpl` auto-completa com o stash-preset (arma/armadura/...) ou vira item simples,
    ///     stack-aware (caminho tpl+count puro inalterado). Posição pela dimensão real do item montado
    ///     (com mods — InventoryHelper.GetItemSize); count&gt;1 composto = N árvores.
    /// </summary>
    private int PackSpecsIntoGrids(List<Item> items, string parentId, List<Grid> grids, List<ItemSpec> specs, string className)
    {
        if (grids.Count == 0)
        {
            logger.Warning($"[CustomClasses] '{className}': contêiner '{parentId}' sem grades — {specs.Count} item(ns) pulado(s).");
            return 0;
        }

        var packers = grids
            .Select(g => (Grid: g, Packer: new GridPacker(g.Properties?.CellsH ?? 0, g.Properties?.CellsV ?? 0)))
            .ToList();
        var added = 0;

        // item 038: itens com posição explícita primeiro — reservam a célula antes do auto-pack preencher
        // o resto (OrderBy estável preserva a ordem relativa original dentro de cada grupo).
        var ordered = specs.OrderByDescending(s => s.X.HasValue && s.Y.HasValue).ToList();
        foreach (var spec in ordered)
        {
            try
            {
                var count = Math.Max(1, spec.Count);

                // CR-EP-01 (1): `preset` explícito — mesma resolução do slot equipado (premium opcional).
                if (!string.IsNullOrWhiteSpace(spec.Preset))
                {
                    var preset = spec.Premium ? ResolvePremiumPreset(new MongoId(spec.Preset)) : ResolvePreset(new MongoId(spec.Preset));
                    if (preset?.Items is null || !preset.Items.Any())
                    {
                        logger.Warning($"[CustomClasses] '{className}': preset/arma '{spec.Preset}' não encontrado — item de contêiner pulado.");
                        continue;
                    }

                    added += PlaceSpecTrees(packers, items, parentId, spec, count, () => ClonePresetTree(preset), className);
                    continue;
                }

                // CR-EP-01 (2): árvore manual (`mods`) — mesmo builder do slot equipado.
                if (spec.Mods is not null)
                {
                    if (string.IsNullOrWhiteSpace(spec.Tpl))
                    {
                        logger.Warning($"[CustomClasses] '{className}': item de contêiner tem 'mods' mas não tem 'tpl' raiz — pulado.");
                        continue;
                    }

                    added += PlaceSpecTrees(packers, items, parentId, spec, count, () =>
                    {
                        // parentId/slotId da raiz são re-escritos pelo PlaceTree na colocação.
                        var tree = BuildManualTree(spec.Tpl!, spec.Mods!, parentId, "main");
                        return (tree, tree[0]);
                    }, className);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(spec.Tpl))
                {
                    logger.Warning($"[CustomClasses] '{className}': item de contêiner sem tpl — pulado.");
                    continue;
                }

                var tpl = new MongoId(spec.Tpl);
                var autoPreset = ResolveStashPreset(tpl);   // etapa 2: prefere preset com óptica p/ armas

                // Composto (arma/armadura/...): uma árvore montada por unidade.
                if (autoPreset?.Items is not null && autoPreset.Items.Any())
                {
                    added += PlaceSpecTrees(packers, items, parentId, spec, count, () => ClonePresetTree(autoPreset), className);
                    continue;
                }

                // Simples, stack-aware (caminho tpl+count puro — inalterado; CR-EP-01 só recursa contents).
                var ci = itemHelper.GetItem(tpl);   // ref: ItemHelper.cs:494
                var stackMax = (ci is { Key: true, Value: not null } ? ci.Value.Properties?.StackMaxSize : null) ?? 1;
                var remaining = count;
                var firstUnit = true;
                while (remaining > 0)
                {
                    var thisStack = stackMax > 1 ? Math.Min(remaining, stackMax) : 1;
                    var item = new Item { Id = new MongoId(), Template = tpl };
                    if (stackMax > 1)
                    {
                        item.Upd = new Upd { StackObjectsCount = thisStack };
                    }

                    // item 038: só a 1ª unidade honra a coord explícita; as demais cópias auto-empacotam.
                    var placed = firstUnit
                        ? PlaceTree(packers, items, parentId, [item], item, spec.X, spec.Y, spec.Rotated ?? false)
                        : PlaceTree(packers, items, parentId, [item], item);
                    firstUnit = false;
                    if (placed)
                    {
                        added++;
                        remaining -= thisStack;
                        // CR-EP-01 (5): contents também em contêiner simples (ex.: mochila sem preset).
                        if (spec.Contents is { Count: > 0 })
                        {
                            PackSpecsIntoGrids(items, item.Id.ToString(), GetGrids(item.Template), spec.Contents, className);
                        }
                    }
                    else
                    {
                        logger.Warning($"[CustomClasses] '{className}': sem espaço p/ '{spec.Tpl}' em '{parentId}' — {remaining} unidade(s) pulada(s).");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warning($"[CustomClasses] '{className}': item de contêiner '{spec.Tpl ?? spec.Preset}' falhou ({ex.Message}) — pulado.");
            }
        }

        return added;
    }

    /// <summary>
    ///     CR-EP-01: coloca <paramref name="count"/> unidades de uma árvore composta (preset explícito,
    ///     manual ou auto-preset) nas grades, espelhando o slot equipado: mira mínima p/ armas
    ///     (EnsureMinimumOptic), `ammo` (LoadAmmo) e recursão de `contents` nas grades do item colocado.
    ///     Specs tpl+count puros passam ilesos (LoadAmmo/contents são no-op sem os campos).
    /// </summary>
    private int PlaceSpecTrees(List<(Grid Grid, GridPacker Packer)> packers, List<Item> items, string parentId,
        ItemSpec spec, int count, Func<(List<Item> Tree, Item Root)> buildUnit, string className)
    {
        var label = spec.Tpl ?? spec.Preset ?? "?";
        var placed = 0;
        for (var i = 0; i < count; i++)
        {
            var (tree, root) = buildUnit();
            // etapa 2: garantir mira mínima em armas do stash sem óptica (snipers etc.).
            if (itemHelper.IsOfBaseclass(root.Template, BaseClasses.WEAPON))
            {
                EnsureMinimumOptic(tree, className);
            }

            // CR-EP-01 (4): carregador/câmara em linha de stash/contents (cartuchos não mudam o footprint).
            LoadAmmo(tree, spec, root, $"stash:{label}", className);

            // item 038: 1ª unidade honra a coord explícita (opt-in); cópias seguintes auto-empacotam.
            var placedOk = i == 0
                ? PlaceTree(packers, items, parentId, tree, root, spec.X, spec.Y, spec.Rotated ?? false)
                : PlaceTree(packers, items, parentId, tree, root);
            if (!placedOk)
            {
                logger.Warning($"[CustomClasses] '{className}': sem espaço p/ '{label}' (montado) em '{parentId}' — {count - i} unidade(s) pulada(s).");
                break;
            }

            placed++;
            // CR-EP-01 (5): contents recursivo — mesmo caminho que o slot equipado usa (Apply).
            if (spec.Contents is { Count: > 0 })
            {
                PackSpecsIntoGrids(items, root.Id.ToString(), GetGrids(root.Template), spec.Contents, className);
            }
        }

        return placed;
    }

    /// <summary>
    ///     Posiciona a raiz de uma árvore na 1ª grade onde couber (first-fit + rotação), usando a dimensão
    ///     REAL do item montado (InventoryHelper.GetItemSize considera ExtraSize dos mods). Em sucesso,
    ///     seta parentId/slotId/location da raiz e adiciona a árvore inteira a `dest`.
    /// </summary>
    private bool PlaceTree(List<(Grid Grid, GridPacker Packer)> packers, List<Item> dest, string parentId, List<Item> tree, Item root,
        int? wantX = null, int? wantY = null, bool wantRotated = false)
    {
        var (w, h) = inventoryHelper.GetItemSize(root.Template, root.Id, tree);   // ref: InventoryHelper.cs:609

        // item 038: posição explícita (opt-in). Coloca na célula pedida com a dimensão REAL (mods incl.);
        // se não couber mais (mod mudou o footprint, colisão), cai no auto-pack abaixo — nunca dropa o item.
        if (wantX is int wx && wantY is int wy)
        {
            foreach (var p in packers)
            {
                if (p.Packer.TryPlaceAt(wx, wy, w, h, wantRotated))
                {
                    root.ParentId = parentId;
                    root.SlotId = p.Grid.Name ?? "main";
                    root.Location = new ItemLocation
                    {
                        X = wx,
                        Y = wy,
                        R = wantRotated ? ItemRotation.Vertical : ItemRotation.Horizontal,
                    };
                    dest.AddRange(tree);
                    return true;
                }
            }
        }

        foreach (var p in packers)
        {
            var pos = p.Packer.Place(w, h);
            if (pos is null)
            {
                continue;
            }

            root.ParentId = parentId;
            root.SlotId = p.Grid.Name ?? "main";
            root.Location = new ItemLocation
            {
                X = pos.Value.X,
                Y = pos.Value.Y,
                R = pos.Value.Rotated ? ItemRotation.Vertical : ItemRotation.Horizontal,
            };
            dest.AddRange(tree);
            return true;
        }

        return false;
    }

    /// <summary>Grades (`_props.Grids`) de um contêiner pelo seu tpl.</summary>
    private List<Grid> GetGrids(MongoId tpl)
    {
        var ci = itemHelper.GetItem(tpl);   // ref: ItemHelper.cs:494; Grids em TemplateItem.cs:353
        return ci is { Key: true, Value: not null } ? ci.Value.Properties?.Grids?.ToList() ?? [] : [];
    }

    /// <summary>PA-02-01: remove o item que ocupa (parentId, slotId), com toda a subárvore.</summary>
    private static void RemoveSlotOccupant(List<Item> items, string parentId, string slotId)
    {
        var occupant = items.FirstOrDefault(i => i.ParentId == parentId && i.SlotId == slotId);
        if (occupant is not null)
        {
            RemoveItemAndChildren(items, occupant.Id);
        }
    }

    /// <summary>PA-02-02: remove recursivamente um item e tudo aninhado nele (por `parentId`).</summary>
    private static void RemoveItemAndChildren(List<Item> items, MongoId id)
    {
        var idStr = id.ToString();
        var childIds = items.Where(i => i.ParentId == idStr).Select(i => i.Id).ToList();
        foreach (var childId in childIds)
        {
            RemoveItemAndChildren(items, childId);
        }

        items.RemoveAll(i => i.Id == id);
    }
}
