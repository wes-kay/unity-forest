#!/usr/bin/env python3
"""
BaseCharacter YAML Generator
Interactively configure or randomize attributes, then outputs a valid YAML file.
"""

import yaml
import random
import sys
import os
from copy import deepcopy

# ── Attribute registry ────────────────────────────────────────────────────────

# (uid, default_float, min, max) — only cvit/mvit have non-zero defaults
ATTRIBUTES = [
    # Core stats
    ("strn", 0, 0, 0), ("endu", 0, 0, 0), ("cons", 0, 0, 0),
    ("agil", 0, 0, 0), ("dext", 0, 0, 0), ("refl", 0, 0, 0),
    ("perc", 0, 0, 0), ("memo", 0, 0, 0),
    # Vitality / pools
    ("cvit", 4, 4, 4), ("mvit", 10, 10, 10),
    ("csta", 0, 0, 0), ("msta", 0, 0, 0),
    ("cwil", 0, 0, 0), ("mwil", 0, 0, 0),
    ("cfoc", 0, 0, 0), ("mfoc", 0, 0, 0),
    ("cbld", 0, 0, 0), ("mbld", 0, 0, 0),
    ("csan", 0, 0, 0), ("msan", 0, 0, 0),
    ("cmor", 0, 0, 0), ("mmor", 0, 0, 0),
    ("chng", 0, 0, 0), ("mhng", 0, 0, 0),
    ("cthr", 0, 0, 0), ("mthr", 0, 0, 0),
    ("cfat", 0, 0, 0), ("mfat", 0, 0, 0),
    ("cexp", 0, 0, 0), ("mexp", 0, 0, 0),
    ("cwrk", 0, 0, 0), ("mwrk", 0, 0, 0),
    ("cbur", 0, 0, 0), ("mbur", 0, 0, 0),
    # Body locations
    ("ched", 0, 0, 0), ("mhed", 0, 0, 0),
    ("ctor", 0, 0, 0), ("mtor", 0, 0, 0),
    ("clar", 0, 0, 0), ("mlar", 0, 0, 0),
    ("crar", 0, 0, 0), ("mrar", 0, 0, 0),
    ("cllg", 0, 0, 0), ("mllg", 0, 0, 0),
    ("crlg", 0, 0, 0), ("mrlg", 0, 0, 0),
    # Status conditions
    ("cpai", 0, 0, 0), ("mpai", 0, 0, 0),
    ("cinf", 0, 0, 0), ("minf", 0, 0, 0),
    ("ccor", 0, 0, 0), ("mcor", 0, 0, 0),
    ("cfvr", 0, 0, 0), ("mfvr", 0, 0, 0),
    ("cstr", 0, 0, 0), ("mstr", 0, 0, 0),
    ("cble", 0, 0, 0), ("mble", 0, 0, 0),
    ("cven", 0, 0, 0), ("mven", 0, 0, 0),
    ("ccur", 0, 0, 0), ("mcur", 0, 0, 0),
    ("cmad", 0, 0, 0), ("mmad", 0, 0, 0),
    ("cmut", 0, 0, 0), ("mmut", 0, 0, 0),
    ("cpos", 0, 0, 0), ("mpos", 0, 0, 0),
    # Resistances
    ("pres", 0, 0, 0), ("ires", 0, 0, 0), ("corr", 0, 0, 0),
    ("sres", 0, 0, 0), ("fres", 0, 0, 0), ("mres", 0, 0, 0),
    ("bres", 0, 0, 0), ("vres", 0, 0, 0), ("dres", 0, 0, 0),
    ("cold", 0, 0, 0), ("heat", 0, 0, 0),
    ("firs", 0, 0, 0), ("ices", 0, 0, 0),
    ("shrs", 0, 0, 0), ("sors", 0, 0, 0),
    ("cars", 0, 0, 0), ("hers", 0, 0, 0),
    ("kres", 0, 0, 0), ("stun", 0, 0, 0),
    ("tors", 0, 0, 0), ("poss", 0, 0, 0),
    # Combat secondary
    ("cfai", 0, 0, 0), ("mfai", 0, 0, 0),
    ("cher", 0, 0, 0), ("mher", 0, 0, 0),
    ("cdar", 0, 0, 0), ("mdar", 0, 0, 0),
    ("armr", 0, 0, 0), ("evas", 0, 0, 0),
    ("bloc", 0, 0, 0), ("crit", 0, 0, 0),
    ("crdm", 0, 0, 0), ("accu", 0, 0, 0),
    ("move", 0, 0, 0), ("init", 0, 0, 0),
    ("heal", 0, 0, 0), ("regn", 0, 0, 0),
    ("carr", 0, 0, 0),
    # Weapon skills
    ("swrd", 0, 0, 0), ("axem", 0, 0, 0), ("spea", 0, 0, 0),
    ("mace", 0, 0, 0), ("dagg", 0, 0, 0), ("arch", 0, 0, 0),
    ("xbow", 0, 0, 0), ("thrw", 0, 0, 0), ("unar", 0, 0, 0),
    ("parr", 0, 0, 0), ("dodg", 0, 0, 0), ("tact", 0, 0, 0),
    ("shld", 0, 0, 0), ("dual", 0, 0, 0),
    # Dark skills
    ("exec", 0, 0, 0), ("tort", 0, 0, 0), ("inti", 0, 0, 0),
    # Survival skills
    ("fish", 0, 0, 0), ("hunt", 0, 0, 0), ("trap", 0, 0, 0),
    ("skin", 0, 0, 0), ("butc", 0, 0, 0), ("trac", 0, 0, 0),
    ("fora", 0, 0, 0), ("camp", 0, 0, 0), ("navi", 0, 0, 0),
    ("swim", 0, 0, 0), ("clim", 0, 0, 0), ("surv", 0, 0, 0),
    ("scav", 0, 0, 0), ("corp", 0, 0, 0),
    # Gathering
    ("mine", 0, 0, 0), ("lumb", 0, 0, 0), ("harv", 0, 0, 0),
    ("herb", 0, 0, 0), ("bldh", 0, 0, 0), ("bone", 0, 0, 0),
    ("reli", 0, 0, 0),
    # Crafting
    ("blks", 0, 0, 0), ("wpsm", 0, 0, 0), ("arsm", 0, 0, 0),
    ("tail", 0, 0, 0), ("leat", 0, 0, 0), ("bons", 0, 0, 0),
    ("fles", 0, 0, 0), ("alch", 0, 0, 0), ("pois", 0, 0, 0),
    ("trmk", 0, 0, 0), ("rune", 0, 0, 0), ("ench", 0, 0, 0),
    ("engi", 0, 0, 0),
    # Medicine
    ("faid", 0, 0, 0), ("surg", 0, 0, 0), ("anat", 0, 0, 0),
    ("diag", 0, 0, 0), ("toxi", 0, 0, 0), ("emba", 0, 0, 0),
    ("auto", 0, 0, 0), ("ampu", 0, 0, 0),
    # Lore / knowledge
    ("read", 0, 0, 0), ("writ", 0, 0, 0), ("rese", 0, 0, 0),
    ("inve", 0, 0, 0), ("hist", 0, 0, 0), ("laws", 0, 0, 0),
    ("theo", 0, 0, 0), ("occu", 0, 0, 0), ("demo", 0, 0, 0),
    ("necr", 0, 0, 0), ("astr", 0, 0, 0), ("ritu", 0, 0, 0),
    ("mons", 0, 0, 0),
    # Social
    ("pers", 0, 0, 0), ("nego", 0, 0, 0), ("lead", 0, 0, 0),
    ("dece", 0, 0, 0), ("etiq", 0, 0, 0), ("inte", 0, 0, 0),
    ("comm", 0, 0, 0), ("fana", 0, 0, 0),
    # Stealth / crime
    ("stea", 0, 0, 0), ("pick", 0, 0, 0), ("lock", 0, 0, 0),
    ("forg", 0, 0, 0), ("espi", 0, 0, 0), ("saba", 0, 0, 0),
    ("assa", 0, 0, 0), ("smug", 0, 0, 0),
    # Trade
    ("trad", 0, 0, 0), ("appr", 0, 0, 0), ("bart", 0, 0, 0),
    ("logi", 0, 0, 0),
    # Faith
    ("fait", 0, 0, 0), ("pray", 0, 0, 0), ("medi", 0, 0, 0),
    ("exor", 0, 0, 0), ("bles", 0, 0, 0), ("curs", 0, 0, 0),
    ("sacr", 0, 0, 0),
    # Magic schools
    ("arca", 0, 0, 0), ("fire", 0, 0, 0), ("icem", 0, 0, 0),
    ("blod", 0, 0, 0), ("necm", 0, 0, 0), ("summ", 0, 0, 0),
    ("illu", 0, 0, 0), ("divi", 0, 0, 0), ("shad", 0, 0, 0),
    ("spir", 0, 0, 0), ("hexc", 0, 0, 0), ("plag", 0, 0, 0),
    ("soul", 0, 0, 0), ("dark", 0, 0, 0), ("drea", 0, 0, 0),
    ("void", 0, 0, 0), ("chao", 0, 0, 0), ("grav", 0, 0, 0),
    # Ritual / misc
    ("mort", 0, 0, 0), ("buri", 0, 0, 0), ("pilg", 0, 0, 0),
    ("vigi", 0, 0, 0), ("here", 0, 0, 0), ("cann", 0, 0, 0),
    ("blst", 0, 0, 0), ("muta", 0, 0, 0), ("rean", 0, 0, 0),
    ("obse", 0, 0, 0),
]

