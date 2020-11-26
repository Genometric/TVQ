<p align="center">
  <a href="https://genometric.github.io/TVQ/docs/">
    <img src="https://raw.githubusercontent.com/Genometric/TVQ/docs/static/img/logo_w_txt.png?raw=true" alt="TVQ" width="300" />
  </a>
</p>


<p align="center">
  <a href=""><img src="https://github.com/Genometric/TVQ/workflows/documentation/badge.svg" alt="DOI"></a>
</p>

<p align="center">
  <a href="https://genometric.github.io/TVQ/docs/tvq/intro">Documentation</a> |
  <a href="https://genometric.github.io/TVQ/docs">Quick Start</a> |
  <a href="https://genometric.github.io/TVQ/api/">Swagger API Documentation</a>
</p>

<br/>

This branch contains the material used to generate the 
[TVQ Website](https://genometric.github.io/TVQ/). 
The documentation is written in Markdown (`.mdx`),
based on which a static website is generated using 
[Docusaurus 2](https://v2.docusaurus.io/).
The website generation is automatically triggered
as a commit is pushed to this branch. 

## Website Development

To start website locally for development purposes, 
on `docs` branch, you may take the following steps: 

```console
$ yarn install
$ yarn start
```

This command starts a local development server and 
opens up a browser window. Most changes are reflected 
live without having to restart the server.
