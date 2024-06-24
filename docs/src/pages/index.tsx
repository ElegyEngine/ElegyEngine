import clsx from 'clsx';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import Layout from '@theme/Layout';
import HomepageOverview from '@site/src/components/HomepageOverview';
import HomepageFeatures from '@site/src/components/HomepageFeatures';
import Heading from '@theme/Heading';

import styles from './index.module.css';
import ElegyLogo from '@site/static/img/elegy_logo_simple.svg'

import Tabs from '@theme/Tabs';
import TabItem from '@theme/TabItem';

function HomepageHeader()
{
  const {siteConfig} = useDocusaurusContext();
  return (
    <header className={clsx('hero hero--primary--mine', styles.heroBanner)}>
      <div className="container">
        <div className={clsx('row')}>
          <div className={clsx('col col--3')}>
            <ElegyLogo title="Elegy logo" />
          </div>
          <div className={clsx('col col--9', styles.heroTitleContainer)}>
            <Heading as="h1" className="hero__title">
              {siteConfig.title}
            </Heading>
            <p className="hero__subtitle">{siteConfig.tagline}</p>
            <div className={styles.buttons}>
              <Link
                className={clsx("button button--lg", styles.buttonHomepage)}
                to="/docs/intro">
                Get started!
              </Link>
              <Link
                className={clsx("button button--lg", styles.buttonHomepage)}
                to="/docs/intro">
                Join the Discord
              </Link>
            </div>
          </div>
        </div>
      </div>
    </header>
  );
}

function HomepageDescription()
{
  return (
    <div className={clsx(styles.description, "container hero__subtitle")}>
      <p>
        Greek <i>elegeia</i>, "poem of mourning."
      </p>
      <p>
        Elegy Engine is an umbrella project that encompasses the following:<br />
        <ul>
          <li>
            A game engine specialised for retro FPS games and the like.
            <ul>
              <li>Think Quake, Half-Life, Unreal, Thief, System Shock.</li>
              <li>Written in C#, targeting .NET 8, running on Windows, Linux and Android/VR.</li>
              <li>Uses Vulkan 1.3 for rendering.</li>
              <li>MIT-licenced, free for everyone, for any purpose, forever.</li>
            </ul>
          </li>
          <li>
            Tools to aid the above:
            <ul>
              <li>Map compiler</li>
              <li>External developer console</li>
              <li>Project wizard</li>
              <li>Model inspector</li>
            </ul>
          </li>
        </ul>
      </p>
      <p>
        What Elegy is <b>not:</b><br />
        <ul>
          <li>
            A Unity/Unreal clone, or competing with any engine per se.
            <ul>
              <li>
                This isn't your typical modern-day game engine. There is no integrated engine editor.
                Work is done in external tools, and when it comes to project management, it's just you
                and the filesystem.
              </li>
              <li>
                The workflow will feel right at home if you've modded Quake or Half-Life.
              </li>
              <li>
                It's not designed for big teams/AAA studios. Rather it's meant for solo devs and tiny teams.
              </li>
            </ul>
          </li>
          <li>
            Meant for all genres, or even 2D.
            <ul>
              <li>
                Elegy aims to be usable for interactive first-person games.
                Whether it's a full-blown immersive sim shooter, or a walking simulator,
                that is the constraint.
              </li>
              <li>
                Up until February 2024, this engine was a specialisation of Godot. So, if you
                would like to make games of other genres (racing, puzzle, turn-based strategy)
                or dimensions than 3D, definitely consider using Godot itself.
              </li>
            </ul>
          </li>
        </ul>
      </p>
    </div>
  );
}

export default function Home() : JSX.Element
{
  const {siteConfig} = useDocusaurusContext();
  return (
    <Layout
      title={`${siteConfig.title}`}
      description="Description will go into a meta tag in <head />">
      <HomepageHeader />
      <main>
        <Tabs className={clsx(styles.homepageTabs)}>
          <TabItem value="overview" label="Overview" default>
            <HomepageOverview />
          </TabItem>
          <TabItem value="feats" label="Features">
            <HomepageFeatures />
          </TabItem>
          <TabItem value="about" label="About">
            <HomepageDescription />
          </TabItem>
        </Tabs>
      </main>
    </Layout>
  );
}