ATTR_UIDS = [a[0] for a in ATTRIBUTES]
ATTR_DEFAULTS = {a[0]: a[1] for a in ATTRIBUTES}

# ── Helpers ───────────────────────────────────────────────────────────────────

def clr(code, text): return f"\033[{code}m{text}\033[0m"
def bold(t): return clr("1", t)
def cyan(t): return clr("36", t)
def green(t): return clr("32", t)
def yellow(t): return clr("33", t)
def red(t): return clr("31", t)
def dim(t): return clr("2", t)

def prompt(msg, default=None):
    suffix = f" [{default}]" if default is not None else ""
    try:
        val = input(f"  {msg}{suffix}: ").strip()
    except (EOFError, KeyboardInterrupt):
        print()
        sys.exit(0)
    return val if val else str(default) if default is not None else ""

def prompt_float(msg, default=0.0):
    while True:
        raw = prompt(msg, default)
        try:
            return float(raw)
        except ValueError:
            print(red(f"    ✗ Enter a number."))

def prompt_int(msg, default=0):
    while True:
        raw = prompt(msg, default)
        try:
            return int(raw)
        except ValueError:
            print(red(f"    ✗ Enter an integer."))

def prompt_bool(msg, default=True):
    d = "y" if default else "n"
    while True:
        raw = prompt(f"{msg} (y/n)", d).lower()
        if raw in ("y", "yes", "1"): return True
        if raw in ("n", "no", "0"): return False
        print(red("    ✗ Enter y or n."))

