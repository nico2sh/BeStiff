#!/usr/bin/env python3
"""Minimal DirectX 9 shader model 2/3 bytecode disassembler.

Scans a binary (e.g. a compiled D3DX effect) for embedded vs_/ps_ shader
blobs and prints their disassembly, with constant table (CTAB) names.
"""
import struct
import sys

OPCODES = {
    0: 'nop', 1: 'mov', 2: 'add', 3: 'sub', 4: 'mad', 5: 'mul', 6: 'rcp', 7: 'rsq',
    8: 'dp3', 9: 'dp4', 10: 'min', 11: 'max', 12: 'slt', 13: 'sge', 14: 'exp',
    15: 'log', 16: 'lit', 17: 'dst', 18: 'lrp', 19: 'frc', 20: 'm4x4', 21: 'm4x3',
    22: 'm3x4', 23: 'm3x3', 24: 'm3x2', 25: 'call', 26: 'callnz', 27: 'loop',
    28: 'ret', 29: 'endloop', 30: 'label', 31: 'dcl', 32: 'pow', 33: 'crs',
    34: 'sgn', 35: 'abs', 36: 'nrm', 37: 'sincos', 38: 'rep', 39: 'endrep',
    40: 'if', 41: 'ifc', 42: 'else', 43: 'endif', 44: 'break', 45: 'breakc',
    46: 'mova', 47: 'defb', 48: 'defi', 64: 'texcoord', 65: 'texkill', 66: 'texld',
    67: 'texbem', 68: 'texbeml', 69: 'texreg2ar', 70: 'texreg2gb', 71: 'texm3x2pad',
    72: 'texm3x2tex', 73: 'texm3x3pad', 74: 'texm3x3tex', 76: 'texm3x3spec',
    77: 'texm3x3vspec', 78: 'expp', 79: 'logp', 80: 'cnd', 81: 'def',
    82: 'texreg2rgb', 83: 'texdp3tex', 84: 'texm3x2depth', 85: 'texdp3',
    86: 'texm3x3', 87: 'texdepth', 88: 'cmp', 89: 'bem', 90: 'dp2add', 91: 'dsx',
    92: 'dsy', 93: 'texldd', 94: 'setp', 95: 'texldl', 96: 'breakp',
    0xFFFD: 'phase', 0xFFFE: 'comment', 0xFFFF: 'end',
}

REGTYPES = {
    0: 'r', 1: 'v', 2: 'c', 3: 't', 4: 'oRast', 5: 'oAttr', 6: 'o', 7: 'i',
    8: 'oC', 9: 'oDepth', 10: 's', 11: 'c2_', 12: 'c3_', 13: 'c4_', 14: 'b',
    15: 'aL', 16: 'r16_', 17: 'misc', 18: 'l', 19: 'p',
}

SRCMOD = {
    0: '{}', 1: '-{}', 2: '{}_bias', 3: '-{}_bias', 4: '{}_bx2', 5: '-{}_bx2',
    6: '1-{}', 7: '{}_x2', 8: '-{}_x2', 9: '{}_dz', 10: '{}_dw', 11: '|{}|',
    12: '-|{}|', 13: '!{}',
}

COMPARE = {0: '', 1: '_gt', 2: '_eq', 3: '_ge', 4: '_lt', 5: '_ne', 6: '_le'}

DCL_USAGE = {
    0: 'position', 1: 'blendweight', 2: 'blendindices', 3: 'normal', 4: 'psize',
    5: 'texcoord', 6: 'tangent', 7: 'binormal', 8: 'tessfactor', 9: 'positiont',
    10: 'color', 11: 'fog', 12: 'depth', 13: 'sample',
}
DCL_TEX = {2: '2d', 3: 'cube', 4: 'volume'}


def regname(tok):
    num = tok & 0x7FF
    rtype = ((tok >> 28) & 7) | (((tok >> 11) & 3) << 3)
    base = REGTYPES.get(rtype, 'x%d' % rtype)
    if base in ('oDepth', 'aL', 'misc'):
        return base
    return '%s%d' % (base, num)


def dst(tok):
    mask = (tok >> 16) & 0xF
    m = ''.join(c for i, c in enumerate('xyzw') if mask & (1 << i))
    mods = ''
    rm = (tok >> 20) & 0xF
    if rm & 1:
        mods += '_sat'
    if rm & 2:
        mods += '_pp'
    if rm & 4:
        mods += '_centroid'
    name = regname(tok)
    if mask != 0xF:
        name += '.' + m
    return name, mods


def src(tok):
    sw = (tok >> 16) & 0xFF
    comps = ''.join('xyzw'[(sw >> (2 * i)) & 3] for i in range(4))
    name = regname(tok)
    if comps != 'xyzw':
        if comps[0] * 4 == comps:
            comps = comps[0]
        name += '.' + comps
    mod = (tok >> 24) & 0xF
    return SRCMOD.get(mod, '?{}').format(name)


