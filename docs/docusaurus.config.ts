import {themes as prismThemes} from 'prism-react-renderer';
import type {Config} from '@docusaurus/types';
import type * as Preset from '@docusaurus/preset-classic';

const config: Config = {
  title: 'Elegy Engine',
  tagline: 'A love letter to the 90s, gamedev and modding~',
  favicon: 'img/FoxEarLogo_NoText_128.png',

  url: 'https://ElegyEngine.github.io',
  // Set the /<baseUrl>/ pathname under which your site is served
  // For GitHub pages deployment, it is often '/<projectName>/'
  baseUrl: '/',

  // GitHub pages deployment config
  organizationName: 'ElegyEngine',
  // Might need to fiddle around with this and all
  projectName: 'ElegyEngine',

  onBrokenLinks: 'warn',
  onBrokenMarkdownLinks: 'warn',

  // Even if you don't use internationalization, you can use this field to set
  // useful metadata like html lang. For example, if your site is Chinese, you
  // may want to replace "en" with "zh-Hans".
  i18n: {
    defaultLocale: 'en',
    locales: ['en']
  },

  presets: [
    [
      'classic',
      {
        docs: {
          sidebarPath: './sidebars.ts'
        },
        blog: {
          showReadingTime: true,
          blogSidebarCount: 'ALL'
        },
        theme: {
          customCss: './src/css/custom.css'
        },
      } satisfies Preset.Options,
    ],
  ],

  themeConfig: {
    // Replace with your project's social card
    image: 'img/social-card.png',
    navbar: {
      title: 'Elegy Engine',
      logo: {
        alt: 'Elegy Logo',
        src: 'img/elegy_logo_simple.svg',
      },
      items: [
        {
          type: 'docSidebar',
          sidebarId: 'docsSidebar',
          position: 'left',
          label: 'Documentation',
        },
        {
          to: '/blog',
          label: 'Articles',
          position: 'left'
        },
        {
          href: 'https://github.com/ElegyEngine/ElegyEngine',
          label: 'GitHub',
          position: 'right',
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
              label: 'Getting started',
              to: '/docs',
            },
            {
              label: 'Guides (N/A)',
              to: '/docs',
            },
            {
              label: 'Community tutorials (N/A)',
              to: '/docs',
            },
            {
              label: 'API reference (N/A)',
              to: '/docs/api',
            },
          ],
        },
        {
          title: 'Community',
          items: [
            {
              label: 'Discord (Planet U2-A, #elegy-engine)',
              href: 'https://discord.gg/tneyeuhgxH',
            }
          ],
        },
        {
          title: 'More',
          items: [
            {
              label: 'Development articles',
              to: '/blog',
            },
            {
              label: 'Source code @ GitHub',
              href: 'https://github.com/ElegyEngine/ElegyEngine',
            },
          ],
        },
      ],
      copyright: `Copyright © 2022-${new Date().getFullYear()} Elegy Engine Organisation, built with Docusaurus.`,
    },
    prism: {
      theme: prismThemes.github,
      darkTheme: prismThemes.dracula,
      additionalLanguages: ['csharp']
    },
  } satisfies Preset.ThemeConfig,
};

export default config;