def divider(title=""):
    width = 60
    if title:
        pad = (width - len(title) - 2) // 2
        print(f"\n{dim('─' * pad)} {bold(title)} {dim('─' * pad)}")
    else:
        print(dim("─" * width))

def make_attr_block(uid, float_val, min_val=None, max_val=None):
    fv = float_val
    mn = fv if min_val is None else min_val
    mx = fv if max_val is None else max_val
    return {
        "uid": uid,
        "stringValue": 0,
        "floatValue": fv,
        "locked": 0,
        "randomChange": 100,
        "isFixed": 1,
        "minValue": mn,
        "maxValue": mx,
        "mAttributeSetting": {"rid": -2},
    }

# ── Attribute group selection ─────────────────────────────────────────────────

GROUPS = {
    "Core Stats":       ["strn","endu","cons","agil","dext","refl","perc","memo"],
    "Vitality/Pools":   ["cvit","mvit","csta","msta","cwil","mwil","cfoc","mfoc",
                         "cbld","mbld","csan","msan","cmor","mmor","chng","mhng",
                         "cthr","mthr","cfat","mfat","cexp","mexp","cwrk","mwrk","cbur","mbur"],
    "Body Locations":   ["ched","mhed","ctor","mtor","clar","mlar","crar","mrar",
                         "cllg","mllg","crlg","mrlg"],
    "Status/Conditions":["cpai","mpai","cinf","minf","ccor","mcor","cfvr","mfvr",
                         "cstr","mstr","cble","mble","cven","mven","ccur","mcur",
                         "cmad","mmad","cmut","mmut","cpos","mpos"],
    "Resistances":      ["pres","ires","corr","sres","fres","mres","bres","vres",
                         "dres","cold","heat","firs","ices","shrs","sors","cars",
                         "hers","kres","stun","tors","poss"],
    "Combat":           ["cfai","mfai","cher","mher","cdar","mdar","armr","evas",
                         "bloc","crit","crdm","accu","move","init","heal","regn","carr"],
    "Weapon Skills":    ["swrd","axem","spea","mace","dagg","arch","xbow","thrw",
                         "unar","parr","dodg","tact","shld","dual","exec","tort","inti"],
    "Survival":         ["fish","hunt","trap","skin","butc","trac","fora","camp",
                         "navi","swim","clim","surv","scav","corp"],
    "Gathering":        ["mine","lumb","harv","herb","bldh","bone","reli"],
    "Crafting":         ["blks","wpsm","arsm","tail","leat","bons","fles","alch",
                         "pois","trmk","rune","ench","engi"],
    "Medicine":         ["faid","surg","anat","diag","toxi","emba","auto","ampu"],
    "Lore/Knowledge":   ["read","writ","rese","inve","hist","laws","theo","occu",
                         "demo","necr","astr","ritu","mons"],
    "Social":           ["pers","nego","lead","dece","etiq","inte","comm","fana"],
    "Stealth/Crime":    ["stea","pick","lock","forg","espi","saba","assa","smug"],
    "Trade":            ["trad","appr","bart","logi"],
    "Faith":            ["fait","pray","medi","exor","bles","curs","sacr"],
    "Magic":            ["arca","fire","icem","blod","necm","summ","illu","divi",
                         "shad","spir","hexc","plag","soul","dark","drea","void",
                         "chao","grav"],
    "Ritual/Misc":      ["mort","buri","pilg","vigi","here","cann","blst","muta",
                         "rean","obse"],
}

