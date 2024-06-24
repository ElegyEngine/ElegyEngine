import clsx from 'clsx';
import Heading from '@theme/Heading';
import styles from './features.module.css';

type WorkflowItem = {
  title: string;
  //Svg: React.ComponentType<React.ComponentProps<'svg'>>;
  description: JSX.Element;
};

const FeatureList: WorkflowItem[] = [
  {
    title: 'General workflow',
    description: (
      <>
        Elegy's workflow philosophy is all about decreasing friction and
        keeping things simple. Modern engines are designed for huge teams of
        specialists, which makes them inherently clunkier than necessary
        for small devs.
        <br /><br />
        In most game engines, you have an integrated engine editor, where you
        import assets into the engine's own file formats, and you run the game
        in-editor. That is not the case here.
        <br /><br />
        Instead, you just place assets into directories, and run the game.
        As you change the assets themselves, they get live-reloaded in-game.
        There's no import step either, simply export your glTF from Blender
        into a location like <code>mygame/models/</code> and you're done,
        it's ready to be used.
        <br /><br />
        In short, it's just you and the filesystem. No bloat in between.
      </>
    ),
  },
  {
    title: 'Graphics',
    description: (
      <>
        While Elegy was primarily designed for more authentic retro FPSes and such,
        it still offers plenty in the graphics department, but not in the way you think.
        <br /><br />
        Elegy has a concept of "rendering styles", which define how surfaces are to be
        shaded. The builtin styles include <code>RenderStyle99</code> and <code>
        RenderStyle2004</code>. One looks very much like Unreal and Quake 3, while
        the other looks like Half-Life 2.
        <br /><br />
        This means you can basically pick your own style of shading (or choose from
        custom, community-made ones!), or write your own. In theory, if you want a
        completely modern PBR renderer, you can have it. Even a ray-tracer. Not that
        the engine is built for that, but I mean, who's going to stop you?
        <br /><br />
        In either case, the engine supports basically the same stuff other engines do:
        static and animated 3D meshes, billboards/sprites/particles, decals, localised
        volumetrics like fog and smoke, water and so on.
        <br /><br />
        There is also a data-driven shader system, so if you want to write a new shader,
        you don't have to touch any of the rendering code at all. Just write your GLSL
        shader and compile it with <code>Elegy.ShaderTool</code>!
      </>
    ),
  },
  {
    title: 'Level design',
    description: (
      <>
        Elegy provides first-class support for TrenchBroom, the Quake level editor.
        <br />
        There is also partial support for J.A.C.K. and NetRadiant-custom, mostly coming
        down to file formats that those editors support.
        <br /><br />
        In essence, you make your levels directly in the level editor, place all objects
        (entities) inside, including triggers and lights, and bake/compile the level.
        If you need more complex level geometry, e.g. terrain, you can sculpt it in Blender
        and embed it into the level.
        <br /><br />
        This approach is not too different from modern engines, except the level editor is
        independent of the game engine (a standalone app), and you already have an easy
        solution for level scripting with triggers. And with the Game SDK, you can already
        start trying out ideas, playing with different
        contraptions that come with the SDK.
      </>
    ),
  },
  {
    title: '2D and 3D art',
    description: (
      <>
        Lorem ipsum bla bla bla.
      </>
    ),
  },
  {
    title: 'Graphical user interfaces',
    description: (
      <>
        Lorem ipsum bla bla bla.
      </>
    ),
  },
  {
    title: 'Programming',
    description: (
      <>
        Lorem ipsum bla bla bla.
      </>
    ),
  },
];

function Feature({title, description}: WorkflowItem) {
  return (
    <div className="text--center padding-horiz--md">
      <Heading as="h2">{title}</Heading>
      <p className={clsx(styles.featureDescription)}>{description}</p>
    </div>
  );
}

export default function HomepageFeatures(): JSX.Element {
  return (
    <section className={clsx("container", styles.features)}>
      {FeatureList.map((props, idx) => (
        <Feature key={idx} {...props} />
      ))}
    </section>
  );
}
