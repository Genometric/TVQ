
args <- commandArgs(trailingOnly = TRUE)
packageName <- args[1]
outFilename <- args[2]
capture.output(citation(packageName), file = outFilename)