GROUP_NAMES = list(GROUPS.keys())

# ── Main flow ─────────────────────────────────────────────────────────────────

def select_groups_to_configure():
    divider("ATTRIBUTE GROUPS")
    print(f"  {dim('Choose which groups to manually configure.')}")
    print(f"  {dim('All others will use default (0) or be randomized.')}\n")
    for i, name in enumerate(GROUP_NAMES, 1):
        uids_preview = ", ".join(GROUPS[name][:4])
        ellipsis = "…" if len(GROUPS[name]) > 4 else ""
        print(f"  {cyan(str(i).rjust(2))}. {bold(name):22s}  {dim(uids_preview + ellipsis)}")
    print()
    raw = prompt("Enter group numbers to configure (e.g. 1,3,5) or 'all' or 'none'", "none")
    if raw.lower() == "all":
        return set(GROUP_NAMES)
    if raw.lower() in ("none", ""):
        return set()
    selected = set()
    for part in raw.replace(" ", "").split(","):
        try:
            idx = int(part) - 1
            if 0 <= idx < len(GROUP_NAMES):
                selected.add(GROUP_NAMES[idx])
        except ValueError:
            pass
    return selected

def configure_group(group_name, uids, rand_uids):
    divider(group_name)
    print(f"  For each attribute enter a value, 'r' to randomize, or press Enter for default.\n")
    values = {}
    for uid in uids:
        default = ATTR_DEFAULTS[uid]
        raw = prompt(f"{cyan(uid):8s}", default).strip()
        if raw.lower() == "r":
            rand_uids.add(uid)
            print(f"    {yellow('→ will be randomized')}")
        else:
            try:
                values[uid] = float(raw) if raw else float(default)
            except ValueError:
                values[uid] = float(default)
    return values

