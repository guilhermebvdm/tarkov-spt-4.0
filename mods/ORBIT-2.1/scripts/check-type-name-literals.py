#!/usr/bin/env python3
"""Checkpoint 3 of the SPT 4.1 -> 4.0 port: type names compared as text.

The ORBIT sources compare GetType().Name against SPT 4.1 class names ("FollowerPatrolLayer"). On SPT 4.0 the
same class is still GClassNNN, so the comparison is silently false and the compiler cannot notice.
modded/Orbit/Compat/Spt40TypeNames.cs translates the 4.0 type back to its 4.1 name; this script fails when a
string literal in the sources equals a 4.1 type name that has no row there.

usage: check-type-name-literals.py            (run from anywhere; paths are resolved from this file)
exit code 0 = every literal is covered, 1 = at least one is not.
"""
import collections
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
MOD = os.path.dirname(HERE)
REPO = os.path.dirname(os.path.dirname(MOD))
MAPPINGS = os.path.join(REPO, 'docs', 'files-from-4.1', 'consolidated-mappings.txt')
SOURCES = [os.path.join(MOD, 'modded', d) for d in ('Orbit', 'Orbit.Fika', 'Shared')]
NAMES_FILE = os.path.join(MOD, 'modded', 'Orbit', 'Compat', 'Spt40TypeNames.cs')

# Literals that equal a 4.1 type name by coincidence. Each was read at its use site.
NOT_A_GAME_TYPE = {
    'Data': 'JSON key of the handbook response (Looting/HandbookPriceCache.cs)',
    'Info': 'SAIN property read by reflection (Sain/SainPersonality.cs)',
    'Shared': 'label written to the debug log (Systems/WaypointSystem.cs)',
    'GrenadeDangerPoint': 'SAIN member read by reflection (Systems/NativeAwakeGrenadeDiagnostics.cs)',
}


def load_right_side(path):
    """4.1 short type name -> list of 4.0 names, from 'LEFT -> RIGHT' lines."""
    right, left = collections.defaultdict(list), set()
    for line in open(path, encoding='utf-8'):
        line = line.strip()
        if not line or line.startswith('#') or ' -> ' not in line:
            continue
        old, new = line.split(' -> ', 1)
        left.add(old)
        short = re.sub(r'`\d+$', '', re.split(r'[.+]', new)[-1])
        right[short].append(old)
    return right, left


def main():
    right, left = load_right_side(MAPPINGS)
    covered = set(re.findall(r'typeof\(\w+\),\s*"(\w+)"', open(NAMES_FILE, encoding='utf-8').read()))
    literal = re.compile(r'"((?:[^"\\]|\\.)*)"')
    found = collections.defaultdict(list)
    for root in SOURCES:
        for folder, dirs, files in os.walk(root):
            dirs[:] = [d for d in dirs if d not in ('obj', 'bin', 'Compat', 'References')]
            for name in files:
                if not name.endswith('.cs'):
                    continue
                path = os.path.join(folder, name)
                for number, line in enumerate(open(path, encoding='utf-8-sig', errors='replace'), 1):
                    if line.strip().startswith('//'):
                        continue
                    for match in literal.finditer(line.split('//')[0]):
                        text = match.group(1)
                        if len(text) < 4 or ' ' in text or '{' in text:
                            continue
                        if text in right and text not in left:
                            found[text].append(f'{os.path.relpath(path, MOD)}:{number}')

    missing = 0
    for text in sorted(found):
        places = '; '.join(found[text][:6])
        if text in covered:
            print(f'OK       "{text}" = {", ".join(right[text])}  ({places})')
        elif text in NOT_A_GAME_TYPE:
            print(f'IGNORED  "{text}" — {NOT_A_GAME_TYPE[text]}')
        else:
            missing += 1
            print(f'MISSING  "{text}" (4.0: {", ".join(right[text])}) has no row in Spt40TypeNames.cs  ({places})')
    unused = sorted(covered - set(found))
    for text in unused:
        print(f'UNUSED   "{text}" is in Spt40TypeNames.cs but no source literal uses it')
    print(f'-- {len(found)} literals equal to a 4.1 type name: {len(found) - missing} handled, {missing} missing')
    return 1 if missing else 0


if __name__ == '__main__':
    sys.exit(main())
