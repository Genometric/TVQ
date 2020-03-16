import os

with open("package_list.csv") as f:
    packages = f.readlines()
    for package in packages:
        with open("input.tmp", "w") as w:
            w.write(package + "\n")
        os.system("Rscript.exe .\get_citations_of_given_packages.R")
