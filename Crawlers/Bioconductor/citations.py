import subprocess
import tempfile
import urllib.request
import yaml
import os
import json
import stat
from distutils.version import StrictVersion
import shutil, errno

R_SCRIPT_PATH = "C:\\Program Files\\R\\R-3.6.1\\bin\\Rscript.exe"
R_LIB_DIRECTORY = "C:\\Program Files\\R\\R-3.6.1\\library"
FIRST_APPEARANCE_FILENAME = "../../data/bioconductor/first_appearance.json"
CITATIONS_FILENAME = "../../data/bioconductor/citations.json"


def get_package_names():
    package_names = []
    with open(FIRST_APPEARANCE_FILENAME) as f:
        releases = json.load(f)
        for release in releases:
            package_names.extend(releases[release]["packages"])
    return package_names


def get_bibitem(filename):
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


def uninstall_package(package_name):
    """
    It basically removes the folder to which the package
    is installed.Sometimes get permission denied error for
    some folders when deleting them; hence, first try to
    ensure the write permission for every file and folder.
    """
    package_dir = os.path.join(R_LIB_DIRECTORY, package_name)
    for root, _, files in os.walk(package_dir):
        for filename in files:
            os.chmod(os.path.join(root, filename), stat.S_IWUSR)
    shutil.rmtree(package_dir)


def get_citation(package_name):
    """
    The overall process:
    1. Clone the package repository;
    2. Move the cloned repository to R's `library` folder;
    3. Call an R script that calls the `citation` method to get
       citation information of the given package and output it
       to a given file;
    4. Read BibItem from the R script's output file;
    5. Delete all the files and folders created as
       a result of running this method;
    6. Return the BibItem the R script has returned.
    """
    # Clone the repository of a given package to a temporary path.
    git_clone_dir = os.path.join(tempfile.mkdtemp(prefix="tvq_"), package_name)
    try:
        print("\tcloning...", end="")
        subprocess.check_output(
            f"git clone https://git.bioconductor.org/packages/{package_name} {git_clone_dir}",
            shell=True, stderr=subprocess.STDOUT)
    except subprocess.CalledProcessError:
        print("failed, skipping.")
        return None
    print("done", end="")

    # Copying the cloned repository to R's library path
    # is equivalent of installing the package. Without
    # this step, the script used to get citations do not
    # recognize the package as installed.
    try:
        print("\tinstalling...", end="")
        shutil.move(git_clone_dir, R_LIB_DIRECTORY)
    except shutil.Error:
        uninstall_package(package_name)
        try:
            shutil.move(git_clone_dir, R_LIB_DIRECTORY)
        except shutil.Error:
            print("failed, skipping.")
            return None
    print("done", end="")

    # Calls the get_package_citation script which
    # gets the citation of a given package and
    # persists it in the given filename.
    print("\textracting citation...", end="")
    tmp_citation_filename = os.path.join(".", "tmp.txt")
    subprocess.run(f"{R_SCRIPT_PATH} .\get_package_citation.R {package_name} {tmp_citation_filename}")

    # Read the BibItem the R script outputs.
    bibitem = get_bibitem(tmp_citation_filename)
    print("done", end="")

    # Clean up.
    uninstall_package(package_name)
    os.remove(tmp_citation_filename)

    return bibitem


if __name__ == "__main__":
    packages = get_package_names()
    citations = {}
    unsuccessful_packages = []
    print("Getting citation information ...")
    i = 0
    for package in packages:
        i += 1
        print(f"[{i:02d}/{len(packages)}] {package}:\t", end="")
        bibitem = get_citation(package)
        if bibitem:
            citations[package] = bibitem
            print("\trecorded.")
        else:
            unsuccessful_packages.append(package)

    print(f"Writing citations to {CITATIONS_FILENAME} ...", end="")
    with open(CITATIONS_FILENAME, "w") as f:
        json.dump(citations, f, indent="\t")
    print("done.")

    with open("unsuccessful_packages.json", "w") as f:
        json.dump(unsuccessful_packages, f, indent="\t")
