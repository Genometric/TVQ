library(yaml)
library(dplyr)
library(tibble)
library(lubridate)
library(readr)
library(memoise)

get_releases <-
    function()
{
    yaml <- read_yaml("https://bioconductor.org/config.yaml")
    enframe(unlist(yaml$release_dates), "version", "release_date") %>%
        mutate(
            version = package_version(version),
            bioc_version = sprintf("%d.%02d", version$major, version$minor),
            release_date = mdy(release_date)
        )
}

get_manifest <-
    memoise(function(version, repository)
{
    message(repository, " " , version)

    release <- paste0("RELEASE_", sub(".", "_", version, fixed = TRUE))
    repository <- paste0(repository, ".txt")
    args <- c(
        "archive",
        "--remote=git@git.bioconductor.org:admin/manifest.git",
        release,
        repository
    )
    tarball <- tempfile()
    err_code <- system2("git", args, stdout = tarball, stderr = NULL)
    if (err_code)
        return(NULL)
    untar(tarball, exdir = tempdir())
    manifest <- readLines(file.path(tempdir(), repository))
    package <- sub("Package: ", "", manifest[startsWith(manifest, "Package")])

    tibble(
        repository,
        package,
        bioc_version = sprintf("%d.%02d", version$major, version$minor)
    )
})

get_manifests <-
    function(releases)
{
    oopt = options(warn = 1)
    on.exit(options(oopt))
    version <- Filter(function(x) x >= "1.5", releases$version)
    software <- lapply(version, get_manifest, "software")
    ## annotation <- lapply(version, get_manifest, "data-annotation")
    ## experiment <- lapply(version, get_manifest, "data-experiment")
    annotation <- experiment <- list()
    do.call(rbind, c(software, annotation, experiment))
}

releases <- get_releases()

manifests <-
    get_manifests(releases) %>%
    left_join(select(releases, -version)) %>%
    arrange(release_date, package)

manifests %>%
    filter(!duplicated(package)) %>%
    write_csv("bioc_first_appearance.csv")

