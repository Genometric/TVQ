import json
import os

def get_citation(filename):
    with open(filename) as f:
        content = f.readlines()
        content = [x.strip() for x in content]
        extract = False
        bibitem = ""
        for line in content:
            line = line.rstrip()
            if line == "A BibTeX entry for LaTeX users is":
                extract = True
                continue
            if extract == True and \
                ("ATTENTION: This citation information has been auto-generated from the" in line or \
                "To obtain the references in BibTex format, enter" in line):
                break
            if extract:
                # the trailing empty char accounts for line breaks.
                bibitem += line.replace("\t", " ") + " "
        return bibitem


if __name__ == "__main__":
    path = "."
    tools = {}
    for file in os.listdir(path):
        if file.endswith(".txt"):
            filename = file.replace(".txt", "")
            citation = get_citation(file)
            tools[filename] = citation

    with open('citations.json', 'w', encoding='utf-8') as f:
        json.dump(tools, f, ensure_ascii=False, indent=4)
