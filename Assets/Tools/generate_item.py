#!/usr/bin/env python3
"""
Item YAML Generator
Interactively builds a single item entry matching the SoftKitty Inventory Engine format.
Reads attribute UIDs from GameAttributeObject.asset.
Maintains a growing actions list in item_actions.json.
"""

import json
import os
import re
import sys

# ── Config ────────────────────────────────────────────────────────────────────

DEFAULT_ASSET_PATH = r"C:\Users\jmgek\unity-forest-guassian\Assets\Assets\Data\GameAttributeObject.asset"
ACTIONS_FILE = os.path.join(os.path.dirname(os.path.abspath(__file__)), "item_actions.json")

CURRENCY_LABELS = {
    0: "Scrap",
    1: "Grim Token",
    2: "Bleeding Tithe",
    3: "Severed Oath",
    4: "Grinding Cog",
    5: "Iron Precept",
    6: "Obsidian Crown",
}

# ── Terminal helpers ──────────────────────────────────────────────────────────

def clr(code, text): return f"\033[{code}m{text}\033[0m"
def bold(t): return clr("1", t)
def cyan(t): return clr("36", t)
def green(t): return clr("32", t)
def yellow(t): return clr("33", t)
def dim(t): return clr("2", t)
def red(t): return clr("31", t)

def prompt(msg, default=None):
    suffix = f" [{default}]" if default is not None else ""
    try:
        val = input(f"  {msg}{suffix}: ").strip()
    except (EOFError, KeyboardInterrupt):
        print()
        sys.exit(0)
    return val if val else (str(default) if default is not None else "")

def prompt_float(msg, default=0.0):
    while True:
        raw = prompt(msg, default)
        try:
            return float(raw)
        except ValueError:
            print(red("    ✗ Enter a number."))

def prompt_int(msg, default=0):
    while True:
        raw = prompt(msg, default)
        try:
            return int(raw)
        except ValueError:
            print(red("    ✗ Enter an integer."))

def prompt_bool(msg, default=True):
    d = "y" if default else "n"
    while True:
        raw = prompt(f"{msg} (y/n)", d).lower()
        if raw in ("y", "yes", "1"): return True
        if raw in ("n", "no",  "0"): return False
        print(red("    ✗ Enter y or n."))

def divider(title=""):
    width = 60
    if title:
        pad = (width - len(title) - 2) // 2
        print(f"\n{dim('─' * pad)} {bold(title)} {dim('─' * pad)}")
    else:
        print(dim("─" * width))

# ── Asset reader ──────────────────────────────────────────────────────────────

def load_attribute_uids(asset_path):
    """Parse uid entries from a Unity .asset YAML file."""
    try:
        with open(asset_path, "r", encoding="utf-8") as f:
            content = f.read()
        uids = re.findall(r"^\s*uid:\s*(\S+)", content, re.MULTILINE)
        # Deduplicate, preserve order, drop empty/numeric-only
        seen = set()
        result = []
        for u in uids:
            if u and u not in seen and not u.lstrip("-").isdigit():
                seen.add(u)
                result.append(u)
        return result
    except FileNotFoundError:
        print(yellow(f"  ⚠  Asset not found: {asset_path}"))
        print(yellow("     Attribute UID list will be unavailable — you can still type UIDs freely.\n"))
        return []
    except Exception as e:
        print(yellow(f"  ⚠  Could not read asset: {e}\n"))
        return []

# ── Actions store ─────────────────────────────────────────────────────────────

def load_actions():
    default = ["AddHp", "attck_sword", "equip"]
    if os.path.exists(ACTIONS_FILE):
        try:
            with open(ACTIONS_FILE) as f:
                data = json.load(f)
            # Merge defaults in case file is older
            merged = list(dict.fromkeys(data + default))
            return merged
        except Exception:
            pass
    return list(dict.fromkeys(default))

def save_actions(actions):
    try:
        with open(ACTIONS_FILE, "w") as f:
            json.dump(actions, f, indent=2)
    except Exception as e:
        print(yellow(f"  ⚠  Could not save actions list: {e}"))

def prompt_actions(known_actions):
    divider("ACTIONS")
    print(f"  {dim('Known actions (for reference):')}")
    for i, a in enumerate(known_actions, 1):
        print(f"    {cyan(str(i).rjust(2))}. {a}")
    print(f"\n  {dim('Enter actions as a comma-separated list.')}")
    print(f"  {dim('New actions are saved automatically for next time.')}\n")
    raw = prompt("Actions (or leave blank for none)", "")
    if not raw:
        return [], known_actions
    entered = [a.strip() for a in raw.split(",") if a.strip()]
    updated = list(dict.fromkeys(known_actions + entered))
    new_ones = [a for a in entered if a not in known_actions]
    if new_ones:
        print(green(f"    ✓ Added to actions list: {', '.join(new_ones)}"))
    return entered, updated

