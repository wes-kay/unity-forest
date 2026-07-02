"""
Extract uid -> name map from a Unity ItemObject .asset file.

Usage:
    python get_items.py                        # print JSON map
    python get_items.py --path <path>           # specify asset file
    python get_items.py --json                  # explicit JSON output (default)
    python get_items.py --python                # print as Python dict literal
    python get_items.py --csv                   # print as uid,name CSV (no header)
"""

import argparse
import json
import os
import re
import sys


def parse_items(asset_path: str) -> dict[str, str]:
    """Read an ItemObject .asset YAML file and return {uid: name} from the items block."""
    with open(asset_path, "r", encoding="utf-8") as f:
        text = f.read()

    # Find the items: block boundary
    items_match = re.search(r"^  items:$", text, re.MULTILINE)
    if not items_match:
        raise ValueError(f"'items' block not found in {asset_path}")

    start = items_match.end()

    # Find the next field at the same 2-space indent level
    next_field = re.search(r"^  [a-z]", text[start:], re.MULTILINE)
    if next_field:
        end = start + next_field.start()
    else:
        end = len(text)

    block = text[start:end]

    # Item entries have uid immediately followed (within a few lines) by name:
    #   - uid: xxx
    #     name: YYY
    # We grab pairs where uid and name are close together (name within next 5 lines).
    uid_pattern = re.compile(r"^\s+- uid:\s*(.+)$", re.MULTILINE)
    name_pattern = re.compile(r"^\s+name:\s*(.+)$", re.MULTILINE)

    uids = uid_pattern.findall(block)
    names = name_pattern.findall(block)

    if not uids or not names:
        raise ValueError(f"No uid/name pairs found in items block of {asset_path}")

    # Build pairs: each item entry has uid followed by name nearby.
    # Walk through uid positions and find the next name after each uid.
    lines = block.splitlines()
    result = {}
    for line_idx, line in enumerate(lines):
        uid_match = re.match(r"^\s+- uid:\s*(.+)$", line)
        if uid_match:
            uid = uid_match.group(1).strip()
            # Look for name in the next 5 lines
            for look_ahead in range(1, 6):
                if line_idx + look_ahead < len(lines):
                    name_match = re.match(r"^\s+name:\s*(.+)$", lines[line_idx + look_ahead])
                    if name_match:
                        name = name_match.group(1).strip()
                        result[uid] = name
                        break
    return result


def main():
    parser = argparse.ArgumentParser(
        description="Extract uid->name map from an ItemObject .asset file."
    )
    parser.add_argument(
        "--path",
        default=None,
        help="Path to the ItemObject.asset file. "
        "Defaults to Assets/SoftKitty/Data/ItemObject.asset (relative to repo root).",
    )
    parser.add_argument(
        "--json", dest="fmt", action="store_const", const="json", default="json",
        help="Output as JSON (default).",
    )
    parser.add_argument(
        "--python", dest="fmt", action="store_const", const="python",
        help="Output as a Python dict literal.",
    )
    parser.add_argument(
        "--csv", dest="fmt", action="store_const", const="csv",
        help="Output as CSV (uid,name per line).",
    )
    args = parser.parse_args()

    # Resolve default path relative to the script's parent directory (repo root)
    if args.path is None:
        script_dir = os.path.dirname(os.path.abspath(__file__))
        default = os.path.join(script_dir, "..", "Assets", "SoftKitty", "Data", "ItemObject.asset")
        args.path = os.path.normpath(default)

    if not os.path.isfile(args.path):
        print(f"Error: file not found: {args.path}", file=sys.stderr)
        sys.exit(1)

    item_map = parse_items(args.path)

    if args.fmt == "json":
        print(json.dumps(item_map, indent=2, ensure_ascii=False))
    elif args.fmt == "python":
        pairs = ",\n    ".join(f"{json.dumps(k)}: {json.dumps(v)}" for k, v in item_map.items())
        print(f"{{\n    {pairs}\n}}")
    elif args.fmt == "csv":
        for uid, name in item_map.items():
            print(f"{uid},{name}")


if __name__ == "__main__":
    main()
