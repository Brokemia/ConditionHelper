using Monocle;
using Soukoku.ExpressionParser;
using Soukoku.ExpressionParser.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.ConditionHelper {
    public class ConditionWatcher {
        public const string FLAG_FCN = "flag";
        public const string LEVEL_FLAG_FCN = "levelFlag";
        public const string GOLDENS_LEVELSET_FCN = "levelSetGoldens";
        public const string GOLDENS_TOTAL_FCN = "totalGoldens";
        public const string BERRIES_CH_FCN = "chapterStrawberries";
        public const string BERRIES_CH_AVAILABLE_FCN = "chapterStrawberriesAvailable";
        public const string BERRIES_LEVELSET_FCN = "levelSetStrawberries";
        public const string BERRY_ID_COLLECTED_FCN = "berryCollected";
        public const string GRABBED_GOLDEN_FCN = "grabbedGolden";
        public const string DASHES_TOTAL_FCN = "totalDashes";
        public const string DASHES_SESSION_FCN = "sessionDashes";
        public const string LEVEL_START_DASHES_FCN = "levelStartDashes";
        public const string STARTED_FROM_BEGINNING_FCN = "startedFromBeginning";
        public const string DEATHS_TOTAL_FCN = "totalDeaths";
        public const string DEATHS_CH_FCN = "chapterDeaths";
        public const string DEATHS_SESSION_FCN = "sessionDeaths";
        public const string DEATHS_SCREEN_FCN = "screenDeaths";
        public const string FULL_CLEAR_FCN = "isFullClear";
        public const string HAS_HEART_FCN = "hasHeart";
        public const string HEARTS_LEVELSET_FCN = "levelSetHearts";
        public const string CASSETTES_LEVELSET_FCN = "levelSetCassettes";
        public const string TIME_FCN = "time";
        public const string IN_OVERWORLD_FCN = "inOverworld";
        public const string SID_FCN = "SID";
        public const string LEVELSET_FCN = "levelSet";
        public const string LEVEL_FCN = "room";
        public const string PLAYER_IN_SCENE_FCN = "playerInScene";
        public const string PLAYER_DEAD_BODY_IN_SCENE_FCN = "playerDeadBodyInScene";
        public const string PLAYER_X_FCN = "playerX";
        public const string PLAYER_Y_FCN = "playerY";

        private struct LabeledCallback {
            public int Label { get; set; }
            public Action Callback { get; set; }
        }

        private readonly Dictionary<string, List<LabeledCallback>> watching = new();

        private readonly InfixTokenizer tokenizer = new();

        private static int ID = 0;

        /// <summary>
        /// Register to be notified when any of the parts of a condition expression might have changed.<br/>
        /// You still need to check if the condition is actually true.
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="callback"></param>
        /// <returns>the unique ID for this callback</returns>
        public int WatchConditions(string expr, Action callback) {
            ExpressionToken[] tokens = tokenizer.Tokenize(expr);
            List<string> alreadyAdded = new();
            
            foreach(ExpressionToken token in tokens) {
                if(token.TokenType == ExpressionTokenType.Function && !alreadyAdded.Contains(token.Value)) {
                    if(!watching.ContainsKey(token.Value)) {
                        watching[token.Value] = new();
                    }
                    watching[token.Value].Add(new() { Label = ID, Callback = callback });
                    alreadyAdded.Add(token.Value);
                }
            }
            ID++;
            return ID - 1;
        }

        public void RemoveCallback(int id) {
            foreach (string key in watching.Keys) {
                watching[key].RemoveAll(lcb => lcb.Label == id);
            }
        }

        public void DoCallbacks(string function) {
            if (watching.ContainsKey(function)) {
                foreach (LabeledCallback cb in watching[function]) {
                    cb.Callback?.Invoke();
                }
            }
        }

        public void Load() {
            On.Celeste.Session.SetFlag += Session_SetFlag;
            On.Celeste.SaveData.AddStrawberry_AreaKey_EntityID_bool += SaveData_AddStrawberry;
            On.Celeste.Player.CallDashEvents += Player_CallDashEvents;
            On.Celeste.Player.Added += Player_Added;
            On.Celeste.Player.Removed += Player_Removed;
            On.Celeste.Player.SceneEnd += Player_SceneEnd;
            On.Monocle.Entity.Added += Entity_Added;
            On.Monocle.Entity.Removed += Entity_Removed;
            On.Monocle.Entity.SceneEnd += Entity_SceneEnd;
            On.Celeste.Level.Reload += Level_Reload;
            On.Celeste.Session.UpdateLevelStartDashes += Session_UpdateLevelStartDashes;
            On.Celeste.Level.UpdateTime += Level_UpdateTime;
            On.Monocle.Scene.Begin += Scene_Begin;
            On.Celeste.SaveData.AddDeath += SaveData_AddDeath;
            On.Celeste.SaveData.StartSession += SaveData_StartSession;
            On.Celeste.Commands.CmdLevelFlag += Commands_CmdLevelFlag;
            On.Celeste.Level.LoadLevel += Level_LoadLevel;
            On.Celeste.Level.Update += Level_Update;
            On.Celeste.MapData.Load += MapData_Load;
            On.Celeste.HeartGem.RegisterAsCollected += HeartGem_RegisterAsCollected;
            On.Celeste.SaveData.RegisterHeartGem += SaveData_RegisterHeartGem;
            On.Celeste.AreaModeStats.Clone += AreaModeStats_Clone;
            On.Celeste.Commands.CmdHearts_int += Commands_CmdHearts_int;
            On.Celeste.Commands.CmdHeartGem += Commands_CmdHeartGem;
            On.Celeste.Commands.CmdOWComplete += Commands_CmdOWComplete;
            On.Celeste.Commands.CmdHearts_int_string += Commands_CmdHearts_int_string;
            On.Celeste.AreaStats.Clone += AreaStats_Clone;
            On.Celeste.SaveData.RegisterCassette += SaveData_RegisterCassette;
            On.Celeste.SaveData.RegisterCompletion += SaveData_RegisterCompletion;
        }

        private void SaveData_RegisterCompletion(On.Celeste.SaveData.orig_RegisterCompletion orig, SaveData self, Session session) {
            orig(self, session);
            DoCallbacks(FULL_CLEAR_FCN);
        }

        private void SaveData_RegisterCassette(On.Celeste.SaveData.orig_RegisterCassette orig, SaveData self, AreaKey area) {
            orig(self, area);
            DoCallbacks(CASSETTES_LEVELSET_FCN);
        }

        private AreaStats AreaStats_Clone(On.Celeste.AreaStats.orig_Clone orig, AreaStats self) {
            var res = orig(self);
            DoCallbacks(CASSETTES_LEVELSET_FCN);
            return res;
        }

        private void SaveData_RegisterHeartGem(On.Celeste.SaveData.orig_RegisterHeartGem orig, SaveData self, AreaKey area) {
            orig(self, area);
            DoCallbacks(HEARTS_LEVELSET_FCN);
        }

        private void Commands_CmdHearts_int_string(On.Celeste.Commands.orig_CmdHearts_int_string orig, int amount, string levelSet) {
            orig(amount, levelSet);
            DoCallbacks(HEARTS_LEVELSET_FCN);
        }

        private void Commands_CmdOWComplete(On.Celeste.Commands.orig_CmdOWComplete orig, int index, int mode, int deaths, int strawberries, bool cassette, bool heartGem, float beatBestTimeBy, float beatBestFullClearTimeBy) {
            orig(index, mode, deaths, strawberries, cassette, heartGem, beatBestTimeBy, beatBestFullClearTimeBy);
            DoCallbacks(HEARTS_LEVELSET_FCN);
            DoCallbacks(CASSETTES_LEVELSET_FCN);
        }

        private void Commands_CmdHeartGem(On.Celeste.Commands.orig_CmdHeartGem orig, int area, int mode, bool gem) {
            orig(area, mode, gem);
            DoCallbacks(HEARTS_LEVELSET_FCN);
        }

        private void Commands_CmdHearts_int(On.Celeste.Commands.orig_CmdHearts_int orig, int amount) {
            orig(amount);
            DoCallbacks(HEARTS_LEVELSET_FCN);
        }

        private AreaModeStats AreaModeStats_Clone(On.Celeste.AreaModeStats.orig_Clone orig, AreaModeStats self) {
            var res = orig(self);
            DoCallbacks(HEARTS_LEVELSET_FCN);
            return res;
        }

        private void HeartGem_RegisterAsCollected(On.Celeste.HeartGem.orig_RegisterAsCollected orig, HeartGem self, Level level, string poemID) {
            orig(self, level, poemID);
            DoCallbacks(HAS_HEART_FCN);
        }

        private void MapData_Load(On.Celeste.MapData.orig_Load orig, MapData self) {
            orig(self);
            DoCallbacks(BERRIES_CH_AVAILABLE_FCN);
        }

        private void Entity_SceneEnd(On.Monocle.Entity.orig_SceneEnd orig, Entity self, Scene scene) {
            orig(self, scene);
            if (self is PlayerDeadBody) {
                DoCallbacks(PLAYER_DEAD_BODY_IN_SCENE_FCN);
            }
        }

        private void Entity_Removed(On.Monocle.Entity.orig_Removed orig, Entity self, Scene scene) {
            orig(self, scene);
            if (self is PlayerDeadBody) {
                DoCallbacks(PLAYER_DEAD_BODY_IN_SCENE_FCN);
            }
        }

        private void Entity_Added(On.Monocle.Entity.orig_Added orig, Entity self, Scene scene) {
            orig(self, scene);
            if(self is PlayerDeadBody) {
                DoCallbacks(PLAYER_DEAD_BODY_IN_SCENE_FCN);
            }
        }

        private void Player_SceneEnd(On.Celeste.Player.orig_SceneEnd orig, Player self, Scene scene) {
            orig(self, scene);
            DoCallbacks(PLAYER_IN_SCENE_FCN);
        }

        private void Player_Removed(On.Celeste.Player.orig_Removed orig, Player self, Scene scene) {
            orig(self, scene);
            DoCallbacks(PLAYER_IN_SCENE_FCN);
        }

        private void Player_Added(On.Celeste.Player.orig_Added orig, Player self, Scene scene) {
            orig(self, scene);
            DoCallbacks(PLAYER_IN_SCENE_FCN);
        }

        private void Level_LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);
            DoCallbacks(LEVEL_FLAG_FCN);
        }

        private void Commands_CmdLevelFlag(On.Celeste.Commands.orig_CmdLevelFlag orig, string flag) {
            orig(flag);
            DoCallbacks(LEVEL_FLAG_FCN);
        }

        private void SaveData_StartSession(On.Celeste.SaveData.orig_StartSession orig, SaveData self, Session session) {
            orig(self, session);
            DoCallbacks(SID_FCN);
            DoCallbacks(LEVELSET_FCN);
        }

        private void SaveData_AddDeath(On.Celeste.SaveData.orig_AddDeath orig, SaveData self, AreaKey area) {
            orig(self, area);
            DoCallbacks(DEATHS_TOTAL_FCN);
        }

        private void Scene_Begin(On.Monocle.Scene.orig_Begin orig, Monocle.Scene self) {
            orig(self);
            DoCallbacks(IN_OVERWORLD_FCN);
        }

        private void Level_UpdateTime(On.Celeste.Level.orig_UpdateTime orig, Level self) {
            orig(self);
            DoCallbacks(TIME_FCN);
        }

        private void Session_UpdateLevelStartDashes(On.Celeste.Session.orig_UpdateLevelStartDashes orig, Session self) {
            orig(self);
            DoCallbacks(LEVEL_START_DASHES_FCN);
        }

        private void Level_Reload(On.Celeste.Level.orig_Reload orig, Level self) {
            orig(self);
            DoCallbacks(DASHES_SESSION_FCN);
        }

        private void Player_CallDashEvents(On.Celeste.Player.orig_CallDashEvents orig, Player self) {
            orig(self);
            DoCallbacks(DASHES_SESSION_FCN);
            DoCallbacks(DASHES_TOTAL_FCN);
        }

        private void SaveData_AddStrawberry(On.Celeste.SaveData.orig_AddStrawberry_AreaKey_EntityID_bool orig, SaveData self, AreaKey area, EntityID strawberry, bool golden) {
            orig(self, area, strawberry, golden);
            DoCallbacks(BERRIES_CH_FCN);
            DoCallbacks(BERRIES_LEVELSET_FCN);
            DoCallbacks(GOLDENS_LEVELSET_FCN);
            DoCallbacks(GOLDENS_TOTAL_FCN);
            DoCallbacks(BERRY_ID_COLLECTED_FCN);
        }

        private void Session_SetFlag(On.Celeste.Session.orig_SetFlag orig, Session self, string flag, bool setTo) {
            orig(self, flag, setTo);
            DoCallbacks(FLAG_FCN);
        }

        private bool lastGrabbedGolden;
        private bool lastStartedFromBeginning;
        private string lastLevel;
        private HashSet<string> lastLevelFlags;

        private void Level_Update(On.Celeste.Level.orig_Update orig, Level self) {
            CheckUpdateChanges(self);
            orig(self);
            CheckUpdateChanges(self);
            if (self.Tracker.GetEntity<Player>() != null) {
                DoCallbacks(PLAYER_X_FCN);
                DoCallbacks(PLAYER_Y_FCN);
            }
        }

        private void CheckUpdateChanges(Level self) {
            if ((self.Session?.GrabbedGolden ?? lastGrabbedGolden) != lastGrabbedGolden) {
                DoCallbacks(GRABBED_GOLDEN_FCN);
                lastGrabbedGolden = self.Session.GrabbedGolden;
            }
            if ((self.Session?.StartedFromBeginning ?? lastStartedFromBeginning) != lastStartedFromBeginning) {
                DoCallbacks(STARTED_FROM_BEGINNING_FCN);
                lastStartedFromBeginning = self.Session.StartedFromBeginning;
            }
            if ((self.Session?.Level ?? lastLevel) != lastLevel) {
                DoCallbacks(LEVEL_FCN);
                lastLevel = self.Session.Level;
            }
            if ((self.Session?.LevelFlags ?? lastLevelFlags) != lastLevelFlags) {
                DoCallbacks(LEVEL_FLAG_FCN);
                lastLevelFlags = self.Session.LevelFlags;
            }
        }

        public void Unload() {
            On.Celeste.Session.SetFlag -= Session_SetFlag;
            On.Celeste.SaveData.AddStrawberry_AreaKey_EntityID_bool -= SaveData_AddStrawberry;
            On.Celeste.Player.CallDashEvents -= Player_CallDashEvents;
            On.Celeste.Player.Added -= Player_Added;
            On.Celeste.Player.Removed -= Player_Removed;
            On.Celeste.Player.SceneEnd -= Player_SceneEnd;
            On.Monocle.Entity.Added -= Entity_Added;
            On.Monocle.Entity.Removed -= Entity_Removed;
            On.Monocle.Entity.SceneEnd -= Entity_SceneEnd;
            On.Celeste.Level.Reload -= Level_Reload;
            On.Celeste.Session.UpdateLevelStartDashes -= Session_UpdateLevelStartDashes;
            On.Celeste.Level.UpdateTime -= Level_UpdateTime;
            On.Monocle.Scene.Begin -= Scene_Begin;
            On.Celeste.SaveData.AddDeath -= SaveData_AddDeath;
            On.Celeste.SaveData.StartSession -= SaveData_StartSession;
            On.Celeste.Commands.CmdLevelFlag -= Commands_CmdLevelFlag;
            On.Celeste.Level.LoadLevel -= Level_LoadLevel;
            On.Celeste.Level.Update -= Level_Update;
            On.Celeste.MapData.Load -= MapData_Load;
            On.Celeste.HeartGem.RegisterAsCollected -= HeartGem_RegisterAsCollected;
            On.Celeste.SaveData.RegisterHeartGem -= SaveData_RegisterHeartGem;
            On.Celeste.AreaModeStats.Clone -= AreaModeStats_Clone;
            On.Celeste.Commands.CmdHearts_int -= Commands_CmdHearts_int;
            On.Celeste.Commands.CmdHeartGem -= Commands_CmdHeartGem;
            On.Celeste.Commands.CmdOWComplete -= Commands_CmdOWComplete;
            On.Celeste.Commands.CmdHearts_int_string -= Commands_CmdHearts_int_string;
            On.Celeste.AreaStats.Clone -= AreaStats_Clone;
            On.Celeste.SaveData.RegisterCassette -= SaveData_RegisterCassette;
            On.Celeste.SaveData.RegisterCompletion -= SaveData_RegisterCompletion;
        }
    }
}
