<p align="center">
  <a href="https://genometric.github.io/TVQ/docs/">
    <img src="https://raw.githubusercontent.com/Genometric/TVQ/docs/static/img/logo_w_txt.png?raw=true" alt="TVQ" width="300" />
  </a>
</p>


<p align="center">
  <a href="https://doi.org/10.5281/zenodo.4276272"><img src="https://zenodo.org/badge/DOI/10.5281/zenodo.4276272.svg" alt="DOI"></a>
</p>

<p align="center">
  <a href="https://genometric.github.io/TVQ/docs/tvq/intro">Documentation</a> |
  <a href="https://genometric.github.io/TVQ/docs">Quick Start</a> |
  <a href="https://genometric.github.io/TVQ/api/">Swagger API Documentation</a>
</p>


The objective of this project is to study the impact of publishing tools to package management systems on their scholarly recognition and adoption. For instance, how much the citation count of a paper increase after its respective software is added to Bioconductor? Currently, the study is focused on tools published to package management systems primarily used by the bioinformatics community: [Bioconda](https://bioconda.github.io), [Bioconductor](https://www.bioconductor.org), [BioTools](https://github.com/bio-tools), and [ToolShed](https://toolshed.g2.bx.psu.edu). 

The study is performed using the following components: 

- [TVQ Service](https://github.com/Genometric/TVQ/tree/master/webservice/WebService). A containerized ASP.NET Web application. This service collects the data required for the study; it crawls Bioconda, Bioconductor, BioTools, and ToolShed for all the packages they host and collects their metadata such as tool name, scholarly references, and date added to the package management system. It then queries the Scopus for the citation count of the scholarly references of each tool.

- [Python Scripts](https://github.com/Genometric/TVQ/tree/master/analytics) for statistical analysis and plotting. These scripts perform statistical tests on the data collected by the TVQ Service and report results in tables and plots.


## Contributing

When it comes to open-source, every contribution you 
make, makes the software better for everyone, and 
that is extensively valuable and warmly appreciated 
by the community. We have a 
[contributing guide](https://github.com/genometric/tvq/blob/master/CONTRIBUTING.md) t
o help guide you.
