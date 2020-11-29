module.exports = {
  title: 'Tool Visibility Quantifier',
  tagline: 'TVQ Documentation',
  url: 'https://genometric.github.io',
  baseUrl: '/TVQ/',
  onBrokenLinks: 'throw',
  onBrokenMarkdownLinks: 'warn',
  favicon: 'logo/favicon.ico',
  organizationName: 'Genometric',
  projectName: 'TVQ',
  themeConfig: {
    navbar: {
      title: 'TVQ',
      logo: {
        alt: 'logo',
        src: '/logo/logo.svg',
		srcDark: 'logo/logo.svg',
        target: '_self', // By default, this value is calculated based on the `href` attribute (the external link will open in a new tab, all others in the current one).
      },
      items: [
        {
          to: 'docs/',
          activeBasePath: 'docs',
          label: 'Documentation',
          position: 'right',
        },
        {
          href: 'https://genometric.github.io/TVQ/api',
          label: 'Swagger API Docs',
          position: 'right',
		  target: '_self',
        },
        {
          href: 'https://github.com/genometric/tvq',
          label: 'GitHub',
          position: 'right',
		  target: '_self',
        },
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: 'Docs',
          items: [
		    {
              label: 'Web service',
              to: 'https://genometric.github.io/TVQ/docs/webservice/about',
            },
		    {
              label: 'Analytics',
              to: 'https://genometric.github.io/TVQ/docs/analytics/about',
            },
            {
              label: 'Swagger API Documentation',
              to: 'https://genometric.github.io/TVQ/api',
            },
          ],
        },
        {
          title: 'Community',
          items: [
            {
              label: 'Github',
              href: 'https://github.com/Genometric/TVQ/issues',
            },
          ],
        },
      ],
    },
  },
  presets: [
    [
      '@docusaurus/preset-classic',
      {
        docs: {
          sidebarPath: require.resolve('./sidebars.js'),
          // Please change this to your repo.
          editUrl:
            'https://github.com/genometric/tvq/edit/docs/',
        },
        theme: {
          customCss: require.resolve('./src/css/custom.css'),
        },
      },
    ],
  ],
};
