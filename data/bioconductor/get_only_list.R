# chooseCRANmirror()
# install.packages("BiocManager")
# install.packages("knitcitations")
# install.packages("RGtk2")
packages <- BiocManager::available()
packages_vector = strsplit(packages, " ")

for(i in packages_vector)
{
  cat(i,file="packages.txt",sep="\n", append=TRUE)
}


