using Elegy.MapCompiler.Assets;
using Elegy.MapCompiler.Data.Processing;

namespace Elegy.MapCompiler.Processors;

public static class EntityExtensions
{
	public static string GetName( this Entity self )
		=> self.Pairs.GetValueOrDefault( "targetname", string.Empty );

	public static bool IsTargetKey( string key )
		// The compiler is not aware of the FGD nor the game DLL, so we just kinda hardcode this here
		=> key == "target" || key.StartsWith( "target_" );

	public static string GetTarget( this Entity self )
		=> self.Pairs.GetValueOrDefault( "target", string.Empty );

	public static string GetEmcDefaultInput( this Entity self )
		=> self.Pairs.GetValueOrDefault( "_emc_default_input", string.Empty );

	public static string GetEmcComponent( this Entity self )
		=> self.Pairs.GetValueOrDefault( "_emc_component", string.Empty );

	public static string[] GetTargets( this Entity self )
	{
		List<string> result = new();

		foreach ( var pair in self.Pairs )
		{
			if ( IsTargetKey( pair.Key ) && !string.IsNullOrEmpty( pair.Value ) )
			{
				result.Add( pair.Value );
			}
		}

		return result.ToArray();
	}

	public static int ForEachTarget( this ProcessingData self, Entity entity, Action<string, string> what )
	{
		// You might come from the hot paths, look at this and say "Eww you're allocating!"
		// Indeed I am. But fear not, this is not going meaningfully to slow down compilation
		// times, even for maps with thousands of entities
		List<KeyValuePair<string, string>> pairs = entity.Pairs
			.Where( p => IsTargetKey( p.Key ) && !string.IsNullOrEmpty( p.Value ) ).ToList();

		// The reason this is done is, these target fields may be modified or wiped out during
		// processing i.e. by the 'what' action. I like to have some real comfort in these areas
		foreach ( var pair in pairs )
		{
			what( pair.Key, pair.Value );
		}

		return pairs.Count;
	}

	public static int ForEachNamedEntity( this ProcessingData self, string name, Action<Entity> what )
	{
		List<Entity> entities = self.Entities.Where( e => e.GetName() == name ).ToList();

		foreach ( var entity in entities )
		{
			what( entity );
		}

		return entities.Count;
	}
}

public class LogicProcessor
{
	public ProcessingData Data { get; }
	public MapCompilerParameters Parameters { get; }

	// Stats for level designers
	private int mNumCallers;
	private int mNumTargets;
	private int mNumEvents;

	private TaggedLogger mLogger = new( "Logic" );

	public LogicProcessor( ProcessingData data, MapCompilerParameters parameters )
	{
		mLogger.Log( "Init" );

		Data = data;
		Parameters = parameters;
	}

	public void AppendOutput( Entity entity, string key, string target, string input, string param, float delay, int flags )
	{
		string value = $"{target},{input},{param},{delay:F},{flags}";

		// Every output counts as an IO event for level designer stats
		mNumEvents++;

		if ( !entity.Pairs.TryGetValue( key, out string? current )
		     || string.IsNullOrEmpty( current )
		     || current == target )
		{
			entity.Pairs[key] = value;
			return;
		}

		entity.Pairs[key] += $";{value}";
	}

	public void TranslateSimpleTrigger( Entity caller, string callerTargetKey, Entity target, float baseDelay = 0.0f )
	{
		string input = target.GetEmcDefaultInput();

		if ( string.IsNullOrEmpty( input ) )
		{
			mLogger.Warning( $"{caller} is trying to fire {target} which has no default input event" );
			return;
		}

		AppendOutput( caller, callerTargetKey, target.GetName(), input, string.Empty, baseDelay, 0 );
	}

	public void TranslateIoLink( Entity caller, string callerTargetKey, Entity link, float baseDelay = 0.0f )
	{
		string target = link.GetTarget();
		string component = link.GetEmcComponent();
		string @event = link.Pairs.GetValueOrDefault( "event", string.Empty );
		string param = link.Pairs.GetValueOrDefault( "param", string.Empty );
		float delay = baseDelay + link.Pairs.GetFloat( "delay" );
		bool fireOnce = link.Pairs.GetBool( "fire_once" );

		bool validationFailed = mLogger.WarningIf( string.IsNullOrEmpty( component ), $"{link}: missing '_emc_component' field, somehow" );
		validationFailed |= mLogger.WarningIf( string.IsNullOrEmpty( @event ), $"{link}: missing 'event' field" );
		if ( validationFailed )
		{
			return;
		}

		AppendOutput( caller, callerTargetKey, target, $"{component}.{@event}", param, delay, fireOnce ? 1 : 0 );
	}

	public void TranslateIoBundle( Entity caller, string callerTargetKey, Entity bundle, float baseDelay = 0.0f )
	{
		Data.ForEachTarget( bundle, ( key, target ) =>
		{
			float delay = bundle.Pairs.GetFloat( key.Replace( "target_", "delay_" ) );

			mNumTargets += HandleTranslationCases( caller, callerTargetKey, target, baseDelay + delay );
		} );
	}

	public int HandleTranslationCases( Entity caller, string key, string target, float baseDelay = 0.0f )
	{
		int targetsHit = Data.ForEachNamedEntity( target, targetEntity =>
		{
			// io_bundle bridges one entity to one or more others, need special logic here
			if ( targetEntity.ClassName is "io_bundle" or "multi_manager" )
			{
				TranslateIoBundle( caller, key, targetEntity, baseDelay );
			}
			else if ( targetEntity.ClassName.StartsWith( "io_" ) )
			{
				TranslateIoLink( caller, key, targetEntity, baseDelay );
			}
			else
			{
				TranslateSimpleTrigger( caller, key, targetEntity, baseDelay );
			}
		} );

		mLogger.WarningIf( targetsHit is 0, $"{caller} has a target '{target}' that doesn't exist" );

		return targetsHit;
	}

	public void TranslateQuakeTriggers()
	{
		// I think this name is safe to use, right?
		mLogger.Log( "TranslateQuakeTriggers" );

		// This translation handles three cases:
		// 1. Entity -> entity (with a default input)
		// 2. Entity -> IO link -> entity
		// 3. Entity -> IO bundle -> ... (repeat case 1, 2 or 3)
		foreach ( var callerEntity in Data.Entities )
		{
			// IOs don't do anything by themselves
			if ( callerEntity.ClassName.StartsWith( "io_" ) )
			{
				continue;
			}

			// There can be multiple target fields like 'target', 'target_1' etc.
			int numTargets = Data.ForEachTarget( callerEntity, ( targetKey, target ) =>
			{
				// It's safe to eat the key here, keeps the logic simple
				callerEntity.Pairs.Remove( targetKey );

				// 'target' and co. may refer to multiple entities with
				// the same name, so we loop once again...
				mNumTargets += HandleTranslationCases( callerEntity, targetKey, target );
			} );

			if ( numTargets > 0 )
			{
				mNumCallers++;
			}
		}

		// io_bundle etc. are foreign to the engine, they only exist at compile-time
		Data.Entities.RemoveAll( e => e.ClassName.StartsWith( "io_" ) );
		foreach ( var entity in Data.Entities )
		{
			entity.Pairs.Remove( "_emc_default_input" );
		}

		mLogger.Success( $"Processed {mNumCallers} triggers, {mNumTargets} targets and {mNumEvents} IO events!" );
	}
}
