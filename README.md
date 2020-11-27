<p align="center">
  <a href="https://genometric.github.io/TVQ/docs/">
    <img src="https://raw.githubusercontent.com/Genometric/TVQ/docs/static/logo/logo_w_txt.png?raw=true" alt="TVQ" width="300" />
  </a>
</p>


<p align="center">
  <a href="https://doi.org/10.5281/zenodo.4276272"><img src="https://zenodo.org/badge/DOI/10.5281/zenodo.4276272.svg" alt="DOI"></a>
</p>

<p align="center">
  <a href="https://genometric.github.io/TVQ/">Documentation</a> |
  <a href="https://genometric.github.io/TVQ/docs/getting_started/quickstart">Quick Start</a> |
  <a href="https://genometric.github.io/TVQ/api/">Swagger API Documentation</a>
</p>


The objective of this project is to study the impact of publishing tools 
to package management systems on their scholarly recognition and adoption. 
For instance, how much the citation count of a scholarly paper increase after its 
respective software is added to Bioconductor. Currently, the study is 
focused on tools published to package management systems primarily used 
by the Bioinformatics community: 
[Bioconda](https://bioconda.github.io), 
[Bioconductor](https://www.bioconductor.org), 
[BioTools](https://github.com/bio-tools), and 
[ToolShed](https://toolshed.g2.bx.psu.edu). 

## Project Structure

The project consists of three major components (see the following figure):

- [**Offline Crawlers**](https://genometric.github.io/TVQ/docs/offline_crawlers/about): 
Scripts to retrieve those package metadata that require 
extensive time or resource consuming operations. These scripts are not run frequently, 
and their generated data is cached under the 
[`data`](https://github.com/Genometric/TVQ/tree/master/data) 
folder to be used by the _Webservice_ 
(read [details]((https://genometric.github.io/TVQ/docs/offline_crawlers/about))).

- [**Webservice**](https://genometric.github.io/TVQ/docs/webservice/about):
Collects all the required metadata about software packages, it uses the cached 
data and queries the package management systems for the "cheap-to-retrieve" data.
It then aggregates the information collected from different package management systems,
and queries Scopus for the citation count of every scholarly paper. The service
generates descriptive statistics about the packages and their citation count, and
outputs raw data to be used for detailed statistical inferences by _analytics scripts_ 
(read webservice [details](https://genometric.github.io/TVQ/docs/webservice/about)).  

- [**Analytics Scripts**](https://genometric.github.io/TVQ/docs/analytics/about): 
[Python Scripts](https://github.com/Genometric/TVQ/tree/master/analytics) 
for statistical analysis and plotting. These scripts perform statistical tests 
on the data collected by the _webservice_ and report results in tables and plots
(read [detail](https://genometric.github.io/TVQ/docs/analytics/about) about 
these scripts).

<br/>
<p align="center">
  <a href="https://genometric.github.io/TVQ/docs/">
    <img src="https://raw.githubusercontent.com/Genometric/TVQ/docs/static/img/overview_wbg.svg?raw=true"/>
  </a>
</p>
<br/>

## 💖 Contributing

When it comes to open-source, every contribution you 
make, makes the software better for everyone, and 
that is extensively valuable and warmly appreciated 
by the community. We have a 
[contributing guide](https://github.com/genometric/tvq/blob/master/CONTRIBUTING.md) 
to help guide you.
