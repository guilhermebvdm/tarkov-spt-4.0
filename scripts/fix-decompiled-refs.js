const fs = require('fs');
const path = require('path');

function cleanFile(filePath) {
    let content = fs.readFileSync(filePath, 'utf8');
    let original = content;

    // Pattern 1: ((Type)(ref target)).member => target.member
    content = content.replace(/\(\([A-Za-z0-9_.]+\)\(ref\s+([A-Za-z0-9_.[\]]+)\)\)\./g, '$1.');

    // Pattern 2: ((Type)(ref target)) => target
    content = content.replace(/\(\([A-Za-z0-9_.]+\)\(ref\s+([A-Za-z0-9_.[\]]+)\)\)/g, '$1');

    // Pattern 3: LayerMask.op_Implicit(expr) => expr
    content = content.replace(/LayerMask\.op_Implicit\(([^)]+)\)/g, '$1');

    // Pattern 4: Vector4.op_Implicit(expr) => (Vector4)(expr)
    content = content.replace(/Vector4\.op_Implicit\(([^)]+)\)/g, '(Vector4)($1)');

    // Pattern 5: Object.op_Implicit((Object)(object)expr) => expr != null
    content = content.replace(/Object\.op_Implicit\(\(Object\)\(object\)([A-Za-z0-9_.[\]]+)\)/g, '$1 != null');

    // Pattern 6: ToAngleAxis parameters
    content = content.replace(/\.ToAngleAxis\((?:ref\s+|out\s+)?([A-Za-z0-9_]+),\s*(?:ref\s+|out\s+)?([A-Za-z0-9_]+)\)/g, '.ToAngleAxis(out $1, out $2)');

    // Pattern 7: Struct _002Ector constructors
    content = content.replace(/([A-Za-z0-9_]+)\s+([A-Za-z0-9_]+)\s*=\s*default\(\1\);\s*\r?\n\s*\2\._002Ector\(([^)]*)\);/g, '$1 $2 = new $1($3);');
    content = content.replace(/([A-Za-z0-9_]+)\._002Ector\(([^)]*)\)/g, 'new $1($2)');

    // Pattern 8: Replace naked CollisionModule with ParticleSystem.CollisionModule
    content = content.replace(/(?<!ParticleSystem\.)CollisionModule\b/g, 'ParticleSystem.CollisionModule');

    // Pattern 9: Add Object and Random aliases if using System and UnityEngine are present
    if (content.includes('using System;') && content.includes('using UnityEngine;')) {
        if (!content.includes('using Object = UnityEngine.Object;')) {
            content = content.replace('using UnityEngine;', 'using UnityEngine;\nusing Object = UnityEngine.Object;');
        }
        if (!content.includes('using Random = UnityEngine.Random;')) {
            content = content.replace('using UnityEngine;', 'using UnityEngine;\nusing Random = UnityEngine.Random;');
        }
    }

    if (content !== original) {
        fs.writeFileSync(filePath, content, 'utf8');
        console.log('Cleaned:', path.relative(process.cwd(), filePath));
    }
}

function walkDir(dir) {
    const entries = fs.readdirSync(dir, { withFileTypes: true });
    for (const entry of entries) {
        const fullPath = path.join(dir, entry.name);
        if (entry.isDirectory()) {
            walkDir(fullPath);
        } else if (entry.isFile() && entry.name.endsWith('.cs')) {
            cleanFile(fullPath);
        }
    }
}

const targetDir = path.join(__dirname, '../mods/VisceralCombat/modded');
walkDir(targetDir);
