module.exports = {
  title: 'Tool Visibility Quantifier',
  tagline: 'TVQ Documentation',
  url: 'https://genometric.github.io',
  baseUrl: '/TVQ/',
  onBrokenLinks: 'throw',
  onBrokenMarkdownLinks: 'warn',
  favicon: 'img/favicon.ico',
  organizationName: 'Genometric',
  projectName: 'TVQ',
  themeConfig: {
    navbar: {
      title: 'TVQ',
      logo: {
        alt: 'logo',
        src: '/img/logo.svg',
		srcDark: 'img/logo.svg',
        target: '_self', // By default, this value is calculated based on the `href` attribute (the external link will open in a new tab, all others in the current one).
      },
      items: [
        {
          to: 'docs/',
          activeBasePath: 'docs',
          label: 'Docs',
          position: 'left',
        },
        {
          href: 'https://github.com/genometric/tvq',
          label: 'GitHub',
          position: 'right',
		  target: '_self',
        },
        {
          href: 'https://genometric.github.io/TVQ/api',
          label: 'API',
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
		    //{
         //     label: 'Web service',
          //    to: 'docs/analytics/bigpic.mdx',
          //  },
            //{
            //  label: 'API Documentation',
            //  to: 'docs/',
           // },
		    //{
          //    label: 'Analytics',
          //    to: 'docs/analytics/',
          //  }, 
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
