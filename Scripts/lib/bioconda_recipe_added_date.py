import os
import shutil
import subprocess
import sys


# A temporary folder where the Bioconda repository
# will be cloned at, and it will be deleted once the
# script has concluded.
GIT_CLONE_FOLDER = "temp"

# The name of the folder from the Bioconda repository
# that contains the recipes. 
RECIPES_FOLDER = "recipes"

# The the name of the file that contains metadata for 
# each recipe in the Bioconda git repository.
METADATA_FILENAME = "meta.yaml"

# The file where the bioconda packages date will be added.
OUTPUT_FILENAME = "bioconda_recipes_add_date.txt"


def run_command(command):
    subprocess.run(command, stdout=subprocess.PIPE, shell=True)


def add_to_file(message):
    os.chdir("..")
    with open(OUTPUT_FILENAME, "a") as file:
        file.write(message + "\n")
    os.chdir(GIT_CLONE_FOLDER)


def get_add_date(filename):
    command = "git log --diff-filter=A -- {0}".format(filename)
    output = str(subprocess.check_output(command), "utf-8")
    messages = output.split("\n")
    for message in messages:
        if message.startswith("Date:"):
            return message.replace("Date:", "")


def get_added_date():
    run_command("mkdir {0}".format(GIT_CLONE_FOLDER))
    os.chdir(GIT_CLONE_FOLDER)
    run_command("git clone https://github.com/bioconda/bioconda-recipes .")
    recipies_count = len(next(os.walk(RECIPES_FOLDER))[1])
    counter = 0
    for (dirpath, dirnames, filenames) in os.walk(RECIPES_FOLDER):
        counter += 1
        recipe_name = dirpath
        recipe_name = recipe_name.replace(RECIPES_FOLDER + "\\", "")
        sys.stdout.write("Processing [{0}/{1}]: {2}\n".format(counter, recipies_count, recipe_name))
        sys.stdout.flush()
        for filename in filenames:
            if(filename == METADATA_FILENAME):
                full_filename = os.path.join(dirpath, filename)
                date = get_add_date(full_filename)
                add_to_file("{0}\t{1}".format(recipe_name, date))
                continue
    os.chdir("..")
    shutil.rmtree(GIT_CLONE_FOLDER, ignore_errors=True)


if __name__ == "__main__":
    if os.path.exists(OUTPUT_FILENAME):
        os.remove(OUTPUT_FILENAME)
    get_added_date()

