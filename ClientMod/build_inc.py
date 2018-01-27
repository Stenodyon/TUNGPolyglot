import re

linematcher = re.compile("AssemblyVersion\\(\"(\\d+)[.](\\d+)[.](\\d+)\"\)")

def main():
    with open("../../Properties/AssemblyInfo.cs", "r") as inFile:
        contents = inFile.read().splitlines()
    newcontents = []
    for line in contents:
        match = linematcher.search(line)
        if match:
            major = match.group(1)
            minor = match.group(2)
            build = int(match.group(3)) + 1
            out = "[assembly: AssemblyVersion(\"%s.%s.%i\")]" % (major, minor, build)
            newcontents.append(out)
        else:
            newcontents.append(line)
    newcontents = "\n".join(newcontents)
    with open("../../Properties/AssemblyInfo.cs", "w") as outFile:
        outFile.write(newcontents)

if __name__ == "__main__":
    main()