def configure_randomized(rand_uids):
    if not rand_uids:
        return {}
    divider("RANDOMIZE SETTINGS")
    print(f"  Set min/max ranges for randomized attributes.\n")
    ranges = {}
    for uid in sorted(rand_uids):
        print(f"  {cyan(uid)}")
        mn = prompt_float("    min", 0)
        mx = prompt_float("    max", 10)
        if mx < mn:
            mx = mn
        ranges[uid] = (mn, mx)
    return ranges

def get_entity_settings():
    divider("ENTITY SETTINGS")
    uid = prompt("Entity uid", "BaseCharacter")
    upgrade = prompt_int("AttributesUpgradeLevel", 0)
    apply_transform = 1 if prompt_bool("ApplyTransformDataWhenInstantiate", True) else 0
    available = 1 if prompt_bool("AvailableForInteraction", True) else 0
    multiple = 1 if prompt_bool("MultipleInstances", False) else 0
    return {
        "uid": uid,
        "AttributesUpgradeLevel": upgrade,
        "ApplyTransformDataWhenInstantiate": apply_transform,
        "AvailableForInteraction": available,
        "MultipleInstances": multiple,
    }

def get_output_path():
    divider("OUTPUT")
    path = prompt("Output filename", "character.yaml")
    if not path.endswith(".yaml") and not path.endswith(".yml"):
        path += ".yaml"
    return path

# ── Inventory builder ─────────────────────────────────────────────────────────

EMPTY_ITEM = {
    "uid": "", "name": "", "description": "", "type": 0,
    "icon": {"instanceID": 0}, "iconLoadMethod": 0, "iconPath": "",
    "quality": 0, "tradeable": True, "deletable": True,
    "useable": False, "consumable": False, "visible": True,
    "price": 0, "currency": 0, "maxiumStack": 99, "upgradeLevel": 0,
    "weight": 0.10000000149011612, "dropRates": 0, "favorite": False,
    "attributes": [], "maximumRandomAttributes": 5, "enchantments": [],
    "craftMaterials": [], "actions": [], "tags": [], "customData": [],
    "restrictionKey": "", "restrictionValue": 0,
    "socketingSlots": 0, "socketedItems": [], "socketingTag": [], "fold": True,
}

EMPTY_STACK = {"Item": EMPTY_ITEM, "Number": 0, "Empty": True, "Fold": True}

CURRENCY_LABELS = {
    0: "Scrap",
    1: "Grim Token",
    2: "Bleeding Tithe",
    3: "Severed Oath",
    4: "Grinding Cog",
    5: "Iron Precept",
    6: "Obsidian Crown",
}

def make_empty_stacks(count):
    import copy
    return [copy.deepcopy(EMPTY_STACK) for _ in range(count)]

