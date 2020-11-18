The objective of this project is to study the impact of publishing tools to package management systems on their scholarly recognition and adoption. For instance, how much the citation count of a paper increase after its respective software is added to Bioconductor? Currently, the study is focused on tools published to package management systems primarily used by the bioinformatics community: Bioconda, Bioconductor, BioTools, and ToolShed. 

The study is performed using the following components: 

TVQ Service. A containerized ASP.NET Web application. This service collects the data required for the study. This service crawls Bioconda, Bioconductor, BioTools, and ToolShed for all the packages they host and collects their metadata such as tool name, scholarly references, and date added to the package management system. It then queries the Scopus for the citation count of the scholarly references of each tool.
Scripts for statistical analysis and plotting. These scripts perform statistical tests on the data collected by the TVQ Service and report results in tables and plots.  