# ── Attribute prompt ──────────────────────────────────────────────────────────

def prompt_attributes(attr_uids):
    divider("ITEM ATTRIBUTES")
    if attr_uids:
        print(f"  {dim('Available UIDs (type uid or number, blank to stop):')}")
        cols = 6
        rows = [attr_uids[i:i+cols] for i in range(0, len(attr_uids), cols)]
        for i, row in enumerate(rows):
            parts = []
            for j, uid in enumerate(row):
                idx = i * cols + j + 1
                parts.append(f"{dim(str(idx).rjust(3))}. {cyan(uid):12s}")
            print("  " + "  ".join(parts))
    else:
        print(f"  {dim('(No attribute list available — type UIDs freely)')}")

    print()
    attributes = []
    idx = 1
    while True:
        raw = prompt(f"Attribute {idx} uid (or blank to finish)", "").strip()
        if not raw:
            break

        # Allow selecting by number
        if raw.isdigit() and attr_uids:
            n = int(raw) - 1
            if 0 <= n < len(attr_uids):
                uid = attr_uids[n]
                print(f"    → {cyan(uid)}")
            else:
                print(red("    ✗ Number out of range."))
                continue
        else:
            uid = raw

        is_fixed = prompt_bool(f"  {uid} — fixed value (same min/max)?", True)
        if is_fixed:
            val = prompt_float(f"  {uid} value", 0.0)
            attributes.append({
                "uid": uid,
                "stringValue": "false",
                "floatValue": val,
                "locked": 0,
                "randomChange": 100,
                "isFixed": 1,
                "minValue": val,
                "maxValue": val,
                "mAttributeSetting": {"rid": -2},
            })
        else:
            mn = prompt_float(f"  {uid} min", 0.0)
            mx = prompt_float(f"  {uid} max", mn)
            import random
            fv = round(random.uniform(mn, mx), 4)
            attributes.append({
                "uid": uid,
                "stringValue": "false",
                "floatValue": fv,
                "locked": 0,
                "randomChange": 100,
                "isFixed": 0,
                "minValue": mn,
                "maxValue": mx,
                "mAttributeSetting": {"rid": -2},
            })
        idx += 1

    return attributes

# ── Craft materials ───────────────────────────────────────────────────────────

def prompt_craft_materials():
    divider("CRAFT MATERIALS")
    print(f"  {dim('Enter craft material entries as item-id:quantity pairs.')}")
    print(f"  {dim('Item ID is the integer index in your item database (x = id, y = qty).')}")
    print(f"  {dim('Leave blank to finish.')}\n")
    materials = []
    tags = []
    idx = 1
    while True:
        raw_id = prompt(f"Material {idx} item ID (or blank to finish)", "").strip()
        if not raw_id:
            break
        try:
            item_id = float(int(raw_id))
        except ValueError:
            print(red("    ✗ Enter an integer ID."))
            continue
        qty = float(prompt_int(f"  Quantity", 1))
        materials.append({"x": item_id, "y": qty})
        tag = prompt(f"  Craft material tag (or blank)", "").strip()
        tags.append(tag)
        idx += 1
    return materials, tags

# ── Tags / socketing ──────────────────────────────────────────────────────────

def prompt_string_list(label, hint=""):
    if hint:
        print(f"  {dim(hint)}")
    raw = prompt(label, "")
    if not raw:
        return []
    return [t.strip() for t in raw.split(",") if t.strip()]

# ── YAML writer ───────────────────────────────────────────────────────────────

def format_icon(guid, file_id=2800000, itype=3):
    if guid:
        return f"{{fileID: {file_id}, guid: {guid}, type: {itype}}}"
    return "{fileID: 0, guid: , type: 0}"

