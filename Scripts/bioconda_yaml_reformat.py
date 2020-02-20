import jinja2
import json
import os
import pickle
import yaml

from collections import defaultdict
from jinja2 import Environment
from pathlib import Path

# Path to read the Bioconda recipes 
ROOT = "." 

# Path to write the Bioconda recipes.
AROT = "."

def fake(foo, **args):
    pass

def parse_bioconda(directory):
    data = dict()
    for p in Path(directory).glob('./*/meta.yaml'):
        try:
            template = jinja2.Template(p.read_text())
            conda = yaml.safe_load(template.render({'os': os, 'compiler': fake, 'environ': '', 'cdt': fake, 'pin_compatible': fake, 'pin_subpackage': fake, 'exact': fake}))
            root = str(p.parent) + "\\"
            new_root = root.replace(ROOT, AROT)
            Path(new_root).mkdir(parents=True, exist_ok=True)
            with open(new_root + str(p.name), 'w') as outfile:
                yaml.dump(conda, outfile, default_flow_style=False)
            continue
        except UnicodeDecodeError as e:
            print("error reading" + str(p) + ":" + str(e))

if __name__ == '__main__':
    conda = parse_bioconda(ROOT + "recipes")
