using System.Linq;
using Monocle;
using MonoMod.ModInterop;
using Soukoku.ExpressionParser;

namespace Celeste.Mod.ConditionHelper {
    public class ConditionHelperModule : EverestModule {
        public static ConditionHelperModule Instance { get; private set; }

        public Evaluator ExpressionEvaluator { get; private set; }

        public ConditionWatcher ConditionWatcher { get; private set; } = new();

        public ConditionHelperModule() {
            Instance = this;
        }

        public override void Load() {
            typeof(ConditionHelperExports).ModInterop();

            ExpressionEvaluator = LoadEvaluator();
            ConditionWatcher.Load();
        }

        private Evaluator LoadEvaluator() {
            var context = new EvaluationContext();

            context.RegisterFunction(ConditionWatcher.FLAG_FCN, new FunctionRoutine(1, (ctx, args) => (Engine.Scene is Level level && level.Session.GetFlag(args[0].Value)) ? ExpressionToken.True : ExpressionToken.False));
            context.RegisterFunction(ConditionWatcher.LEVEL_FLAG_FCN, new FunctionRoutine(1, (ctx, args) => (Engine.Scene is Level level && level.Session.GetLevelFlag(args[0].Value)) ? ExpressionToken.True : ExpressionToken.False));
            context.RegisterFunction(ConditionWatcher.BERRIES_CH_FCN, new FunctionRoutine(0, (ctx, args) =>
                (SaveData.Instance == null || Engine.Scene is not Level level || level.Session is not Session session) ? new ExpressionToken("-1")
                : new(SaveData.Instance.Areas[session.Area.ID].Modes[(int)session.Area.Mode].TotalStrawberries.ToString())
            ));
            context.RegisterFunction(ConditionWatcher.BERRIES_CH_AVAILABLE_FCN, new FunctionRoutine(0, (ctx, args) =>
                (Engine.Scene is not Level level || level.Session is not Session session) ? new ExpressionToken("-1")
                : new(session.MapData.DetectedStrawberries.ToString())
            ));
            context.RegisterFunction(ConditionWatcher.BERRIES_LEVELSET_FCN, new FunctionRoutine(0, (ctx, args) =>
                (SaveData.Instance == null) ? new ExpressionToken("-1")
                : new(SaveData.Instance.LevelSetStats.TotalStrawberries.ToString())
            ));
            context.RegisterFunction(ConditionWatcher.GOLDENS_LEVELSET_FCN, new FunctionRoutine(0, (ctx, args) =>
                (SaveData.Instance == null) ? new ExpressionToken("-1")
                : new(SaveData.Instance.LevelSetStats.TotalGoldenStrawberries.ToString())
            ));
            context.RegisterFunction(ConditionWatcher.GOLDENS_TOTAL_FCN, new FunctionRoutine(0, (ctx, args) =>
                (SaveData.Instance == null) ? new ExpressionToken("-1")
                : new(SaveData.Instance.TotalGoldenStrawberries.ToString())
            ));
            context.RegisterFunction(ConditionWatcher.BERRY_ID_COLLECTED_FCN, new FunctionRoutine(2, (ctx, args) =>
                (SaveData.Instance != null && SaveData.Instance.CheckStrawberry(SaveData.Instance.LastArea_Safe, new EntityID(args[0].Value, (int)args[1].ToDecimal(context)))) ? ExpressionToken.True : ExpressionToken.False
            ));
            context.RegisterFunction(ConditionWatcher.GRABBED_GOLDEN_FCN, new FunctionRoutine(0, (ctx, args) => (Engine.Scene is Level level && level.Session.GrabbedGolden) ? ExpressionToken.True : ExpressionToken.False));
            context.RegisterFunction(ConditionWatcher.STARTED_FROM_BEGINNING_FCN, new FunctionRoutine(0, (ctx, args) => (Engine.Scene is Level level && level.Session.StartedFromBeginning) ? ExpressionToken.True : ExpressionToken.False));
            context.RegisterFunction(ConditionWatcher.DASHES_TOTAL_FCN, new FunctionRoutine(0, (ctx, args) =>
                SaveData.Instance == null ? new ExpressionToken("-1")
                : new(SaveData.Instance.TotalDashes.ToString())
            ));
            context.RegisterFunction(ConditionWatcher.DASHES_SESSION_FCN, new FunctionRoutine(0, (ctx, args) => Engine.Scene is Level level ? new ExpressionToken(level.Session.Dashes.ToString()) : new ExpressionToken("-1")));
            context.RegisterFunction(ConditionWatcher.LEVEL_START_DASHES_FCN, new FunctionRoutine(0, (ctx, args) => Engine.Scene is Level level ? new ExpressionToken(level.Session.DashesAtLevelStart.ToString()) : new ExpressionToken("-1")));
            context.RegisterFunction(ConditionWatcher.DEATHS_TOTAL_FCN, new FunctionRoutine(0, (ctx, args) =>
                SaveData.Instance == null ? new ExpressionToken("-1")
                : new(SaveData.Instance.TotalDeaths.ToString())
            ));
            context.RegisterFunction(ConditionWatcher.DEATHS_CH_FCN, new FunctionRoutine(0, (ctx, args) =>
                (SaveData.Instance == null || Engine.Scene is not Level level || level.Session is not Session session) ? new ExpressionToken("-1")
                : new(SaveData.Instance.Areas[session.Area.ID].Modes[(int)session.Area.Mode].Deaths.ToString())
            ));
            context.RegisterFunction(ConditionWatcher.DEATHS_SESSION_FCN, new FunctionRoutine(0, (ctx, args) => Engine.Scene is Level level ? new ExpressionToken(level.Session.Deaths.ToString()) : new ExpressionToken("-1")));
            context.RegisterFunction(ConditionWatcher.DEATHS_SCREEN_FCN, new FunctionRoutine(0, (ctx, args) => Engine.Scene is Level level ? new ExpressionToken(level.Session.DeathsInCurrentLevel.ToString()) : new ExpressionToken("-1")));
            context.RegisterFunction(ConditionWatcher.FULL_CLEAR_FCN, new FunctionRoutine(0, (ctx, args) => Engine.Scene is Level level ? (level.Session.FullClear ? ExpressionToken.True : ExpressionToken.False) : new ExpressionToken("-1")));
            context.RegisterFunction(ConditionWatcher.HAS_HEART_FCN, new FunctionRoutine(0, (ctx, args) => Engine.Scene is Level level ? (level.Session.HeartGem ? ExpressionToken.True : ExpressionToken.False) : new ExpressionToken("-1")));
            context.RegisterFunction(ConditionWatcher.HEARTS_LEVELSET_FCN, new FunctionRoutine(0, (ctx, args) => new(SaveData.Instance?.LevelSetStats?.TotalHeartGems.ToString() ?? "-1")));
            context.RegisterFunction(ConditionWatcher.CASSETTES_LEVELSET_FCN, new FunctionRoutine(0, (ctx, args) => new(SaveData.Instance?.LevelSetStats?.TotalCassettes.ToString() ?? "-1")));
            context.RegisterFunction(ConditionWatcher.TIME_FCN, new FunctionRoutine(0, (ctx, args) => Engine.Scene is Level level ? new ExpressionToken(level.Session.Time.ToString()) : new ExpressionToken("-1")));
            context.RegisterFunction(ConditionWatcher.IN_OVERWORLD_FCN, new FunctionRoutine(0, (ctx, args) => Engine.Scene is Overworld ? ExpressionToken.True : ExpressionToken.False));
            context.RegisterFunction(ConditionWatcher.SID_FCN, new FunctionRoutine(0, (ctx, args) => (Engine.Scene is Level level && level.Session is not null) ? new ExpressionToken(level.Session.Area.SID ?? "") : new ExpressionToken("")));
            context.RegisterFunction(ConditionWatcher.LEVELSET_FCN, new FunctionRoutine(0, (ctx, args) => (Engine.Scene is Level level && level.Session is not null) ? new ExpressionToken(level.Session.Area.LevelSet) : new ExpressionToken("")));
            context.RegisterFunction(ConditionWatcher.LEVEL_FCN, new FunctionRoutine(0, (ctx, args) => (Engine.Scene is Level level && level.Session is not null) ? new ExpressionToken(level.Session.Level) : new ExpressionToken("")));
            context.RegisterFunction(ConditionWatcher.PLAYER_IN_SCENE_FCN, new FunctionRoutine(0, (ctx, args) => Engine.Scene.Tracker.GetEntity<Player>() != null ? ExpressionToken.True : ExpressionToken.False));
            context.RegisterFunction(ConditionWatcher.PLAYER_DEAD_BODY_IN_SCENE_FCN, new FunctionRoutine(0, (ctx, args) => Engine.Scene.Entities.Any(e => e is PlayerDeadBody) ? ExpressionToken.True : ExpressionToken.False));
            context.RegisterFunction(ConditionWatcher.PLAYER_X_FCN, new FunctionRoutine(0, (ctx, args) => Engine.Scene.Tracker.GetEntity<Player>() is Player player ? new ExpressionToken(player.X.ToString()) : new ExpressionToken("0")));
            context.RegisterFunction(ConditionWatcher.PLAYER_Y_FCN, new FunctionRoutine(0, (ctx, args) => Engine.Scene.Tracker.GetEntity<Player>() is Player player ? new ExpressionToken(player.Y.ToString()) : new ExpressionToken("0")));

            // Utility
            context.RegisterFunction("strContains", new FunctionRoutine(2, (ctx, args) => args[0].Value.Contains(args[1].Value) ? ExpressionToken.True : ExpressionToken.False));
            context.RegisterFunction("strStartsWith", new FunctionRoutine(2, (ctx, args) => args[0].Value.StartsWith(args[1].Value) ? ExpressionToken.True : ExpressionToken.False));
            context.RegisterFunction("strEndsWith", new FunctionRoutine(2, (ctx, args) => args[0].Value.EndsWith(args[1].Value) ? ExpressionToken.True : ExpressionToken.False));
            context.RegisterFunction("strLen", new FunctionRoutine(1, (ctx, args) => new(args[0].Value.Length.ToString())));
            context.RegisterFunction("strToLower", new FunctionRoutine(1, (ctx, args) => new(args[0].Value.ToLower())));
            context.RegisterFunction("strToUpper", new FunctionRoutine(1, (ctx, args) => new(args[0].Value.ToUpper())));
            context.RegisterFunction("strTrim", new FunctionRoutine(1, (ctx, args) => new(args[0].Value.Trim())));

            return new Evaluator(context);
        }

        public override void Unload() {
            ConditionWatcher.Unload();
        }
    }
}