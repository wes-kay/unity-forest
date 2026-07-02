"""
Extract uid -> name map from a Unity GameAttributeObject .asset file.

Usage:
    python get_attributes.py                        # print JSON map
    python get_attributes.py --path <path>           # specify asset file
    python get_attributes.py --json                  # explicit JSON output (default)
    python get_attributes.py --python                # print as Python dict literal
    python get_attributes.py --csv                   # print as uid,name CSV (no header)
"""

import argparse
import json
import os
import re
import sys


def parse_attribute_list(asset_path: str) -> dict[str, str]:
    """Read a GameAttributeObject .asset YAML file and return {uid: name}."""
    with open(asset_path, "r", encoding="utf-8") as f:
        text = f.read()

    # Split on the AttributeList block so we only scan inside it.
    # The list items look like:
    #   - uid: someuid
    #     name: Some Name
    # We grab every -uid / name pair in order.
    uid_pattern = re.compile(r"^\s+- uid:\s*(.+)$", re.MULTILINE)
    name_pattern = re.compile(r"^\s+name:\s*(.+)$", re.MULTILINE)

    # Find the AttributeList section boundaries
    attr_match = re.search(r"^  AttributeList:$", text, re.MULTILINE)
    if not attr_match:
        raise ValueError(f"'AttributeList' not found in {asset_path}")

    start = attr_match.end()

    # Find the next top-level field (IdManager, uiFold, etc.) to bound the list
    next_field = re.search(r"^  [A-Z]", text[start:], re.MULTILINE)
    if next_field:
        end = start + next_field.start()
    else:
        end = len(text)

    block = text[start:end]

    uids = uid_pattern.findall(block)
    names = name_pattern.findall(block)

    if len(uids) != len(names):
        raise ValueError(
            f"Mismatch: {len(uids)} uid entries vs {len(names)} name entries "
            f"in {asset_path}"
        )

    return dict(zip(uids, names))


def main():
    parser = argparse.ArgumentParser(
        description="Extract uid->name map from a GameAttributeObject .asset file."
    )
    parser.add_argument(
        "--path",
        default=None,
        help="Path to the GameAttributeObject.asset file. "
        "Defaults to Assets/Assets/Data/GameAttributeObject.asset (relative to repo root).",
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
        default = os.path.join(script_dir, "..", "Assets", "Assets", "Data", "GameAttributeObject.asset")
        args.path = os.path.normpath(default)

    if not os.path.isfile(args.path):
        print(f"Error: file not found: {args.path}", file=sys.stderr)
        sys.exit(1)

    attr_map = parse_attribute_list(args.path)

    if args.fmt == "json":
        print(json.dumps(attr_map, indent=2, ensure_ascii=False))
    elif args.fmt == "python":
        # Preserve insertion order (Python 3.7+ dicts are ordered).
        pairs = ",\n    ".join(f"{json.dumps(k)}: {json.dumps(v)}" for k, v in attr_map.items())
        print(f"{{\n    {pairs}\n}}")
    elif args.fmt == "csv":
        for uid, name in attr_map.items():
            print(f"{uid},{name}")


if __name__ == "__main__":
    main()