def configure_inventory(entity_uid):
    import json

    divider("GENERAL INVENTORY  (Type 4)")
    print(f"  {dim('The main carry inventory — items, loot, equipment picks.')}\n")

    inv_name_4   = prompt("Inventory name", "Inventory")
    inv_size_4   = prompt_int("Number of slots", 10)
    max_weight_4 = prompt_float("Max carry weight", 1000.0)
    sell_mult_4  = prompt_float("Sell price multiplier", 1.0)
    buy_mult_4   = prompt_float("Buy price multiplier", 1.0)
    trade_all_4  = prompt_bool("Trade all items?", True)

    print(f"\n  {dim('Starting currency (enter 0 to skip each):')}")
    currency_vals = []
    for idx, label in CURRENCY_LABELS.items():
        val = prompt_int(f"  {label}", 0)
        currency_vals.append(val)
    keep_positive_4 = prompt_bool("Keep currency positive (prevent negatives)?", True)

    divider("EQUIPMENT INVENTORY  (Type 5)")
    print(f"  {dim('The equipment/gear slots inventory — typically no stacks.')}\n")

    inv_name_5   = prompt("Inventory name", "Equipment")
    inv_size_5   = prompt_int("Number of slots", 10)
    max_weight_5 = prompt_float("Max carry weight", 1000.0)
    sell_mult_5  = prompt_float("Sell price multiplier", 1.0)
    buy_mult_5   = prompt_float("Buy price multiplier", 1.0)
    trade_all_5  = prompt_bool("Trade all items?", True)

    print(f"\n  {dim('Starting currency:')}")
    currency_vals_5 = []
    for idx, label in CURRENCY_LABELS.items():
        val = prompt_int(f"  {label}", 0)
        currency_vals_5.append(val)
    keep_positive_5 = prompt_bool("Keep currency positive?", True)

    # Build the two inventory dicts
    inv4 = {
        "Name": inv_name_4,
        "EntityUid": entity_uid,
        "Type": 4,
        "Stacks": make_empty_stacks(inv_size_4),
        "HiddenStacks": [],
        "Currency": {"Currency": currency_vals, "KeepPostive": keep_positive_4},
        "InventorySize": inv_size_4,
        "MaxiumCarryWeight": float(max_weight_4),
        "SellPriceMultiplier": float(sell_mult_4),
        "BuyPriceMultiplier": float(buy_mult_4),
        "SpecificPriceMultiplier": [],
        "TradeAllItems": trade_all_4,
        "TradeList": [],
        "TradeCategoryList": [],
    }

    inv5 = {
        "Name": inv_name_5,
        "EntityUid": entity_uid,
        "Type": 5,
        "Stacks": [],
        "HiddenStacks": [],
        "Currency": {"Currency": currency_vals_5, "KeepPostive": keep_positive_5},
        "InventorySize": inv_size_5,
        "MaxiumCarryWeight": float(max_weight_5),
        "SellPriceMultiplier": float(sell_mult_5),
        "BuyPriceMultiplier": float(buy_mult_5),
        "SpecificPriceMultiplier": [],
        "TradeAllItems": trade_all_5,
        "TradeList": [],
        "TradeCategoryList": [],
    }

    module_data = {
        "entity": {"rid": -2},
        "InventorySave": [],
        "Inventory": [inv4, inv5],
        "lootpackFold": True,
        "LootPacks": [],
        "references": {
            "version": 2,
            "RefIds": [{"rid": -2, "type": {"class": "", "ns": "", "asm": ""}, "data": {}}],
        },
    }

    return json.dumps(module_data, separators=(",", ":"))

# ── Document builder ──────────────────────────────────────────────────────────

def build_yaml(entity_settings, fixed_values, rand_ranges, inventory_json):
    attrs = []
    for uid, default_f, *_ in ATTRIBUTES:
        if uid in rand_ranges:
            mn, mx = rand_ranges[uid]
            fv = round(random.uniform(mn, mx), 4)
            attrs.append(make_attr_block(uid, fv, mn, mx))
        else:
            fv = fixed_values.get(uid, default_f)
            attrs.append(make_attr_block(uid, fv))

    doc = {
        "uid": entity_settings["uid"],
        "Attributes": attrs,
        "AttributesUpgradeLevel": entity_settings["AttributesUpgradeLevel"],
        "ApplyTransformDataWhenInstantiate": entity_settings["ApplyTransformDataWhenInstantiate"],
        "Position": {"x": 0, "y": 0, "z": 0},
        "Forward":  {"x": 0, "y": 0, "z": 1},
        "Scale":    {"x": 1, "y": 1, "z": 1},
        "Tags": [],
        "AvailableForInteraction": entity_settings["AvailableForInteraction"],
        "MultipleInstances": entity_settings["MultipleInstances"],
        "CustomFloat": [],
        "CustomInt": None,
        "CustomBool": None,
        "CustomString": [],
        "CustomIntList": [],
        "CustomIdIntList": [],
        "CustomIdFloatList": [],
        "Modules": [
            {
                "type": "SoftKitty.InventoryEngine.InventoryModule",
                "jsonData": inventory_json,
            }
        ],
    }
    return [{"- uid": doc}]  # match source list-of-entity format

