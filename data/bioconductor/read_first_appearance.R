## input:

library(readr)
library(dplyr)

manifest <- read_csv(
    "first_appearance.csv",
    col_types = list(
        repository = col_character(),
        package = col_character(),
        bioc_version = col_character(),
        release_date = col_date(format = "")
    )
)

manifest %>% count(bioc_version)

pkgs <- c("IRanges", "GenomicRanges", "SummarizedExperiment")
manifest %>% filter(package %in% pkgs)
