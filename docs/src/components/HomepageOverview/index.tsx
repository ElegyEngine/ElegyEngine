import clsx from 'clsx';
import Link from '@docusaurus/Link';
import Heading from '@theme/Heading';
import styles from './styles.module.css';

type FeatureItem = {
  title: string;
  Svg: React.ComponentType<React.ComponentProps<'svg'>>;
  description: JSX.Element;
};

const FeatureList: FeatureItem[] = [
  {
    title: 'Modular - Plug-in, plug-out',
    Svg: require('@site/static/img/features_plug.svg').default,
    description: (
      <>
        Elegy is practically a framework. There are several modules
        to facilitate game and app development, 2D and 3D alike.
        Or even CLI tools! You can extend the engine with plugins too.
      </>
    ),
  },
  {
    title: 'Retromodern - Best of both worlds',
    Svg: require('@site/static/img/features_editor.svg').default,
    description: (
      <>
        Elegy's workflow is quite similar to the likes of Quake and Source engine.
        However, it uses widely-supported file formats (glTF 2.0...), meaning there are
        no complications while importing assets.
      </>
    ),
  },
  {
    title: 'Optimised - Light as a feather',
    Svg: require('@site/static/img/features_feather.svg').default,
    description: (
      <>
        Written in high-performance C#, with a Vulkan backend and small filesize,
        Elegy swiftly runs on hardware from 8 years ago. It also provides you
        with plenty tools to optimise your game!
      </>
    ),
  },
  {
    title: 'Productive - Batteries included!!!',
    Svg: require('@site/static/img/features_batteries.svg').default,
    description: (
      <>
        That's right! The Elegy Game SDK has you covered:
        client-server model, weapon system, AI system,
        player controllers, all out of the box.
        Make games by modding!
      </>
    ),
  }
];

function Feature({title, Svg, description}: FeatureItem) {
  return (
    <div className={clsx('col col--6')}>
      <div className="text--center">
        <Svg className={styles.featureSvg} role="img" />
      </div>
      <div className="text--center padding-horiz--md">
        <Heading as="h3">{title}</Heading>
        <p>{description}</p>
      </div>
    </div>
  );
}

export default function HomepageOverview(): JSX.Element {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className='row'>
          <p className='col col--12 hero__subtitle text--center'>
            Note: This engine is a very early work in progress! 🛠 <br/>
            Check out the <Link to='/blog'>development articles</Link> in the meantime.
          </p>
        </div>
        <div className="row">
          {FeatureList.map((props, idx) => (
            <Feature key={idx} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
