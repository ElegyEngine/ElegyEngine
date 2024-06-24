import clsx from 'clsx';
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
        Elegy has a workflow very reminiscent of engines like idTech, GoldSRC and Source.
        However, it <i>directly</i> uses widely-supported file formats like glTF 2.0, meaning
        no complications when you're importing assets.
      </>
    ),
  },
  {
    title: 'Optimised - Light as a feather',
    Svg: require('@site/static/img/features_feather.svg').default,
    description: (
      <>
        Written in high-performance C#, with a Vulkan backend and small filesize,
        Elegy swiftly runs on hardware from 10 years ago. It also provides you
        with plenty tools to optimise your game!
      </>
    ),
  },
  {
    title: 'Productive - Batteries included!!!',
    Svg: require('@site/static/img/features_batteries.svg').default,
    description: (
      <>
        That's right! You don't have to come up with your own game systems.
        The Elegy Game SDK has you covered, together with a
        decent player controller and a main menu, all for you to make yours.
        Make games by modding!
      </>
    ),
  },
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
        <div className="row">
          {FeatureList.map((props, idx) => (
            <Feature key={idx} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