def write_item_yaml(item, path):
    def yval(v):
        if isinstance(v, bool):
            return "1" if v else "0"
        if isinstance(v, float):
            # Trim unnecessary trailing zeros
            s = f"{v:.10f}".rstrip("0").rstrip(".")
            return s if s else "0"
        return str(v)

    def attr_block(a, indent=4):
        pad = " " * indent
        lines = [
            f"{pad}- uid: {a['uid']}",
            f"{pad}  stringValue: {a['stringValue']}",
            f"{pad}  floatValue: {yval(a['floatValue'])}",
            f"{pad}  locked: {a['locked']}",
            f"{pad}  randomChange: {a['randomChange']}",
            f"{pad}  isFixed: {a['isFixed']}",
            f"{pad}  minValue: {yval(a['minValue'])}",
            f"{pad}  maxValue: {yval(a['maxValue'])}",
            f"{pad}  mAttributeSetting:",
            f"{pad}    rid: -2",
        ]
        return "\n".join(lines)

    lines = []
    lines.append(f"  - uid: {item['uid']}")
    lines.append(f"    name: {item['name']}")
    lines.append(f"    description: {item['description']}")
    lines.append(f"    type: {item['type']}")
    lines.append(f"    icon: {item['icon']}")
    lines.append(f"    iconLoadMethod: {item['iconLoadMethod']}")
    lines.append(f"    iconPath: {item['iconPath']}")
    lines.append(f"    quality: {item['quality']}")
    lines.append(f"    tradeable: {1 if item['tradeable'] else 0}")
    lines.append(f"    deletable: {1 if item['deletable'] else 0}")
    lines.append(f"    useable: {1 if item['useable'] else 0}")
    lines.append(f"    consumable: {1 if item['consumable'] else 0}")
    lines.append(f"    visible: {1 if item['visible'] else 0}")
    lines.append(f"    price: {item['price']}")
    lines.append(f"    currency: {item['currency']}")
    lines.append(f"    maxiumStack: {item['maxiumStack']}")
    lines.append(f"    upgradeLevel: {item['upgradeLevel']}")
    lines.append(f"    weight: {yval(item['weight'])}")
    lines.append(f"    dropRates: {item['dropRates']}")
    lines.append(f"    favorite: {1 if item['favorite'] else 0}")

    if item['attributes']:
        lines.append(f"    attributes:")
        for a in item['attributes']:
            lines.append(attr_block(a, indent=4))
    else:
        lines.append(f"    attributes:")

    lines.append(f"    maximumRandomAttributes: {item['maximumRandomAttributes']}")
    lines.append(f"    enchantments: ")

    if item['craftMaterials']:
        lines.append(f"    craftMaterials:")
        for m in item['craftMaterials']:
            lines.append(f"    - {{x: {yval(m['x'])}, y: {yval(m['y'])}}}")
    else:
        lines.append(f"    craftMaterials: []")

    if item['craftMaterialTags']:
        lines.append(f"    craftMaterialTags:")
        for t in item['craftMaterialTags']:
            lines.append(f"    - {t}")
    else:
        lines.append(f"    craftMaterialTags: []")

    if item['actions']:
        lines.append(f"    actions:")
        for a in item['actions']:
            lines.append(f"    - {a}")
    else:
        lines.append(f"    actions: []")

    if item['tags']:
        lines.append(f"    tags:")
        for t in item['tags']:
            lines.append(f"    - {t}")
    else:
        lines.append(f"    tags: []")

    lines.append(f"    customData: []")
    lines.append(f"    restrictionKey: {item['restrictionKey']}")
    lines.append(f"    restrictionValue: {item['restrictionValue']}")
    lines.append(f"    socketingSlots: {item['socketingSlots']}")
    lines.append(f"    socketedItems: ")

    if item['socketingTag']:
        lines.append(f"    socketingTag:")
        for t in item['socketingTag']:
            lines.append(f"    - {t}")
    else:
        lines.append(f"    socketingTag: []")

    lines.append(f"    fold: 0")

    # Append to file if it exists, otherwise create
    mode = "a" if os.path.exists(path) else "w"
    with open(path, mode, encoding="utf-8") as f:
        if mode == "a":
            f.write("\n")
        f.write("\n".join(lines) + "\n")

# ── Main ──────────────────────────────────────────────────────────────────────

