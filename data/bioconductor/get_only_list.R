# chooseCRANmirror()
# install.packages("BiocManager")
# install.packages("knitcitations")
# install.packages("RGtk2")
biocsoft <- BiocManager::repositories()["BioCsoft"]
biocsoftpkgs <- rownames(available.packages(repos=biocsoft))

for(i in biocsoftpkgs)
{
  cat(i,file="packages.txt",sep="\n", append=TRUE)
}
