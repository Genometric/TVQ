import subprocess
import tempfile
import urllib.request
import yaml
import os
import json


FIRST_APPEARANCE_FILENAME = "sample.json"
MISSING_CACHE_RELEASES = ["1.1", "1.2", "1.3"]


class Release:
    def __init__(self, branch, release_date):
        self.branch = branch
        self.release_date = release_date
        self.packages = []


def version_to_branch(version):
    return f"RELEASE_{version.replace('.', '_')}"


def branch_to_version(branch):
    return branch.replace("RELEASE_", "").replace('_', '.')


def load_cached_releases(releases):
    root, _, filenames = next(os.walk(os.path.join(".", "cached_releases")))
    cached_versions = []
    for filename in filenames:
        filename = os.path.join(root, filename)
        branch = os.path.splitext(os.path.basename(filename))[0]
        version = branch_to_version(branch)
        with open(filename) as f:
            releases[version].packages = [line.replace("/", "").replace("\n", "")
                                          for line in f.readlines()]
        cached_versions.append(version)
    return releases, cached_versions


def get_release_dates():
    (config_yaml, _) = urllib.request.urlretrieve("https://bioconductor.org/config.yaml")
    with open(config_yaml, 'r') as stream:
        try:
            config = yaml.safe_load(stream)
        except yaml.YAMLError as e:
            print(e)
            exit()

    releases = {}
    for key, value in config["release_dates"].items():
        releases[key] = Release(version_to_branch(key), value)

    return releases


def get_manifest(release):
    git_clone_dir = tempfile.mkdtemp(prefix="tvq_")
    null_output = open(os.devnull, 'w')
    subprocess.run(f"git clone -b {release} https://git.bioconductor.org/admin/manifest.git {git_clone_dir}",
                   shell=True, stdout=null_output, stderr=subprocess.STDOUT)

    software_file = os.path.join(git_clone_dir, "software.txt")
    with open(software_file) as f:
        software = [line.replace("Package: ", "").replace("\n", "")
                     for line in f.readlines()
                     if (line.strip() and "## Blank lines between all entries" not in line)]
    return software


if __name__ == "__main__":
    print("Getting release dates ...   ", end="")
    releases = get_release_dates()
    print(f"found {len(releases)} releases dates.")
    print("Getting cached releases ... ", end="")
    releases, cached_versions = load_cached_releases(releases)
    print(f"found {len(cached_versions)} releases: {cached_versions}")
    print("Getting the list of packages for releases:")
    i = 0
    for release in releases:
        version = branch_to_version(release)
        i += 1
        print(f"\tRelease: {version} [{i:02d}/{len(releases)}] ... ", end="")
        if version in cached_versions:
            print("using cached information; skipping.")
            continue
        if version in MISSING_CACHE_RELEASES:
            print("list of packages for this release is not available; skipping.")
            continue

        releases[release].packages = get_manifest(releases[release].branch)
        print("done.")

    print(f"Serializing first appearance dates to file {FIRST_APPEARANCE_FILENAME} ", end="")
    with open("./sample.json", "w") as f:
        json.dump(releases, f, indent="\t", default = lambda x: x.__dict__)
    print("done.")
    print("All process completed successfully.")