def main():
    print()
    print(bold("  ╔══════════════════════════════════════════╗"))
    print(bold("  ║         Item YAML Generator              ║"))
    print(bold("  ╚══════════════════════════════════════════╝"))
    print()

    # Asset path
    divider("ATTRIBUTE SOURCE")
    override = prompt(f"GameAttributeObject.asset path (Enter to use default)", "").strip()
    asset_path = override if override else DEFAULT_ASSET_PATH
    attr_uids = load_attribute_uids(asset_path)
    if attr_uids:
        print(green(f"  ✓ Loaded {len(attr_uids)} attribute UIDs from asset."))

    # Load known actions
    known_actions = load_actions()

    # ── Core identity ──
    divider("IDENTITY")
    uid         = prompt("uid (unique string key)", "new_item")
    name        = prompt("name", "New Item")
    description = prompt("description", "No description yet.")

    # ── Type & quality ──
    divider("TYPE & QUALITY")
    print(f"  {dim('Common types: 0=Consumable, 1=Equipment, 2=Blueprint, 3=Material, 4=Skill')}")
    itype   = prompt_int("type", 0)
    quality = prompt_int("quality (0=Common … 3=Legendary)", 0)

    # ── Icon ──
    divider("ICON")
    print(f"  {dim('Leave guid blank to use empty icon reference.')}")
    guid        = prompt("icon guid", "").strip()
    icon_str    = format_icon(guid)
    icon_path   = prompt("iconPath (sprite path string)", "").strip()

    # ── Economy ──
    divider("ECONOMY")
    price    = prompt_int("price", 0)
    print(f"  {dim('Currency types: ' + ', '.join(f'{k}={v}' for k, v in CURRENCY_LABELS.items()))}")
    currency = prompt_int("currency index", 0)
    weight   = prompt_float("weight", 0.1)
    drop_rates = prompt_int("dropRates", 0)

    # ── Stack & flags ──
    divider("FLAGS")
    max_stack    = prompt_int("maxiumStack", 99)
    tradeable    = prompt_bool("tradeable", True)
    deletable    = prompt_bool("deletable", True)
    useable      = prompt_bool("useable", False)
    consumable   = prompt_bool("consumable", False)
    visible      = prompt_bool("visible", True)
    favorite     = prompt_bool("favorite", False)
    upgrade_lvl  = prompt_int("upgradeLevel", 0)

    # ── Attributes ──
    attributes = prompt_attributes(attr_uids)
    max_random_attrs = prompt_int("maximumRandomAttributes", 5)

    # ── Craft materials ──
    do_craft = prompt_bool("\nAdd craft materials?", False)
    craft_materials, craft_material_tags = ([], [])
    if do_craft:
        craft_materials, craft_material_tags = prompt_craft_materials()

    # ── Actions ──
    actions, known_actions = prompt_actions(known_actions)
    save_actions(known_actions)

    # ── Tags ──
    divider("TAGS & SOCKETING")
    print(f"  {dim('Tags define equipment slots (e.g. MainHand, Torso, Helmet, Boots).')}")
    tags = prompt_string_list("tags (comma-separated, or blank)", "")

    restriction_key   = prompt("restrictionKey", "").strip()
    restriction_value = prompt_int("restrictionValue", 0)

    socketing_slots = prompt_int("socketingSlots", 0)
    socketing_tag   = []
    if socketing_slots > 0:
        socketing_tag = prompt_string_list("socketingTag colours (comma-separated)", "e.g. Red, Yellow")

    # ── Output ──
    divider("OUTPUT")
    out_path = prompt("Output filename", f"{uid}.yaml")
    if not out_path.endswith(".yaml") and not out_path.endswith(".yml"):
        out_path += ".yaml"

    item = {
        "uid": uid,
        "name": name,
        "description": description,
        "type": itype,
        "icon": icon_str,
        "iconLoadMethod": 0,
        "iconPath": icon_path,
        "quality": quality,
        "tradeable": tradeable,
        "deletable": deletable,
        "useable": useable,
        "consumable": consumable,
        "visible": visible,
        "price": price,
        "currency": currency,
        "maxiumStack": max_stack,
        "upgradeLevel": upgrade_lvl,
        "weight": weight,
        "dropRates": drop_rates,
        "favorite": favorite,
        "attributes": attributes,
        "maximumRandomAttributes": max_random_attrs,
        "craftMaterials": craft_materials,
        "craftMaterialTags": craft_material_tags,
        "actions": actions,
        "tags": tags,
        "restrictionKey": restriction_key,
        "restrictionValue": restriction_value,
        "socketingSlots": socketing_slots,
        "socketingTag": socketing_tag,
    }

    write_item_yaml(item, out_path)

    divider()
    existed = os.path.exists(out_path)
    print(f"\n  {green('✓')} {'Appended to' if existed else 'Written to'} {bold(out_path)}")
    print(f"  {dim(f'{len(attributes)} attribute(s)  •  {len(actions)} action(s)  •  {len(craft_materials)} craft material(s)')}\n")

if __name__ == "__main__":
    main()
