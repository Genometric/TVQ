# chooseCRANmirror()
# install.packages("BiocManager")
# install.packages("knitcitations")
# install.packages("RGtk2")
# packages <- BiocManager::available()
# or:
packages <- available.packages(repos = BiocManager::repositories()[["BioCsoft"]])
packages_vector = strsplit(packages, " ")

package_count = length(packages_vector)
counter = 0
for(i in packages_vector)
{
  counter = counter + 1
  if(!(i == "ArrayExpressHTS") && 
     !(i == "bayesCL") &&
     !(i == "ChAMP") && 
     !(i == "ChIC") && 
     !(i == "clippda") && 
     !(i == "diggit") &&
     !(i == "FEM") &&
     !(i == "gmapR") &&
     !(i == "HTSeqGenie") &&
     !(i == "msa") &&
     !(i == "MTseeker") &&
     !(i == "MSstatsTMT") &&
     !(i == "networkBMA") &&
     !(i == "Rcwl") &&
     !(i == "RcwlPipelines") &&
     !(i == "rMAT") &&
     !(i == "scAlign") &&
     !(i == "SICtools") &&
     !(i == "xps") &&
     !(i == "ChemmineDrugs") &&
     !(i == "cMAP") &&
     !(i == "hgu95av2"))
  {
    str_trim(i, side = c("both"))
    print(paste("\n>>>>>>>>>>>>>>>>>>>>>>>>>>", counter, "/", package_count, "\t", i))
    if (!requireNamespace(i, quietly = TRUE))
      BiocManager::install(i)
    capture.output(citation(i), file = paste(i, ".citations.txt"))
    capture.output(packageDescription(i), file = paste(i, ".description.txt"))
    print("<<<<<<<<<<<<<<<<<<<<<<<<< Done.")
    cat(i,file="extracted_citations.txt",sep="\n", append=TRUE)
  }
}