def parse_ctab(data):
    """data starts right after the 'CTAB' fourcc."""
    names = {}
    try:
        (size, creator, version, ncon, coninfo, flags, target) = struct.unpack_from('<7I', data, 0)
        for i in range(ncon):
            off = coninfo + i * 20
            (name_off, regset, regidx, regcnt, _res, type_off, def_off) = struct.unpack_from('<IHHHHII', data, off)
            end = data.index(b'\0', name_off)
            name = data[name_off:end].decode('ascii', 'replace')
            prefix = {0: 'b', 1: 'i', 2: 'c', 3: 's'}.get(regset, '?')
            (tclass, ttype, rows, cols, elems, members, member_off) = struct.unpack_from('<HHHHHHI', data, type_off)
            for r in range(regcnt):
                names['%s%d' % (prefix, regidx + r)] = name if regcnt == 1 else '%s[%d]' % (name, r)
        tend = data.index(b'\0', target)
        names['__target'] = data[target:tend].decode('ascii', 'replace')
    except Exception as e:  # pragma: no cover
        names['__error'] = str(e)
    return names


def disasm(blob):
    """blob: bytes starting at version token."""
    toks = struct.unpack_from('<%dI' % (len(blob) // 4), blob, 0)
    ver = toks[0]
    kind = 'vs' if (ver >> 16) == 0xFFFE else 'ps'
    major, minor = (ver >> 8) & 0xFF, ver & 0xFF
    lines = ['%s_%d_%d' % (kind, major, minor)]
    names = {}
    i = 1
    while i < len(toks):
        t = toks[i]
        op = t & 0xFFFF
        if op == 0xFFFF:
            lines.append('end')
            break
        if op == 0xFFFE:
            ln = (t >> 16) & 0x7FFF
            payload = blob[(i + 1) * 4:(i + 1 + ln) * 4]
            if payload[:4] == b'CTAB':
                names.update(parse_ctab(payload[4:]))
            i += 1 + ln
            continue
        ln = (t >> 24) & 0xF
        name = OPCODES.get(op, 'op%d' % op)
        ctrl = (t >> 16) & 0xFF
        params = toks[i + 1:i + 1 + ln]
        i += 1 + ln
        if name == 'dcl':
            usage_tok, dtok = params[0], params[1]
            rname, _ = dst(dtok)
            if rname.startswith('s'):
                lines.append('dcl_%s %s' % (DCL_TEX.get((usage_tok >> 27) & 0xF, '?'), rname))
            else:
                usage = DCL_USAGE.get(usage_tok & 0x1F, '?')
                idx = (usage_tok >> 16) & 0xF
                lines.append('dcl_%s%s %s' % (usage, idx if idx else '', rname))
            continue
        if name == 'def':
            rname, _ = dst(params[0])
            vals = struct.unpack('<4f', struct.pack('<4I', *params[1:5]))
            lines.append('def %s, %g, %g, %g, %g' % ((rname,) + vals))
            continue
        if name in ('defi', 'defb'):
            rname, _ = dst(params[0])
            lines.append('%s %s, %s' % (name, rname, ', '.join(str(p) for p in params[1:])))
            continue
        if name in ('if', 'ifc', 'breakc', 'setp'):
            name += COMPARE.get(ctrl & 7, '')
        if name == 'sincos':
            pass
        if ln == 0:
            lines.append(name)
            continue
        d, mods = dst(params[0])
        srcs = [src(p) for p in params[1:]]
        if name in ('if', 'ifc', 'breakc', 'break', 'call', 'callnz', 'loop', 'rep', 'label', 'texkill'):
            lines.append('%s %s' % (name, ', '.join([src(params[0])] + srcs)))
        else:
            lines.append('%s%s %s' % (name, mods, ', '.join([d] + srcs)))
    return lines, names


def find_shaders(data):
    out = []
    i = 0
    while i + 4 <= len(data):
        t = struct.unpack_from('<I', data, i)[0]
        if (t >> 16) in (0xFFFE, 0xFFFF) and (t & 0xFF00) in (0x0200, 0x0300) and (t & 0xFF) == 0:
            # locate end token
            j = i + 4
            ok = False
            while j + 4 <= len(data):
                u = struct.unpack_from('<I', data, j)[0]
                if u == 0x0000FFFF:
                    ok = True
                    break
                if (u & 0xFFFF) == 0xFFFE:
                    j += 4 * (((u >> 16) & 0x7FFF) + 1)
                    continue
                if (u & 0xFFFF) < 0x100 or (u & 0xFFFF) == 0xFFFD:
                    j += 4 * (((u >> 24) & 0xF) + 1)
                    continue
                break
            if ok:
                out.append((i, data[i:j + 4]))
                i = j + 4
                continue
        i += 4
    return out


def main():
    for path in sys.argv[1:]:
        data = open(path, 'rb').read()
        print('=' * 20, path)
        for off, blob in find_shaders(data):
            lines, names = disasm(blob)
            print('-- shader @0x%x (%s)' % (off, names.pop('__target', '?')))
            for k in sorted(names):
                print('   ; %s = %s' % (k, names[k]))
            for l in lines:
                print('   ' + l)


if __name__ == '__main__':
    main()