def write_yaml(data, path):
    # Custom representer for cleaner output
    def represent_none(dumper, _):
        return dumper.represent_scalar("tag:yaml.org,2002:null", "")

    yaml.add_representer(type(None), represent_none)

    # Build manually to match source structure
    lines = []
    entity = data[0]["- uid"]  # our internal key trick

    def attr_block(a):
        return (
            f"      - uid: {a['uid']}\n"
            f"        stringValue: {a['stringValue']}\n"
            f"        floatValue: {a['floatValue']}\n"
            f"        locked: {a['locked']}\n"
            f"        randomChange: {a['randomChange']}\n"
            f"        isFixed: {a['isFixed']}\n"
            f"        minValue: {a['minValue']}\n"
            f"        maxValue: {a['maxValue']}\n"
            f"        mAttributeSetting:\n"
            f"          rid: -2\n"
        )

    lines.append(f"  - uid: {entity['uid']}")
    lines.append(f"    Attributes:")
    for a in entity["Attributes"]:
        lines.append(attr_block(a).rstrip())
    lines.append(f"    AttributesUpgradeLevel: {entity['AttributesUpgradeLevel']}")
    lines.append(f"    ApplyTransformDataWhenInstantiate: {entity['ApplyTransformDataWhenInstantiate']}")
    p = entity["Position"]
    lines.append(f"    Position: {{x: {p['x']}, y: {p['y']}, z: {p['z']}}}")
    f_ = entity["Forward"]
    lines.append(f"    Forward: {{x: {f_['x']}, y: {f_['y']}, z: {f_['z']}}}")
    s = entity["Scale"]
    lines.append(f"    Scale: {{x: {s['x']}, y: {s['y']}, z: {s['z']}}}")
    lines.append(f"    Tags: []")
    lines.append(f"    AvailableForInteraction: {entity['AvailableForInteraction']}")
    lines.append(f"    MultipleInstances: {entity['MultipleInstances']}")
    lines.append(f"    CustomFloat: []")
    lines.append(f"    CustomInt: ")
    lines.append(f"    CustomBool: ")
    lines.append(f"    CustomString: []")
    lines.append(f"    CustomIntList: []")
    lines.append(f"    CustomIdIntList: []")
    lines.append(f"    CustomIdFloatList: []")
    lines.append(f"    Modules:")
    for mod in entity["Modules"]:
        lines.append(f"      - type: {mod['type']}")
        lines.append(f"        jsonData: '{mod['jsonData']}'")

    with open(path, "w") as fh:
        fh.write("\n".join(lines) + "\n")

# ── Entry point ───────────────────────────────────────────────────────────────

def main():
    print()
    print(bold("  ╔══════════════════════════════════════════╗"))
    print(bold("  ║      BaseCharacter YAML Generator        ║"))
    print(bold("  ╚══════════════════════════════════════════╝"))
    print()

    entity_settings = get_entity_settings()

    # Select groups to manually configure
    config_groups = select_groups_to_configure()

    fixed_values = {}
    rand_uids = set()

    for group_name in GROUP_NAMES:
        if group_name in config_groups:
            vals = configure_group(group_name, GROUPS[group_name], rand_uids)
            fixed_values.update(vals)

    rand_ranges = {}
    if rand_uids:
        rand_ranges = configure_randomized(rand_uids)

    # Ask about randomizing entire unconfigured groups
    unconfigured = [g for g in GROUP_NAMES if g not in config_groups]
    if unconfigured:
        divider("UNCONFIGURED GROUPS")
        print(f"  {len(unconfigured)} group(s) were not configured.")
        randomize_rest = prompt_bool("Randomize all unconfigured attributes?", False)
        if randomize_rest:
            rmin = prompt_float("  Global random min", 0)
            rmax = prompt_float("  Global random max", 10)
            for g in unconfigured:
                for uid in GROUPS[g]:
                    if uid not in fixed_values and uid not in rand_uids:
                        rand_ranges[uid] = (rmin, rmax)

    inventory_json = configure_inventory(entity_settings["uid"])

    output_path = get_output_path()

    data = build_yaml(entity_settings, fixed_values, rand_ranges, inventory_json)
    write_yaml(data, output_path)

    divider()
    print(f"\n  {green('✓')} Written to {bold(output_path)}")
    configured_count = len(fixed_values)
    randomized_count = len(rand_ranges)
    default_count = len(ATTR_UIDS) - configured_count - randomized_count
    print(f"  {dim(f'{configured_count} configured  •  {randomized_count} randomized  •  {default_count} defaulted')}\n")

if __name__ == "__main__":
    main()
