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
                bibitem += line.replace("\t", " ") + " "
        return bibitem
        

"""
This code is not optimized.

Some BiocViews are multi-line, e.g.,

    RoxygenNote: 6.1.1
    biocViews: Genetics, GeneExpression, DifferentialExpression,
            Sequencing, Microarray, Regression, DimensionReduction,
            MultipleComparison
    git_url: https://git.bioconductor.org/packages/adaptest
    
This method decides whether it should continue reading next line 
or not, based on the trailing `,` of last read biocView.
"""
def get_bioc_view(filename):
    with open(filename) as f:
        content = f.readlines()
        content = [x.strip() for x in content]
        biocViews = []
        started_reading = False
        for line in content:
            line = line.rstrip()
            if started_reading:
                if len(line.split(":")) == 1:
                    views = line.split(", ")
                    for view in views:
                        biocViews.append(view.strip())
                else:
                    break
            if not started_reading and line.startswith("biocViews:"):
                started_reading = True
                views = line.replace("biocViews:", "")
                views = views.split(",")
                for view in views:
                    biocViews.append(view.strip())
            
            if started_reading:
                if biocViews[-1].endswith(","):
                    continue
                else:
                    break

        return biocViews


if __name__ == "__main__":
    path = "."
    tools_citation = {}
    tools_bioc_views = {}
    for file in os.listdir(path):
        if file.endswith(".citations.txt"):
            filename = file.replace(".citations.txt", "")
            citation = get_citation(file)
            tools_citation[filename] = citation
        elif file.endswith(".description.txt"):
            filename = file.replace(".description.txt", "")
            bioc_view = get_bioc_view(file)
            tools_bioc_views[filename] = bioc_view

    with open('citations.json', 'w', encoding='utf-8') as f:
        json.dump(tools_citation, f, ensure_ascii=False, indent=4, )
    
    with open('biocViews.json', 'w', encoding='utf-8') as f:
        json.dump(tools_bioc_views, f, ensure_ascii=False, indent=4, )
