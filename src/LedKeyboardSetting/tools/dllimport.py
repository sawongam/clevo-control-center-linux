import re,sys
for f in sys.argv[1:]:
    t=open(f).read()
    def rep(m):
        dll=m.group(1); vis=m.group(2); ret=m.group(3); name=m.group(4); args=m.group(5)
        names=[]
        call=[]
        for a in [x.strip() for x in args.split(',') if x.strip()]:
            parts=a.split()
            n=parts[-1]
            if parts[0]=='ref' : call.append('ref '+n)
            else: call.append(n)
        cl=', '.join(call)
        d=dll.lower()
        if d.startswith('insydedchu'): tgt='InsydeDCHU'
        elif d.startswith('perkey_api'): tgt='PerkeyApi'
        elif d.startswith('lineledapi'): tgt='LineLEDAPI'
        elif d.startswith('kernel32'): tgt='Ini'
        else: return ''
        return '\t%s static %s %s(%s) => %s.%s(%s);' % (vis,ret,name,args,tgt,name,cl)
    t=re.sub(r'\t\[DllImport\("([^"]+)"[^\]]*\]\n\t(public|private|internal) static extern ([\w\[\]]+) (\w+)\(([^)]*)\);',rep,t)
    open(f,'w').write(t)
